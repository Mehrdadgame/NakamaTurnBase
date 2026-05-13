using System;
using System.Collections;
using DG.Tweening;
using Nakama.Helpers;
using NinjaBattle.Game;
using RTLTMPro;
using UnityEngine;

/// <summary>
/// مدیریت قطعی حریف توی بازی:
///   - OpponentDisconnected (11): پاپ‌آپ "حریف قطع شد" با countdown
///   - OpponentReconnected  (12): مخفی کردن پاپ‌آپ، ادامه بازی
///   - DisconnectWin       (13): "شما بردی"
///
/// Inspector wiring:
///   popupPanel    — پنل پاپ‌آپ قطعی حریف
///   canvasGroup   — CanvasGroup برای fade
///   countdownText — عدد تایمر
///   messageText   — متن توضیح
/// </summary>
public class ReconnectPopup : MonoBehaviour
{
    public static ReconnectPopup Instance { get; private set; }

    [Header("Opponent Disconnect Popup")]
    [SerializeField] private GameObject     popupPanel;
    [SerializeField] private CanvasGroup    canvasGroup;
    [SerializeField] private RTLTextMeshPro countdownText;
    [SerializeField] private RTLTextMeshPro messageText;

    [Serializable]
    private class DisconnectData
    {
        public string userId;
        public int    remainingSeconds;
    }

    private bool      _winTriggered;
    private Coroutine _countdownCoroutine;
    private int       _remainingSeconds;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake() => Instance = this;

    private void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);

        var mm = MultiplayerManager.Instance;
        if (mm == null) return;
        mm.Subscribe(MultiplayerManager.Code.OpponentDisconnected, OnOpponentDisconnected);
        mm.Subscribe(MultiplayerManager.Code.OpponentReconnected,  OnOpponentReconnected);
        mm.Subscribe(MultiplayerManager.Code.DisconnectWin,        OnDisconnectWin);
    }

    private void OnDestroy()
    {
        var mm = MultiplayerManager.Instance;
        if (mm == null) return;
        mm.Unsubscribe(MultiplayerManager.Code.OpponentDisconnected, OnOpponentDisconnected);
        mm.Unsubscribe(MultiplayerManager.Code.OpponentReconnected,  OnOpponentReconnected);
        mm.Unsubscribe(MultiplayerManager.Code.DisconnectWin,        OnDisconnectWin);
    }

    // ── Handlers ──────────────────────────────────────────────────────────────

    private void OnOpponentDisconnected(MultiplayerMessage message)
    {
        var data = message.GetData<DisconnectData>();
        _remainingSeconds = data != null && data.remainingSeconds > 0 ? data.remainingSeconds : 20;

        ShowPopup();
        SetCountdown(_remainingSeconds);  // فوری عدد رو نشون بده
        if (TimerTurn.instance != null) TimerTurn.instance.TimerPause = true;

        if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
        _countdownCoroutine = StartCoroutine(CountdownCoroutine());
    }

    private void OnOpponentReconnected(MultiplayerMessage message)
    {
        if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
        _countdownCoroutine = null;
        HidePopup();
        if (TimerTurn.instance != null) TimerTurn.instance.TimerPause = false;
    }

    private void OnDisconnectWin(MultiplayerMessage message)
    {
        if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);
        _countdownCoroutine = null;
        HidePopup();
        TriggerWin();
    }

    // ── Countdown ─────────────────────────────────────────────────────────────

    private IEnumerator CountdownCoroutine()
    {
        while (_remainingSeconds > 0)
        {
            SetCountdown(_remainingSeconds);
            yield return new WaitForSeconds(1f);
            _remainingSeconds--;
        }
        SetCountdown(0);

        // Fallback: اگه سرور تا ۳ ثانیه DisconnectWin نفرستاد، خودمون trigger کن
        yield return new WaitForSeconds(3f);
        if (!_winTriggered)
        {
            Debug.LogWarning("[ReconnectPopup] Server timeout — triggering win locally");
            HidePopup();
            TriggerWin();
        }
    }

    // ── Win flow ──────────────────────────────────────────────────────────────

    public void TriggerWin()
    {
        if (_winTriggered) return;
        _winTriggered = true;

        ShowWinResult();
        StartCoroutine(RefreshWalletDelayed());
    }

    private IEnumerator RefreshWalletDelayed()
    {
        int coinsBefore = WalletManager.Instance != null ? WalletManager.Instance.Coins : -1;
        Debug.Log("[ReconnectPopup] coins before refresh = " + coinsBefore);

        // چند بار refresh کن — شاید سرور دیر coin رو commit کرده
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(1f);
            if (WalletManager.Instance != null)
            {
                var task = WalletManager.Instance.RefreshAsync();
                yield return new WaitUntil(() => task.IsCompleted);
                int coinsAfter = WalletManager.Instance.Coins;
                Debug.Log("[ReconnectPopup] refresh #" + i + " coins=" + coinsAfter);
                if (coinsAfter > coinsBefore) yield break;  // coin اضافه شد، تمام
            }
        }
    }

    private void ShowWinResult()
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

    // ── UI ────────────────────────────────────────────────────────────────────

    private void ShowPopup()
    {
        if (popupPanel == null) return;
        if (messageText != null)
            messageText.text = "حریف قطع شد!\nمنتظر بازگشت...";

        popupPanel.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.25f);
        }
    }

    private void HidePopup()
    {
        if (popupPanel == null) return;
        if (canvasGroup != null)
            canvasGroup.DOFade(0f, 0.2f).OnComplete(() => popupPanel.SetActive(false));
        else
            popupPanel.SetActive(false);
    }

    private void SetCountdown(int seconds)
    {
        if (countdownText != null)
            countdownText.text = PersianTextUtils.ToPersianDigits(seconds.ToString());
    }
}
