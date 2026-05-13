using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nakama.Helpers
{
    public class NakamaDisconnectedChangeScene : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private string sceneName = null;

        #endregion

        #region BEHAVIORS

        private void OnEnable()
        {
            if (NakamaManager.Instance != null)
                NakamaManager.Instance.onDisconnected += Disconnected;
        }

        private void OnDisable()
        {
            if (NakamaManager.Instance != null)
                NakamaManager.Instance.onDisconnected -= Disconnected;
        }

        private void Disconnected()
        {
            // در صحنه بازی توسط SelfReconnectHandler خاموش میشه — اضافه‌ی محکم‌کار
            if (!enabled) return;
            SceneManager.LoadScene(sceneName);
        }

        #endregion
    }
}
