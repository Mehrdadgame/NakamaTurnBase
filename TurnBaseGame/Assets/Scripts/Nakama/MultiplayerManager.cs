using NinjaBattle.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;


namespace Nakama.Helpers
{
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

        private Dictionary<Code, Action<MultiplayerMessage>> onReceiveData = new Dictionary<Code, Action<MultiplayerMessage>>();
        private IMatch match = null;

        #endregion

        #region EVENTS

        public event Action onMatchJoin = null;
        public event Action onMatchLeave = null;
        public event Action onLocalTick = null;
        public event Action onTurnMe = null;

        public bool isTurn;


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
        #endregion

        #region BEHAVIORS

        private void Awake()
        {

            Instance = this;
        }

        private void SetRowAndCol()
        {
            switch (GameManager.Instance.modeGame)
            {
                case ModeGame.ThreeByThree:
                    rowTable = 4;
                    colTable = 4; ;
                    break;
                case ModeGame.FourByThree:
                    rowTable = 4;
                    colTable = 3; ;
                    break;
                case ModeGame.VerticalAndHorizontal:
                    rowTable =3;
                    colTable = 3; ;
                    break;
                default:
                    break;
            }
            if (GameManager.Instance.modeGame == ModeGame.FourByThree)
            {
              
            }
           
        }
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



        private void Disconnected()
        {
            NakamaManager.Instance.onDisconnected -= Disconnected;
            NakamaManager.Instance.Socket.ReceivedMatchState -= Receive;
            match = null;
            onMatchLeave?.Invoke();
        }

        public async void LeaveMatchAsync()
        {
            NakamaManager.Instance.onDisconnected -= Disconnected;
            NakamaManager.Instance.Socket.ReceivedMatchState -= Receive;
            await NakamaManager.Instance.Socket.LeaveMatchAsync(match);
            match = null;
            onMatchLeave?.Invoke();
        }

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

        private void Receive(IMatchState newState)
        {
            if (enableLog)
            {
                var encoding = System.Text.Encoding.UTF8;
                var json = encoding.GetString(newState.State);
                LogData(ReceivedDataLog, newState.OpCode, json);
            }


            MultiplayerMessage multiplayerMessage = new MultiplayerMessage(newState);
            if (onReceiveData.ContainsKey(multiplayerMessage.DataCode))
                onReceiveData[multiplayerMessage.DataCode]?.Invoke(multiplayerMessage);
            Opp = newState.UserPresence;


        }


        public void Subscribe(Code code, Action<MultiplayerMessage> action)
        {
            if (!onReceiveData.ContainsKey(code))
                onReceiveData.Add(code, null);

            onReceiveData[code] += action;
        }

        public void Unsubscribe(Code code, Action<MultiplayerMessage> action)
        {
            if (onReceiveData.ContainsKey(code))
                onReceiveData[code] -= action;
        }

        private void LogData(string description, long dataCode, string json)
        {
            Debug.Log(string.Format(LogFormat, description, (Code)dataCode, json));
        }

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
