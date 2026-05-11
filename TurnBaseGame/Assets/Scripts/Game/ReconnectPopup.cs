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
        var data = message.GetData<DisconnectData>();
        ShowPopup(data?.remainingSeconds ?? 20);

        // Pause the turn timer while waiting
        if (TimerTurn.instance != null)
            TimerTurn.instance.TimerPause = true;
    }

    private void OnOpponentReconnected(MultiplayerMessage message)
    {
        HidePopup();

        // Resume turn timer
        if (TimerTurn.instance != null)
            TimerTurn.instance.TimerPause = false;
    }

    private void OnDisconnectWin(MultiplayerMessage message)
    {
        HidePopup(instant: true);
        // Show win result — reuse existing end-game result panel
        ShowDisconnectWinResult();
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    public void ShowPopup(int seconds)
    {
        if (popupPanel == null) return;

        if (messageText != null)
            messageText.text = "اتصال حریف قطع شد!\nدر حال انتظار برای برگشت...";

        UpdateCountdown(seconds);

        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);

        popupPanel.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.25f);
        }
    }

    /// <summary>Called each time the server broadcasts a new remainingSeconds.</summary>
    public void UpdateCountdown(int seconds)
    {
        if (countdownText != null)
            countdownText.text = seconds.ToString();
    }

    public void HidePopup(bool instant = false)
    {
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

    private void ShowDisconnectWinResult()
    {
        // Delegate to ActionEndGame — same result panel used for normal end-game
        if (ActionEndGame.instance == null) return;
        ActionEndGame.instance.ResultPanel.SetActive(true);
        ActionEndGame.instance.ResultText.text = "شما بردی";

        var league = ClientLeagues.Get(GameManager.Instance.modeGame);
        if (UiManager.instance != null)
            UiManager.instance.TasiWin.text =
                "‏+" + PersianTextUtils.FormatNumber(league.winnerReward) + " تاسی";

        if (TimerTurn.instance != null)
        {
            TimerTurn.instance.TimerPause = true;
            TimerTurn.instance.TimerRunning = false;
        }
    }

    // ── Data ──────────────────────────────────────────────────────────────────

    [System.Serializable]
    private class DisconnectData
    {
        public int remainingSeconds;
    }
}
