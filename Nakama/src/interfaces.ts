interface MatchLabel
{
    open: boolean
}

/* Defining the type of the variable `GameState` */
interface GameState
{
    players: Player[]
    playersWins: number[]
    roundDeclaredWins: number[][]
    roundDeclaredDraw: number[]
    scene: Scene
    countdown: number
    endMatch: boolean
    CountTurnPlayer1:any
    CountTurnPlayer2:any
  namesForrematch:string[]
}

interface Player
{
    presence: nkruntime.Presence
    displayName: string
    ScorePlayer:number
}

interface TimeRemainingData
{
    time: number
}

interface PlayerWonData
{
    tick: number
    playerNumber: number
}
/* Defining a type. */
interface DataPlayer{
    UserId : string
    Score:number
    NumberTile :number
    NameTile:string
    NumberLine:number
    NumberRow : number
    ResultLine:number
    ResultRow:number[]
    EndGame:boolean
    PlayerWin :string
    ScoreOtherPlayer:number
    MinesScore:boolean
    ValueMines:number
    sumRow1: number[]
    sumRow2:number[] 
    master:boolean


}

interface IReMatch{
    userId:string,
    Answer:string
}




interface DrawData
{
    tick: number
}

interface TrophiesData
{
    amount: number
}
