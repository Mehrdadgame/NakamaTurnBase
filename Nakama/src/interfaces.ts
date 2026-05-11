interface MatchLabel {
    open: boolean;
    game_mode: string;
}

enum GameMode {
    ThreeByThree,
    FourByThree,
    VerticalAndHorizontal,
}

interface GameState {
    players: Player[];
    playersWins: number[];
    roundDeclaredWins: number[][];
    roundDeclaredDraw: number[];
    scene: Scene;
    countdown: number;
    endMatch: boolean;
    CountTurnPlayer1: any;
    CountTurnPlayer2: any;
    namesForrematch: string[];
    BeforeEndGame: boolean;
    VerticalMode: boolean;
    array3DPlayerFirst: any[][];
    array3DPlayerSecend: any[][];
    ModeText: string;
    // Bot
    hasBot: boolean;
    botDifficulty: number;
    botNeedsToMove: boolean;
    botThinkTick: number;
    // Tutorial
    isTutorial: boolean;
    tutorialBotMoveIndex: number;
}

interface Player {
    presence: nkruntime.Presence;
    displayName: string;
    avatarId: string;
    ScorePlayer: number;
    isBot?: boolean;
}

interface TimeRemainingData { time: number; }

interface PlayerWonData {
    tick: number;
    playerNumber: number;
}

interface DataPlayer {
    UserId: string;
    Score: number;
    NumberTile: number;
    NameTile: string;
    NumberLine: number;
    NumberRow: number;
    EndGame: boolean;
    PlayerWin: string;
    ScoreOtherPlayer: number;
    MinesScore: boolean;
    ValueMines: number;
    sumRow1: number[];
    sumRow2: number[];
    master: boolean;
    Array2DTilesPlayer: number[][];
    Array2DTilesOtherPlayer: number[][];
}

interface IReMatch {
    userId: string;
    Answer: string;
}

interface StickerData {
    id: string;
    nameSticker: string;
}

interface DrawData { tick: number; }

interface TrophiesData { amount: number; }

interface PendingRewardData {
    rank:    number;
    reward:  number;
    type:    string;  // "weekly" | "monthly"
    claimed: boolean;
}

interface WalletSnapshot {
    coins?: number;
}

interface ProfileData {
    email:             string;
    phone:             string;
    emailLocked:       boolean;
    phoneLocked:       boolean;
    emailBonusClaimed: boolean;
    phoneBonusClaimed: boolean;
    avatarId:          string;    // selected avatar id, e.g. "avatar_0"
    ownedAvatars:      string[];
    welcomeBonusClaimed: boolean;
}

interface AppVersionConfig {
    requiredVersion: string;  // e.g. "1.2.0"
    updateUrl:       string;  // store/download link
}

interface UpdateProfileResult {
    displayName:  string;
    email:        string;
    phone:        string;
    emailLocked:  boolean;
    phoneLocked:  boolean;
    coinsAwarded: number;
    error:        string;
}
