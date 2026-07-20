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
///   Step 6 — Place 2 near your 3 for bonus value
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

        // Wait for player entry animations (~2.75 s) then block interaction with step 0
        StartCoroutine(DelayedStart(8.5f));
    }

    // ── Public game events ────────────────────────────────────────────────────

    public void OnDiceRolled()
    {
        if (!_active || !_waitingDice) return;
        _waitingDice = false;
        AdvanceTo(_step + 1);
    }

    /// <param name="line">Grid row where the player placed (from UiManager).</param>
    public void OnCellPlaced(int line = -1)
    {
        if (!_active || !_waitingCell) return;
        if (_step == 2 && line >= 0) _firstCellLine = line;
        _waitingCell = false;
        AdvanceTo(_step + 1);
    }

    /// <summary>Called by UiManager after the bot's move is rendered on screen.</summary>
    public void OnBotMovePlayed()
    {
        if (!_active || !_waitingBotMove) return;
        _waitingBotMove = false;
        // Force dice value 2 so the player's next roll is scripted
        GameManager.Instance?.diceRoller?.ForceNextValue(2);
        StartCoroutine(ShowStepDelayed(5, 0.8f));
    }

    public void OnOpponentTurnStarted()
    {
        if (!_active || _step != 4) return;
        ShowStep(4);
    }

    public void OnEliminationOccurred()
    {
        if (!_active || _step != 6) return;
        ShowStep(6);
    }

    // ── Fly bubble ────────────────────────────────────────────────────────────

    /// <summary>
    /// Animates a dice-value bubble flying between the two grids.
    /// playerToOpp=true  → dice button → opponent grid (player placed).
    /// playerToOpp=false → opponent area → player grid (bot placed).
    /// </summary>
    public void ShowDiceFlyBubble(int diceValue, bool playerToOpp)
    {
        if (!_active || flyBubbleRect == null) return;

        RectTransform from = playerToOpp ? diceBtnRect : oppGridRect;
        RectTransform to = playerToOpp ? oppGridRect : myGridRect;
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
            Debug.LogWarning("[Tutorial] Bot move timeout — forcing step 5");
            _waitingBotMove = false;
            GameManager.Instance?.diceRoller?.ForceNextValue(2);
            _step = 5;
            ShowStep(5);
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
                      "این بازی ترکیبی از سرعت، استراتژی و مدیریت ریسک است.\n\n" +
                      "در بالا: خانه‌های حریف\n" +
                      "در پایین: خانه‌های تو\n" +
                      "در وسط: دکمه تاس\n\n" +
                      "هدف اصلی اینه که سریع‌تر از حریف، خانه‌های خودت رو پر کنی.";
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
                      "اولین کار اینه که دکمه تاس رو فشار بدی.\n" +
                      "یک عدد تصادفی بهت می‌ده؛ بعد باید آن را در صفحه‌ی خودت قرار بدی.\n\n" +
                      "روی دکمه تاس کلیک کن.";
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
                      "حالا باید این عدد رو روی یکی از خانه‌های خودت بگذاری.\n" +
                      "می‌تونی هر خانه‌ای رو انتخاب کنی؛ مهم اینه که در آینده، موقعیتش رو برای امتیاز و دفاع از خودت حساب کنی.\n\n" +
                      "روی یک خانه کلیک کن.";
                highlight = myGridRect;
                _waitingCell = true;
                break;

            // ── 3: Scoring rules ──────────────────────────────────────────────
            case 3:
                IsBotMoveSuppressed = true;
                msg = "امتیازدهی بازی خیلی مهمه!\n\n" +
                      "- اعداد متفاوت: جمع معمولی اعداد\n" +
                      "  مثال: [2، 4، 6] = 12\n\n" +
                      "- دو عدد یکسان: امتیاز بیشتری می‌دهد\n" +
                      "  مثال: [3، 3، 5] = 17\n\n" +
                      "- سه عدد یکسان: بیشترین امتیاز را می‌دهد\n" +
                      "  مثال: [4، 4، 4] = 36\n\n" +
                      "پس قرار دادن اعداد تکراری می‌تواند خیلی پرامتیاز باشد.";
                highlight = scoreAreaRect;
                break;

            // ── 4: Elimination mechanic ───────────────────────────────────────
            case 4:
                IsBotMoveSuppressed = true;
                msg = "یکی از مهم‌ترین مکانیک‌ها، حذف است!\n\n" +
                      "اگر دوستت در ردیف روبه‌رو، یک عدد مشابه قرار بدهد،\n" +
                      "همه‌ی اعداد مشابه تو در آن ردیف پاک می‌شوند.\n\n" +
                      "یعنی هم امتیاز تو از بین می‌رود و هم فضای صفحه برایت سخت‌تر می‌شود.\n\n" +
                      "این یعنی باید بین امتیاز گرفتن و حفاظت از خودت تعادل داشته باشی.";
                highlight = null;
                break;

            // ── 5: Opponent turn and bot move ────────────────────────────────
            case 5:
                msg = "حالا نوبت حریفه!\n\n" +
                      "او هم مثل تو تاس می‌اندازد و عددی را در صفحه‌ی خودش قرار می‌دهد.\n\n" +
                      "وقتی این مرحله تمام شد، دوباره نوبت تو خواهد بود.\n\n" +
                      "روی دکمه تاس کلیک کن.";
                highlight = diceBtnRect;
                btnLabel = "باشه";
                _waitingDice = true;
                break;

            // ── 6: Place 2 next to the 3 (double score) ──────────────────────
            case 6:
                msg = "عدد 2 گرفتی!\n\n" +
                      "اگر این عدد را در همان ردیفی که 3 داری بگذاری،\n" +
                      "امتیاز آن ردیف خیلی بیشتر می‌شود.\n\n" +
                      "اما مراقب باش؛ اگر حریف هم در آن ردیف عدد مشابهی بگذارد،\n" +
                      "می‌تواند آن بخش را از تو پاک کند.\n\n" +
                      "روی خانه‌ای کلیک کن.";
                highlight = myGridRect;
                _waitingCell = true;
                break;

            // ── 7: Speed and win condition ───────────────────────────────────
            case 7:
                msg = "هدف بازی خیلی ساده است:\n\n" +
                      "اولین بازیکنی که همه‌ی خانه‌های خود را پر کند، برنده است!\n\n" +
                      "پس در این بازی باید هم سریع حرکت کنی،\n" +
                      "هم امتیاز جمع‌آوری کنی،\n" +
                      "و هم از حذف شدن توسط حریف جلوگیری کنی.\n\n" +
                      "این تعادل، کلید پیروزی است.";
                btnLabel = "ادامه";
                highlight = null;
                break;

            // ── 8: Final summary ─────────────────────────────────────────────
            case 8:
                msg = "حالا بازی را خوب فهمیدی!\n\n" +
                      "تاس بگیر، عدد را در صفحه‌ی خود قرار بده،\n" +
                      "امتیاز جمع کن، از حذف جلوگیری کن،\n" +
                      "و زودتر از حریف خانه‌ها را کامل کن.\n\n" +
                      "آماده‌ای که بازی را شروع کنی؟";
                btnLabel = "شروع بازی";
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
            // Pass clicks through to game elements when player must interact
            bool block = !_waitingDice && !_waitingCell;
            overlayGroup.blocksRaycasts = block;
            overlayGroup.interactable = block;
            overlayGroup.alpha = 0f;
            overlayGroup.DOFade(0.72f, 0.25f);
        }
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
}
