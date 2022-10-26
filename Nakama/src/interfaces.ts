interface MatchLabel
{
    open: boolean,
    game_mode:string
}
enum GameMode{
    ThreeByThree ,
    FourByThree ,
    VerticalAndHorizontal ,
}

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
  BeforeEndGame:boolean,
  VerticalMode:boolean,
  array3DPlayerFirst:any[][],
  array3DPlayerSecend:any[][],
  ModeText:string
  
}

interface Player
{
    presence: nkruntime.Presence
    displayName: string
     ScorePlayer:number
     amuntMony:number
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
/* An interface. */
interface DataPlayer{
    UserId : string
    Score:number
    NumberTile :number
    NameTile:string
    NumberLine:number
    NumberRow : number
    EndGame:boolean
    PlayerWin :string
    ScoreOtherPlayer:number
    MinesScore:boolean
    ValueMines:number
    sumRow1: number[]
    sumRow2:number[] 
    master:boolean
    Array2DTilesPlayer:number[][]
    Array2DTilesOtherPlayer:number[][]


}

interface IReMatch{
    userId:string,
    Answer:string
}
interface StickerData{
    id:string,
    nameSticker:string
}




interface DrawData
{
    tick: number
}

interface TrophiesData
{
    amount: number
}

