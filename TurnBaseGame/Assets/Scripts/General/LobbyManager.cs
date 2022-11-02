using System.Collections.Generic;
using NinjaBattle.Game;
using UnityEngine;

namespace NinjaBattle.General
{
    public class LobbyManager : MonoBehaviour
    {
        #region FIELDS

        private PlayersManager playersManager = null;

        [SerializeField] private GameObject waitingText = null;
        [SerializeField] private Timer timer = null;

        #endregion

        #region BEHAVIORS



        private void OnEnable()
        {
            playersManager = PlayersManager.Instance;
            playersManager.onPlayerJoined += PlayerJoined;
            playersManager.onPlayerLeft += PlayerLeft;
            playersManager.onPlayersReceived += PlayersReceived;
       

        }

        private void OnDestroy()
        {
            playersManager.onPlayerJoined -= PlayerJoined;
            playersManager.onPlayerLeft -= PlayerLeft;
            playersManager.onPlayersReceived -= PlayersReceived;
        }

        private void PlayersReceived(List<PlayerData> players)
        {
          
        }

        private void PlayerLeft(PlayerData player)
        {
          
        }

        private void PlayerJoined(PlayerData player)
        {

        }

      

        #endregion
    }
}
