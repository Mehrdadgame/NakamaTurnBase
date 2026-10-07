using System;
using System.Collections;
using DG.Tweening;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
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

        private const string ClaimChestRpcId    = "ClaimChestRpc";
        private const string GetChestStatusRpcId = "GetChestStatusRpc";

        /// <summary>Panel opened event</summary>
        public static event Action OnPanelOpened;
        /// <summary>Panel closed event</summary>
        public static event Action OnPanelClosed;
        /// <summary>Chest claimed event</summary>
        public static event Action<int> OnChestClaimed;

        private int _remainingSeconds;
        private bool _ready;
        private int _pendingReward;
        private Coroutine _countdownCoroutine;
        private Coroutine _shakeCoroutine;
        private Vector3 _originalChestScale = Vector3.one;
        private Quaternion _originalChestRotation = Quaternion.identity;

        // Unity -----------------------------------------------------------------------

        private void Awake()
        {
            Instance = this;
            if (chestButton != null)
            {
                _originalChestScale = chestButton.transform.localScale;
                _originalChestRotation = chestButton.transform.localRotation;
            }
        }

        private void OnEnable()
        {
            OnPanelOpened?.Invoke();
            if (_ready) StartShake();
        }

        private void OnDisable()
        {
            OnPanelClosed?.Invoke();
            StopShake();
        }

        private void OnDestroy()
        {
            StopShake();
            if (chestButton != null) chestButton.transform.DOKill();
        }

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

        // Server calls ----------------------------------------------------------------

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
            StopShake();
            SetButtonReady(false);

            if (chestButton != null)
            {
                chestButton.transform.DOKill();
                chestButton.transform.DOPunchScale(Vector3.one * -0.08f, 0.2f, 6, 0.5f).SetUpdate(true);
            }

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
                    // Race condition -> restart timer
                    ApplyTimer(result.remainingSeconds);
                    return;
                }

                _pendingReward = result.coinsAwarded;

                // Show popup
                if (rewardText != null)
                    rewardText.text = Localization.IsPersian
                        ? "+" + PersianTextUtils.FormatNumber(result.coinsAwarded) + " تاسی برنده شدید!"
                        : "You won +" + PersianTextUtils.FormatNumber(result.coinsAwarded) + " Tasi!";

                if (rewardPopup != null)
                {
                    rewardPopup.SetActive(true);
                    rewardPopup.transform.DOKill();
                    rewardPopup.transform.localScale = Vector3.one * 0.85f;
                    rewardPopup.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
                }

                // Start next cooldown + notify home button
                ApplyTimer(result.remainingSeconds);
                OnChestClaimed?.Invoke(result.remainingSeconds);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Chest] Claim error: " + e.Message);
                SetButtonReady(_ready);
            }
        }

        private async void OnClaimClicked()
        {
            NinjaBattle.General.GameSfx.PlayCoin();
            if (rewardPopup != null) rewardPopup.SetActive(false);

            // Add coins immediately to local display
            if (WalletManager.Instance != null)
                WalletManager.Instance.SetCoins(WalletManager.Instance.Coins + _pendingReward);

            _pendingReward = 0;

            // Sync from server to confirm authoritative balance
            if (WalletManager.Instance != null)
                await WalletManager.Instance.RefreshAsync();
        }

        // Timer -----------------------------------------------------------------------

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
            if (timerText != null) timerText.text = Localization.L("آماده!", "Ready!");
            SetButtonReady(true);
        }

        private void SetButtonReady(bool on)
        {
            if (chestButton != null) chestButton.interactable = on;
            if (chestReadyFX != null) chestReadyFX.SetActive(on);

            if (on && gameObject.activeInHierarchy)
                StartShake();
            else
                StopShake();
        }

        // Piggy-bank shake loop (تکون خوردن مثل قلک هر چند ثانیه) ---------------------------

        private void StartShake()
        {
            if (_shakeCoroutine != null || chestButton == null) return;
            _shakeCoroutine = StartCoroutine(PiggyBankShakeLoop());
        }

        private void StopShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }
            if (chestButton != null)
            {
                chestButton.transform.localRotation = _originalChestRotation;
                chestButton.transform.localScale = _originalChestScale;
            }
        }

        private IEnumerator PiggyBankShakeLoop()
        {
            yield return new WaitForSeconds(0.8f);

            while (_ready && chestButton != null)
            {
                float duration = 0.45f;
                float elapsed = 0f;
                Transform targetTransform = chestButton.transform;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    // Decaying tilt wobble
                    float angle = Mathf.Sin(t * Mathf.PI * 6f) * (1f - t) * 7.5f;
                    targetTransform.localRotation = _originalChestRotation * Quaternion.Euler(0f, 0f, angle);

                    // Subtle squash/stretch bounce
                    float bounce = Mathf.Sin(t * Mathf.PI * 4f) * (1f - t) * 0.055f;
                    targetTransform.localScale = new Vector3(_originalChestScale.x * (1f - bounce * 0.5f),
                                                           _originalChestScale.y * (1f + bounce),
                                                           _originalChestScale.z);
                    yield return null;
                }

                targetTransform.localRotation = _originalChestRotation;
                targetTransform.localScale = _originalChestScale;

                yield return new WaitForSeconds(2.8f);
            }
        }

        // Data models -----------------------------------------------------------------

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