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
CountTurnPlayer1:0,
CountTurnPlayer2:0,
namesForrematch :[]
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

  
    return { state: gameState };
}


let matchTerminate: nkruntime.MatchTerminateFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, graceSeconds: number)
{
    return { state };
}

/**
 * "The match signal function is called when a match signal is received from the server."
 * 
 * The match signal function is called when a match signal is received from the server
 * @param context - The context of the match.
 * @param logger - A logger object that can be used to log messages to the server console.
 * @param nk - The Nakama server instance.
 * @param dispatcher - The match dispatcher.
 * @param {number} tick - The current tick of the match.
 * @param state - The current state of the match.
 * @param {string} data - The data sent from the client.
 * @returns The state of the match.
 */
let matchSignal: nkruntime.MatchSignalFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, data: string)
{
    return { state };
}

/**
 * ProcessMessages(messages: nkruntime.MatchMessage[], gameState: GameState, dispatcher:
 * nkruntime.MatchDispatcher, nakama: nkruntime.Nakama, logger : nkruntime.Logger): void
 * 
 * The above function is called every time a message is sent to the server. 
 * 
 * The messages parameter is an array of messages sent to the server. 
 * 
 * The gameState parameter is the current state of the game. 
 * 
 * The dispatcher parameter is used to send messages to the client. 
 * 
 * The nakama parameter is used to access the Nakama server. 
 * 
 * The logger parameter is used to log messages to the Nakama server. 
 * 
 * The above function is called every time a message is sent to the server. 
 * 
 * The messages parameter is an array of messages sent to the server. 
 * 
 * The gameState parameter is the current state of
 * @param {nkruntime.MatchMessage[]} messages - nkruntime.MatchMessage[]
 * @param {GameState} gameState - This is the state of the game. It's a JSON object that you can use to
 * store any data you want.
 * @param dispatcher - This is the object that you use to send messages to the client.
 * @param nakama - nkruntime.Nakama
 * @param logger - nkruntime.Logger
 */
function processMessages(messages: nkruntime.MatchMessage[], gameState: GameState, dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama, logger : nkruntime.Logger): void
{
    for (let message of messages)
    {
        let opCode: number = message.opCode;
       // if (MessagesLogic.hasOwnProperty(opCode))
       {
            MessagesLogic[opCode](message, gameState, dispatcher, nakama,logger);
    
        }
            
        // else
        //     messagesDefaultLogic(message, gameState, dispatcher);
    }
}
/* Creating a 3D array. */
let array3DPlayerFirst:any[][] = [[null,null,null],[null,null,null],[null,null,null]];
let array3DPlayerSecend:any[][] = [[null,null,null],[null,null,null],[null,null,null]];


/*  */
/**
 * The above function is used to choose the turn of the player.
 * @param message - The message that was sent to the server.
 * @param {GameState} gameState - The current state of the game.
 * @param dispatcher - The match dispatcher.
 * @param nakama - The Nakama server instance.
 * @param logger - A logger object that you can use to log messages to the server console.
 */
