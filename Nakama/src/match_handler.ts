let matchInit: nkruntime.MatchInitFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, params: { [key: string]: string })
{
    var label: MatchLabel = { open: true }
    var gameState: GameState =
    {
        players: [],
        playersWins: [],
        roundDeclaredWins: [[]],
        roundDeclaredDraw: [],
        scene: Scene.Lobby,
        countdown: DurationLobby * TickRate,
        endMatch: false,

    }

    return {
        state: gameState,
        tickRate: TickRate,
        label: JSON.stringify(label),
    }
}

let matchJoinAttempt: nkruntime.MatchJoinAttemptFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presence: nkruntime.Presence, metadata: { [key: string]: any })
{
    let gameState = state as GameState;
    return {
        state: gameState,
        accept: gameState.scene == Scene.Lobby,
    }
}

let matchJoin: nkruntime.MatchJoinFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presences: nkruntime.Presence[])
{
    let gameState = state as GameState;
    if (gameState.scene != Scene.Lobby)
        return { state: gameState };

    let presencesOnMatch: nkruntime.Presence[] = [];
    gameState.players.forEach(player => { if (player != undefined) presencesOnMatch.push(player.presence); });
    for (let presence of presences)
    {
        var account: nkruntime.Account = nakama.accountGetId(presence.userId);
        let player: Player =
        {
            presence: presence,
            displayName: account.user.displayName,
            ScorePlayer : 0
        }

        let nextPlayerNumber: number = getNextPlayerNumber(gameState.players);
        gameState.players[nextPlayerNumber] = player;
        gameState.playersWins[nextPlayerNumber] = 0;
        dispatcher.broadcastMessage(OperationCode.PlayerJoined, JSON.stringify(player), presencesOnMatch);
        presencesOnMatch.push(presence);
    }
   
  
    dispatcher.broadcastMessage(OperationCode.Players, JSON.stringify(gameState.players), presences);
    dispatcher.broadcastMessage(OperationCode.TurnMe,JSON.stringify(gameState.players[0].presence.userId));
    gameState.countdown = DurationLobby * TickRate;
    return { state: gameState };
}

let matchLoop: nkruntime.MatchLoopFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, messages: nkruntime.MatchMessage[])
{
    let gameState = state as GameState;
    processMessages(messages, gameState, dispatcher, nakama,logger);
    processMatchLoop(gameState, nakama, dispatcher, logger);
    return gameState.endMatch ? null : { state: gameState };
}

let matchLeave: nkruntime.MatchLeaveFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presences: nkruntime.Presence[])
{
    let gameState = state as GameState;
    for (let presence of presences)
    {
        let playerNumber: number = getPlayerNumber(gameState.players, presence.sessionId);
        delete gameState.players[playerNumber];
    }

    if (getPlayersCount(gameState.players) == 0)
        return null;

    return { state: gameState };
}


let matchTerminate: nkruntime.MatchTerminateFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, graceSeconds: number)
{
    return { state };
}

let matchSignal: nkruntime.MatchSignalFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, data: string)
{
    return { state };
}

