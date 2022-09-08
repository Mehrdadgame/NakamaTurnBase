interface MatchLabel
{
    open: boolean
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

  
}

interface Player
{
    presence: nkruntime.Presence
    displayName: string
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

}



interface DrawData
{
    tick: number
}

interface TrophiesData
{
    amount: number
}
