using System.Collections;
using Nakama.Helpers;
using NinjaBattle.Game;
using UnityEngine;

/// <summary>
/// وقتی حریف از بازی خارج شد → سرور DisconnectWin میفرسته → نتیجه "شما بردی" نشون داده میشه.
/// </summary>
public class ReconnectPopup : MonoBehaviour
{
    public static ReconnectPopup Instance { get; private set; }

    private bool _winTriggered;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake() => Instance = this;

    private void Start()
    {
        var mm = MultiplayerManager.Instance;
        mm.Subscribe(MultiplayerManager.Code.DisconnectWin, OnDisconnectWin);
    }

    private void OnDestroy()
    {
        var mm = MultiplayerManager.Instance;
        if (mm == null) return;
        mm.Unsubscribe(MultiplayerManager.Code.DisconnectWin, OnDisconnectWin);
    }

    // ── Handler ───────────────────────────────────────────────────────────────

    private void OnDisconnectWin(MultiplayerMessage message)
    {
        TriggerWin();
    }

    public void TriggerWin()
    {
        if (_winTriggered) return;
        _winTriggered = true;

        ShowWinResult();
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
}
