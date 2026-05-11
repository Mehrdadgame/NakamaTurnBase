namespace Nakama.Helpers
{
    public partial class MultiplayerManager
    {
        public enum Code
        {
            Players     = 0,
            PlayerJoined = 1,
            PlayerInput  = 2,
            PlayerWon   = 3,
            Draw        = 4,
            ChangeScene = 5,
            TurnMe      = 6,
            ChosseTurn  = 7,
            Rematch     = 8,
            PlayerLeft           = 9,
            SendSticker          = 10,
            OpponentDisconnected = 11,  // payload: { remainingSeconds: int }
            OpponentReconnected  = 12,  // payload: { userId: string }
            DisconnectWin        = 13,  // payload: { winnerUserId: string }
        }
    }
}