function ChooseTurnPlayer(message: nkruntime.MatchMessage, gameState: GameState, dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama , logger : nkruntime.Logger) : void{
    let dataPlayer : DataPlayer = JSON.parse(nakama.binaryToString(message.data));
let valuMines = 0;
    dataPlayer.MinesScore =false;
    if(message.sender.userId == gameState.players[0].presence.userId)
    {
        dataPlayer.master=true;
        array3DPlayerFirst[dataPlayer.NumberLine][dataPlayer.NumberRow] = (dataPlayer.NumberTile);
        let readc=  ReadScore(message.sender.userId,nakama);
        var score=  CalculatorScore(array3DPlayerFirst,dataPlayer.NumberLine,dataPlayer.NumberTile,logger,readc.ScoreF)[0];
        dataPlayer.sumRow1[dataPlayer.NumberLine] = CalculatorScore(array3DPlayerFirst,dataPlayer.NumberLine,dataPlayer.NumberTile,logger)[1];
        readc.ScoreF = score;
        dataPlayer.Score =  readc.ScoreF;
        SaveScore(message.sender.userId,0,nakama,readc);
        gameState.CountTurnPlayer1++;
 
    var resultTile = CalculatorArray2D(array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberRow,dataPlayer.NumberTile,logger);

    if (resultTile.length>0) {
        let coutPow =0;
        dataPlayer.ResultRow = resultTile;
        for (let index = 0; index < resultTile.length; index++) {
            dataPlayer.ResultLine = dataPlayer.NumberLine;
            logger.info(dataPlayer.ResultLine + " /"+ resultTile[index]);
            array3DPlayerSecend[dataPlayer.ResultLine][resultTile[index]] = (null);
            coutPow++;
        }
       let Read= ReadScore(gameState.players[1].presence.userId,nakama);
       logger.info(Read.ScoreF + " read");
       valuMines =dataPlayer.NumberTile+1; 
       let miness = (valuMines*coutPow)*coutPow;
       logger.info(miness + " miness");
       let resultSave= SaveScore(gameState.players[1].presence.userId, miness,nakama,Read);
       dataPlayer.ValueMines = miness;
       logger.info(resultSave + " miness");
       dataPlayer.ScoreOtherPlayer = resultSave;
        resultTile=[];
        dataPlayer.MinesScore =true;
    }
   
    logger.info(gameState.CountTurnPlayer2 + "  dataPlayer.CountTurnPlayer2");
    logger.info(gameState.CountTurnPlayer1 + "  dataPlayer.CountTurnPlayer1");
    var checkEnd1 = ActionWinPlayer(array3DPlayerFirst);
    var checkEnd2 = ActionWinPlayer(array3DPlayerSecend);
var end = parseInt (gameState.CountTurnPlayer1) == parseInt( gameState.CountTurnPlayer2);
logger.info(end + "  dataPlayer.End");
    if(checkEnd1 == true || checkEnd2 ==true ){
        if(end ==true){

            if (gameState.players[1].ScorePlayer< gameState.players[0].ScorePlayer) {
                dataPlayer.PlayerWin = gameState.players[0].presence.userId;
            }
            else if(gameState.players[1].ScorePlayer> gameState.players[0].ScorePlayer){
                dataPlayer.PlayerWin =gameState.players[1].presence.userId;
            }
            else{
                dataPlayer.PlayerWin="";
            }
            dataPlayer.EndGame =true;
        }


      }
  
 }
 else{

    dataPlayer.master=false;
    gameState.CountTurnPlayer2++;
     array3DPlayerSecend[dataPlayer.NumberLine][dataPlayer.NumberRow] =(dataPlayer.NumberTile);
     logger.info(dataPlayer.NumberLine + " "+ dataPlayer.NumberRow);
    // dataPlayer.sumRow2 = [0,0,0];
     let read=   ReadScore(message.sender.userId,nakama);
     logger.info(read.ScoreF + "read.ScoreF");
     let score= CalculatorScore(array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberTile,logger,read.ScoreF)[0];
     dataPlayer.sumRow2[dataPlayer.NumberLine] = CalculatorScore(array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberTile,logger)[1];
     read.ScoreF = score;
     dataPlayer.Score =read.ScoreF;
     SaveScore(message.sender.userId,0,nakama,read);
     var resultTile2 = CalculatorArray2D(array3DPlayerFirst,dataPlayer.NumberLine,dataPlayer.NumberRow,dataPlayer.NumberTile, logger);
    // dataPlayer.sumRow2[dataPlayer.NumberLine] += CalculatorScore(array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberTile,read.ScoreF,logger)[1];
//     var totalRow = array3DPlayerSecend.map(r => r.reduce((a, b) => a + b));

     logger.info(resultTile2  + " resultTile2");
     if (resultTile2.length>0) {
        dataPlayer.ResultRow = resultTile2;
         let countPow=0;
        for (let index = 0; index < resultTile2.length; index++) {
     
             countPow++;
            dataPlayer.ResultLine = dataPlayer.NumberLine;
            array3DPlayerFirst[dataPlayer.ResultLine][ resultTile2[index]] =(null);
        }
        let read1 =  ReadScore( gameState.players[0].presence.userId,nakama);
        valuMines = dataPlayer.NumberTile+1;
        let miness= (valuMines*countPow)*countPow;

        dataPlayer.ValueMines = miness;
        //dataPlayer.sumRow1[dataPlayer.NumberLine] -= miness;
       let resultSave= SaveScore(gameState.players[0].presence.userId, miness ,nakama,read1);
      // dataPlayer.sumRow2[dataPlayer.NumberLine] -= miness;
       dataPlayer.ScoreOtherPlayer = resultSave;
       dataPlayer.MinesScore =true;
        resultTile2=[];
     }
     logger.info(gameState.CountTurnPlayer2 + "  dataPlayer.CountTurnPlayer2");
     logger.info(gameState.CountTurnPlayer1 + "  dataPlayer.CountTurnPlayer1");
     
        dataPlayer.ResultLine = dataPlayer.NumberLine;
       var checkEnd1 = ActionWinPlayer(array3DPlayerSecend );
       var checkEnd2 = ActionWinPlayer(array3DPlayerFirst );
        var end=  parseInt( gameState.CountTurnPlayer1) === parseInt( gameState.CountTurnPlayer2);
        logger.info(end + "  dataPlayer.End");
      if(checkEnd1 == true ||checkEnd2 ==true  )
      {
if(end == true)
{

    if (gameState.players[1].ScorePlayer< gameState.players[0].ScorePlayer) {
        dataPlayer.PlayerWin = gameState.players[0].presence.userId;
    }
    else if(gameState.players[1].ScorePlayer> gameState.players[0].ScorePlayer){
    
        dataPlayer.PlayerWin =  gameState.players[1].presence.userId;
    }
    else{
        dataPlayer.PlayerWin="";
    }
    dataPlayer.EndGame=true;
}

    
      }
  
}



    var dataSendToClint = JSON.stringify(dataPlayer);
    
   dispatcher.broadcastMessage(message.opCode,dataSendToClint,null,message.sender);
 dataPlayer.EndGame=false;
}

