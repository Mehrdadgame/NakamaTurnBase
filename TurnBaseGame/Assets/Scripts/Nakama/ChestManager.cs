using System;
using System.Collections;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    /// <summary>
    /// Free chest that recharges every 3 hours.
    ///
    /// Inspector wiring:
    ///   chestButton     — دکمه‌ی باز کردن صندوق
    ///   chestReadyFX    — افکت/انیمیشن وقتی آماده‌ست (مثلاً glow)
    ///   timerText       — متن تایمر معکوس "02:45:10"
    ///   rewardPopup     — پنل پاپ‌آپ جایزه
    ///   rewardText      — متن "+500 تاسی بردی!"
    ///   claimButton     — دکمه "دریافت جایزه"
    /// </summary>
    public class ChestManager : MonoBehaviour
    {
        public static ChestManager Instance { get; private set; }

        [Header("Chest UI")]
        [SerializeField] private Button chestButton;
        [SerializeField] private GameObject chestReadyFX;
        [SerializeField] private RTLTextMeshPro timerText;

        [Header("Reward Popup")]
        [SerializeField] private GameObject rewardPopup;
        [SerializeField] private RTLTextMeshPro rewardText;
        [SerializeField] private Button claimButton;

        private const string ClaimChestRpcId = "ClaimChestRpc";
        private const string GetChestStatusRpcId = "GetChestStatusRpc";

        private int _remainingSeconds;
        private bool _ready;
        private int _pendingReward;
        private Coroutine _countdownCoroutine;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake() => Instance = this;

        private void Start()
        {
            if (rewardPopup != null) rewardPopup.SetActive(false);

            if (chestButton != null) chestButton.onClick.AddListener(OnChestClicked);
            if (claimButton != null) claimButton.onClick.AddListener(OnClaimClicked);

            SetButtonReady(false);
            StartCoroutine(InitAfterLogin());
        }

        private IEnumerator InitAfterLogin()
        {
            while (NakamaUserManager.Instance == null || !NakamaUserManager.Instance.LoadingFinished)
                yield return null;

            var task = FetchStatus();
            yield return new WaitUntil(() => task.IsCompleted);
        }

        // ── Server calls ──────────────────────────────────────────────────────

        private async System.Threading.Tasks.Task FetchStatus()
        {
            try
            {
                var rpc = await NakamaManager.Instance.SendRPC(GetChestStatusRpcId, "{}");
                if (rpc == null || string.IsNullOrEmpty(rpc.Payload)) return;
                var status = rpc.Payload.Deserialize<ChestStatus>();
                ApplyTimer(status.remainingSeconds);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Chest] FetchStatus error: " + e.Message);
            }
        }

        private async void OnChestClicked()
        {
            if (!_ready) return;
            SetButtonReady(false);

            try
            {
                var rpc = await NakamaManager.Instance.SendRPC(ClaimChestRpcId, "{}");
                if (rpc == null || string.IsNullOrEmpty(rpc.Payload))
                {
                    SetButtonReady(true);
                    return;
                }

                var result = rpc.Payload.Deserialize<ChestClaimResult>();

                if (!result.success)
                {
                    // Race condition — restart timer
                    ApplyTimer(result.remainingSeconds);
                    return;
                }

                _pendingReward = result.coinsAwarded;

                // Show popup
                if (rewardText != null)
                    rewardText.text = "+" + PersianTextUtils.FormatNumber(result.coinsAwarded) + " تاسی بردی!";

                if (rewardPopup != null) rewardPopup.SetActive(true);

                // Start next cooldown
                ApplyTimer(result.remainingSeconds);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Chest] Claim error: " + e.Message);
                SetButtonReady(_ready);
            }
        }

        private async void OnClaimClicked()
        {
            if (rewardPopup != null) rewardPopup.SetActive(false);

            // Add coins immediately to local display
            if (WalletManager.Instance != null)
                WalletManager.Instance.SetCoins(WalletManager.Instance.Coins + _pendingReward);

            _pendingReward = 0;

            // Sync from server to confirm authoritative balance
            if (WalletManager.Instance != null)
                await WalletManager.Instance.RefreshAsync();
        }

        // ── Timer ─────────────────────────────────────────────────────────────

        private void ApplyTimer(int seconds)
        {
            _remainingSeconds = seconds;
            _ready = seconds <= 0;

            if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);

            if (_ready)
                ShowReady();
            else
                _countdownCoroutine = StartCoroutine(CountdownCoroutine());
        }

        private IEnumerator CountdownCoroutine()
        {
            SetButtonReady(false);
            while (_remainingSeconds > 0)
            {
                UpdateTimerText();
                yield return new WaitForSeconds(1f);
                _remainingSeconds--;
            }
            ShowReady();
        }

        private void UpdateTimerText()
        {
            int h = _remainingSeconds / 3600;
            int m = (_remainingSeconds % 3600) / 60;
            int s = _remainingSeconds % 60;
            if (timerText != null)
                timerText.text = PersianTextUtils.ToPersianDigits(
                    string.Format("{2:D2}:{1:D2}:{0:D2}", h, m, s));
        }

        private void ShowReady()
        {
            _ready = true;
            if (timerText != null) timerText.text = "آماده!";
            SetButtonReady(true);
        }

        private void SetButtonReady(bool on)
        {
            if (chestButton != null) chestButton.interactable = on;
            if (chestReadyFX != null) chestReadyFX.SetActive(on);
        }

        // ── Data models ───────────────────────────────────────────────────────

        [Serializable]
        private class ChestStatus
        {
            public int remainingSeconds;
            public bool ready;
        }

        [Serializable]
        private class ChestClaimResult
        {
            public bool success;
            public int coinsAwarded;
            public int remainingSeconds;
            public string error;
        }
    }
}
