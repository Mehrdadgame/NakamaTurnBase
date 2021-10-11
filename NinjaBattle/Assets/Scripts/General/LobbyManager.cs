using System.Collections.Generic;
using UnityEngine;

using Nakama;

using NinjaBattle.Game;

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

        private void Start()
        {
            playersManager = PlayersManager.Instance;
            playersManager.onPlayerJoined += PlayerJoined;
            playersManager.onPlayerLeft += PlayerLeft;
            playersManager.onPlayersReceived += PlayersReceived;
            UpdateStatus();
        }

        private void OnDestroy()
        {
            playersManager.onPlayerJoined -= PlayerJoined;
            playersManager.onPlayerLeft -= PlayerLeft;
            playersManager.onPlayersReceived -= PlayersReceived;
        }

        private void PlayersReceived(List<IUserPresence> players)
        {
            UpdateStatus();
        }

        private void PlayerLeft(IUserPresence player)
        {
            UpdateStatus();
        }

        private void PlayerJoined(IUserPresence player)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            bool gameStarting = playersManager.PlayersCount > 1;
            Debug.Log(playersManager.PlayersCount);
            waitingText.SetActive(!gameStarting);
            timer.gameObject.SetActive(gameStarting);
            if (gameStarting)
                timer.ResetTimer();
        }

        #endregion
    }
}