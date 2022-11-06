using NinjaBattle.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Nakama.Helpers
{/* A partial class. */

    public partial class MultiplayerManager : MonoBehaviour
    {
        #region FIELDS

        private const int TickRate = 5;
        private const float SendRate = 1f / (float)TickRate;
        private const string JoinOrCreateMatchRpc = "JoinOrCreateMatchRpc";
        private const string LogFormat = "{0} with code {1}:\n{2}";
        private const string SendingDataLog = "Sending data";
        private const string ReceivedDataLog = "Received data";

        [SerializeField] private bool enableLog = false;

        private Dictionary<Code, Action<MultiplayerMessage>> onReceiveData = new ();
        private IMatch match = null;

        #endregion

        #region EVENTS

        public event Action onMatchJoin = null;
        public event Action onMatchLeave = null;
        public event Action onLocalTick = null;
        public event Action onTurnMe = null;

        public bool isTurn;

        public TimeSpan ping;

        #endregion

        #region PROPERTIES

        public static MultiplayerManager Instance { get; private set; } = null;
        public IUserPresence Self { get => match == null ? null : match.Self; }
        public IUserPresence Opp;
        public bool IsOnMatch { get => match != null; }
        public int ValueHXDInGameTurn;
        public NakamaUserManager players;

        private short rowTable;
        private short colTable;

        public DateTime start;
        public DateTime end;
      
        #endregion

        #region BEHAVIORS

        private void Awake()
        {

            Instance = this;
        }
      
      /// <summary>
      /// It sets the row and column of the table based on the mode of the game
      /// </summary>
        private void SetRowAndCol()
        {
            switch (GameManager.Instance.modeGame)
            {
                case ModeGame.ThreeByThree:
                    rowTable = 4;
                    colTable = 4;
                    break;
                case ModeGame.FourByThree:
                    rowTable = 4;
                    colTable = 3;
                    break;
                case ModeGame.VerticalAndHorizontal:
                    rowTable = 3;
                    colTable = 3;
                    break;

            }


        }
      
     /// <summary>
     /// A function that is used to join a match.
     /// </summary>
     /// <param name="ModeGame">The mode of the game you want to join.</param>
        public async void JoinMatchAsync(ModeGame mode)
        {
           
            PlayerPrefs.DeleteKey("Opp");
            NakamaManager.Instance.Socket.ReceivedMatchState -= Receive;
            NakamaManager.Instance.Socket.ReceivedMatchState += Receive;
            NakamaManager.Instance.onDisconnected += Disconnected;
            IApiRpc rpcResult = await NakamaManager.Instance.SendRPC(JoinOrCreateMatchRpc, mode.ToString());
            string matchId = rpcResult.Payload;

            var stringProperties = new Dictionary<string, string>()
            {
               {"mode", mode.ToString()}
            };
            match = await NakamaManager.Instance.Socket.JoinMatchAsync(matchId, stringProperties);

            onMatchJoin?.Invoke();
        }
      /// <summary>
      /// If the player is disconnected from the server, then remove the player from the match and
      /// invoke the onMatchLeave event
      /// </summary>
        private void Disconnected()
        {
            NakamaManager.Instance.onDisconnected -= Disconnected;
            NakamaManager.Instance.Socket.ReceivedMatchState -= Receive;
            match = null;
            onMatchLeave?.Invoke();
        }
       
      /// <summary>
      /// This function is called when the player leaves the match. It removes the player from the
      /// match, removes the event listeners, and sets the match to null
      /// </summary>
        public async void LeaveMatchAsync()
        {
            await NakamaManager.Instance.Socket.LeaveMatchAsync(match);
            NakamaManager.Instance.onDisconnected -= Disconnected;
            NakamaManager.Instance.Socket.ReceivedMatchState -= Receive;
            match = null;
            onMatchLeave?.Invoke();
            isTurn = false;
        }
      
      /// <summary>
      /// It sends a message to the server.
      /// 
      /// The first parameter is the code, which is an enum that we'll define later. The second
      /// parameter is the data, which is an object that we'll serialize into a JSON string.
      /// 
      /// The function first checks if the match is null. If it is, it returns.
      /// 
      /// The next line serializes the data into a JSON string. If the data is null, it sets the json
      /// variable to an empty string.
      /// 
      /// The next line logs the data if the enableLog variable is true.
      /// 
      /// The last line sends the message to the server.
      /// </summary>
      /// <param name="Code">This is the code that you will use to identify the message.</param>
      /// <param name="data">The data you want to send.</param>
      /// <returns>
      /// The code is being returned.
      /// </returns>
        public void Send(Code code, object data = null)
        {
            if (match == null)
                return;

            string json = data != null ? data.Serialize() : string.Empty;
            if (enableLog)
                LogData(SendingDataLog, (long)code, json);

            NakamaManager.Instance.Socket.SendMatchStateAsync(match.Id, (long)code, json);
        }

     
        public void Send(Code code, object data = null, IEnumerable<IUserPresence> player = null)
        {
            if (match == null)
                return;

            string json = data != null ? data.Serialize() : string.Empty;
            if (enableLog)
                LogData(SendingDataLog, (long)code, json);

                NakamaManager.Instance.Socket.SendMatchStateAsync(match.Id, (long)code, json, player);
        }

        public void Send(Code code, byte[] bytes)
        {
            if (match == null)
                return;

            if (enableLog)
                LogData(SendingDataLog, (long)code, String.Empty);

            NakamaManager.Instance.Socket.SendMatchStateAsync(match.Id, (long)code, bytes);
        }
       
     /// <summary>
     /// The function receives the data from the opponent and then calls the function that is registered
     /// to the data code
     /// </summary>
     /// <param name="IMatchState">This is the data that is received from the opponent.</param>
        private void Receive(IMatchState newState)
        {
            if (enableLog)
            {
                var encoding = System.Text.Encoding.UTF8;
                var json = encoding.GetString(newState.State);
                LogData(ReceivedDataLog, newState.OpCode, json);
            }

            MultiplayerMessage multiplayerMessage = new(newState);
            if (onReceiveData.ContainsKey(multiplayerMessage.DataCode))
                onReceiveData[multiplayerMessage.DataCode]?.Invoke(multiplayerMessage);
            Opp = newState.UserPresence;
          
        }

    
      /// <summary>
      /// This function adds a new action to the dictionary of actions
      /// </summary>
      /// <param name="Code">The code that the message is being sent with.</param>
      /// <param name="action">The method that will be called when the message is received.</param>
        public void Subscribe(Code code, Action<MultiplayerMessage> action)
        {
            if (!onReceiveData.ContainsKey(code))
                onReceiveData.Add(code, null);

            onReceiveData[code] += action;
        }
      
    /// <summary>
    /// > Unsubscribe from a specific code
    /// </summary>
    /// <param name="Code">The code that the message is being sent with.</param>
    /// <param name="action">The method that will be called when the message is received.</param>
        public void Unsubscribe(Code code, Action<MultiplayerMessage> action)
        {
            if (onReceiveData.ContainsKey(code))
                onReceiveData[code] -= action;
        }

       /// <summary>
       /// Logs the data to the console
       /// </summary>
       /// <param name="description">A string that describes the data.</param>
       /// <param name="dataCode">The code that identifies the type of data being sent.</param>
       /// <param name="json">The JSON string that you want to send to the server.</param>
        private void LogData(string description, long dataCode, string json)
        {
            Debug.Log(string.Format(LogFormat, description, (Code)dataCode, json));
        }

        /// <summary>
        /// I send the data to the server, and the server sends it to the other player
        /// </summary>
        /// <param name="nameTile">the name of the tile</param>
        /// <param name="number">the number of the tile</param>
        /// <param name="line">the line of the tile</param>
        /// <param name="row">the row of the tile</param>
        public void SendTurn(string nameTile, int number, int line, int row)
        {
            SetRowAndCol();
            var data = new DataPlayer()
            {
                UserId = players.User.Id,
                NameTile = nameTile,
                NumberTile = number,
                NumberRow = row,
                NumberLine = line,
                PlayerWin = "",
                sumRow1 = new int[colTable],
                sumRow2 = new int[rowTable],
                Array2DTilesOtherPlayer = new int[3][],
                Array2DTilesPlayer = new int[3][],
             
            };

            Send(Code.ChosseTurn, data);

        }

        #endregion



    }



}
