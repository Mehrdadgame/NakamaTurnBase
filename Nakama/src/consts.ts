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

// Currency rewards
const COINS_WIN  = 150;
const COINS_DRAW = 50;
const COINS_LOSE = 20;

// Card storage
const CollectionCards = "Cards";
const KeyOwnedCards   = "owned_cards";

// Bot configuration
const BotThinkMinTicks = TickRate * 1;
const BotThinkMaxTicks = TickRate * 3;

const BOT_NAMES: string[] = [
    "Ali_K99", "Daniel_P", "Sara_GG", "xX_Cobra_Xx", "Reza_77",
    "ProGamer88", "IronWolf7", "NightStar", "Champion_K", "Shadow_X9",
    "MasterPlay", "Kamran_Ace", "DarkFire", "QuickShot9", "EliteKing",
    "Arash_Pro", "Ninja_Storm", "CoolBreeze", "FlashPlayer", "TitanFist"
];

const BOT_DIFFICULTIES = [0, 1, 2];

// Card definitions (id, name, cost)
interface CardDefinition {
    id: number;
    name: string;
    cost: number;
}

const CARD_DEFINITIONS: CardDefinition[] = [
    { id: 0,  name: "PowerRow",       cost: 300 },
    { id: 1,  name: "PowerColumn",    cost: 300 },
    { id: 2,  name: "GuardianShield", cost: 400 },
    { id: 3,  name: "SecondChance",   cost: 250 },
    { id: 4,  name: "DiceLock",       cost: 350 },
    { id: 5,  name: "IceLock",        cost: 450 },
    { id: 6,  name: "Breaker",        cost: 650 },
    { id: 7,  name: "MirrorDice",     cost: 500 },
    { id: 8,  name: "ChaosSwap",      cost: 500 },
    { id: 9,  name: "LuckyRow",       cost: 450 },
    { id: 10, name: "LuckySix",       cost: 700 },
    { id: 11, name: "DiceThief",      cost: 750 },
    { id: 12, name: "CounterPulse",   cost: 500 },
];

const MessagesLogic: {
    [opCode: number]: (
        message: nkruntime.MatchMessage,
        state: GameState,
        dispatcher: nkruntime.MatchDispatcher,
        nakama: nkruntime.Nakama,
        logger: nkruntime.Logger
    ) => void
} = {
    7:  ChooseTurnPlayer,
    8:  Rematch,
    10: StickersManager,
    11: UseCard,
};
