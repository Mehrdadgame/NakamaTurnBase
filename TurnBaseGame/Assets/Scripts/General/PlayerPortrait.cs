using System;
using System.Collections.Generic;
using NinjaBattle.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.General
{
    public class PlayerPortrait : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private int playerNumber = 0;
        [SerializeField] private Image portrait = null;
        [SerializeField] private Color noPlayerColor = Color.white;
        [SerializeField] private Color connectedPlayerColor = Color.white;
        [SerializeField] private Color youColor = Color.white;
        [SerializeField] private Color othersColor = Color.white;
        [SerializeField] private TMP_Text displayName = null;

        private PlayersManager playersManager = null;

        #endregion

        #region PROPERTIES

        public int PlayerNumber { get => playerNumber; set => playerNumber = value; }

        #endregion

        #region BEHAVIORS

        private void Awake()
        {
            playersManager = PlayersManager.Instance;
        }
        /// <summary>
        /// call event for match making
        /// </summary>
        private void Start()
        {
            playersManager.onPlayerJoined += PlayerJoined;
            playersManager.onPlayerLeft += PlayerLeft;
            playersManager.onPlayersReceived += PlayersReceived;
            playersManager.onLocalPlayerObtained += LocalPlayerObtained;
            SetPortrait(playersManager.Players);
        }

        private void OnDestroy()
        {
            playersManager.onPlayerJoined -= PlayerJoined;
            playersManager.onPlayerLeft -= PlayerLeft;
            playersManager.onPlayersReceived -= PlayersReceived;
            playersManager.onLocalPlayerObtained -= LocalPlayerObtained;
        }

        private void LocalPlayerObtained(PlayerData player, int playerNumber)
        {
            displayName.color = this.playerNumber == playerNumber ? youColor : othersColor;
        }

        private void PlayersReceived(List<PlayerData> players)
        {
            SetPortrait(players);
        }

        private void PlayerLeft(PlayerData player)
        {
            SetPortrait(playersManager.Players);
        }

        private void PlayerJoined(PlayerData player)
        {
            SetPortrait(playersManager.Players);
        }
        /// <summary>
        /// set data other player in board
        /// </summary>
        /// <param name="players"></param>
        private void SetPortrait(List<PlayerData> players)
        {
            bool hasPlayer = players != null && players.Count > playerNumber && players[playerNumber] != null;
            portrait.color = hasPlayer ? connectedPlayerColor : noPlayerColor;
            displayName.text = hasPlayer ? players[playerNumber].DisplayName : string.Empty;
            displayName.color = playersManager.CurrentPlayerNumber == playerNumber ? youColor : othersColor;
          
        }

        #endregion
    }
}
