using System.Collections;
using DG.Tweening;
using Nakama.Helpers;
using NinjaBattle.Game;
using NinjaBattle.General;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tutorial scripted sequence:
///   Step 0 — Board intro and turn structure
///   Step 1 — Roll dice (forced value: 3)
///   Step 2 — Place your 3 in any cell
///   Step 3 — Scoring rules
///   Step 4 — Elimination mechanic
///   Step 5 — Opponent turn and bot move
///   Step 6 — Place another 3 in the same line to create a double
///   Step 7 — Speed and win condition
///   Step 8 — Final summary → CompleteTutorial
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Overlay")]
    [SerializeField] private CanvasGroup overlayGroup;
    [SerializeField] private GameObject overlayPanel;

    [Header("Bubble")]
    [SerializeField] private RectTransform bubbleRect;
    [SerializeField] private CanvasGroup bubbleGroup;
    [SerializeField] private RTLTextMeshPro messageText;
    [SerializeField] private RTLTextMeshPro stepText;
    [SerializeField] private Button nextButton;
    [SerializeField] private RTLTextMeshPro nextButtonText;
    [SerializeField] private Button skipButton;

    [Header("Pointer")]
    [SerializeField] private RectTransform arrowImage;

    [Header("Highlight")]
    [SerializeField] private RectTransform highlightRect;

    [Header("Targets")]
    [SerializeField] private RectTransform diceBtnRect;
    [SerializeField] private RectTransform myGridRect;
    [SerializeField] private RectTransform oppGridRect;
    [SerializeField] private RectTransform scoreAreaRect;

    [Header("Dice Fly Bubble")]
    [SerializeField] private RectTransform flyBubbleRect;
    [SerializeField] private CanvasGroup flyBubbleGroup;
    [SerializeField] private RTLTextMeshPro flyBubbleText;

    // ── State ─────────────────────────────────────────────────────────────────
    private int _step = 0;
    private bool _active = false;
    private bool _waitingDice = false;
    private bool _waitingCell = false;
    private bool _waitingBotMove = false;   // waiting for bot to play before showing step 5
    private int _firstCellLine = -1;       // row where player placed first cell
    private int _firstCellRow = -1;
    private bool _firstAnchorRemoved = false;

    private Canvas _interactionCanvas;
    private GraphicRaycaster _interactionRaycaster;
    private bool _addedInteractionCanvas;
    private bool _addedInteractionRaycaster;
    private bool _previousOverrideSorting;
    private int _previousSortingOrder;

    private const int TotalSteps = 9;

    // Bot suppression: while true PlayersManager buffers incoming bot messages
    public bool IsBotMoveSuppressed { get; private set; }
    public bool IsActive => _active;
    private System.Action _onBotReleased;

    public void SetBotReleaseCallback(System.Action callback) => _onBotReleased = callback;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake() => Instance = this;

    private void Start()
    {
        if (overlayPanel != null) overlayPanel.SetActive(false);
        if (flyBubbleRect != null) flyBubbleRect.gameObject.SetActive(false);

        if (PlayerPrefs.GetInt(WelcomePopup.TutorialModeKey, 0) != 1)
            return;

        _active = true;
        AnalyticsTracker.SendDesign("tutorial_started");
        if (nextButton != null) nextButton.onClick.AddListener(OnNext);
        if (skipButton != null) skipButton.onClick.AddListener(OnSkip);

        // Wait only for the battle entry animation, then start teaching immediately.
        StartCoroutine(DelayedStart(3f));
    }

    // ── Public game events ────────────────────────────────────────────────────

    public void OnDiceRolled()
    {
        if (!_active || !_waitingDice) return;
        _waitingDice = false;
        AdvanceTo(_step + 1);
    }

    /// <param name="line">Grid line where the player placed (from UiManager).</param>
    /// <param name="row">Grid row where the player placed (from UiManager).</param>
    public void OnCellPlaced(int line = -1, int row = -1)
    {
        if (!_active || !_waitingCell) return;
        if (_step == 2 && line >= 0)
        {
            _firstCellLine = line;
            _firstCellRow = row;
        }
        _waitingCell = false;
        AdvanceTo(_step + 1);
    }

    /// <summary>Called by UiManager after the bot's move is rendered on screen.</summary>
    public void OnBotMovePlayed()
    {
        if (!_active || !_waitingBotMove) return;
        _waitingBotMove = false;
        // Repeat the first value so the next placement visibly creates a double.
        GameManager.Instance?.diceRoller?.ForceNextValue(3);
        StartCoroutine(ShowStepDelayed(5, 0.8f));
    }

    public void OnOpponentTurnStarted()
    {
        if (!_active || _step != 4) return;
        ShowStep(4);
    }

    public void OnEliminationOccurred(int line, int row)
    {
        if (!_active) return;
        if (line == _firstCellLine && row == _firstCellRow)
            _firstAnchorRemoved = true;
    }

    public bool CanRollDice()
    {
        return TutorialInputRules.CanRoll(_active, _waitingDice);
    }

    public bool CanPlaceCell(ClickInCell cell)
    {
        if (cell == null)
            return false;

        return TutorialInputRules.CanPlace(
            _active,
            _waitingCell,
            _step,
            _firstCellLine,
            _firstAnchorRemoved,
            cell.numberLine);
    }

    public void NotifyInvalidCell(ClickInCell cell)
    {
        if (!_active || !_waitingCell)
            return;

        if (_step == 6 && !_firstAnchorRemoved && cell != null && cell.numberLine != _firstCellLine)
            messageText.text = "برای ساخت دابل، تاس ۳ دوم را در همان ردیف تاس ۳ قبلی قرار بده.";
        else
            messageText.text = "اول تاس را بینداز، سپس یکی از خانه‌های روشن صفحه خودت را انتخاب کن.";

        bubbleRect?.DOShakeAnchorPos(0.35f, 12f, 12, 70f, false, true);
    }

    // ── Fly bubble ────────────────────────────────────────────────────────────

    /// <summary>
    /// Animates a dice-value bubble flying between the two grids.
    /// isLocalMove=true  → dice button → local grid.
    /// isLocalMove=false → dice button → opponent grid.
    /// </summary>
    public void ShowDiceFlyBubble(int diceValue, bool isLocalMove)
    {
        if (!_active || flyBubbleRect == null) return;

        RectTransform from = diceBtnRect;
        RectTransform to = isLocalMove ? myGridRect : oppGridRect;
        if (from == null || to == null) return;

        if (flyBubbleText != null) flyBubbleText.text = diceValue.ToString();

        flyBubbleRect.DOKill();
        flyBubbleRect.gameObject.SetActive(true);
        flyBubbleRect.SetAsLastSibling();   // render above overlay panel
        flyBubbleRect.position = from.position;
        flyBubbleRect.localScale = Vector3.one * 0.5f;
        if (flyBubbleGroup != null) flyBubbleGroup.alpha = 0f;

        var seq = DOTween.Sequence();
        if (flyBubbleGroup != null)
            seq.Append(flyBubbleGroup.DOFade(1f, 0.2f));
        seq.Join(flyBubbleRect.DOScale(1.1f, 0.25f).SetEase(Ease.OutBack));
        seq.Append(flyBubbleRect.DOMove(to.position, 0.65f).SetEase(Ease.InOutSine));
        if (flyBubbleGroup != null)
            seq.Join(flyBubbleGroup.DOFade(0f, 0.25f).SetDelay(0.45f));
        seq.Join(flyBubbleRect.DOScale(0.6f, 0.3f).SetDelay(0.4f));
        seq.OnComplete(() => flyBubbleRect.gameObject.SetActive(false));
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private IEnumerator DelayedStart(float delay)
    {
        yield return new WaitForSeconds(delay);

        // UiManager is enabled by GameManager only after the battle entry
        // animation finishes — start teaching once the board is interactable.
        float timeout = 10f;
        while (timeout > 0f && (UiManager.instance == null || !UiManager.instance.enabled))
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.35f);

        ShowStep(0);
    }

    private IEnumerator ShowStepDelayed(int step, float delay)
    {
        yield return new WaitForSeconds(delay);
        _step = step;
        ShowStep(step);
    }

    private void OnNext()
    {
        if (!_active) return;
        AdvanceTo(_step + 1);
    }

    private void OnSkip() => CompleteTutorial();

    private void AdvanceTo(int step)
    {
        // Step 4 → 5: release bot, wait for OnBotMovePlayed() before showing step 5
        if (_step == 4 && step == 5)
        {
            IsBotMoveSuppressed = false;
            _waitingBotMove = true;
            HideOverlay();

            // Legacy callback path
            _onBotReleased?.Invoke();
            _onBotReleased = null;

            // Robust direct call (no script-execution-order dependency)
            Debug.Log($"[Tutorial] Step 4→5 release. HasPending={PlayersManager.Instance?.HasPendingBotMessage}");
            PlayersManager.Instance?.ApplyPendingBotMessage();

            // Safety net if bot move never arrives
            StartCoroutine(BotMoveTimeoutFallback(8f));
            return;
        }

        _step = step;
        if (_step >= TotalSteps)
        {
            CompleteTutorial();
            return;
        }
        ShowStep(_step);
    }

    private IEnumerator BotMoveTimeoutFallback(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (_active && _waitingBotMove)
        {
            PlayersManager.Instance?.ApplyPendingBotMessage();
            if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.isTurn)
            {
                _waitingBotMove = false;
                GameManager.Instance?.diceRoller?.ForceNextValue(3);
                _step = 5;
                ShowStep(5);
                yield break;
            }

            Debug.LogWarning("[Tutorial] Bot move is delayed; keeping gameplay input locked.");
            UpdateBubble(
                "حرکت حریف آموزشی کمی طول کشیده است. اتصال را بررسی کن؛ به محض رسیدن حرکت، آموزش خودکار ادامه پیدا می‌کند.",
                "در انتظار حریف...",
                4,
                true);
            UpdateHighlight(oppGridRect);
            ShowOverlay();

            // Keep polling — if the connection recovers the tutorial resumes on its own.
            StartCoroutine(BotMoveTimeoutFallback(4f));
        }
    }

    // ── Step content ──────────────────────────────────────────────────────────

    private void ShowStep(int step)
    {
        _step = step;
        _waitingDice = false;
        _waitingCell = false;

        string msg = "";
        string btnLabel = "متوجه شدم";
        RectTransform highlight = null;

        switch (step)
        {
            // ── 0: Board intro ────────────────────────────────────────────────
            case 0:
                msg = "به آموزش بازی تاس خوش اومدی!\n\n" +
                      "صفحه پایین برای تو و صفحه بالا برای حریف است.\n" +
                      "دکمه تاس وسط صفحه قرار دارد.\n\n" +
                      "در هر نوبت فقط دو کار انجام می‌دهی: تاس می‌اندازی و عدد را در یکی از خانه‌های خودت می‌گذاری.";
                highlight = myGridRect;
                btnLabel = "بریم";
                break;

            // ── 1: Roll dice (forced 3) ───────────────────────────────────────
            case 1:
                GameManager.Instance?.diceRoller?.ForceNextValue(3);
                // Resume timer so player doesn't wait forever at an un-timed screen
                if (TimerTurn.instance != null)
                {
                    TimerTurn.instance.TimerPause = false;
                    TimerTurn.instance.TimerRunning = false; // controlled by IsTurn events
                }
                msg = "نوبت توست!\n\n" +
                        "دکمه‌ای که حرکت می‌کند آماده کلیک است.\n" +
                        "روی تاس بزن؛ برای این تمرین عدد ۳ می‌آید.";
                highlight = diceBtnRect;
                btnLabel = "باشه";
                _waitingDice = true;
                break;

            // ── 2: Place the 3 ───────────────────────────────────────────────
            case 2:
                // Suppress bot moves NOW so any move arriving after the player places
                // is guaranteed to be buffered (don't wait for step 3)
                IsBotMoveSuppressed = true;
                msg = "عدد 3 گرفتی!\n\n" +
                        "خانه‌های روشن پایین، انتخاب‌های مجاز تو هستند.\n" +
                        "تاس ۳ را روی یکی از آن‌ها بگذار. جای آن را به خاطر می‌گیریم تا بعداً دابل بسازیم.";
                highlight = myGridRect;
                _waitingCell = true;
                break;

            // ── 3: Scoring rules ──────────────────────────────────────────────
            case 3:
                IsBotMoveSuppressed = true;
                msg = "امتیازدهی بازی خیلی مهمه!\n\n" +
                        "اعداد متفاوت با هم جمع می‌شوند.\n" +
                        "دو عدد یکسان، چهار برابر ارزش تاس امتیاز می‌دهند: [۳، ۳] = ۱۲.\n" +
                        "سه عدد یکسان، نه برابر ارزش تاس هستند: [۴، ۴، ۴] = ۳۶.\n\n" +
                        "وقتی دابل یا تریپل بسازی، پارتیکل رنگی زیر همان تاس‌ها روشن می‌شود.";
                highlight = scoreAreaRect;
                break;

            // ── 4: Elimination mechanic ───────────────────────────────────────
            case 4:
                IsBotMoveSuppressed = true;
                msg = "یکی از مهم‌ترین مکانیک‌ها، حذف است!\n\n" +
                        "اگر حریف در خط روبه‌رو عددی برابر با تاس تو بگذارد، تاس‌های هم‌عدد تو از آن خط حذف می‌شوند.\n\n" +
                        "این کار امتیاز حریف را کم می‌کند و دوباره برای تو جا باز می‌کند. حالا حرکت حریف آموزشی را ببین.";
                highlight = oppGridRect;
                break;

            // ── 5: Opponent turn and bot move ────────────────────────────────
            case 5:
                msg = "حریف حرکتش را انجام داد!\n\n" +
                        "دیدی که تاسش را در صفحه بالا گذاشت. حالا دوباره نوبت توست.\n" +
                        "برای تمرین دابل، این بار هم عدد ۳ می‌آید. روی دکمه تاس کلیک کن.";
                highlight = diceBtnRect;
                btnLabel = "باشه";
                _waitingDice = true;
                break;

            // ── 6: Place the repeated 3 in the original line ─────────────────
            case 6:
                msg = _firstAnchorRemoved
                    ? "عدد ۳ گرفتی. حریف تاس قبلی تو را حذف کرد؛ این نمونه واقعی مکانیک حذف بود. حالا ۳ جدید را در یکی از خانه‌های روشن بگذار."
                    : "عدد ۳ گرفتی! آن را در همان ردیف تاس ۳ قبلی بگذار تا دابل ساخته شود. خانه‌های ردیف‌های دیگر فعلاً قبول نمی‌شوند.";
                highlight = myGridRect;
                _waitingCell = true;
                break;

            // ── 7: Speed and win condition ───────────────────────────────────
            case 7:
                msg = _firstAnchorRemoved
                    ? "حالا هم رول‌کردن، جای‌گذاری و حذف را در عمل دیدی. برای امتیاز بیشتر، در نوبت‌های بعدی عددهای یکسان را در یک خط کنار هم بساز."
                    : "عالی! دابل ۳ ساخته شد؛ امتیاز این جفت ۱۲ است و پارتیکل رنگی زیر هر دو تاس باید روشن باشد. تریپل همان عدد، امتیاز بیشتری می‌دهد.";
                btnLabel = "ادامه";
                highlight = scoreAreaRect;
                break;

            // ── 8: Final summary ─────────────────────────────────────────────
            case 8:
                msg = "حالا بازی را خوب فهمیدی!\n\n" +
                        "در نوبت خودت: تاس بینداز و آن را روی صفحه پایین قرار بده.\n" +
                        "عددهای یکسان را هم‌خط کن تا دابل و تریپل بسازی.\n" +
                        "با عدد مساوی، تاس‌های خط روبه‌روی حریف را حذف کن.\n\n" +
                        "آماده‌ای همین بازی با بات را ادامه بدهی؟";
                btnLabel = "ادامه بازی با بات";
                highlight = null;
                break;
        }

        AnalyticsTracker.SendDesign("tutorial_step", 1f, new System.Collections.Generic.Dictionary<string, object>
        {
            ["step"] = step + 1
        });

        UpdateBubble(msg, btnLabel, step, _waitingDice || _waitingCell);
        UpdateHighlight(highlight);
        PositionBubble(step);
        ShowOverlay();
    }

    /// <summary>
    /// Move the bubble so it never blocks the grid the player needs to see.
    /// Player-turn steps (1,2,5,6): bubble goes to opponent's grid (top).
    /// Bot-turn step (4): bubble goes to player's grid (bottom).
    /// </summary>
    private void PositionBubble(int step)
    {
        if (bubbleRect == null) return;

        RectTransform target = null;
        if (step == 1 || step == 2 || step == 5 || step == 6)
            target = oppGridRect;   // player rolling/placing → bubble over opp grid
        else if (step == 4)
            target = myGridRect;    // bot's turn → bubble over my grid

        if (target == null) return;

        bubbleRect.DOKill();
        bubbleRect.DOMove(target.position, 0.3f).SetEase(Ease.OutQuad);
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    private void ShowOverlay()
    {
        if (overlayPanel == null) return;
        overlayPanel.SetActive(true);

        if (overlayGroup != null)
        {
            // Keep the overlay blocking everything; only the requested target is
            // lifted into a temporary higher canvas by ExposeInteractionTarget.
            overlayGroup.blocksRaycasts = true;
            overlayGroup.interactable = true;
            overlayGroup.alpha = 0f;
            overlayGroup.DOFade(0.72f, 0.25f);
        }

        ExposeInteractionTarget(_waitingDice ? diceBtnRect : _waitingCell ? myGridRect : null);
        if (bubbleRect != null)
        {
            bubbleRect.localScale = Vector3.one * 0.85f;
            bubbleRect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
        if (bubbleGroup != null)
        {
            bubbleGroup.alpha = 0f;
            bubbleGroup.DOFade(1f, 0.25f);
        }
    }

    private void UpdateBubble(string msg, string btnLabel, int step, bool waitingInput)
    {
        if (messageText != null) messageText.text = msg;
        if (stepText != null) stepText.text = $"مرحله {step + 1} از {TotalSteps}";
        if (nextButtonText != null) nextButtonText.text = btnLabel;
        if (nextButton != null) nextButton.gameObject.SetActive(!waitingInput);
    }

    private void UpdateHighlight(RectTransform target)
    {
        if (highlightRect == null) return;
        if (target == null)
        {
            highlightRect.gameObject.SetActive(false);
            if (arrowImage != null) arrowImage.gameObject.SetActive(false);
            return;
        }

        highlightRect.gameObject.SetActive(true);
        highlightRect.position = target.position;
        highlightRect.sizeDelta = target.rect.size + new Vector2(20f, 20f);

        highlightRect.DOKill();
        highlightRect.DOScale(new Vector3(1.04f, 1.04f, 1f), 0.6f)
                     .SetLoops(-1, LoopType.Yoyo)
                     .SetEase(Ease.InOutSine);

        if (arrowImage != null)
        {
            arrowImage.gameObject.SetActive(true);
            arrowImage.position = target.position + new Vector3(0, -target.rect.height * 0.5f - 40f, 0);
            arrowImage.DOKill();
            arrowImage.DOAnchorPosY(arrowImage.anchoredPosition.y - 10f, 0.5f)
                      .SetLoops(-1, LoopType.Yoyo)
                      .SetEase(Ease.InOutSine);
        }
    }

    private void HideOverlay()
    {
        RestoreInteractionTarget();
        highlightRect?.DOKill();
        arrowImage?.DOKill();

        if (overlayGroup != null)
            overlayGroup.DOFade(0f, 0.2f).OnComplete(() =>
            {
                if (overlayPanel != null) overlayPanel.SetActive(false);
            });
        else if (overlayPanel != null)
            overlayPanel.SetActive(false);
    }

    private void CompleteTutorial()
    {
        _active = false;
        IsBotMoveSuppressed = false;
        if (PlayersManager.Instance != null && PlayersManager.Instance.HasPendingBotMessage)
            PlayersManager.Instance.ApplyPendingBotMessage();
        AnalyticsTracker.SendDesign("tutorial_complete");
        PlayerPrefs.SetInt(WelcomePopup.TutorialDoneKey, 1);
        PlayerPrefs.SetInt(WelcomePopup.TutorialModeKey, 0);
        PlayerPrefs.Save();
        HideOverlay();

        // Re-enable timer so normal game rules apply after tutorial
        if (TimerTurn.instance != null)
        {
            TimerTurn.instance.TimerPause = false;
            TimerTurn.instance.TimerRunning = MultiplayerManager.Instance?.isTurn ?? false;
        }
    }

    private void ExposeInteractionTarget(RectTransform target)
    {
        RestoreInteractionTarget();
        if (target == null)
            return;

        Canvas parentCanvas = target.parent != null ? target.parent.GetComponentInParent<Canvas>() : null;
        _interactionCanvas = target.GetComponent<Canvas>();
        _addedInteractionCanvas = _interactionCanvas == null;
        if (_addedInteractionCanvas)
            _interactionCanvas = target.gameObject.AddComponent<Canvas>();
        else
        {
            _previousOverrideSorting = _interactionCanvas.overrideSorting;
            _previousSortingOrder = _interactionCanvas.sortingOrder;
        }

        _interactionCanvas.overrideSorting = true;
        _interactionCanvas.sortingOrder = (parentCanvas != null ? parentCanvas.sortingOrder : 0) + 50;

        _interactionRaycaster = target.GetComponent<GraphicRaycaster>();
        _addedInteractionRaycaster = _interactionRaycaster == null;
        if (_addedInteractionRaycaster)
            _interactionRaycaster = target.gameObject.AddComponent<GraphicRaycaster>();
    }

    private void RestoreInteractionTarget()
    {
        if (_interactionRaycaster != null && _addedInteractionRaycaster)
            Destroy(_interactionRaycaster);

        if (_interactionCanvas != null)
        {
            if (_addedInteractionCanvas)
                Destroy(_interactionCanvas);
            else
            {
                _interactionCanvas.overrideSorting = _previousOverrideSorting;
                _interactionCanvas.sortingOrder = _previousSortingOrder;
            }
        }

        _interactionCanvas = null;
        _interactionRaycaster = null;
        _addedInteractionCanvas = false;
        _addedInteractionRaycaster = false;
    }

    private void OnDestroy()
    {
        RestoreInteractionTarget();
        bubbleRect?.DOKill();
        highlightRect?.DOKill();
        arrowImage?.DOKill();
        flyBubbleRect?.DOKill();
        if (nextButton != null) nextButton.onClick.RemoveListener(OnNext);
        if (skipButton != null) skipButton.onClick.RemoveListener(OnSkip);
        if (Instance == this) Instance = null;
    }
}

public static class TutorialInputRules
{
    public static bool CanRoll(bool tutorialActive, bool waitingForDice)
    {
        return !tutorialActive || waitingForDice;
    }

    public static bool CanPlace(bool tutorialActive, bool waitingForCell, int step,
        int firstCellLine, bool firstAnchorRemoved, int candidateLine)
    {
        if (!tutorialActive)
            return true;
        if (!waitingForCell)
            return false;
        if (step != 6 || firstAnchorRemoved || firstCellLine < 0)
            return true;

        return candidateLine == firstCellLine;
    }
}
