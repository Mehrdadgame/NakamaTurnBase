using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    public class NakamaDisconnectButton : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private Button button = null;

        #endregion

        #region BEHAVIORS

        private void Awake()
        {
            button.onClick.AddListener(Disconect);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(Disconect);
        }

     /// <summary>
     /// It disconnects the user from the server.
     /// </summary>
        private void Disconect()
        {
            NakamaManager.Instance.LogOut();
        }

        #endregion
    }
}
