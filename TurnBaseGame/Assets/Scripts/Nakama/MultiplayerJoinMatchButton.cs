using NinjaBattle.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    public class MultiplayerJoinMatchButton : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private Button button = null;

        #endregion

        #region BEHAVIORS

        private void Awake()
        {
            button.onClick.AddListener(FindMatch);
        }

        private void FindMatch()
        {
            button.interactable = false;
            MultiplayerManager.Instance.JoinMatchAsync();
            GameManager.Instance.modeGame = GetComponent<SetModeGame>().modeGame;
        }

        #endregion
    }
}