function processMessages(messages: nkruntime.MatchMessage[], gameState: GameState, dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama, logger : nkruntime.Logger): void
{
    for (let message of messages)
    {
        let opCode: number = message.opCode;
       // if (MessagesLogic.hasOwnProperty(opCode))
       {
        logger.info(message.sender.userId +" TTTTTTTTTTTTTTTTTTTTTTT");
            MessagesLogic[opCode](message, gameState, dispatcher, nakama,logger);
    
        }
            
        // else
        //     messagesDefaultLogic(message, gameState, dispatcher);
    }
}
let array3DPlayerFirst:number[][] = [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
let array3DPlayerSecend:number[][] = [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];

function ChooseTurnPlayer(message: nkruntime.MatchMessage, gameState: GameState, dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama , logger : nkruntime.Logger) : void{
    let dataPlayer : DataPlayer = JSON.parse(nakama.binaryToString(message.data));

    dataPlayer.MinesScore =false;
    
    if(message.sender.userId == gameState.players[0].presence.userId)
    {
        array3DPlayerFirst[dataPlayer.NumberLine][dataPlayer.NumberRow] = (dataPlayer.NumberTile);

        let readc=  ReadScore(message.sender.userId,nakama);
      
        var score=  CalculatorScore(array3DPlayerFirst,dataPlayer.NumberLine,dataPlayer.NumberTile,readc.ScoreF );
        readc.ScoreF = score;
        dataPlayer.Score =  readc.ScoreF;
      
        SaveScore(message.sender.userId,0,nakama,readc);
    
 
    var resultTile = CalculatorArray2D(array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberRow,dataPlayer.NumberTile,logger);

    if (resultTile.length>0) {
        let coutPow =0;
        dataPlayer.ResultRow = resultTile;
        for (let index = 0; index < resultTile.length; index++) {
            dataPlayer.ResultLine = dataPlayer.NumberLine;
            logger.info(dataPlayer.ResultLine + " /"+ resultTile[index]);
            array3DPlayerSecend[dataPlayer.ResultLine][resultTile[index]] = (-1);
            coutPow++;
        }
       let Read= ReadScore(gameState.players[1].presence.userId,nakama);
       logger.info(Read.ScoreF + " read");
       let miness = Math.pow(dataPlayer.NumberTile+1,coutPow);
       logger.info(miness + " miness");
       let resultSave= SaveScore(gameState.players[1].presence.userId, miness,nakama,Read);
dataPlayer.ValueMines = miness;
       logger.info(resultSave + " miness");
       dataPlayer.ScoreOtherPlayer = resultSave;
        resultTile=[];
        dataPlayer.MinesScore =true;
    }
   
    dataPlayer.EndGame = ActionWinPlayer(array3DPlayerFirst);

    if(dataPlayer.EndGame == true){
        if (gameState.players[1].ScorePlayer< gameState.players[0].ScorePlayer) {
            dataPlayer.PlayerWin = gameState.players[0].presence.userId;
        }
        else if(gameState.players[1].ScorePlayer> gameState.players[0].ScorePlayer){
            dataPlayer.PlayerWin =gameState.players[1].presence.userId;
        }
        else{
           
        }

        let storageDelete: nkruntime.StorageDeleteRequest[]=[{
            key:"Score",
            userId: message.sender.userId,
            collection:CollectionUser
        }];
        nakama.storageDelete(storageDelete);
      
        let storageDelete1: nkruntime.StorageDeleteRequest[]=[{
            key:"Score",
            userId: gameState.players[1].presence.userId,
            collection:CollectionUser
        }];
        nakama.storageDelete(storageDelete1);
      }
  
 }
 else{


     array3DPlayerSecend[dataPlayer.NumberLine][dataPlayer.NumberRow] =(dataPlayer.NumberTile);
     logger.info(dataPlayer.NumberLine + " "+ dataPlayer.NumberRow);

     let read=   ReadScore(message.sender.userId,nakama);
     let score= CalculatorScore(array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberTile,read.ScoreF);
     read.ScoreF = score;
     dataPlayer.Score =read.ScoreF;
     SaveScore(message.sender.userId,0,nakama,read);
     var resultTile2 = CalculatorArray2D(array3DPlayerFirst,dataPlayer.NumberLine,dataPlayer.NumberRow,dataPlayer.NumberTile, logger);
     logger.info(resultTile2  + " resultTile2");
     if (resultTile2.length>0) {
        dataPlayer.ResultRow = resultTile2;
         let countPow=0;
        for (let index = 0; index < resultTile2.length; index++) {
     
             countPow++;
            dataPlayer.ResultLine = dataPlayer.NumberLine;
            array3DPlayerFirst[dataPlayer.ResultLine][ resultTile2[index]] =(-1);
        }
        let read1 =  ReadScore( gameState.players[0].presence.userId,nakama);
        logger.info(read1.ScoreF + " read1");
        let miness= Math.pow(dataPlayer.NumberTile+1,countPow);
        dataPlayer.ValueMines = miness;
        logger.info(miness + " miness");
       let resultSave= SaveScore(gameState.players[0].presence.userId, miness ,nakama,read1);
       dataPlayer.ScoreOtherPlayer = resultSave;
       dataPlayer.MinesScore =true;
        resultTile2=[];
     }
     

     
        dataPlayer.ResultLine = dataPlayer.NumberLine;
        dataPlayer.EndGame = ActionWinPlayer(array3DPlayerSecend );
      if(dataPlayer.EndGame == true){

        if (gameState.players[1].ScorePlayer< gameState.players[0].ScorePlayer) {
            dataPlayer.PlayerWin = gameState.players[0].presence.userId;
        }
        else if(gameState.players[1].ScorePlayer> gameState.players[0].ScorePlayer){

            dataPlayer.PlayerWin =  gameState.players[1].presence.userId;
        }
        else{
        }
        let storageDelete: nkruntime.StorageDeleteRequest[]=[{
            key:"Score",
            userId: message.sender.userId,
            collection:CollectionUser
        }];
        nakama.storageDelete(storageDelete);
        let storageDelete1: nkruntime.StorageDeleteRequest[]=[{
            key:"Score",
            userId: gameState.players[0].presence.userId,
            collection:CollectionUser
        }];

        nakama.storageDelete(storageDelete1);
      }
  
}

    var dataSendToClint = JSON.stringify(dataPlayer);
   dispatcher.broadcastMessage(message.opCode,dataSendToClint,null,message.sender);
 
}

 function SaveScore(id:string,mines:number ,nakama:nkruntime.Nakama, Scorecalss:ScoreCalss): number{
    Scorecalss.ScoreF -= mines;
    let storageWriteRequests2: nkruntime.StorageWriteRequest[] = [{
        collection: CollectionUser,
        key: "Score",
        userId:id,
        value: Scorecalss
    }];
  
    nakama.storageWrite(storageWriteRequests2);
    return Scorecalss.ScoreF
    
 }
 function ReadScore(id:string  ,nakama:nkruntime.Nakama ):ScoreCalss{
    var score1:ScoreCalss=new ScoreCalss;
    let storagReadRequestsFirst: nkruntime.StorageReadRequest[] = [{
        collection: CollectionUser,
        key: "Score",
        userId:id,
        
       }];

       let resultScore: nkruntime.StorageObject[] = nakama.storageRead(storagReadRequestsFirst);
       
       for (let storageObject of resultScore)
       {
        score1 = <ScoreCalss>storageObject.value;
           break;
       }
      
    

    return score1;
 }

function ActionWinPlayer(array1:number[][] ) : boolean {
    let count :number=0;
for (let index = 0; index <array1.length; index++) {
   for (let index1 = 0; index1 < array1[index].length; index1++) {
 if(array1[index][index1] == -1){
    count++;
 }
}
}
if(count==0){
  return true;
}

    return false;
}
function CalculatorArray2D(array1:number[][],x:number,y:number,input:number , logger : nkruntime.Logger):number[]
{
    let arrayResult : number[] =[];
    array1[x].forEach((element, index) => {
        if (element === input) {
        logger.info(index + " "+ element + " "+ input + " "+ x+ "FFFFFFFFFFFFFFFFFFFFEEE");
        arrayResult.push(index);
    }
   
 });

 if(arrayResult.length>0){
    return arrayResult;
 }
 arrayResult=[];
    return [];
}

function CalculatorScore(array1:number[][],x:number,input:number,scoreSaved:number):number{
    let countNumber:number=0;
    let powScore:number =0;
    array1[x].forEach((element) => {
        if (element == input) {
            countNumber++;
    }
   
 });

 if(countNumber>0){
    powScore = Math.pow(input+1,countNumber);
    return powScore+scoreSaved;
 }


return scoreSaved+(input+1);

}

function messagesDefaultLogic(message: nkruntime.MatchMessage, gameState: GameState, dispatcher: nkruntime.MatchDispatcher): void
{
    dispatcher.broadcastMessage(message.opCode, message.data, null, message.sender);
}

function processMatchLoop(gameState: GameState, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, logger: nkruntime.Logger): void
{
    switch (gameState.scene)
    {
        case Scene.Battle: matchLoopBattle(gameState, nakama, dispatcher); break;
        case Scene.Lobby: matchLoopLobby(gameState, nakama, dispatcher); break;
        case Scene.RoundResults: matchLoopRoundResults(gameState, nakama, dispatcher); break;
    }
}

function matchLoopBattle(gameState: GameState, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher): void
{
    if (gameState.countdown > 0)
    {
        gameState.countdown--;
        if (gameState.countdown == 0)
        {
            gameState.roundDeclaredWins = [];
            gameState.roundDeclaredDraw = [];
            gameState.countdown = DurationRoundResults * TickRate;
            gameState.scene = Scene.RoundResults;
            dispatcher.broadcastMessage(OperationCode.ChangeScene, JSON.stringify(gameState.scene));
        }
    }
}

function matchLoopLobby(gameState: GameState, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher): void
{
    if (gameState.countdown > 0 && getPlayersCount(gameState.players) > 1)
    {
        gameState.countdown--;
        if (gameState.countdown == 0)
        {
            gameState.scene = Scene.Battle;
            dispatcher.broadcastMessage(OperationCode.ChangeScene, JSON.stringify(gameState.scene));
            dispatcher.matchLabelUpdate(JSON.stringify({ open: false }));
          
        }
    }
}

function matchLoopRoundResults(gameState: GameState, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher): void
{
    if (gameState.countdown > 0)
    {
        gameState.countdown--;
        if (gameState.countdown == 0)
        {
            var winner = getWinner(gameState.playersWins, gameState.players);
            if (winner != null)
            {
                let storageReadRequests: nkruntime.StorageReadRequest[] = [{
                    collection: CollectionUser,
                    key: KeyTrophies,
                    userId: winner.presence.userId
                }];

                let result: nkruntime.StorageObject[] = nakama.storageRead(storageReadRequests);
                var trophiesData: TrophiesData = { amount: 0 };
                for (let storageObject of result)
                {
                    trophiesData = <TrophiesData>storageObject.value;
                    break;
                }

                trophiesData.amount++;
                let storageWriteRequests: nkruntime.StorageWriteRequest[] = [{
                    collection: CollectionUser,
                    key: KeyTrophies,
                    userId: winner.presence.userId,
                    value: trophiesData
                }];

                nakama.storageWrite(storageWriteRequests);
                gameState.endMatch = true;
                gameState.scene = Scene.FinalResults;
            }
            else
            {
                gameState.scene = Scene.Battle;
            }

            dispatcher.broadcastMessage(OperationCode.ChangeScene, JSON.stringify(gameState.scene));
        }
    }
}

function playerWon(message: nkruntime.MatchMessage, gameState: GameState, dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama): void 
{
    if (gameState.scene != Scene.Battle || gameState.countdown > 0)
        return;

    
    let data: PlayerWonData = JSON.parse(nakama.binaryToString(message.data));
    let tick: number = data.tick;
    let playerNumber: number = data.playerNumber;
    if (gameState.roundDeclaredWins[tick] == undefined)
        gameState.roundDeclaredWins[tick] = [];

    if (gameState.roundDeclaredWins[tick][playerNumber] == undefined)
        gameState.roundDeclaredWins[tick][playerNumber] = 0;

    gameState.roundDeclaredWins[tick][playerNumber]++;
    if (gameState.roundDeclaredWins[tick][playerNumber] < getPlayersCount(gameState.players))
        return;

    gameState.playersWins[playerNumber]++;
    gameState.countdown = DurationBattleEnding * TickRate;
    dispatcher.broadcastMessage(message.opCode, message.data, null, message.sender);
}

function draw(message: nkruntime.MatchMessage, gameState: GameState, dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama ,logger:nkruntime.Logger) : void
{

    
    if (gameState.scene != Scene.Battle || gameState.countdown > 0)
        return;

    let data: DrawData = JSON.parse(nakama.binaryToString(message.data));
    let tick: number = data.tick;
    if (gameState.roundDeclaredDraw[tick] == undefined)
        gameState.roundDeclaredDraw[tick] = 0;

    gameState.roundDeclaredDraw[tick]++;
    if (gameState.roundDeclaredDraw[tick] < getPlayersCount(gameState.players))
        return;

    gameState.countdown = DurationBattleEnding * TickRate;
    dispatcher.broadcastMessage(message.opCode, message.data, null, message.sender);
}

function getPlayersCount(players: Player[]): number
{
    var count: number = 0;
    for (let playerNumber = 0; playerNumber < MaxPlayers; playerNumber++)
        if (players[playerNumber] != undefined)
            count++;

    return count;
}

function playerObtainedNecessaryWins(playersWins: number[]): boolean
{
    for (let playerNumber = 0; playerNumber < MaxPlayers; playerNumber++)
        if (playersWins[playerNumber] == NecessaryWins)
            return true;

    return false;
}

function getWinner(playersWins: number[], players: Player[]): Player | null
{
    for (let playerNumber = 0; playerNumber < MaxPlayers; playerNumber++)
        if (playersWins[playerNumber] == NecessaryWins)
            return players[playerNumber];

    return null;
}

function getPlayerNumber(players: Player[], sessionId: string): number
{
    for (let playerNumber = 0; playerNumber < MaxPlayers; playerNumber++)
        if (players[playerNumber] != undefined && players[playerNumber].presence.sessionId == sessionId)
            return playerNumber;

    return PlayerNotFound;
}


function getNextPlayerNumber(players: Player[]): number
{
    for (let playerNumber = 0; playerNumber < MaxPlayers; playerNumber++)
        if (!playerNumberIsUsed(players, playerNumber))
            return playerNumber;

    return PlayerNotFound;
}

function playerNumberIsUsed(players: Player[], playerNumber: number): boolean
{
    return players[playerNumber] != undefined;
}

class ScoreCalss{
     ScoreF:number=0;
}
