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
        public int AmountHXDPlayer;
        
        #endregion

        #region BEHAVIORS

        private void Awake()
        {
            
            button.onClick.AddListener(FindMatch);
        }

    /// <summary>
    /// The function FindMatch() is called when the player clicks the button to find a match
    /// </summary>
        public void FindMatch()
        {

            if (AmountHXDPlayer<= HXDManager.instance.HXDAmount)
            {
                button.interactable = false;
                MultiplayerManager.Instance.JoinMatchAsync(GameManager.Instance.modeGame);
            }
            else
            {
                Debug.Log("Low Coin");
            }
          
        }

        #endregion
    }
}
