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
interface DataSend{
    userId : string
    numberTile :number
    nameTile:string
    numberLine:number
    numberRow : number
    resulyLine:string
    resultRow:string
}

interface DrawData
{
    tick: number
}

interface TrophiesData
{
    amount: number
}
