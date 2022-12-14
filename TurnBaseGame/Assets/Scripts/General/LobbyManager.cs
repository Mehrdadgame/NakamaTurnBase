using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Nakama;
using Nakama.Helpers;
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
            UpdateStatus();

        }

        private void OnDestroy()
        {
            playersManager.onPlayerJoined -= PlayerJoined;
            playersManager.onPlayerLeft -= PlayerLeft;
            playersManager.onPlayersReceived -= PlayersReceived;
        }

        private void PlayersReceived(List<PlayerData> players)
        {
            if (!PlayerPrefs.HasKey("Opp"))

                UpdateStatus();
        }

        private void PlayerLeft(PlayerData player)
        {
            UpdateStatus();
        }

        private void PlayerJoined(PlayerData player)
        {


            UpdateStatus();

        }

        private  void UpdateStatus()
        {
            Debug.Log("Left");
            //bool gameStarting = playersManager.PlayersCount > 1;


            //if (gameStarting)
            //{
            //    // timer.ResetTimer();
            //    // waitingText.SetActive(!gameStarting);
            //    // timer.gameObject.SetActive(gameStarting);



            // gameStarting = false;
            //}

        }

        #endregion
    }
}
