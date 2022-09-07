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
            displayName: account.user.displayName
        }

        let nextPlayerNumber: number = getNextPlayerNumber(gameState.players);
        gameState.players[nextPlayerNumber] = player;
        gameState.playersWins[nextPlayerNumber] = 0;
        dispatcher.broadcastMessage(OperationCode.PlayerJoined, JSON.stringify(player), presencesOnMatch);
        presencesOnMatch.push(presence);
    }
   
  

    dispatcher.broadcastMessage(OperationCode.Players, JSON.stringify(gameState.players), presences);
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
    let NumberLine:number = dataPlayer.NumberLine;
    let NumberRow:number = dataPlayer.NumberRow;
    var scoreC :ScoreCalss = {ScoreF :0 } ;
   
    logger.info(" StorageReadRequest " +scoreC .ScoreF);
    
    
    
    array3DPlayerFirst[NumberLine][NumberRow] = (dataPlayer.NumberTile);
    
    
    var score=  CalculatorScore(array3DPlayerFirst,dataPlayer.NumberLine,dataPlayer.NumberTile,scoreC.ScoreF );
    scoreC.ScoreF = score;
    if(message.sender.userId == gameState.players[0].presence.userId){
        let storagReadRequestsFirst: nkruntime.StorageReadRequest[] = [{
            collection: CollectionUser,
            key: "Score",
            userId: message.sender.userId,
    
        }];
    
        let resultScore: nkruntime.StorageObject[] = nakama.storageRead(storagReadRequestsFirst);
    
        for (let storageObject of resultScore)
        {
            scoreC = <ScoreCalss>storageObject.value;
            break;
        }
    let storageWriteRequests: nkruntime.StorageWriteRequest[] = [{
        collection: CollectionUser,
        key: "Score",
        userId: message.sender.userId,
        value: scoreC
    }];
    nakama.storageWrite(storageWriteRequests);
    logger.info(score + " Score");
    
 
    var resultTile = CalculatorArray2D(array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberRow,dataPlayer.NumberTile,logger);
    logger.info(resultTile.length + " %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
    for (let index = 0; index <3; index++) {
      for (let index1 = 0; index1 < 3; index1++) {
       logger.info(array3DPlayerFirst[index][index1].toString());
        
      }
        
    }
    if (resultTile.length>0) {
        dataPlayer.ResultRow = resultTile;
        for (let index = 0; index < resultTile.length; index++) {
     
          
            dataPlayer.ResultLine = dataPlayer.NumberLine;
            array3DPlayerSecend[dataPlayer.ResultLine][ dataPlayer.ResultRow[ resultTile[index]]] =-1;
        }
        resultTile=[];
    }
   
 
 }
 else{


     array3DPlayerSecend[dataPlayer.NumberLine][dataPlayer.NumberRow] =(dataPlayer.NumberTile);
     var resultTile2 = CalculatorArray2D(array3DPlayerFirst,dataPlayer.NumberLine,dataPlayer.NumberRow,dataPlayer.NumberTile, logger);
     
     logger.info(scoreC.ScoreF + " ScoreOPP");
     let score= CalculatorScore(array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberTile,  scoreC.ScoreF);
     scoreC.ScoreF = score;
     let storagReadRequestsFirst: nkruntime.StorageReadRequest[] = [{
         collection: CollectionUser,
         key: "Score",
         userId: message.sender.userId,
 
     }];
 
     let resultScore: nkruntime.StorageObject[] = nakama.storageRead(storagReadRequestsFirst);
 
     for (let storageObject of resultScore)
     {
         scoreC = <ScoreCalss>storageObject.value;
         break;
     }

    let storageWriteRequests2: nkruntime.StorageWriteRequest[] = [{
        collection: CollectionUser,
        key: "Score",
        userId: message.sender.userId,
        value: scoreC
    }];
    nakama.storageWrite(storageWriteRequests2);
  
    for (let index = 0; index <3; index++) {
        for (let index1 = 0; index1 < 3; index1++) {
         logger.info(array3DPlayerSecend[index][index1].toString());
          
        }
          
      }
      if(resultTile2.length>0){
        dataPlayer.ResultRow =resultTile2;
        dataPlayer.ResultLine = dataPlayer.NumberLine;
        for (let index = 0; index < resultTile2.length; index++) {
            array3DPlayerFirst[dataPlayer.NumberLine][resultTile2[index]] = -1;
          }
          resultTile2=[];
      }
    

   
}
dataPlayer.Score = scoreC.ScoreF;
    var dataSendToClint = JSON.stringify(dataPlayer);

    
   dispatcher.broadcastMessage(message.opCode,dataSendToClint,null,message.sender);
   dataPlayer.Score = 0;

//    if(arrayResult.length>0){

//        arrayResult.splice(0,arrayResult.length);
//    }
}
function CalculatorArray2D(array1:number[][],x:number,y:number,input:number , logger : nkruntime.Logger):number[]
{
    let arrayResult : number[] =[];
    array1[x].forEach((element, index) => {
        if (element == input) {
        logger.info(index + " "+ element + " "+ input + " "+ x);
        arrayResult.push(index);
    }
   
 });

 if(arrayResult.length>0){
    return arrayResult;
 }

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
            dispatcher.broadcastMessage(OperationCode.TurnMe,JSON.stringify(gameState.players[0].presence.userId));
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
    console.log("fffffffffffffffffffffffffffffffffffffff");
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
    logger.info("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

    
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
