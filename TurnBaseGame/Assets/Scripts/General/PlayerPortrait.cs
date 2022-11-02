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

     /// <summary>
     /// If the player number is the same as the player number of the player who is currently logged in,
     /// then the color of the text will be the color that is set in the inspector. Otherwise, the color
     /// will be the color that is set in the inspector
     /// </summary>
     /// <param name="PlayerData">This is the player data that is obtained from the server.</param>
     /// <param name="playerNumber">The player number of the player who joined the game.</param>
        private void LocalPlayerObtained(PlayerData player, int playerNumber)
        {
            displayName.color = this.playerNumber == playerNumber ? youColor : othersColor;
        }

     /// <summary>
     /// It sets the portrait of the player.
     /// </summary>
     /// <param name="players">List of PlayerData objects</param>
        private void PlayersReceived(List<PlayerData> players)
        {
            SetPortrait(players);
        }

      /// <summary>
      /// This function is called when a player leaves the game. It updates the portrait of the player
      /// who left
      /// </summary>
      /// <param name="PlayerData">This is a class that contains all the information about the
      /// player.</param>
        private void PlayerLeft(PlayerData player)
        {
            SetPortrait(playersManager.Players);
        }

     /// <summary>
     /// When a player joins, set the portrait of all players
     /// </summary>
     /// <param name="PlayerData"></param>
        private void PlayerJoined(PlayerData player)
        {
            SetPortrait(playersManager.Players);
        }
      
       /// <summary>
       /// If the player is connected, set the portrait to the connectedPlayerColor, otherwise set it to
       /// the noPlayerColor
       /// </summary>
       /// <param name="players">List of PlayerData</param>
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
