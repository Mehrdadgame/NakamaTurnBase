"use strict";
var JoinOrCreateMatchRpc = "JoinOrCreateMatchRpc";
var LogicLoadedLoggerInfo = "Custom logic loaded.";
var MatchModuleName = "match";
function InitModule(ctx, logger, nk, initializer) {
    initializer.registerRpc(JoinOrCreateMatchRpc, joinOrCreateMatch);
    initializer.registerMatch(MatchModuleName, {
        matchInit: matchInit,
        matchJoinAttempt: matchJoinAttempt,
        matchJoin: matchJoin,
        matchLeave: matchLeave,
        matchLoop: matchLoop,
        matchTerminate: matchTerminate,
        matchSignal: matchSignal
    });
    logger.info(LogicLoadedLoggerInfo);
}
/**
 * If there are any open matches, join the first one. If there are no open matches, create a new one.
 * @param context - The context of the RPC call.
 * @param logger - A logger object that can be used to log messages to the server console.
 * @param nakama - nkruntime.Nakama - The Nakama runtime object.
 * @param {string} payload - string - The payload sent from the client.
 * @returns The match ID.
 */
var gameMode = "";
var joinOrCreateMatch = function (context, logger, nakama, payload) {
    var matches;
    var MatchesLimit = 1;
    var MinimumPlayers = 1;
    var label = { open: true, game_mode: payload };
    var query = "+label.open:true +label.game_mode:" + payload;
    matches = nakama.matchList(MatchesLimit, true, JSON.stringify(label), MinimumPlayers, MaxPlayers);
    if (matches.length > 0) {
        //var s= new ScoreCalss;
        // s.ScoreF=0;
        // SaveScore(context.userId,0,nakama,s);
        return matches[0].matchId;
    }
    // var s= new ScoreCalss;
    // s.ScoreF=0;
    // SaveScore(context.userId,0,nakama,s);
    // nakama.leaderboardRecordWrite(IdLeaderboard,context.userId,context.username,10)
    //CreateLeaderborad(context,logger,nakama);
    var persons = {};
    persons = { "mode": payload };
    return nakama.matchCreate(MatchModuleName, persons);
};
function CreateLeaderborad(context, logger, nakama) {
    var id = IdLeaderboard;
    var authoritative = true;
    var sort = "descending" /* DESCENDING */;
    var operator = "best" /* BEST */;
    var reset = null;
    var metadata = {};
    try {
        nakama.leaderboardCreate(id, authoritative, sort, operator, reset, metadata);
    }
    catch (error) {
        // Handle error
    }
}
var matchInit = function (context, logger, nakama, params) {
    logger.info(gameMode + " gameMode((((((((");
    var value = "";
    for (var key in params) {
        value = params[key];
        logger.info(value + " CCCCCCCCCCCCCCCCCCCCC");
        // Use `key` and `value`
    }
    var label = { open: true, game_mode: value };
    var gameState = {
        players: [],
        playersWins: [],
        roundDeclaredWins: [[]],
        roundDeclaredDraw: [],
        scene: 3 /* Lobby */,
        countdown: DurationLobby * TickRate,
        endMatch: false,
        CountTurnPlayer1: 0,
        CountTurnPlayer2: 0,
        namesForrematch: [],
        BeforeEndGame: false,
        VerticalMode: Checkmode(value)[2],
        array3DPlayerSecend: Checkmode(value)[1],
        array3DPlayerFirst: Checkmode(value)[0],
        ModeText: value
    };
    return {
        state: gameState,
        tickRate: TickRate,
        label: JSON.stringify(label),
    };
};
function Checkmode(value) {
    var arraOne = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    var arraTow = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    var vertical = false;
    if (value == "VerticalAndHorizontal") {
        vertical = true;
        arraOne = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
        arraTow = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    }
    if (value == "FourByThree") {
        arraOne = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
        arraTow = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    }
    if (value == "ThreeByThree") {
        arraOne = [[-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1]];
        arraTow = [[-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1]];
    }
    return [arraOne, arraTow, vertical];
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
var matchJoinAttempt = function (context, logger, nakama, dispatcher, tick, state, presence, metadata) {
    var gameState = state;
    return {
        state: gameState,
        accept: gameState.scene == 3 /* Lobby */,
    };
};
var matchJoin = function (context, logger, nakama, dispatcher, tick, state, presences) {
    var gameState = state;
    if (gameState.scene != 3 /* Lobby */)
        return { state: gameState };
    var presencesOnMatch = [];
    gameState.players.forEach(function (player) { if (player != undefined)
        presencesOnMatch.push(player.presence); });
    for (var _i = 0, presences_1 = presences; _i < presences_1.length; _i++) {
        var presence = presences_1[_i];
        var account = nakama.accountGetId(presence.userId);
        var player = {
            presence: presence,
            displayName: account.user.displayName,
            ScorePlayer: 0
        };
        var nextPlayerNumber = getNextPlayerNumber(gameState.players);
        gameState.players[nextPlayerNumber] = player;
        gameState.playersWins[nextPlayerNumber] = 0;
        dispatcher.broadcastMessage(1 /* PlayerJoined */, JSON.stringify(player), presencesOnMatch);
        presencesOnMatch.push(presence);
    }
    //gameState.startGame =true;
    dispatcher.broadcastMessage(0 /* Players */, JSON.stringify(gameState.players), presences);
    dispatcher.broadcastMessage(6 /* TurnMe */, JSON.stringify(gameState.players[0].presence.userId));
    gameState.countdown = DurationLobby * TickRate;
    return { state: gameState };
};
var matchLoop = function (context, logger, nakama, dispatcher, tick, state, messages) {
    var gameState = state;
    processMessages(messages, gameState, dispatcher, nakama, logger);
    processMatchLoop(gameState, nakama, dispatcher, logger);
    return gameState.endMatch ? null : { state: gameState };
};
var matchLeave = function (context, logger, nakama, dispatcher, tick, state, presences) {
    var gameState = state;
    for (var _i = 0, presences_2 = presences; _i < presences_2.length; _i++) {
        var presence = presences_2[_i];
        var playerNumber = getPlayerNumber(gameState.players, presence.sessionId);
        var nameplayer = JSON.stringify(gameState.players[playerNumber].displayName);
        if (gameState.BeforeEndGame == false) {
            dispatcher.broadcastMessage(9, nameplayer);
        }
        delete gameState.players[playerNumber];
    }
    return { state: gameState };
};
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
var matchTerminate = function (context, logger, nakama, dispatcher, tick, state, graceSeconds) {
    return { state: state };
};
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
var matchSignal = function (context, logger, nk, dispatcher, tick, state, data) {
    return { state: state };
};
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
function processMessages(messages, gameState, dispatcher, nakama, logger) {
    for (var _i = 0, messages_1 = messages; _i < messages_1.length; _i++) {
        var message = messages_1[_i];
        var opCode = message.opCode;
        // if (MessagesLogic.hasOwnProperty(opCode))
        {
            MessagesLogic[opCode](message, gameState, dispatcher, nakama, logger);
        }
        // else
        //     messagesDefaultLogic(message, gameState, dispatcher);
    }
}
function StickersManager(message, gameState, dispatcher, nakama, logger) {
    var data = JSON.parse(nakama.binaryToString(message.data));
    //  data.id = message.sender.userId;
    logger.info(data.id + "  User ID");
    dispatcher.broadcastMessage(10 /* Sticker */, JSON.stringify(data));
}
/* Creating a 3D array. */
/*  */
/**
 * The above function is used to choose the turn of the player.
 * @param message - The message that was sent to the server.
 * @param {GameState} gameState - The current state of the game.
 * @param dispatcher - The match dispatcher.
 * @param nakama - The Nakama server instance.
 * @param logger - A logger object that you can use to log messages to the server console.
 */
function ChooseTurnPlayer(message, gameState, dispatcher, nakama, logger) {
    var dataPlayer = JSON.parse(nakama.binaryToString(message.data));
    var valuMines = 0;
    dataPlayer.MinesScore = false;
    gameState.BeforeEndGame = false;
    if (message.sender.userId == gameState.players[0].presence.userId) {
        dataPlayer.master = true;
        gameState.array3DPlayerFirst[dataPlayer.NumberLine][dataPlayer.NumberRow] = (dataPlayer.NumberTile);
        // let readc=  ReadScore(message.sender.userId,nakama);
        // var score=  CalculatorScore(array3DPlayerFirst,dataPlayer.NumberLine,dataPlayer.NumberTile,logger,readc.ScoreF)[0];
        // dataPlayer.sumRow1[dataPlayer.NumberLine] = CalculatorScore(array3DPlayerFirst,dataPlayer.NumberLine,dataPlayer.NumberTile,logger)[1];
        //  readc.ScoreF = score;
        //  dataPlayer.Score =  readc.ScoreF;
        // gameState.players[0].ScorePlayer =readc.ScoreF;
        // SaveScore(message.sender.userId,0,nakama,readc);
        gameState.CountTurnPlayer1++;
        dataPlayer.Score = TotalScore(gameState.array3DPlayerFirst, logger, gameState.VerticalMode);
        gameState.players[0].ScorePlayer = dataPlayer.Score;
        var resultTile = CalculatorArray2D(gameState.array3DPlayerSecend, dataPlayer.NumberLine, dataPlayer.NumberRow, dataPlayer.NumberTile, logger);
        logger.info(gameState.VerticalMode + " VerticalMode@@@@@@@@@  ");
        var countPow = 0;
        if (gameState.VerticalMode == true) {
            var resultTileVertical = CalculatorArray2DWithVertical(gameState.array3DPlayerSecend, dataPlayer.NumberLine, dataPlayer.NumberRow, dataPlayer.NumberTile, logger);
            for (var index = 0; index < resultTileVertical.length; index++) {
                logger.info(dataPlayer.NumberRow.toString() + resultTileVertical[index] + "  %%%%%%%%%%%%%%%%");
                gameState.array3DPlayerSecend[resultTileVertical[index]][dataPlayer.NumberRow] = (-1);
                countPow++;
            }
            if (countPow > 0) {
                dataPlayer.ScoreOtherPlayer = TotalScore(gameState.array3DPlayerSecend, logger, gameState.VerticalMode);
                //    let read1 =  ReadScore( gameState.players[1].presence.userId,nakama);
                valuMines = dataPlayer.NumberTile + 1;
                var miness = (valuMines * countPow) * countPow;
                dataPlayer.ValueMines = miness;
                //    gameState.players[1].ScorePlayer  = read1.ScoreF;
                //   let resultSave = SaveScore(gameState.players[1].presence.userId, miness ,nakama,read1);
                //   dataPlayer.ScoreOtherPlayer = resultSave;
                dataPlayer.MinesScore = true;
                resultTile = [];
            }
            countPow = 0;
        }
        if (resultTile.length > 0) {
            for (var index = 0; index < resultTile.length; index++) {
                countPow++;
                gameState.array3DPlayerSecend[dataPlayer.NumberLine][resultTile[index]] = -1;
            }
            if (countPow > 0) {
                dataPlayer.ScoreOtherPlayer = TotalScore(gameState.array3DPlayerSecend, logger, gameState.VerticalMode);
                //     let read1 =  ReadScore( gameState.players[1].presence.userId,nakama);
                valuMines = dataPlayer.NumberTile + 1;
                var miness = (valuMines * countPow) * countPow;
                dataPlayer.ValueMines = miness;
                //     gameState.players[1].ScorePlayer  = read1.ScoreF;
                //    let resultSave = SaveScore(gameState.players[1].presence.userId, miness ,nakama,read1);
                //    dataPlayer.ScoreOtherPlayer = resultSave;
                dataPlayer.MinesScore = true;
                resultTile = [];
            }
        }
        dataPlayer.Array2DTilesPlayer = gameState.array3DPlayerFirst;
        dataPlayer.Array2DTilesOtherPlayer = gameState.array3DPlayerSecend;
        logger.info(gameState.players[0].ScorePlayer + "  dataPlayer.CountTurnPlayer1");
        logger.info(gameState.players[1].ScorePlayer + "  dataPlayer.CountTurnPlayer2");
        var checkEnd1 = ActionWinPlayer(gameState.array3DPlayerFirst);
        var checkEnd2 = ActionWinPlayer(gameState.array3DPlayerSecend);
        var end = parseInt(gameState.CountTurnPlayer1) == parseInt(gameState.CountTurnPlayer2);
        logger.info(end + "  dataPlayer.End");
        if (checkEnd1 == true || checkEnd2 == true) {
            if (end == true) {
                if (gameState.players[1].ScorePlayer < gameState.players[0].ScorePlayer) {
                    dataPlayer.PlayerWin = gameState.players[0].presence.userId;
                    //  var readCountWin = ReadScoreLeaderboard( gameState.players[0].presence.userId,nakama);
                    //  readCountWin.win+=1;
                    //  logger.info(readCountWin.win.toString() + "Player0");
                    // SaveScoreLeaderboard( gameState.players[0].presence.userId,nakama,readCountWin);
                    // nakama.leaderboardRecordWrite(IdLeaderboard,dataPlayer.PlayerWin,gameState.players[0].presence.username,readCountWin.win)
                }
                else if (gameState.players[1].ScorePlayer > gameState.players[0].ScorePlayer) {
                    dataPlayer.PlayerWin = gameState.players[1].presence.userId;
                    //  var readCountWin = ReadScoreLeaderboard( gameState.players[1].presence.userId,nakama);
                    // readCountWin.win+=1;
                    // logger.info(readCountWin.win.toString()+"Player1");
                    // SaveScoreLeaderboard( gameState.players[1].presence.userId,nakama,readCountWin);
                    // nakama.leaderboardRecordWrite(IdLeaderboard,dataPlayer.PlayerWin,gameState.players[1].presence.username,readCountWin.win)
                }
                else {
                    dataPlayer.PlayerWin = "";
                }
                dataPlayer.EndGame = true;
                gameState.BeforeEndGame = true;
                // gameState.endMatch =true;
                // gameState.startGame =false;
            }
        }
    }
    else {
        dataPlayer.master = false;
        gameState.CountTurnPlayer2++;
        gameState.array3DPlayerSecend[dataPlayer.NumberLine][dataPlayer.NumberRow] = (dataPlayer.NumberTile);
        logger.info(dataPlayer.NumberLine + " " + dataPlayer.NumberRow);
        // dataPlayer.sumRow2 = [0,0,0];
        dataPlayer.Score = TotalScore(gameState.array3DPlayerSecend, logger, gameState.VerticalMode);
        // let read=   ReadScore(message.sender.userId,nakama);
        //logger.info(read.ScoreF + "read.ScoreF");
        //let score= CalculatorScore(array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberTile,logger,read.ScoreF)[0];
        //dataPlayer.sumRow2[dataPlayer.NumberLine] = CalculatorScore(array3DPlayerSecend,dataPlayer.NumberLine,dataPlayer.NumberTile,logger)[1];
        //read.ScoreF = score;
        // dataPlayer.Score =read.ScoreF;
        gameState.players[1].ScorePlayer = dataPlayer.Score;
        // SaveScore(message.sender.userId,0,nakama,read);
        var resultTile2 = CalculatorArray2D(gameState.array3DPlayerFirst, dataPlayer.NumberLine, dataPlayer.NumberRow, dataPlayer.NumberTile, logger);
        var countPow = 0;
        if (gameState.VerticalMode == true) {
            var resultTileVertical = CalculatorArray2DWithVertical(gameState.array3DPlayerFirst, dataPlayer.NumberLine, dataPlayer.NumberRow, dataPlayer.NumberTile, logger);
            for (var index = 0; index < resultTileVertical.length; index++) {
                logger.info(dataPlayer.NumberRow.toString() + resultTileVertical[index] + "  %%%%%%%%%%%%%%%%");
                gameState.array3DPlayerFirst[resultTileVertical[index]][dataPlayer.NumberRow] = (-1);
                countPow++;
            }
            if (countPow > 0) {
                //let read1 =  ReadScore( gameState.players[0].presence.userId,nakama);
                valuMines = dataPlayer.NumberTile + 1;
                var miness = (valuMines * countPow) * countPow;
                dataPlayer.ValueMines = miness;
                gameState.players[0].ScorePlayer = TotalScore(gameState.array3DPlayerFirst, logger, gameState.VerticalMode);
                //   let resultSave = SaveScore(gameState.players[0].presence.userId, miness ,nakama,read1);
                dataPlayer.ScoreOtherPlayer = gameState.players[0].ScorePlayer;
                dataPlayer.MinesScore = true;
                resultTile = [];
            }
            countPow = 0;
        }
        if (resultTile2.length > 0) {
            for (var index = 0; index < resultTile2.length; index++) {
                countPow++;
                gameState.array3DPlayerFirst[dataPlayer.NumberLine][resultTile2[index]] = -1;
            }
            if (countPow > 0) {
                gameState.players[0].ScorePlayer = TotalScore(gameState.array3DPlayerFirst, logger, gameState.VerticalMode);
                //   let read1 =  ReadScore( gameState.players[0].presence.userId,nakama);
                valuMines = dataPlayer.NumberTile + 1;
                var miness = (valuMines * countPow) * countPow;
                dataPlayer.ValueMines = miness;
                gameState.players[0].ScorePlayer = gameState.players[0].ScorePlayer;
                // let resultSave = SaveScore(gameState.players[0].presence.userId, miness ,nakama,read1);
                dataPlayer.ScoreOtherPlayer = gameState.players[0].ScorePlayer;
                dataPlayer.MinesScore = true;
                resultTile2 = [];
            }
        }
        logger.info(gameState.players[0].ScorePlayer + "  dataPlayer.CountTurnPlayer1");
        logger.info(gameState.players[1].ScorePlayer + "  dataPlayer.CountTurnPlayer2");
        dataPlayer.Array2DTilesPlayer = gameState.array3DPlayerSecend;
        dataPlayer.Array2DTilesOtherPlayer = gameState.array3DPlayerFirst;
        var checkEnd1 = ActionWinPlayer(gameState.array3DPlayerSecend);
        var checkEnd2 = ActionWinPlayer(gameState.array3DPlayerFirst);
        var end = parseInt(gameState.CountTurnPlayer1) === parseInt(gameState.CountTurnPlayer2);
        logger.info(end + "  dataPlayer.End");
        if (checkEnd1 == true || checkEnd2 == true) {
            if (end == true) {
                if (gameState.players[1].ScorePlayer < gameState.players[0].ScorePlayer) {
                    dataPlayer.PlayerWin = gameState.players[0].presence.userId;
                    //   var readCountWin = ReadScoreLeaderboard( gameState.players[0].presence.userId,nakama);
                    //  readCountWin.win+=1;
                    //  logger.info(readCountWin.win.toString()+"Player0");
                    //   SaveScoreLeaderboard( gameState.players[0].presence.userId,nakama,readCountWin);
                    //  nakama.leaderboardRecordWrite(IdLeaderboard,dataPlayer.PlayerWin,gameState.players[0].presence.username,readCountWin.win)
                }
                else if (gameState.players[1].ScorePlayer > gameState.players[0].ScorePlayer) {
                    dataPlayer.PlayerWin = gameState.players[1].presence.userId;
                    //  var readCountWin = ReadScoreLeaderboard( gameState.players[1].presence.userId,nakama);
                    //  readCountWin.win +=1;
                    //  logger.info(readCountWin.win.toString()+"Player1");
                    //  SaveScoreLeaderboard( gameState.players[1].presence.userId,nakama,readCountWin);
                    // nakama.leaderboardRecordWrite(IdLeaderboard,dataPlayer.PlayerWin,gameState.players[1].presence.username, readCountWin.win)
                }
                else {
                    dataPlayer.PlayerWin = "";
                }
                dataPlayer.EndGame = true;
                gameState.BeforeEndGame = true;
                //  gameState.startGame =false;
            }
        }
    }
    var dataSendToClint = JSON.stringify(dataPlayer);
    dispatcher.broadcastMessage(message.opCode, dataSendToClint, null, message.sender);
    dataPlayer.EndGame = false;
}
function TotalScore(array2D, logger, mode) {
    var score = 0;
    for (var index = 0; index < array2D.length; index++) {
        score += CalculatorArray(array2D[index], logger);
    }
    if (mode == true) {
        {
            var _loop_1 = function (indexx) {
                score += CalculatorArray(array2D.map(function (d) { return d[indexx]; }), logger);
            };
            for (var indexx = 0; indexx < array2D.length; indexx++) {
                _loop_1(indexx);
            }
        }
    }
    logger.info(score.toString() + " Score");
    return score;
}
function CalculatorArray(arrayInput, logger) {
    var countInArray = arrayInput.reduce(function (tally, fruit) {
        if (!tally[fruit]) {
            tally[fruit] = 1;
        }
        else {
            tally[fruit] = tally[fruit] + 1;
        }
        return tally;
    }, {});
    logger.info(JSON.stringify(countInArray) + " countInArray");
    var duplicates = Object.keys(countInArray).map(function (k) {
        return {
            key: k,
            count: countInArray[k]
        };
    });
    logger.info(JSON.stringify(duplicates) + " duplicates");
    var sum = 0;
    if (duplicates.length > 0) {
        for (var i = 0; i < duplicates.length; i++) {
            if (duplicates[i].key != "-1") {
                var count = duplicates[i].count;
                var key = Number(duplicates[i].key);
                if (count == 4) {
                    sum = (key + 1) * 16;
                    return sum;
                }
                else if (count == 3) {
                    sum += (key + 1) * 9;
                }
                else if (count == 2) {
                    sum += (key + 1) * 4;
                }
                else
                    sum += (key + 1);
            }
        }
    }
    logger.info(JSON.stringify(sum) + " sum 00");
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
function Rematch(message, gameState, dispatcher, nakama, logger) {
    var dataPlayer = JSON.parse(nakama.binaryToString(message.data));
    //  if(gameState.namesForrematch.some(e=> e!= dataPlayer.userId))
    gameState.namesForrematch.push(dataPlayer.userId);
    if (getPlayersCount(gameState.players) == 1) {
        dataPlayer.Answer = "left";
        var dataSendToClint = JSON.stringify(dataPlayer);
        dispatcher.broadcastMessage(message.opCode, dataSendToClint, null, message.sender);
    }
    if (gameState.namesForrematch.length > 1) {
        if (dataPlayer.Answer == "no") {
            dataPlayer.Answer = "no";
            var dataSendToClint = JSON.stringify(dataPlayer);
            gameState.endMatch = true;
            dispatcher.broadcastMessage(message.opCode, dataSendToClint, null, message.sender);
            return;
        }
        else if (dataPlayer.Answer == "yes" || dataPlayer.Answer == "send") {
            gameState.endMatch = false;
            gameState.BeforeEndGame = false;
            dataPlayer.Answer = "yes";
            var dataSendToClint = JSON.stringify(dataPlayer);
            dispatcher.broadcastMessage(message.opCode, dataSendToClint, null, message.sender);
            dispatcher.broadcastMessage(6 /* TurnMe */, JSON.stringify(gameState.players[0].presence.userId));
            for (var index = 0; index < gameState.array3DPlayerFirst.length; index++) {
                for (var index1 = 0; index1 < gameState.array3DPlayerFirst[index].length; index1++) {
                    gameState.array3DPlayerFirst[index][index1] = -1;
                    gameState.array3DPlayerSecend[index][index1] = -1;
                }
            }
            gameState.CountTurnPlayer1 = 0;
            gameState.CountTurnPlayer2 = 0;
            var s = new ScoreCalss;
            s.ScoreF = 0;
            for (var index = 0; index < gameState.players.length; index++) {
                SaveScore(gameState.players[index].presence.userId, 0, nakama, s);
            }
        }
    }
    if (dataPlayer.Answer == "send") {
        dataPlayer.userId = message.sender.userId;
        dataPlayer.Answer = "req";
        var send = JSON.stringify(dataPlayer);
        dispatcher.broadcastMessage(message.opCode, send, null, message.sender);
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
function SaveScore(id, mines, nakama, Scorecalss) {
    Scorecalss.ScoreF -= mines;
    var storageWriteRequests2 = [{
            collection: CollectionUser,
            key: "Score",
            userId: id,
            value: Scorecalss
        }];
    nakama.storageWrite(storageWriteRequests2);
    return Scorecalss.ScoreF;
}
/**
 * This function is used to save the score of the player in the leaderboard
 * @param {string} id - The user ID of the player.
 * @param {number} mines - the number of mines in the game
 * @param nakama - nkruntime.Nakama
 * @param {number} scoreleaderboard - the score that you want to save
 */
function SaveScoreLeaderboard(id, nakama, scoreleaderboard) {
    var storageWriteRequests2 = [{
            collection: "Rank",
            key: "leaderboard",
            userId: id,
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
function ReadScoreLeaderboard(id, nakama) {
    var score = new CountWin;
    var storagReadRequestsFirst = [{
            collection: "Rank",
            key: "leaderboard",
            userId: id,
        }];
    var resultScore = nakama.storageRead(storagReadRequestsFirst);
    for (var _i = 0, resultScore_1 = resultScore; _i < resultScore_1.length; _i++) {
        var storageObject = resultScore_1[_i];
        score = storageObject.value;
        break;
    }
    return score;
}
/**
 * It reads the score of the user from the database and returns it
 * @param {string} id - The user ID of the player.
 * @param nakama - nkruntime.Nakama
 * @returns ScoreCalss
 */
function ReadScore(id, nakama) {
    var score1 = new ScoreCalss;
    var storagReadRequestsFirst = [{
            collection: CollectionUser,
            key: "Score",
            userId: id,
        }];
    var resultScore = nakama.storageRead(storagReadRequestsFirst);
    for (var _i = 0, resultScore_2 = resultScore; _i < resultScore_2.length; _i++) {
        var storageObject = resultScore_2[_i];
        score1 = storageObject.value;
        break;
    }
    return score1;
}
/**
 * It returns true if the array is full of numbers, and false if it's not.
 * @param {number[][]} array1 - the array that contains the game board
 * @returns A boolean value.
 */
function ActionWinPlayer(array1) {
    var count = 0;
    for (var index = 0; index < array1.length; index++) {
        for (var index1 = 0; index1 < array1[index].length; index1++) {
            if (array1[index][index1] == -1) {
                count++;
            }
        }
    }
    if (count == 0) {
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
function CalculatorArray2D(array1, x, y, input, logger) {
    var arrayResult = [];
    array1[x].forEach(function (element, index) {
        if (element === input) {
            arrayResult.push(index);
        }
    });
    if (arrayResult.length > 0) {
        return arrayResult;
    }
    arrayResult = [];
    return [];
}
function CalculatorArray2DWithVertical(array1, X, y, input, logger) {
    var arrayResult = [];
    var arrayColumn = array1.map(function (x) { return x[y]; });
    logger.warn(JSON.stringify(arrayColumn) + " " + " $$$$");
    arrayColumn.map(function (element, index) {
        if (element === input) {
            logger.info(index.toString() + " " + y);
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
function CalculatorScore(array1, x, input, logger, scoreSaved) {
    if (scoreSaved === void 0) { scoreSaved = null; }
    var countNumber = 0;
    var powScore = 0;
    var i = 0;
    array1[x].forEach(function (element) {
        if (element == input) {
            countNumber++;
        }
    });
    if (countNumber > 1) {
        i = input + 1;
        powScore = (i * countNumber) * countNumber;
        logger.info(powScore + "  logger : count !!!! " + i);
        if (countNumber == 2)
            return [powScore + scoreSaved - (i), powScore];
        if (countNumber == 3) {
            logger.info(powScore + " " + scoreSaved + " " + i);
            return [powScore + scoreSaved - (i * 4), powScore];
        }
    }
    return [scoreSaved + (input + 1), input + 1];
}
function messagesDefaultLogic(message, gameState, dispatcher) {
    dispatcher.broadcastMessage(message.opCode, message.data, null, message.sender);
}
function processMatchLoop(gameState, nakama, dispatcher, logger) {
    switch (gameState.scene) {
        case 4 /* Battle */:
            matchLoopBattle(gameState, nakama, dispatcher);
            break;
        case 3 /* Lobby */:
            matchLoopLobby(gameState, nakama, dispatcher);
            break;
        case 5 /* RoundResults */:
            matchLoopRoundResults(gameState, nakama, dispatcher);
            break;
    }
}
function matchLoopBattle(gameState, nakama, dispatcher) {
    if (gameState.countdown > 0) {
        gameState.countdown--;
        if (gameState.countdown == 0) {
            gameState.roundDeclaredWins = [];
            gameState.roundDeclaredDraw = [];
            gameState.countdown = DurationRoundResults * TickRate;
            gameState.scene = 5 /* RoundResults */;
            dispatcher.broadcastMessage(5 /* ChangeScene */, JSON.stringify(gameState.scene));
        }
    }
}
function matchLoopLobby(gameState, nakama, dispatcher) {
    if (gameState.countdown > 0 && getPlayersCount(gameState.players) > 1) {
        gameState.countdown--;
        if (gameState.countdown == 0) {
            gameState.scene = 4 /* Battle */;
            dispatcher.broadcastMessage(5 /* ChangeScene */, JSON.stringify(gameState.scene));
            dispatcher.matchLabelUpdate(JSON.stringify({ open: false }));
        }
    }
}
function matchLoopRoundResults(gameState, nakama, dispatcher) {
    if (gameState.countdown > 0) {
        gameState.countdown--;
        if (gameState.countdown == 0) {
            var winner = getWinner(gameState.playersWins, gameState.players);
            if (winner != null) {
                var storageReadRequests = [{
                        collection: CollectionUser,
                        key: KeyTrophies,
                        userId: winner.presence.userId
                    }];
                var result = nakama.storageRead(storageReadRequests);
                var trophiesData = { amount: 0 };
                for (var _i = 0, result_1 = result; _i < result_1.length; _i++) {
                    var storageObject = result_1[_i];
                    trophiesData = storageObject.value;
                    break;
                }
                trophiesData.amount++;
                var storageWriteRequests = [{
                        collection: CollectionUser,
                        key: KeyTrophies,
                        userId: winner.presence.userId,
                        value: trophiesData
                    }];
                nakama.storageWrite(storageWriteRequests);
                gameState.endMatch = true;
                gameState.scene = 6 /* FinalResults */;
            }
            else {
                gameState.scene = 4 /* Battle */;
            }
            dispatcher.broadcastMessage(5 /* ChangeScene */, JSON.stringify(gameState.scene));
        }
    }
}
function playerWon(message, gameState, dispatcher, nakama) {
    if (gameState.scene != 4 /* Battle */ || gameState.countdown > 0)
        return;
    var data = JSON.parse(nakama.binaryToString(message.data));
    var tick = data.tick;
    var playerNumber = data.playerNumber;
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
function draw(message, gameState, dispatcher, nakama, logger) {
    if (gameState.scene != 4 /* Battle */ || gameState.countdown > 0)
        return;
    var data = JSON.parse(nakama.binaryToString(message.data));
    var tick = data.tick;
    if (gameState.roundDeclaredDraw[tick] == undefined)
        gameState.roundDeclaredDraw[tick] = 0;
    gameState.roundDeclaredDraw[tick]++;
    if (gameState.roundDeclaredDraw[tick] < getPlayersCount(gameState.players))
        return;
    gameState.countdown = DurationBattleEnding * TickRate;
    dispatcher.broadcastMessage(message.opCode, message.data, null, message.sender);
}
function getPlayersCount(players) {
    var count = 0;
    for (var playerNumber = 0; playerNumber < MaxPlayers; playerNumber++)
        if (players[playerNumber] != undefined)
            count++;
    return count;
}
function playerObtainedNecessaryWins(playersWins) {
    for (var playerNumber = 0; playerNumber < MaxPlayers; playerNumber++)
        if (playersWins[playerNumber] == NecessaryWins)
            return true;
    return false;
}
function getWinner(playersWins, players) {
    for (var playerNumber = 0; playerNumber < MaxPlayers; playerNumber++)
        if (playersWins[playerNumber] == NecessaryWins)
            return players[playerNumber];
    return null;
}
function getPlayerNumber(players, sessionId) {
    for (var playerNumber = 0; playerNumber < MaxPlayers; playerNumber++)
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
function getNextPlayerNumber(players) {
    for (var playerNumber = 0; playerNumber < MaxPlayers; playerNumber++)
        if (!playerNumberIsUsed(players, playerNumber))
            return playerNumber;
    return PlayerNotFound;
}
function playerNumberIsUsed(players, playerNumber) {
    return players[playerNumber] != undefined;
}
var ScoreCalss = /** @class */ (function () {
    function ScoreCalss() {
        this.ScoreF = 0;
    }
    return ScoreCalss;
}());
var CountWin = /** @class */ (function () {
    function CountWin() {
        this.win = 0;
    }
    return CountWin;
}());
var GameMode;
(function (GameMode) {
    GameMode[GameMode["ThreeByThree"] = 0] = "ThreeByThree";
    GameMode[GameMode["FourByThree"] = 1] = "FourByThree";
    GameMode[GameMode["VerticalAndHorizontal"] = 2] = "VerticalAndHorizontal";
})(GameMode || (GameMode = {}));
var TickRate = 16;
var DurationLobby = 10;
var DurationRoundResults = 5;
var DurationBattleEnding = 3;
var NecessaryWins = 3;
var MaxPlayers = 2;
var PlayerNotFound = -1;
var CollectionUser = "User";
var KeyTrophies = "Trophies";
var ScoreFirstPlayer = 0;
var ScoreSecendPlayer = 0;
var IdLeaderboard = "b7c182b36521Win";
var Mode = "ThreeByThree";
var MessagesLogic = {
    7: ChooseTurnPlayer,
    8: Rematch,
    10: StickersManager
};
