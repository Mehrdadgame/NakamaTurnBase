

/**
 * It takes in a bunch of parameters, and returns an object with three properties: state, tickRate, and
 * label
 * @param context - The context of the match.
 * @param logger - A logger object that can be used to log messages to the server console.
 * @param nakama - The Nakama server instance.
 * @param params - { [key: string]: string }
 * @returns The match initialization function returns a MatchInit object.
 */
let matchInit: nkruntime.MatchInitFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, params: { [key: string]: string })
{
  let value="";
  for (let key in params) {
       value = params[key];
    }
   
    var label: MatchLabel = { open: true ,game_mode:value}
   

    
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
         namesForrematch :[],
        BeforeEndGame :false,
        VerticalMode:Checkmode(value)[2],
        array3DPlayerSecend:Checkmode(value)[1],
        array3DPlayerFirst:Checkmode(value)[0],

        ModeText:value,
        ValueHXD :Checkmode(value)[3]

    }

    return {
        state: gameState,
        tickRate: TickRate,
        label: JSON.stringify(label),
    }
}
/**
 * It takes a string as an argument and returns an array of two arrays of arrays of numbers and a
 * boolean
 * @param {string} value - the value of the dropdown
 */
function Checkmode(value:string):[any[][],any[][],boolean,number]{
    let arraOne :any[][] =   [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
    let arraTow :any[][]=  [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
    let valueHXD:number=0;
   let vertical =false;
    if(value == "VerticalAndHorizontal"){
        vertical=true;
        arraOne=    [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
        arraTow= [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
        valueHXD=50;
    }
    if(value =="FourByThree" ){
        arraOne=    [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
        arraTow= [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
        valueHXD = 500;
    }
    if(value == "FourByFour"){
        arraOne=    [[-1,-1,-1,-1],[-1,-1,-1,-1],[-1,-1,-1,-1],[-1,-1,-1,-1]];
        arraTow= [[-1,-1,-1,-1],[-1,-1,-1,-1],[-1,-1,-1,-1],[-1,-1,-1,-1]];
        valueHXD =5000;
    }
    if(value == "ThreeByThree"){
        arraOne=    [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
        arraTow= [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
        valueHXD=50;
    }
    return[arraOne,arraTow,vertical,valueHXD];
    
   
}

/**
 * If the game is in the lobby, accept the player.
 * @param context - The context of the match.
 * @param logger - A logger object that can be used to log messages to the server console.
 * @param nakama - The Nakama server instance.
 * @param dispatcher - nkruntime.MatchDispatcher
 * @param {number} tick - The current tick of the match.
 * @param state - The current state of the match.
 * @param presence - nkruntime.Presence
 * @param metadata - { [key: string]: any }
 */
let matchJoinAttempt: nkruntime.MatchJoinAttemptFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presence: nkruntime.Presence, metadata: { [key: string]: any })
{
    let gameState = state as GameState;
    return {
        state: gameState,
        accept: gameState.scene == Scene.Lobby,
    
    }
}
function SaveAndReadHXD( idUser:string,nakama: nkruntime.Nakama ,decimal:number , mines:number){
    var hxd=new ScoreCalss();
    hxd = ReadHXD(idUser,decimal,nakama);
   hxd.hxdAmount =hxd.hxdAmount - mines ;
    SaveHXD(idUser,0,decimal,nakama,hxd);
}
function SaveAndReadHXDPluse(idUser:string,nakama: nkruntime.Nakama ,decimal:number ,gameState:GameState){
    var hxd= ReadHXD( idUser,0,nakama);
    hxd.hxdAmount += gameState.ValueHXD*2;
    SaveHXD( idUser,0,2,nakama,hxd);
}

/**
 * The match join function is called when a player joins the match
 * @param context - nkruntime.Context
 * @param logger - A logger object that you can use to log messages.
 * @param nakama - nkruntime.Nakama
 * @param dispatcher - nkruntime.MatchDispatcher
 * @param {number} tick - The current tick number.
 * @param state - The current state of the match.
 * @param {nkruntime.Presence[]} presences - nkruntime.Presence[]
 * @returns an object with a state property.
 */
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
            ScorePlayer : 0,
            amuntMony :0
            
        }
      
     
        let nextPlayerNumber: number = getNextPlayerNumber(gameState.players);
        gameState.players[nextPlayerNumber] = player;
      
        gameState.playersWins[nextPlayerNumber] = 0;
        dispatcher.broadcastMessage(OperationCode.PlayerJoined, JSON.stringify(player), presencesOnMatch);
        presencesOnMatch.push(presence);
    }
  
    dispatcher.broadcastMessage(OperationCode.Players, JSON.stringify(gameState.players), presences);
    dispatcher.broadcastMessage(OperationCode.TurnMe,JSON.stringify(presencesOnMatch[0].userId));
    gameState.countdown = DurationLobby * TickRate;
    presencesOnMatch =[];
    return { state: gameState };
}

/**
 * `matchLoop` is called every tick of the match. It processes the messages sent by the clients,
 * processes the game logic, and returns the new game state
 * @param context - The context of the match.
 * @param logger - A logger object that can be used to log messages to the server console.
 * @param nakama - The Nakama server instance.
 * @param dispatcher - This is the object that allows you to send messages to the clients.
 * @param {number} tick - The current tick of the match.
 * @param state - This is the state of the match. It's a JSON object that you can use to store any data
 * you want.
 * @param {nkruntime.MatchMessage[]} messages - An array of messages sent by the client.
 * @returns The game state is being returned.
 */
let matchLoop: nkruntime.MatchLoopFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, messages: nkruntime.MatchMessage[])
{
    let gameState = state as GameState;
    processMessages(messages, gameState, dispatcher, nakama,logger);
    processMatchLoop(gameState, nakama, dispatcher, logger);
    return gameState.endMatch ? null : { state: gameState };
}



/**
 * When a player leaves the match, the game sends a message to all the other players in the match,
 * telling them that the player has left.
 * 
 * Arguments:
 * 
 * * `context`: The context of the match.
 * * `logger`: A logger object that can be used to log messages.
 * * `nakama`: The Nakama server instance.
 * * `dispatcher`: The match dispatcher object.
 * * `tick`: The current tick number.
 * * `state`: The current state of the match.
 * * `presences`: nkruntime.Presence[]
 * 
 * Returns:
 * 
 * The match state is being returned.
 */
let matchLeave: nkruntime.MatchLeaveFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher, tick: number, state: nkruntime.MatchState, presences: nkruntime.Presence[])
{
    let gameState = state as GameState;
    for (let presence of presences)
    {
        let playerNumber: number = getPlayerNumber(gameState.players, presence.sessionId);
        var nameplayer = JSON.stringify(gameState.players[playerNumber].displayName);
        if(   gameState.BeforeEndGame ==false)
        {
            if(gameState.players.length>1){
                
                dispatcher.broadcastMessage(9,nameplayer);
                var playerWin= gameState.players.filter(e=>e.presence.userId!=gameState.players[playerNumber].presence.userId);
                SaveAndReadHXDPluse(playerWin[0].presence.userId,nakama,0,gameState);
                logger.info("Win Player !!!!!!!!!!!!" +playerWin[0].displayName );
            }
        }

        delete gameState.players[playerNumber];
    }
  
    return { state: gameState };
}

/**
 * "Return the current match state."
 * 
 * The match terminate function is called when the match is about to be terminated. This is the last
 * chance to save any data before the match is destroyed
 * @param context - The context of the match.
 * @param logger - A logger object that can be used to log messages to the server console.
 * @param nakama - The Nakama server instance.
 * @param dispatcher - The match dispatcher.
 * @param {number} tick - The current tick of the match.
 * @param state - The current match state.
 * @param {number} graceSeconds - The number of seconds to wait before terminating the match.
 * @returns The state of the match.
 */

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

/**
 * "When a player sends a sticker, broadcast the sticker to all players."
 * 
 * The first line of the function is a TypeScript annotation. It tells the compiler what type of data
 * to expect in each parameter
 * @param message - nkruntime.MatchMessage
 * @param {GameState} gameState - This is the current state of the game.
 * @param dispatcher - This is the dispatcher that you can use to send messages to the client.
 * @param nakama - nkruntime.Nakama - This is the Nakama server instance.
 * @param logger - A logger object that can be used to log messages to the console.
 */
function StickersManager(message: nkruntime.MatchMessage, gameState: GameState, dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama , logger : nkruntime.Logger) : void{

  var data:StickerData = JSON.parse(nakama.binaryToString(message.data));
  
    dispatcher.broadcastMessage(OperationCode.Sticker, JSON.stringify(data));
}
/* Creating a 2D array. */


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
    gameState.BeforeEndGame =false;

    if(message.sender.userId == gameState.players[0].presence.userId)
    {
        dataPlayer.master=true;
        gameState.  array3DPlayerFirst[dataPlayer.NumberLine][dataPlayer.NumberRow] = (dataPlayer.NumberTile);
        gameState.CountTurnPlayer1++;

         dataPlayer.Score = TotalScore(gameState.array3DPlayerFirst,logger,gameState.VerticalMode);
         gameState.players[0].ScorePlayer =  dataPlayer.Score ;
         var resultTile = CalculatorArray2D(gameState.array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberRow,dataPlayer.NumberTile,logger);
         let countPow=0;
if(gameState.VerticalMode == true){
    var resultTileVertical = CalculatorArray2DWithVertical(gameState.array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberRow,dataPlayer.NumberTile,logger);
   
    for (let index = 0; index < resultTileVertical.length; index++) {
       gameState.array3DPlayerSecend[resultTileVertical[index]][dataPlayer.NumberRow] = (-1);
        countPow++;
    }
        if(countPow>0)
        {
            dataPlayer.ScoreOtherPlayer =  TotalScore(gameState.array3DPlayerSecend,logger,gameState.VerticalMode);
            valuMines = dataPlayer.NumberTile+1;
            let miness= (valuMines*countPow)*countPow;
            dataPlayer.ValueMines = miness;
            dataPlayer.MinesScore =true;
            resultTile=[];
      

         }
    countPow=0;
}
 if (resultTile.length>0) {
   
   
    for (let index = 0; index < resultTile.length; index++) {
         countPow++;
         gameState. array3DPlayerSecend[dataPlayer.NumberLine][resultTile[index]]=-1;
    }

    if(countPow>0)
 {
    dataPlayer.ScoreOtherPlayer =  TotalScore(gameState.array3DPlayerSecend,logger,gameState.VerticalMode);
    valuMines = dataPlayer.NumberTile+1;
    let miness= (valuMines*countPow)*countPow;
    dataPlayer.ValueMines = miness;
    dataPlayer.MinesScore =true;
    resultTile=[];
 }
  
 }

    dataPlayer.Array2DTilesPlayer = gameState.array3DPlayerFirst;
    dataPlayer.Array2DTilesOtherPlayer =gameState.array3DPlayerSecend;;

    var checkEnd1 = ActionWinPlayer(gameState.array3DPlayerFirst);
    var checkEnd2 = ActionWinPlayer(gameState.array3DPlayerSecend);
    var end = parseInt (gameState.CountTurnPlayer1) == parseInt( gameState.CountTurnPlayer2);
    if(checkEnd1 == true || checkEnd2 ==true ){
        if(end ==true){

            if (gameState.players[1].ScorePlayer< gameState.players[0].ScorePlayer) {
                dataPlayer.PlayerWin = gameState.players[0].presence.userId;

                var hxd= ReadHXD( dataPlayer.PlayerWin,0,nakama);
                hxd.hxdAmount += gameState.ValueHXD*2;
                SaveHXD( dataPlayer.PlayerWin,0,2,nakama,hxd);
            }
            else if(gameState.players[1].ScorePlayer> gameState.players[0].ScorePlayer){
                dataPlayer.PlayerWin =gameState.players[1].presence.userId;
                var hxd= ReadHXD( dataPlayer.PlayerWin,0,nakama);
                hxd.hxdAmount += gameState.ValueHXD*2;
                SaveHXD(  dataPlayer.PlayerWin,0,2,nakama,hxd);
            }
            else{
                dataPlayer.PlayerWin="";
                for (let index = 0; index <  gameState.players.length; index++) {
                  
                    var hxd= ReadHXD( gameState.players[index].presence.userId,0,nakama);
                    hxd.hxdAmount+= gameState.ValueHXD;
                    SaveHXD( gameState.players[index].presence.userId,0,2,nakama,hxd);
                }
               
            }
            dataPlayer.EndGame =true;
            gameState.BeforeEndGame=true;
        }


      }
  
 }
 else{

    dataPlayer.master=false;
    gameState.CountTurnPlayer2++;
    gameState. array3DPlayerSecend[dataPlayer.NumberLine][dataPlayer.NumberRow] =(dataPlayer.NumberTile);
     logger.info(dataPlayer.NumberLine + " "+ dataPlayer.NumberRow);
    dataPlayer.Score = TotalScore(gameState.array3DPlayerSecend,logger,gameState.VerticalMode);
     gameState.players[1].ScorePlayer = dataPlayer.Score;
     var resultTile2 = CalculatorArray2D(gameState.array3DPlayerFirst,dataPlayer.NumberLine,dataPlayer.NumberRow,dataPlayer.NumberTile, logger);
     let countPow=0;
     if(gameState.VerticalMode == true){
        var resultTileVertical = CalculatorArray2DWithVertical(gameState.array3DPlayerFirst,dataPlayer.NumberLine,dataPlayer.NumberRow,dataPlayer.NumberTile,logger);
       
        for (let index = 0; index < resultTileVertical.length; index++) {
           gameState. array3DPlayerFirst[resultTileVertical[index]][dataPlayer.NumberRow] = (-1);
            countPow++;
        }

            if(countPow>0)
            {
              valuMines = dataPlayer.NumberTile+1;
               let miness= (valuMines*countPow)*countPow;
               dataPlayer.ValueMines = miness;
               gameState.players[0].ScorePlayer  =TotalScore(gameState.array3DPlayerFirst,logger,gameState.VerticalMode);
              dataPlayer.ScoreOtherPlayer =  gameState.players[0].ScorePlayer;
              dataPlayer.MinesScore =true;
              resultTile=[];
            }
        countPow=0;
    }
     if (resultTile2.length>0) {
       
       
        for (let index = 0; index < resultTile2.length; index++) {
             countPow++;
             gameState. array3DPlayerFirst[dataPlayer.NumberLine][resultTile2[index]]=-1;
        }
    
        if(countPow>0)
     {
        gameState.players[0].ScorePlayer  =TotalScore(gameState.array3DPlayerFirst,logger,gameState.VerticalMode);
        valuMines = dataPlayer.NumberTile+1;
        let miness= (valuMines*countPow)*countPow;
        dataPlayer.ValueMines = miness;
        gameState.players[0].ScorePlayer  = gameState.players[0].ScorePlayer;
       dataPlayer.ScoreOtherPlayer =  gameState.players[0].ScorePlayer;
       dataPlayer.MinesScore =true;
       resultTile2=[];
     }
      
     }
     dataPlayer.Array2DTilesPlayer =gameState.array3DPlayerSecend ;
     dataPlayer.Array2DTilesOtherPlayer =gameState.array3DPlayerFirst;
     
       var checkEnd1 = ActionWinPlayer(gameState.array3DPlayerSecend );
       var checkEnd2 = ActionWinPlayer(gameState.array3DPlayerFirst );
        var end=  parseInt( gameState.CountTurnPlayer1) === parseInt( gameState.CountTurnPlayer2);
      if(checkEnd1 == true ||checkEnd2 ==true  )
      {
if(end == true)
{

    if (gameState.players[1].ScorePlayer< gameState.players[0].ScorePlayer) {
        dataPlayer.PlayerWin = gameState.players[0].presence.userId;

        var hxd= ReadHXD( dataPlayer.PlayerWin,0,nakama);
        hxd.hxdAmount += gameState.ValueHXD*2;
        SaveHXD(  dataPlayer.PlayerWin,0,2,nakama,hxd);
    }
    else if(gameState.players[1].ScorePlayer> gameState.players[0].ScorePlayer){
        dataPlayer.PlayerWin =gameState.players[1].presence.userId;
        var hxd= ReadHXD( dataPlayer.PlayerWin,0,nakama);
        hxd.hxdAmount += gameState.ValueHXD*2;
        SaveHXD(  dataPlayer.PlayerWin,0,2,nakama,hxd);
    }
    else{
        dataPlayer.PlayerWin="";
        for (let index = 0; index <  gameState.players.length; index++) {
          
            var hxd= ReadHXD( gameState.players[index].presence.userId,0,nakama);
            hxd.hxdAmount+= gameState.ValueHXD;
            SaveHXD( gameState.players[index].presence.userId,0,2,nakama,hxd);
        }
       
    }
    dataPlayer.EndGame=true;
    gameState.BeforeEndGame=true;
}

    
 }
  
}

    var dataSendToClint = JSON.stringify(dataPlayer);
   dispatcher.broadcastMessage(message.opCode,dataSendToClint,null,message.sender);
 dataPlayer.EndGame=false;
}


/**
 * It takes a 2D array, a logger, and a boolean, and returns a number.
 * @param {number[][]} array2D - The 2D array you want to calculate the score of.
 * @param logger - This is the logger object that you can use to log messages to the console.
 * @param {boolean} mode - boolean - true if you want to calculate the score of the columns as well as
 * the rows.
 * @returns The total score of the array.
 */
function TotalScore(array2D:number[][],logger:nkruntime.Logger,mode:boolean):number {
    let score = 0;
for (let index = 0; index < array2D.length; index++) {
    score += CalculatorArray(array2D[index], logger);
    
}
if(mode==true){
 {
   for (let indexx = 0; indexx < array2D.length; indexx++) {

    score += CalculatorArray(array2D.map(d=>d[indexx]), logger);
   }
    
}
   
}
 
    return score;
}


/**
 * It takes an array of numbers, and returns the sum of the numbers in the array.
 * @param {any[]} arrayInput - any[] - This is the array of numbers that you want to calculate the sum
 * of.
 * @param logger - This is a logger object that you can use to log messages to the console.
 * @returns The sum of the values of the array.
 */
function CalculatorArray( arrayInput: any[] , logger : nkruntime.Logger) :number {
    let countInArray = arrayInput.reduce((tally, fruit) => {
        if (!tally[fruit]) {
            tally[fruit] = 1;
        } else {
            tally[fruit] = tally[fruit] + 1;
        }
        return tally;
    }, {});

    let duplicates = Object.keys(countInArray).map(k => {
        return {
            key: k ,
            count: countInArray[k]
        }
    });
    let sum = 0;
    if (duplicates.length > 0) {
        for (let i = 0; i < duplicates.length; i++) {
            if (duplicates[i].key !="-1") {
                let count = duplicates[i].count
                let key = Number(duplicates[i].key);
                if (count == 4) {
                    sum =( key+1) * 16;
                    return sum;
                } else if (count == 3){
                    sum += (key+1) * 9;
                }
                else if(count==2)
                {
                    sum += (key+1) * 4;
                }else
                    sum +=(key+1);
            }
            }
         
    }
    return sum;
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
    gameState.namesForrematch.push(dataPlayer.userId);
  
   
    if ( getPlayersCount(gameState.players)==1) {
        dataPlayer.Answer ="left"
        var dataSendToClint = JSON.stringify(dataPlayer);
        dispatcher.broadcastMessage(message.opCode,dataSendToClint,null,message.sender);
        
    }
 
    
    if( gameState.namesForrematch.length>1)
    {
        if(dataPlayer.Answer == "no")
        {
            dataPlayer.Answer = "no";
            var dataSendToClint = JSON.stringify(dataPlayer);
            gameState.endMatch =true;
            dispatcher.broadcastMessage(message.opCode,dataSendToClint,null,message.sender);
            return;
        }

     else if(  dataPlayer.Answer == "yes" ||dataPlayer.Answer =="send" ){
        gameState.endMatch =false;
        gameState.BeforeEndGame=false;
         dataPlayer.Answer = "yes";
         var dataSendToClint = JSON.stringify(dataPlayer);
         dispatcher.broadcastMessage(message.opCode,dataSendToClint,null,message.sender);
         dispatcher.broadcastMessage(OperationCode.TurnMe,JSON.stringify(gameState.players[0].presence.userId));
         for (let index = 0; index <gameState. array3DPlayerFirst.length; index++) {
           for (let index1 = 0; index1 < gameState.array3DPlayerFirst[index].length; index1++) {
                gameState.array3DPlayerFirst[index][index1] = -1;
                gameState.array3DPlayerSecend[index][index1]=-1;
           }
             
         }
         for (let index = 0; index < gameState.players.length; index++) {
           SaveAndReadHXD(gameState.players[index].presence.userId,nakama,0,gameState.ValueHXD);
            
         }
      
         gameState.CountTurnPlayer1=0;
         gameState.CountTurnPlayer2=0;
 
    

    }
    }
     if(dataPlayer.Answer =="send"){
       dataPlayer.userId = message.sender.userId;
       dataPlayer.Answer ="req";
       var send = JSON.stringify(dataPlayer);
        dispatcher.broadcastMessage(message.opCode,send,null,message.sender);
    }
    dataPlayer.Answer ="";
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
 function SaveHXD(id:string,mines : number, decimal: number ,nakama:nkruntime.Nakama, Scorecalss:ScoreCalss): number{
    Scorecalss.hxdAmount -= mines;
    Scorecalss._decimal = decimal;
    let storageWriteRequests2: nkruntime.StorageWriteRequest[] = [{
        collection: CollectionUser,
        key: "HXD",
        userId:id,
        value: Scorecalss
      
        
    }];
  
    nakama.storageWrite(storageWriteRequests2);
    return Scorecalss.hxdAmount
    
 }

 
/**
 * It reads the score of the user from the database and returns it
 * @param {string} id - The user ID of the player.
 * @param nakama - nkruntime.Nakama
 * @returns ScoreCalss
 */
function ReadHXD(id:string, decimal:number ,nakama:nkruntime.Nakama ):ScoreCalss{
    var score1:ScoreCalss=new ScoreCalss;
    let storagReadRequestsFirst: nkruntime.StorageReadRequest[] = [{
        collection: CollectionUser,
        key: "HXD",
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
 * This function is used to save the score of the player in the leaderboard
 * @param {string} id - The user ID of the player.
 * @param {number} mines - the number of mines in the game
 * @param nakama - nkruntime.Nakama
 * @param {number} scoreleaderboard - the score that you want to save
 */
 function SaveScoreLeaderboard(id:string,nakama:nkruntime.Nakama, scoreleaderboard:CountWin){
  
    let storageWriteRequests2: nkruntime.StorageWriteRequest[] = [{
        collection: "Rank",
        key: "leaderboard",
        userId:id,
        value: scoreleaderboard
      
        
    }];
    
    nakama.storageWrite(storageWriteRequests2);
    
    
}

 /**
  * Reads the leaderboard score of the user with the given id
  * @param {string} id - The user id of the player you want to read the score of.
  * @param nakama - nkruntime.Nakama
  * @returns The score of the user.
  */
 function ReadScoreLeaderboard(id:string  ,nakama:nkruntime.Nakama ):CountWin{
   var score:CountWin = new CountWin ;
    let storagReadRequestsFirst: nkruntime.StorageReadRequest[] = [{
        collection: "Rank",
        key: "leaderboard",
        userId:id,
        
       }];

       let resultScore: nkruntime.StorageObject[] = nakama.storageRead(storagReadRequestsFirst);
       
       for (let storageObject of resultScore)
       {
        score = <CountWin>storageObject.value;
           break;
       }
      
    

    return score;
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
        if (element === input) 
        {
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
 * It takes an array of arrays, a column number, a row number, and a value, and returns an array of row
 * numbers where the value is found in the column
 * @param {number[][]} array1 - The array to search
 * @param {number} X - The X coordinate of the cell you want to check.
 * @param {number} y - the column number
 * @param {number} input - The value you're looking for in the array.
 * @param logger - This is the logger object that you can use to log messages to the console.
 * @returns an array of numbers.
 */
function CalculatorArray2DWithVertical(array1:number[][],X:number,y:number,input:number , logger : nkruntime.Logger ):number[]
{
    let arrayResult : number[] =[];
    let arrayColumn =  array1.map(x => x[y]);
   
    arrayColumn.map((element, index) => {
        if (element === input) 
        {
            arrayResult.push(index);
        }
   
 });
    

    return arrayResult;

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
    if(countNumber==2)
    return [powScore+scoreSaved-(i), powScore];
    if(countNumber==3){
       
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
            for (let index = 0; index < gameState.players.length; index++)
            {
               SaveAndReadHXD(gameState.players[index].presence.userId,nakama,0,gameState.ValueHXD);
            }
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
     hxdAmount:number=0;
     _decimal:number=0;
}
class CountWin{
    win : number=0;
}

