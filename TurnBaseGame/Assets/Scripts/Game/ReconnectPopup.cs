using System.Collections;
using DG.Tweening;
using Nakama.Helpers;
using NinjaBattle.Game;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// نمایش پاپ‌آپ "حریف قطع شد" با تایمر معکوس ۲۰ ثانیه‌ای.
///
/// Inspector wiring:
///   popupPanel      — پنل اصلی پاپ‌آپ
///   canvasGroup     — CanvasGroup پنل (برای fade)
///   countdownText   — متن عدد تایمر
///   messageText     — متن توضیحات
/// </summary>
public class ReconnectPopup : MonoBehaviour
{
    public static ReconnectPopup Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RTLTextMeshPro countdownText;
    [SerializeField] private RTLTextMeshPro messageText;

    private Coroutine _hideCoroutine;
    private Coroutine _countdownCoroutine;
    private bool      _winTriggered;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake() => Instance = this;

    private void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);

        var mm = MultiplayerManager.Instance;
        mm.Subscribe(MultiplayerManager.Code.OpponentDisconnected, OnOpponentDisconnected);
        mm.Subscribe(MultiplayerManager.Code.OpponentReconnected, OnOpponentReconnected);
        mm.Subscribe(MultiplayerManager.Code.DisconnectWin, OnDisconnectWin);
    }

    private void OnDestroy()
    {
        var mm = MultiplayerManager.Instance;
        if (mm == null) return;
        mm.Unsubscribe(MultiplayerManager.Code.OpponentDisconnected, OnOpponentDisconnected);
        mm.Unsubscribe(MultiplayerManager.Code.OpponentReconnected, OnOpponentReconnected);
        mm.Unsubscribe(MultiplayerManager.Code.DisconnectWin, OnDisconnectWin);
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private void OnOpponentDisconnected(MultiplayerMessage message)
    {
        _winTriggered = false;
        var data = message.GetData<DisconnectData>();
        ShowPopup(data?.remainingSeconds ?? 20);

        if (TimerTurn.instance != null)
            TimerTurn.instance.TimerPause = true;
    }

    private void OnOpponentReconnected(MultiplayerMessage message)
    {
        _winTriggered = false;
        HidePopup();

        if (TimerTurn.instance != null)
            TimerTurn.instance.TimerPause = false;
    }

    private void OnDisconnectWin(MultiplayerMessage message)
    {
        TriggerWin();
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    public void ShowPopup(int seconds)
    {
        if (popupPanel == null) return;

        if (messageText != null)
            messageText.text = "اتصال حریف قطع شد!\nدر حال انتظار برای برگشت...";

        // شروع countdown کلاینت‌ساید
        if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
        _countdownCoroutine = StartCoroutine(CountdownCoroutine(seconds));

        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);

        popupPanel.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.25f);
        }
    }

    private IEnumerator CountdownCoroutine(int seconds)
    {
        while (seconds > 0)
        {
            if (countdownText != null)
                countdownText.text = PersianTextUtils.ToPersianDigits(seconds.ToString());
            yield return new WaitForSeconds(1f);
            seconds--;
        }
        if (countdownText != null)
            countdownText.text = PersianTextUtils.ToPersianDigits("0");

        // سرور ممکنه match رو قبل از رسیدن پیام ببنده — client خودش win رو trigger می‌کنه
        yield return new WaitForSeconds(0.5f);
        TriggerWin();
    }

    public void HidePopup(bool instant = false)
    {
        if (_countdownCoroutine != null)
        {
            StopCoroutine(_countdownCoroutine);
            _countdownCoroutine = null;
        }

        if (popupPanel == null) return;

        if (instant)
        {
            popupPanel.SetActive(false);
            return;
        }

        if (canvasGroup != null)
            canvasGroup.DOFade(0f, 0.2f).OnComplete(() => popupPanel.SetActive(false));
        else
            popupPanel.SetActive(false);
    }

    private void TriggerWin()
    {
        if (_winTriggered) return;
        _winTriggered = true;

        HidePopup(instant: true);
        ShowDisconnectWinResult();

        // کوین‌ها رو از سرور بگیر — سرور قبلاً awardMatchResult رو صدا زده
        StartCoroutine(RefreshWalletDelayed());
    }

    private IEnumerator RefreshWalletDelayed()
    {
        yield return new WaitForSeconds(1f);
        if (WalletManager.Instance != null)
        {
            var task = WalletManager.Instance.RefreshAsync();
            yield return new WaitUntil(() => task.IsCompleted);
        }
    }

    private void ShowDisconnectWinResult()
    {
        if (ActionEndGame.instance == null)
        {
            Debug.LogWarning("[ReconnectPopup] ActionEndGame.instance is null!");
            return;
        }

        ActionEndGame.instance.ResultPanel.SetActive(true);
        ActionEndGame.instance.ResultText.text = "شما بردی";

        var league = ClientLeagues.Get(GameManager.Instance.modeGame);
        if (UiManager.instance != null)
            UiManager.instance.TasiWin.text =
                "‏+" + PersianTextUtils.FormatNumber(league.winnerReward) + " تاسی";

        if (TimerTurn.instance != null)
        {
            TimerTurn.instance.TimerPause   = true;
            TimerTurn.instance.TimerRunning = false;
        }

        // آیکون‌های end game
        if (ActionEndGame.instance.IconMe != null)
        {
            ActionEndGame.instance.IconMe.transform.parent = FindObjectOfType<Canvas>().transform;
            ActionEndGame.instance.IconMe.enabled = true;
            ActionEndGame.instance.IconMe.Play("EndGamePlayer1Icon");
        }
        if (ActionEndGame.instance.IconOpp != null)
        {
            ActionEndGame.instance.IconOpp.transform.parent = FindObjectOfType<Canvas>().transform;
            ActionEndGame.instance.IconOpp.enabled = true;
            ActionEndGame.instance.IconOpp.Play("EndGamePlater2Icon");
        }
    }

    // ── Data ──────────────────────────────────────────────────────────────────

    [System.Serializable]
    private class DisconnectData
    {
        public int remainingSeconds;
    }
}
