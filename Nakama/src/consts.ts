
const TickRate = 16;
const DurationLobby = 10;
const DurationRoundResults = 5;
const DurationBattleEnding = 3;
const NecessaryWins = 3;
const MaxPlayers = 2;
const PlayerNotFound = -1;
const CollectionUser = "User";
const KeyTrophies = "Trophies";
let ScoreFirstPlayer = 0;
let ScoreSecendPlayer =0;


const MessagesLogic: { [opCode: number]: (message: nkruntime.MatchMessage, state: GameState, dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama , logger : nkruntime.Logger) => void } =
{
    7: ChooseTurnPlayer
   
}