/**
 * *|CURSOR_MARCADOR|*
 * @param message - nkruntime.MatchMessage
 * @param {GameState} gameState - This is the state of the game. It's a JSON object that you can store
 * any data in.
 * @param dispatcher - The match dispatcher.
 * @param nakama - nkruntime.Nakama
 * @param logger - A logger object that can be used to log messages to the server console.
 */
function Rematch(message: nkruntime.MatchMessage, gameState: GameState, dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama , logger : nkruntime.Logger) : void{
   
    let dataPlayer : IReMatch = JSON.parse(nakama.binaryToString(message.data));
  //  if(gameState.namesForrematch.some(e=> e!= dataPlayer.userId))
    gameState.namesForrematch.push(dataPlayer.userId);
  
    logger.info( getPlayersCount(gameState.players) + "   count player");
    if ( getPlayersCount(gameState.players)==1) {
        dataPlayer.Answer ="left"
        var dataSendToClint = JSON.stringify(dataPlayer);
        dispatcher.broadcastMessage(message.opCode,dataSendToClint,null,message.sender);
        
    }
    
    if(gameState.namesForrematch.length>1)
    {
        if(dataPlayer.Answer == "no")
        {
            dataPlayer.Answer = "no";
            var dataSendToClint = JSON.stringify(dataPlayer);
            dispatcher.broadcastMessage(message.opCode,dataSendToClint,null,message.sender);
            return;
        }

     else if(  dataPlayer.Answer == "yes"){

         dataPlayer.Answer = "yes";
         var dataSendToClint = JSON.stringify(dataPlayer);
         dispatcher.broadcastMessage(message.opCode,dataSendToClint,null,message.sender);
         dispatcher.broadcastMessage(OperationCode.TurnMe,JSON.stringify(gameState.players[0].presence.userId));
         for (let index = 0; index < array3DPlayerFirst.length; index++) {
           for (let index1 = 0; index1 < array3DPlayerFirst[index].length; index1++) {
             array3DPlayerFirst[index][index1] = null;
             array3DPlayerSecend[index][index1]=null;
           }
             
         }
       
         gameState.CountTurnPlayer1=0;
         gameState.CountTurnPlayer2=0;
 
      var s= new ScoreCalss;
     s.ScoreF=0;
     for (let index = 0; index < gameState.players.length; index++) {
     
         
         SaveScore(gameState.players[index].presence.userId,0,nakama,s);
     }
    }
    }
     if(dataPlayer.Answer =="send"){
       dataPlayer.userId = message.sender.userId;
       dataPlayer.Answer ="req";
       var send = JSON.stringify(dataPlayer);
        dispatcher.broadcastMessage(message.opCode,send,null,message.sender);
    }
  
}


 /**
  * It takes in a string, a number, a Nakama object, and a ScoreCalss object. It subtracts the number
  * from the ScoreCalss object's ScoreF property, then writes the ScoreCalss object to Nakama's
  * storage. It then returns the ScoreCalss object's ScoreF property
  * @param {string} id - The user ID of the player.
  * @param {number} mines - number - The number of mines to add to the player's score.
  * @param nakama - nkruntime.Nakama
  * @param {ScoreCalss} Scorecalss - is a class that contains the score and the name of the player
  * @returns The return value is the value of the ScoreF property of the ScoreCalss object.
  */
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
 /**
  * It reads the score of the user from the database and returns it
  * @param {string} id - The user ID of the player.
  * @param nakama - nkruntime.Nakama
  * @returns ScoreCalss
  */
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

