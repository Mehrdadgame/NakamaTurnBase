using NinjaBattle.Game;
using System;
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

    /// <summary>
    /// The function FindMatch() is called when the player clicks the button to find a match
    /// </summary>
        private void FindMatch()
        {
            GameManager.Instance.modeGame = GetComponent<SetModeGame>().modeGame;
        
            button.interactable = false;
            MultiplayerManager.Instance.JoinMatchAsync(GameManager.Instance.modeGame);
        
         

        }

        #endregion
    }
}
