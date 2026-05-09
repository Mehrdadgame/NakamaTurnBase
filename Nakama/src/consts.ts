const TickRate = 16;
const DurationLobby = 10;
const DurationRoundResults = 5;
const DurationBattleEnding = 3;
const NecessaryWins = 3;
const MaxPlayers = 2;
const PlayerNotFound = -1;
const CollectionUser = "User";
const KeyTrophies = "Trophies";

// ─── Leagues ──────────────────────────────────────────────────────────────────

interface LeagueConfig {
    displayName: string;
    entryFee:     number;
    winnerReward: number;
    drawRefund:   number;  // 50% of entry fee returned on draw
    rankPoints:   number;
}

const LEAGUES: { [mode: string]: LeagueConfig } = {
    "ThreeByThree": {
        displayName:  "SHOWDOWN DICE",
        entryFee:     50,
        winnerReward: 80,
        drawRefund:   25,
        rankPoints:   50,
    },
    "FourByThree": {
        displayName:  "DICEPUNK LEAGUE",
        entryFee:     150,
        winnerReward: 250,
        drawRefund:   75,
        rankPoints:   120,
    },
    "VerticalAndHorizontal": {
        displayName:  "DICE MASTER",
        entryFee:     250,
        winnerReward: 420,
        drawRefund:   125,
        rankPoints:   250,
    },
};

// ─── Leaderboards ─────────────────────────────────────────────────────────────

const LeaderboardWeekly  = "weekly_leaderboard";
const LeaderboardMonthly = "monthly_leaderboard";

// Top-10 reward tables (index 0 = rank 1)
const WEEKLY_REWARDS:  number[] = [1000, 500, 250, 150, 125, 100, 75, 50, 25, 10];
const MONTHLY_REWARDS: number[] = [5000, 2500, 1000, 750, 500, 300, 200, 100, 50, 10];

const CollectionSeason        = "Season";
const CollectionProfile       = "Profile";
const KeyProfileData          = "profile_data";
const FirstLoginBonusCoins    = 2000;

// Avatar catalog — prices validated server-side (client cannot lie about price)
// id must match AvatarLibrary ScriptableObject ids in Unity client
const AVATAR_PRICES: { [id: string]: number } = {
    "avatar_0": 0,
    "avatar_1": 0,
    "avatar_2": 250,
    "avatar_3": 300,
    "avatar_4": 500,
    "avatar_5": 600,
    "avatar_6": 700,
    "avatar_7": 700
    
};
const KeyPendingRewardWeekly  = "pending_weekly";
const KeyPendingRewardMonthly = "pending_monthly";

// ─── App Version (Force Update) ───────────────────────────────────────────────

const CollectionConfig     = "config";
const KeyAppVersion        = "app_version";
// System user ID — used to store global config readable by all authenticated users
const SystemUserId         = "00000000-0000-0000-0000-000000000000";

// ─── Bot ──────────────────────────────────────────────────────────────────────

const BotThinkMinTicks = TickRate * 1;
const BotThinkMaxTicks = TickRate * 3;

const BOT_NAMES: string[] = [
    "Ali_K99", "Daniel_P", "Sara_GG", "xX_Cobra_Xx", "Reza_77",
    "ProGamer88", "IronWolf7", "NightStar", "Champion_K", "Shadow_X9",
    "MasterPlay", "Kamran_Ace", "DarkFire", "QuickShot9", "EliteKing",
    "Arash_Pro", "Ninja_Storm", "CoolBreeze", "FlashPlayer", "TitanFist"
];
const BOT_DIFFICULTIES = [0, 1, 2];

// ─── Message routing ──────────────────────────────────────────────────────────

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
};