/**
 * It returns true if the array is full of numbers, and false if it's not.
 * @param {number[][]} array1 - the array that contains the game board
 * @returns A boolean value.
 */
function ActionWinPlayer(array1:number[][] ) : boolean {
    let count :number=0;
for (let index = 0; index <array1.length; index++) {
   for (let index1 = 0; index1 < array1[index].length; index1++) {
 if(array1[index][index1] == null){
    count++;
 }
}
}
if(count==0){
  return true;
}

    return false;
}
/**
 * It takes an array of arrays, a number, a number, a number, and a logger, and returns an array of
 * numbers
 * @param {number[][]} array1 - the array you want to search
 * @param {number} x - the row number
 * @param {number} y - the row number
 * @param {number} input - the number you want to find
 * @param logger - nkruntime.Logger
 * @returns An array of numbers
 */
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

/**
 * It takes an array of arrays, an index, an input, a logger, and an optional scoreSaved parameter. It
 * returns an array of two numbers
 * @param {number[][]} array1 - the array of arrays that you want to check
 * @param {number} x - the row number of the array
 * @param {number} input - the number of the player's choice
 * @param logger - nkruntime.Logger
 * @param {any} [scoreSaved=null] - The score of the player before the current turn.
 * @returns an array of two numbers.
 */
function CalculatorScore(array1:number[][],x:number,input:number,logger : nkruntime.Logger ,scoreSaved:any =null):[number ,number]{
    let countNumber:number=0;
    let powScore:number =0;
    var i=0
    array1[x].forEach((element) => {
        if (element == input) {
            countNumber++;
    }
   
 });

 if(countNumber>1){
     i = input+1;
    powScore = (i*countNumber)*countNumber;
    logger.info(powScore + "  logger : count !!!! " + i);
    if(countNumber==2)
    return [powScore+scoreSaved-(i), powScore];
    if(countNumber==3){
       
        logger.info(powScore + " "+ scoreSaved + " "+ i);
        return [powScore+scoreSaved-(i*4), powScore];
    }
 }


return [scoreSaved+(input+1),input+1];

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


/**
 * "Return the first unused player number, or -1 if all player numbers are used."
 * 
 * The function is a bit more complicated than that, but it's still pretty simple
 * @param {Player[]} players - Player[]
 * @returns The next available player number.
 */
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
