namespace Nakama.Helpers
{
   /* It's a list of all the codes that we will use to communicate between the server and the client */
    public partial class MultiplayerManager
    {
        public enum Code
        {
            Players = 0,
            PlayerJoined = 1,
            PlayerInput = 2,
            PlayerWon = 3,
            Draw = 4,
            ChangeScene = 5,
            TurnMe = 6,
            ChosseTurn=7,
            Rematch = 8,
            PlayerLeft = 9,
            SendSticker =10,
        }
    }
}
