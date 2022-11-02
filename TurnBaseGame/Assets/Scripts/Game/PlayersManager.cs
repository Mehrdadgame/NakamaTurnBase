using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nakama;
using Nakama.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.Game
{

    public enum ResultGame
    {
        win,
        loose,
        draw
    }
    /// <summary>
    /// 
    /// </summary>
    public class PlayersManager : MonoBehaviour
    {
        #region FIELDS

        private NakamaManager nakamaManager = null;
        private MultiplayerManager multiplayerManager = null;
        private bool blockJoinsAndLeaves = false;

        #endregion

        #region EVENTS action for joine and left player

        public event Action<List<PlayerData>> onPlayersReceived;
        public event Action<PlayerData> onPlayerJoined;
        public event Action<PlayerData> onPlayerLeft;
        public event Action<PlayerData, int> onLocalPlayerObtained;
        public event Action<bool> IsTurn;
        public event Action<DataPlayer> onSetDataInTurn;
        public event Action<int, int> onSetDataInRowMe;
        public event Action<int, int> onSetDataInRowOpp;
        public event Action<int, int, DataPlayer> onSetScoreMe;
        public event Action<int, int, DataPlayer> onSetScoreOpp;
        public event Action<RematchData> onRematch;
        public event Action<string> LeftPlayer;
        #endregion

        #region PROPERTIES

        public static PlayersManager Instance { get; private set; } = null;
        public List<PlayerData> Players { get; private set; } = new List<PlayerData>();
        public int PlayersCount { get => Players.Count(player => player != null); }
        public PlayerData CurrentPlayer { get; private set; } = null;
        public int CurrentPlayerNumber { get; private set; } = -1;

        public int ScoreMe;
        public int ScoreOpp;

        private ResultGame resultGame;
        #endregion

        #region BEHAVIORS

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            multiplayerManager = MultiplayerManager.Instance;
            nakamaManager = NakamaManager.Instance;
            multiplayerManager.onMatchJoin += MatchJoined;
            multiplayerManager.onMatchLeave += ResetLeaved;
            multiplayerManager.Subscribe(MultiplayerManager.Code.Players, SetPlayers);
            multiplayerManager.Subscribe(MultiplayerManager.Code.PlayerJoined, PlayerJoined);
            multiplayerManager.Subscribe(MultiplayerManager.Code.ChangeScene, MatchStarted);
            multiplayerManager.Subscribe(MultiplayerManager.Code.TurnMe, SetTurn);
            multiplayerManager.Subscribe(MultiplayerManager.Code.ChosseTurn, ChosseTurnPlayer);
            multiplayerManager.Subscribe(MultiplayerManager.Code.Rematch, RematchEvent);
            multiplayerManager.Subscribe(MultiplayerManager.Code.PlayerLeft, EventPlayerLeft);
            multiplayerManager.Subscribe(MultiplayerManager.Code.SendSticker, RiseveSticker);

        }

      /// <summary>
      /// A function that is called when the object is destroyed.
      /// </summary>
        private void OnDestroy()
        {
            multiplayerManager.onMatchJoin -= MatchJoined;
            multiplayerManager.onMatchLeave -= ResetLeaved;
            nakamaManager.Socket.ReceivedMatchPresence -= PlayersChanged;
            multiplayerManager.Unsubscribe(MultiplayerManager.Code.Players, SetPlayers);
            multiplayerManager.Unsubscribe(MultiplayerManager.Code.PlayerJoined, PlayerJoined);
            multiplayerManager.Unsubscribe(MultiplayerManager.Code.ChangeScene, MatchStarted);
            multiplayerManager.Unsubscribe(MultiplayerManager.Code.TurnMe, SetTurn);
            multiplayerManager.Unsubscribe(MultiplayerManager.Code.ChosseTurn, ChosseTurnPlayer);
            multiplayerManager.Unsubscribe(MultiplayerManager.Code.Rematch, RematchEvent);
            multiplayerManager.Unsubscribe(MultiplayerManager.Code.PlayerLeft, EventPlayerLeft);
            multiplayerManager.Unsubscribe(MultiplayerManager.Code.SendSticker, RiseveSticker);
        }

/// <summary>
/// A function that is called when a player receives a sticker.
/// </summary>
/// <param name="MultiplayerMessage">The message that was sent to the server.</param>
        public void RiseveSticker(MultiplayerMessage message)
        {
            var nameSticker = message.GetData<StickerData>();

            if (nameSticker.ID != multiplayerManager.Self.UserId)
            {
                UiManager.instance.StickerOpp.GetComponent<Image>().sprite = UiManager.instance.AllAssets.GetSprite(nameSticker.StickerName);

                UiManager.instance.StickerOpp.GetComponent<Animator>().Play("StickerOpp", 0, 0);


            }

        }
/// <summary>
/// A function that is called when a player leaves the game.
/// </summary>
/// <param name="MultiplayerMessage">This is the message that was sent from the server.</param>
        public void EventPlayerLeft(MultiplayerMessage message)
        {
            var data = message.GetData<string>();

            LeftPlayer?.Invoke(data);

        }

/// <summary>
/// A function that is called when a player requests a rematch.
/// </summary>
/// <param name="MultiplayerMessage">This is the message that was sent to the client.</param>
        public void RematchEvent(MultiplayerMessage message)
        {
            var data = message.GetData<RematchData>();
            onRematch?.Invoke(data);


        }
/// <summary>
/// 
/// </summary>
/// <param name="MultiplayerMessage">This is the message that was sent from the server.</param>
        public void SetTurn(MultiplayerMessage message)
        {
            if (message.GetData<string>() == multiplayerManager.Self.UserId)
            {
                multiplayerManager.isTurn = true;
                IsTurn?.Invoke(true);
            }
            else
            {
                IsTurn?.Invoke(false);
            }

        }
   /// <summary>
   /// call back select turn for palyer
   /// </summary>
   /// <param name="MultiplayerMessage">This is the message that was sent from the other player.</param>
        private void ChosseTurnPlayer(MultiplayerMessage message)
        {
            var hxdTotal = PlayerPrefs.GetInt("HXD");
            var data = message.GetData<DataPlayer>();
            onSetDataInTurn?.Invoke(data);
            if (multiplayerManager.Self.UserId != data.UserId)
            {
                if (data.MinesScore)
                    onSetScoreOpp?.Invoke(data.ScoreOtherPlayer, data.ValueMines, data);

                onSetScoreMe.Invoke(data.Score, 0, data);

                for (int i = 0; i < data.Array2DTilesOtherPlayer.Length; i++)
                {
                    for (int j = 0; j < data.Array2DTilesOtherPlayer[i].Length; j++)
                    {
                        if (data.Array2DTilesOtherPlayer[i][j] == -1)
                        {
                            onSetDataInRowMe(i, j);



                        }

                    }
                }
                ScoreMe = data.Score;
                if (data.ScoreOtherPlayer > 0)
                    ScoreOpp = data.ScoreOtherPlayer;

                if (data.EndGame != true)
                    IsTurn?.Invoke(true);

                if (data.EndGame == true)
                {
                    TimerTurn.instance.TimerPause = true;
                    if (ScoreMe < ScoreOpp)
                    {
                        resultGame = ResultGame.win;

                        ShowResultEndGame("You Win", ScoreOpp, ScoreMe);
                        UiManager.instance.HXDWin.text = "+" + (multiplayerManager.ValueHXDInGameTurn * 2).ToString() + "HXD";
                        PlayerPrefs.SetInt("HXD", hxdTotal + (multiplayerManager.ValueHXDInGameTurn * 2));
                    }
                    else if (ScoreMe > ScoreOpp)
                    {
                        resultGame = ResultGame.loose;

                        ShowResultEndGame("You Loss", ScoreOpp, ScoreMe);
                        UiManager.instance.HXDWin.text = "-" + (multiplayerManager.ValueHXDInGameTurn).ToString() + "HXD";
                        PlayerPrefs.SetInt("HXD", hxdTotal - multiplayerManager.ValueHXDInGameTurn);
                    }
                    else
                    {
                        resultGame = ResultGame.draw;
                        UiManager.instance.HXDWin.text = "+" + multiplayerManager.ValueHXDInGameTurn.ToString() + "HXD";
                        PlayerPrefs.SetInt("HXD", hxdTotal + multiplayerManager.ValueHXDInGameTurn);
                        ShowResultEndGame("Match is Tied", ScoreOpp, ScoreMe);
                    }
                    multiplayerManager.isTurn = false;

                    AudioClipManager.instance.PlaySoundResultGame(resultGame);
                }

            }
            else
            {

                if (data.MinesScore)
                    onSetScoreMe.Invoke(data.ScoreOtherPlayer, data.ValueMines, data);
                onSetScoreOpp?.Invoke(data.Score, 0, data);
                ScoreOpp = data.Score;
                if (data.ScoreOtherPlayer > 0)
                    ScoreMe = data.ScoreOtherPlayer;
                for (int i = 0; i < data.Array2DTilesOtherPlayer.Length; i++)
                {
                    for (int j = 0; j < data.Array2DTilesOtherPlayer[i].Length; j++)
                    {
                        if (data.Array2DTilesOtherPlayer[i][j] == -1)
                        {
                            onSetDataInRowOpp(i, j);


                        }

                    }
                }


                if (data.EndGame == true)
                {
                    TimerTurn.instance.TimerPause = true;
                    if (ScoreMe < ScoreOpp)
                    {
                        ShowResultEndGame("You Win", ScoreOpp, ScoreMe);
                        resultGame = ResultGame.win;
                        PlayerPrefs.SetInt("HXD", hxdTotal + (multiplayerManager.ValueHXDInGameTurn * 2));
                        UiManager.instance.HXDWin.text = "+" + (multiplayerManager.ValueHXDInGameTurn * 2).ToString() + "HXD";
                    }
                    else if (ScoreMe > ScoreOpp)
                    {
                        resultGame = ResultGame.loose;
                        ShowResultEndGame("You Loss", ScoreOpp, ScoreMe);
                        PlayerPrefs.SetInt("HXD", hxdTotal - (multiplayerManager.ValueHXDInGameTurn));
                        UiManager.instance.HXDWin.text = "-" + multiplayerManager.ValueHXDInGameTurn.ToString() + "HXD";
                    }
                    else
                    {
                        resultGame = ResultGame.draw;
                        PlayerPrefs.SetInt("HXD", hxdTotal + multiplayerManager.ValueHXDInGameTurn);
                        UiManager.instance.HXDWin.text = "+" + multiplayerManager.ValueHXDInGameTurn.ToString() + "HXD";
                        ShowResultEndGame("Match is Tied", ScoreOpp, ScoreMe);
                    }
                    multiplayerManager.isTurn = false;

                    AudioClipManager.instance.PlaySoundResultGame(resultGame);
                }



            }
        }

    /// <summary>
    /// It takes a message from the server, gets the data from the message, and sets the Players
    /// variable to the data
    /// </summary>
    /// <param name="MultiplayerMessage">A class that contains the data that is sent from the
    /// server.</param>
        private void SetPlayers(MultiplayerMessage message)
        {
            Players = message.GetData<List<PlayerData>>();
            PlayerPrefs.SetString("Opp", Players.Find(e => e != CurrentPlayer).DisplayName);
            onPlayersReceived?.Invoke(Players);
            GetCurrentPlayer();
        }

     /// <summary>
     /// "When a player joins, add them to the list of players, and then invoke the onPlayerJoined
     /// event."
     /// 
     /// The onPlayerJoined event is a UnityEvent that we'll use to update the UI
     /// </summary>
     /// <param name="MultiplayerMessage">This is the message that is sent from the server.</param>
        private void PlayerJoined(MultiplayerMessage message)
        {
            PlayerData player = message.GetData<PlayerData>();
            int index = Players.IndexOf(null);
            if (index > -1)
                Players[index] = player;
            else
            {
                Players.Add(player);
                PlayerPrefs.SetString("Opp", player.DisplayName);
            }

            onPlayerJoined?.Invoke(player);
        }

/// <summary>
/// A function that is called when the players in the match change.
/// </summary>
/// <param name="IMatchPresenceEvent">This is the event that is sent to the client. It contains
/// information about the players that have joined or left the match.</param>
        private void PlayersChanged(IMatchPresenceEvent matchPresenceEvent)
        {
            if (blockJoinsAndLeaves)
                return;

            foreach (IUserPresence userPresence in matchPresenceEvent.Leaves)
            {
                for (int i = 0; i < Players.Count(); i++)
                {
                    if (Players[i] != null && Players[i].Presence.SessionId == userPresence.SessionId)
                    {
                        onPlayerLeft?.Invoke(Players[i]);
                        Debug.Log(Players[i].DisplayName);
                        Players[i] = null;
                    }
                }
            }
        }

      /// <summary>
      /// This function is called when the player joins a match. It adds a listener to the socket that
      /// will call the PlayersChanged function when the players in the match change. It also calls the
      /// GetCurrentPlayer function
      /// </summary>
        private void MatchJoined()
        {
            nakamaManager.Socket.ReceivedMatchPresence += PlayersChanged;
            GetCurrentPlayer();

        }
/// <summary>
/// show ui result in end game
/// </summary>
/// <param name="resutlText">The text to display in the result text box.</param>
/// <param name="score1">The score of the first player.</param>
/// <param name="score2">The score of the second player.</param>
        private void ShowResultEndGame(string resutlText, int score1, int score2)
        {
            ActionEndGame.instance.ResultPanel.SetActive(true);
            ActionEndGame.instance.ScoreMe.text = score1.ToString();
            ActionEndGame.instance.ScoreOpp.text = score2.ToString();
            ActionEndGame.instance.ResultText.text = resutlText;
            ActionEndGame.instance.IconMe.transform.parent = FindObjectOfType<Canvas>().transform;
            ActionEndGame.instance.IconOpp.transform.parent = FindObjectOfType<Canvas>().transform;
            ActionEndGame.instance.IconMe.enabled = true;
            ActionEndGame.instance.IconOpp.enabled = true;
            ActionEndGame.instance.IconMe.Play("EndGamePlayer1Icon");
            ActionEndGame.instance.IconOpp.Play("EndGamePlater2Icon");
            ActionEndGame.instance.IconMe.GetComponentInChildren<ParticleSystem>().Stop();
            ActionEndGame.instance.IconOpp.GetComponentInChildren<ParticleSystem>().Stop();
        }

        private void GetCurrentPlayer()
        {
            if (Players == null)
                return;

            if (multiplayerManager.Self == null)
                return;

            if (CurrentPlayer != null)
                return;

            CurrentPlayer = Players.Find(player => player.Presence.SessionId == multiplayerManager.Self.SessionId);
            CurrentPlayerNumber = Players.IndexOf(CurrentPlayer);
            onLocalPlayerObtained?.Invoke(CurrentPlayer, CurrentPlayerNumber);


        }

      /// <summary>
      /// This function is called when a player leaves the match. It resets the match and the players
      /// </summary>
        private void ResetLeaved()
        {
            nakamaManager.Socket.ReceivedMatchPresence -= PlayersChanged;
            blockJoinsAndLeaves = false;
            Players = null;
            CurrentPlayer = null;
            CurrentPlayerNumber = -1;

        }

      /// <summary>
      /// This function is called when a match is started
      /// </summary>
      /// <param name="MultiplayerMessage">This is the message that was sent to the client.</param>
        public void MatchStarted(MultiplayerMessage message)
        {
            blockJoinsAndLeaves = true;


        }

        #endregion
    }
}
