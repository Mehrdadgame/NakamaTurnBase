using System;
using System.Collections;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    /// <summary>
    /// روی دکمه‌ی صفحه‌ی هوم قرار می‌گیرد.
    /// تایمر باقی‌مانده را روی خود دکمه نشان می‌دهد.
    /// وقتی صفر شد badge "آماده!" فعال می‌شود.
    ///
    /// Inspector wiring:
    ///   timerText   — RTLTextMeshPro روی/زیر دکمه (مثلاً "02:45:10" یا "آماده!")
    ///   readyBadge  — یک Image/GameObject که وقتی چست آماده‌ست فعال می‌شه (مثلاً نقطه‌ی قرمز یا glow)
    /// </summary>
    public class ChestHomeButton : MonoBehaviour
    {
        [SerializeField] private RTLTextMeshPro timerText;
        [SerializeField] private GameObject readyBadge;

        private int _remainingSeconds;
        private Coroutine _countdownCoroutine;

        private const string GetChestStatusRpcId = "GetChestStatusRpc";

        // ── Unity ─────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            // هر بار که صفحه هوم فعال می‌شه وضعیت رو از سرور می‌گیریم
            StartCoroutine(InitAfterLogin());
        }

        private void OnDisable()
        {
            if (_countdownCoroutine != null)
            {
                StopCoroutine(_countdownCoroutine);
                _countdownCoroutine = null;
            }
        }

        // ── Init ──────────────────────────────────────────────────────────────

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

        // ── Timer ─────────────────────────────────────────────────────────────

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
            if (timerText != null) timerText.text = "آماده!";
            SetReady(true);
        }

        private void SetReady(bool on)
        {
            if (readyBadge != null) readyBadge.SetActive(on);
        }

        // ── Data ──────────────────────────────────────────────────────────────

        [Serializable]
        private class ChestStatus
        {
            public int remainingSeconds;
            public bool ready;
        }
    }
}
