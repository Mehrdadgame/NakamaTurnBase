using System;
using System.Collections.Generic;
using System.Linq;
using Nakama;
using Nakama.Helpers;
using UnityEngine;

namespace NinjaBattle.Game
{
    public class PlayersManager : MonoBehaviour
    {
        #region FIELDS

        private NakamaManager nakamaManager = null;
        private MultiplayerManager multiplayerManager = null;
        private bool blockJoinsAndLeaves = false;

        #endregion

        #region EVENTS

        public event Action<List<PlayerData>> onPlayersReceived;
        public event Action<PlayerData> onPlayerJoined;
        public event Action<PlayerData> onPlayerLeft;
        public event Action<PlayerData, int> onLocalPlayerObtained;
        public event Action<bool> IsTurn;
        public event Action<DataPlayer> onSetDataInTurn;
        public event Action<int, int > onSetDataInRowMe;
        public event Action<int, int > onSetDataInRowOpp;
        public event Action<int, int , DataPlayer> onSetScoreMe;
        public event Action<int,int, DataPlayer > onSetScoreOpp;
        #endregion

        #region PROPERTIES

        public static PlayersManager Instance { get; private set; } = null;
        public List<PlayerData> Players { get; private set; } = new List<PlayerData>();
        public int PlayersCount { get => Players.Count(player => player != null); }
        public PlayerData CurrentPlayer { get; private set; } = null;
        public int CurrentPlayerNumber { get; private set; } = -1;

        public int ScoreMe;
        public int ScoreOpp;
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
        }

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
        }
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
        private void ChosseTurnPlayer(MultiplayerMessage message)
        {
         
            var data = message.GetData<DataPlayer>();

            Debug.Log(data);
            if(multiplayerManager.Self.UserId != data.UserId)
            {
                if (data.ResultRow.Length > 0)
                {
                    for (int i = 0; i < data.ResultRow.Length; i++)
                    {
                        Debug.Log(data.ResultRow[i] + data.ResultLine);
                        onSetDataInRowMe(data.ResultLine, data.ResultRow[i] );
                    }
                  
                }
                IsTurn?.Invoke(true);
                ScoreMe = data.Score;
                //ScoreOpp = data.ScoreOtherPlayer;
                if(data.MinesScore)
                onSetScoreOpp?.Invoke(data.ScoreOtherPlayer,data.ValueMines,data);
                Debug.Log(data.ScoreOtherPlayer + "other");

                if (data.EndGame == true)
                {
                    Debug.Log(data.PlayerWin + " " + multiplayerManager.Self.UserId);
                    if (ScoreMe<ScoreOpp)
                    {
                        ShowResultEndGame("You Win", ScoreOpp, ScoreMe);

                    }
                    else if(ScoreMe > ScoreOpp)
                    {
                        ShowResultEndGame("You Loss",  ScoreOpp, ScoreMe);
                    }
                    else
                    {
                        ShowResultEndGame("Match is Tied", ScoreOpp, ScoreMe);
                    }
                    multiplayerManager.isTurn = false;
                    IsTurn?.Invoke(false);
                }
                onSetScoreMe.Invoke(data.Score,0,data);
                Debug.Log(data.Score);
            }
            else
            {
           
                for (int i = 0; i < data.sumRow1.Length; i++)
                {
                    Debug.Log(data.sumRow1[i]);
                }
                for (int i = 0; i < data.sumRow2.Length; i++)
                {
                    Debug.Log(data.sumRow2[i]);
                }
                // ScoreMe = data.ScoreOtherPlayer;
                if (data.ResultRow.Length >0)
                {
                    for (int i = 0; i < data.ResultRow.Length; i++)
                    {
                        
                       
                        onSetDataInRowOpp(data.ResultLine, data.ResultRow[i]);
                       
                    }
                  
                }
                if (data.MinesScore)
                    onSetScoreMe.Invoke(data.ScoreOtherPlayer,data.ValueMines,data);
                Debug.Log(data.ScoreOtherPlayer + "other");
                ScoreOpp = data.Score;
                Debug.Log(data.Score);
                onSetScoreOpp?.Invoke(data.Score ,0,data);
                if (data.EndGame == true)
                {
                       Debug.Log(data.PlayerWin + " " + multiplayerManager.Self.UserId);
                    if (ScoreMe<ScoreOpp)
                    {
                        ShowResultEndGame("You Win",  ScoreOpp, ScoreMe);
                        Debug.Log("Win Me");
                    }
                    else if(ScoreMe > ScoreOpp)
                    {
                        ShowResultEndGame("You Loos",  ScoreOpp, ScoreMe);
                        Debug.Log("Win Opp");
                    }
                    else
                    {
                        ShowResultEndGame("Match is Tied", ScoreOpp, ScoreMe);
                    }
                    multiplayerManager.isTurn = false;
                   
                }
                IsTurn?.Invoke(false);

            }
           
            onSetDataInTurn?.Invoke(data);
        }

        private void SetPlayers(MultiplayerMessage message)
        {
            Players = message.GetData<List<PlayerData>>();
            PlayerPrefs.SetString("Opp", Players.Find(e => e != CurrentPlayer).DisplayName);
            onPlayersReceived?.Invoke(Players);
            GetCurrentPlayer();
        }

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
                        Players[i] = null;
                    }
                }
            }
        }

        private  void MatchJoined()
        {
            nakamaManager.Socket.ReceivedMatchPresence += PlayersChanged;
            GetCurrentPlayer();
          
        }
        private void ShowResultEndGame(string resutlText , int score1 , int score2)
        {
            ActionEndGame.instance.ResultPanel.SetActive(true);
            ActionEndGame.instance.ScoreMe.text = score1.ToString();
            ActionEndGame.instance.ScoreOpp.text = score2.ToString();
            ActionEndGame.instance.ResultText.text = resutlText;
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

        private void ResetLeaved()
        {
            nakamaManager.Socket.ReceivedMatchPresence -= PlayersChanged;
            blockJoinsAndLeaves = false;
            Players = null;
            CurrentPlayer = null;
            CurrentPlayerNumber = -1;

        }

        public  void MatchStarted(MultiplayerMessage message)
        {
            blockJoinsAndLeaves = true;
         
           
        }

        #endregion
    }
}
