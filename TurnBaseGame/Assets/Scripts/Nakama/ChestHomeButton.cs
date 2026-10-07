using System;
using System.Collections;
using DG.Tweening;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    /// <summary>
    /// Chest button on Home screen.
    /// Handles timer display and ready notification.
    /// When ready to claim, performs a subtle piggy-bank shake every few seconds.
    /// </summary>
    public class ChestHomeButton : MonoBehaviour
    {
        [SerializeField] private RTLTextMeshPro timerText;
        [SerializeField] private GameObject readyBadge;

        private int _remainingSeconds;
        private Coroutine _countdownCoroutine;
        private Coroutine _shakeCoroutine;
        private Vector3 _originalScale;
        private Quaternion _originalRotation;
        private Button _button;

        private const string GetChestStatusRpcId = "GetChestStatusRpc";

        // Unity Lifecycle -------------------------------------------------------------

        private void Awake()
        {
            _originalScale = transform.localScale;
            _originalRotation = transform.localRotation;
            _button = GetComponent<Button>();
            if (_button != null)
                _button.onClick.AddListener(OnButtonClicked);

            ChestManager.OnChestClaimed += OnChestClaimed;
        }

        private void Start()
        {
            StartCoroutine(InitAfterLogin());
        }

        private void OnDisable()
        {
            StopShake();
        }

        private void OnDestroy()
        {
            ChestManager.OnChestClaimed -= OnChestClaimed;
            StopShake();
            transform.DOKill();
        }

        private void OnButtonClicked()
        {
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * -0.06f, 0.18f, 5, 0.5f).SetUpdate(true);
        }

        private void OnChestClaimed(int remainingSeconds) => ApplyTimer(remainingSeconds);

        // Init ------------------------------------------------------------------------

        private IEnumerator InitAfterLogin()
        {
            while (NakamaUserManager.Instance == null || !NakamaUserManager.Instance.LoadingFinished)
                yield return null;

            var task = FetchStatus();
            yield return new WaitUntil(() => task.IsCompleted);
        }

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
                Debug.LogWarning("[ChestHomeButton] FetchStatus error: " + e.Message);
            }
        }

        // Timer -----------------------------------------------------------------------

        private void ApplyTimer(int seconds)
        {
            _remainingSeconds = seconds;

            if (_countdownCoroutine != null) StopCoroutine(_countdownCoroutine);

            if (seconds <= 0)
                ShowReady();
            else
                _countdownCoroutine = StartCoroutine(CountdownCoroutine());
        }

        private IEnumerator CountdownCoroutine()
        {
            SetReady(false);
            while (_remainingSeconds > 0)
            {
                UpdateText();
                yield return new WaitForSeconds(1f);
                _remainingSeconds--;
            }
            ShowReady();
        }

        private void UpdateText()
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
            if (timerText != null) timerText.text = Localization.L("آماده!", "Ready!");
            SetReady(true);
        }

        private void SetReady(bool on)
        {
            if (readyBadge != null) readyBadge.SetActive(on);

            if (on)
                StartShake();
            else
                StopShake();
        }

        // Piggy-bank shake loop (تکون خوردن مثل قلک هر چند ثانیه) ---------------------------

        private void StartShake()
        {
            if (_shakeCoroutine != null) return;
            _shakeCoroutine = StartCoroutine(PiggyBankShakeLoop());
        }

        private void StopShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }
            transform.localRotation = _originalRotation;
            transform.localScale = _originalScale;
        }

        private IEnumerator PiggyBankShakeLoop()
        {
            // Initial small delay before starting the loop
            yield return new WaitForSeconds(1.0f);

            while (true)
            {
                float duration = 0.45f;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    // Decaying sine wave: tilt left and right
                    float angle = Mathf.Sin(t * Mathf.PI * 6f) * (1f - t) * 7.5f;
                    transform.localRotation = _originalRotation * Quaternion.Euler(0f, 0f, angle);

                    // Subtle squash/stretch bounce
                    float bounce = Mathf.Sin(t * Mathf.PI * 4f) * (1f - t) * 0.055f;
                    transform.localScale = new Vector3(_originalScale.x * (1f - bounce * 0.5f),
                                                      _originalScale.y * (1f + bounce),
                                                      _originalScale.z);
                    yield return null;
                }

                transform.localRotation = _originalRotation;
                transform.localScale = _originalScale;

                // Wait ~2.8 seconds between shakes
                yield return new WaitForSeconds(2.8f);
            }
        }

        // Data ------------------------------------------------------------------------

        [Serializable]
        private class ChestStatus
        {
            public int remainingSeconds;
            public bool ready;
        }
    }
}