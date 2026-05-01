const TickRate = 16;
const DurationLobby = 10;
const DurationRoundResults = 5;
const DurationBattleEnding = 3;
const NecessaryWins = 3;
const MaxPlayers = 2;
const PlayerNotFound = -1;
const CollectionUser = "User";
const KeyTrophies = "Trophies";
const IdLeaderboard = "b7c182b36521Win";

// Bot configuration
const BotThinkMinTicks = TickRate * 1;  // min 1 second thinking delay
const BotThinkMaxTicks = TickRate * 3;  // max 3 seconds thinking delay

const BOT_NAMES: string[] = [
    "Ali_K99", "Daniel_P", "Sara_GG", "xX_Cobra_Xx", "Reza_77",
    "ProGamer88", "IronWolf7", "NightStar", "Champion_K", "Shadow_X9",
    "MasterPlay", "Kamran_Ace", "DarkFire", "QuickShot9", "EliteKing",
    "Arash_Pro", "Ninja_Storm", "CoolBreeze", "FlashPlayer", "TitanFist"
];

// 0=Easy, 1=Normal, 2=Hard
const BOT_DIFFICULTIES = [0, 1, 2];

const MessagesLogic: {
    [opCode: number]: (
        message: nkruntime.MatchMessage,
        state: GameState,
        dispatcher: nkruntime.MatchDispatcher,
        nakama: nkruntime.Nakama,
        logger: nkruntime.Logger
    ) => void
} = {
    7: ChooseTurnPlayer,
    8: Rematch,
    10: StickersManager,
};
