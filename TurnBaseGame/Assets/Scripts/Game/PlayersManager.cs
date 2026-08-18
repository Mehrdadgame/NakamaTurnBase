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
    /// <summary>
    /// 
    /// </summary>
    public class PlayersManager : MonoBehaviour
    {
        #region FIELDS

        private NakamaManager nakamaManager = null;
        private MultiplayerManager multiplayerManager = null;
        private bool blockJoinsAndLeaves = false;
        private readonly Dictionary<string, string> botNamesByUserId = new Dictionary<string, string>();

        // Tutorial: hold the full bot message while the overlay is blocking
        private MultiplayerMessage _pendingBotMessage = null;

        private static readonly string[] BotNamePool =
        {
            "DiceBot", "NinjaBot", "StormBot", "BladeBot", "ShadowBot",
            "CobraBot", "FalconBot", "TigerBot", "NovaBot", "IronBot"
        };

        #endregion

        #region EVENTS

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
        public event Action<MatchResult> onMatchEnded;
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
            TutorialManager.Instance?.SetBotReleaseCallback(ApplyPendingBotMessage);
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
            multiplayerManager.Unsubscribe(MultiplayerManager.Code.Rematch, RematchEvent);
            multiplayerManager.Unsubscribe(MultiplayerManager.Code.PlayerLeft, EventPlayerLeft);
            multiplayerManager.Unsubscribe(MultiplayerManager.Code.SendSticker, RiseveSticker);
        }

        public void RiseveSticker(MultiplayerMessage message)
        {
            var nameSticker = message.GetData<StickerData>();

            if (nameSticker.ID != multiplayerManager.Self.UserId)
            {
                UiManager.instance.StickerOpp.GetComponent<Image>().sprite = UiManager.instance.AllAssets.GetSprite(nameSticker.StickerName);

                UiManager.instance.StickerOpp.GetComponent<Animator>().Play("StickerOpp", 0, 0);


            }

        }
        public void EventPlayerLeft(MultiplayerMessage message)
        {
            var data = message.GetData<string>();
            //
            //onRematch?.Invoke(data);

            LeftPlayer?.Invoke(data);

        }

        public void RematchEvent(MultiplayerMessage message)
        {
            var data = message.GetData<RematchData>();
            onRematch?.Invoke(data);


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
        public bool HasPendingBotMessage => _pendingBotMessage != null;

        public void ApplyPendingBotMessage()
        {
            if (_pendingBotMessage == null)
            {
                Debug.Log("[Tutorial] ApplyPendingBotMessage called but no pending bot message");
                return;
            }
            var msg = _pendingBotMessage;
            _pendingBotMessage = null;
            Debug.Log("[Tutorial] Applying pending bot message");
            ChosseTurnPlayer(msg);
        }

        private void ChosseTurnPlayer(MultiplayerMessage message)
        {
            var data = message.GetData<DataPlayer>();

            var league = ClientLeagues.Get(GameManager.Instance.modeGame);

            if (multiplayerManager.Self.UserId != data.UserId)
            {
                // Tutorial: buffer the entire message so ALL side-effects are deferred
                if (TutorialManager.Instance != null && TutorialManager.Instance.IsBotMoveSuppressed)
                {
                    Debug.Log("[Tutorial] Buffering bot message (suppression on)");
                    _pendingBotMessage = message;
                    return;
                }

                Debug.Log($"[Tutorial] Processing bot message immediately (suppression off). Active={TutorialManager.Instance?.IsActive}, line={data.NumberLine}, row={data.NumberRow}, tile={data.NumberTile}");
                onSetDataInTurn?.Invoke(data);
                if (data.MinesScore)
                    onSetScoreOpp?.Invoke(data.ScoreOtherPlayer, data.ValueMines, data);

                onSetScoreMe.Invoke(data.Score, 0, data);

                for (int i = 0; i < data.Array2DTilesOtherPlayer.Length; i++)
                {
                    for (int j = 0; j < data.Array2DTilesOtherPlayer[i].Length; j++)
                    {
                        if (data.Array2DTilesOtherPlayer[i][j] == -1)
                            onSetDataInRowMe(i, j);
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
                    bool isDraw = ScoreMe == ScoreOpp;
                    bool isLocalWinner = false;
                    int localScore = data.ScoreOtherPlayer > 0 ? data.ScoreOtherPlayer : ScoreOpp;
                    int opponentScore = data.Score;
                    if (localScore != opponentScore)
                        isLocalWinner = localScore > opponentScore;

                    if (ScoreMe < ScoreOpp)
                    {
                        ShowResultEndGame("شما بردی", ScoreOpp, ScoreMe);
                        UiManager.instance.TasiWin.text = "‏+" + PersianTextUtils.FormatNumber(league.winnerReward) + " تاسی";
                    }
                    else if (ScoreMe > ScoreOpp)
                    {
                        ShowResultEndGame("شما باختی", ScoreOpp, ScoreMe);
                        UiManager.instance.TasiWin.text = "‏-" + PersianTextUtils.FormatNumber(league.entryFee) + " تاسی";
                    }
                    else
                    {
                        ShowResultEndGame("مساوی شدید", ScoreOpp, ScoreMe);
                        UiManager.instance.TasiWin.text = "‏+" + PersianTextUtils.FormatNumber(league.drawRefund) + " تاسی";
                    }
                    multiplayerManager.isTurn = false;
                    onMatchEnded?.Invoke(new MatchResult
                    {
                        LocalScore = localScore,
                        OpponentScore = opponentScore,
                        IsDraw = isDraw,
                        IsLocalWinner = isDraw ? false : isLocalWinner
                    });
                }
            }
            else
            {
                onSetDataInTurn?.Invoke(data);
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
                            onSetDataInRowOpp(i, j);
                    }
                }

                if (data.EndGame == true)
                {
                    TimerTurn.instance.TimerPause = true;
                    bool isDraw = ScoreMe == ScoreOpp;
                    bool isLocalWinner = false;
                    int localScore = ScoreOpp;
                    int opponentScore = data.ScoreOtherPlayer > 0 ? data.ScoreOtherPlayer : ScoreMe;
                    if (localScore != opponentScore)
                        isLocalWinner = localScore > opponentScore;

                    if (ScoreMe < ScoreOpp)
                    {
                        ShowResultEndGame("شما بردی", ScoreOpp, ScoreMe);
                        UiManager.instance.TasiWin.text = "‏+" + PersianTextUtils.FormatNumber(league.winnerReward) + " تاسی";
                    }
                    else if (ScoreMe > ScoreOpp)
                    {
                        ShowResultEndGame("شما باختی", ScoreOpp, ScoreMe);
                        UiManager.instance.TasiWin.text = "‏-" + PersianTextUtils.FormatNumber(league.entryFee) + " تاسی";
                    }
                    else
                    {
                        ShowResultEndGame("مساوی شدید", ScoreOpp, ScoreMe);
                        UiManager.instance.TasiWin.text = "‏+" + PersianTextUtils.FormatNumber(league.drawRefund) + " تاسی";
                    }
                    multiplayerManager.isTurn = false;
                    onMatchEnded?.Invoke(new MatchResult
                    {
                        LocalScore = localScore,
                        OpponentScore = opponentScore,
                        IsDraw = isDraw,
                        IsLocalWinner = isDraw ? false : isLocalWinner
                    });
                }
                if (data.EndGame != true)
                    IsTurn?.Invoke(false);
            }
        }

        private void SetPlayers(MultiplayerMessage message)
        {
            Players = message.GetData<List<PlayerData>>();
            GetCurrentPlayer();
            UpdateOpponentNameCache();
            onPlayersReceived?.Invoke(Players);
        }

        private void PlayerJoined(MultiplayerMessage message)
        {
            PlayerData player = message.GetData<PlayerData>();
            if (Players == null)
                Players = new List<PlayerData>();

            int index = Players.IndexOf(null);
            if (index > -1)
                Players[index] = player;
            else
                Players.Add(player);

            GetCurrentPlayer();
            UpdateOpponentNameCache();
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
                        Debug.Log(Players[i].DisplayName);
                        Players[i] = null;
                    }
                }
            }
        }

        private void MatchJoined()
        {
            nakamaManager.Socket.ReceivedMatchPresence += PlayersChanged;
            GetCurrentPlayer();
            UpdateOpponentNameCache();

        }
        private void ShowResultEndGame(string resutlText, int score1, int score2)
        {
            ActionEndGame.instance.ResultPanel.SetActive(true);
            ActionEndGame.instance.ScoreMe.text = score1.ToString();
            ActionEndGame.instance.ScoreOpp.text = score2.ToString();
            ActionEndGame.instance.ResultText.text = resutlText;
            ActionEndGame.instance.RefreshResultPresentation();
            ActionEndGame.instance.IconMe.enabled = true;
            ActionEndGame.instance.IconOpp.enabled = true;
            ActionEndGame.instance.IconMe.Play("EndGamePlayer1Icon");
            ActionEndGame.instance.IconOpp.Play("EndGamePlater2Icon");

        }

        private void GetCurrentPlayer()
        {
            if (Players == null)
                return;

            if (multiplayerManager.Self == null)
                return;

            CurrentPlayer = Players.Find(player => player != null &&
                                                   player.Presence != null &&
                                                   player.Presence.SessionId == multiplayerManager.Self.SessionId);
            CurrentPlayerNumber = Players.IndexOf(CurrentPlayer);

            if (CurrentPlayer != null)
                onLocalPlayerObtained?.Invoke(CurrentPlayer, CurrentPlayerNumber);


        }

        private void ResetLeaved()
        {
            nakamaManager.Socket.ReceivedMatchPresence -= PlayersChanged;
            blockJoinsAndLeaves = false;
            Players = null;
            CurrentPlayer = null;
            CurrentPlayerNumber = -1;
            botNamesByUserId.Clear();

        }

        private void UpdateOpponentNameCache()
        {
            if (Players == null || multiplayerManager == null || multiplayerManager.Self == null)
                return;

            string mySessionId = multiplayerManager.Self.SessionId;
            var opponent = Players.Find(player => player != null &&
                                                  player.Presence != null &&
                                                  player.Presence.SessionId != mySessionId);

            if (opponent == null)
                return;

            string name = ResolvePlayerDisplayName(opponent);
            if (string.IsNullOrWhiteSpace(name))
                name = "Opponent";

            PlayerPrefs.SetString("Opp", name);
        }

        private string ResolvePlayerDisplayName(PlayerData player)
        {
            if (player == null)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(player.DisplayName))
                return player.DisplayName.Trim();

            if (player.Presence != null && !string.IsNullOrWhiteSpace(player.Presence.Username))
                return player.Presence.Username.Trim();

            if (player.Presence != null && IsBotUser(player.Presence.UserId))
                return GetOrCreateBotName(player.Presence.UserId);

            return string.Empty;
        }

        private static bool IsBotUser(string userId)
        {
            return !string.IsNullOrWhiteSpace(userId) &&
                   userId.StartsWith("bot_", StringComparison.OrdinalIgnoreCase);
        }

        private string GetOrCreateBotName(string botUserId)
        {
            if (string.IsNullOrWhiteSpace(botUserId))
                return "Bot";

            if (botNamesByUserId.TryGetValue(botUserId, out var cached))
                return cached;

            string baseName = BotNamePool[UnityEngine.Random.Range(0, BotNamePool.Length)];
            string randomName = $"{baseName}_{UnityEngine.Random.Range(10, 99)}";
            botNamesByUserId[botUserId] = randomName;
            return randomName;
        }

        public void MatchStarted(MultiplayerMessage message)
        {
            blockJoinsAndLeaves = true;


        }

        #endregion
    }
}
