using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    /// <summary>
    /// این کامپوننت را روی هر دکمه‌ای در صحنه بگذار.
    /// وقتی کلیک می‌شود، پنل لاگین با ایمیل را باز می‌کند.
    ///
    /// Inspector wiring:
    ///   button            — دکمه (اگر خالی بماند از GetComponent استفاده می‌شود)
    ///   emailLoginPanel   — اختیاری: اگر Instance در صحنه نباشد اینجا assign کن
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class OpenEmailLoginButton : MonoBehaviour
    {
        [SerializeField] private EmailLoginPanel emailLoginPanel;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            EmailLoginPanel target = emailLoginPanel != null
                ? emailLoginPanel
                : EmailLoginPanel.Instance;

            if (target == null)
            {
                target = FindFirstObjectByType<EmailLoginPanel>(FindObjectsInactive.Include);
                emailLoginPanel = target;
            }

            if (target != null)
                target.Show();
            else
                Debug.LogWarning("[OpenEmailLoginButton] EmailLoginPanel not found in scene.");
        }
    }
}
