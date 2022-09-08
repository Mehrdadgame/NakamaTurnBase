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
var joinOrCreateMatch = function (context, logger, nakama, payload) {
    var matches;
    var MatchesLimit = 1;
    var MinimumPlayers = 1;
    var label = { open: true };
    matches = nakama.matchList(MatchesLimit, true, JSON.stringify(label), MinimumPlayers, MaxPlayers);
    if (matches.length > 0) {
        return matches[0].matchId;
    }
    return nakama.matchCreate(MatchModuleName);
};
var matchInit = function (context, logger, nakama, params) {
    var label = { open: true };
    var gameState = {
        players: [],
        playersWins: [],
        roundDeclaredWins: [[]],
        roundDeclaredDraw: [],
        scene: 3 /* Lobby */,
        countdown: DurationLobby * TickRate,
        endMatch: false,
    };
    return {
        state: gameState,
        tickRate: TickRate,
        label: JSON.stringify(label),
    };
};
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
            displayName: account.user.displayName
        };
        var nextPlayerNumber = getNextPlayerNumber(gameState.players);
        gameState.players[nextPlayerNumber] = player;
        gameState.playersWins[nextPlayerNumber] = 0;
        dispatcher.broadcastMessage(1 /* PlayerJoined */, JSON.stringify(player), presencesOnMatch);
        presencesOnMatch.push(presence);
    }
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
        delete gameState.players[playerNumber];
    }
    if (getPlayersCount(gameState.players) == 0)
        return null;
    return { state: gameState };
};
var matchTerminate = function (context, logger, nakama, dispatcher, tick, state, graceSeconds) {
    return { state: state };
};
var matchSignal = function (context, logger, nk, dispatcher, tick, state, data) {
    return { state: state };
};
function processMessages(messages, gameState, dispatcher, nakama, logger) {
    for (var _i = 0, messages_1 = messages; _i < messages_1.length; _i++) {
        var message = messages_1[_i];
        var opCode = message.opCode;
        // if (MessagesLogic.hasOwnProperty(opCode))
        {
            logger.info(message.sender.userId + " TTTTTTTTTTTTTTTTTTTTTTT");
            MessagesLogic[opCode](message, gameState, dispatcher, nakama, logger);
        }
        // else
        //     messagesDefaultLogic(message, gameState, dispatcher);
    }
}
var array3DPlayerFirst = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
var array3DPlayerSecend = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
function ChooseTurnPlayer(message, gameState, dispatcher, nakama, logger) {
    var dataPlayer = JSON.parse(nakama.binaryToString(message.data));
    var scoreC = { ScoreF: 0 };
    var ScoreFirst = 0;
    var scoreSecend = 0;
    if (message.sender.userId == gameState.players[0].presence.userId) {
        array3DPlayerFirst[dataPlayer.NumberLine][dataPlayer.NumberRow] = (dataPlayer.NumberTile);
        logger.info(dataPlayer.NumberLine + " " + dataPlayer.NumberRow);
        var storagReadRequestsFirst = [{
                collection: CollectionUser,
                key: "Score",
                userId: message.sender.userId,
            }];
        var resultScore = nakama.storageRead(storagReadRequestsFirst);
        for (var _i = 0, resultScore_1 = resultScore; _i < resultScore_1.length; _i++) {
            var storageObject = resultScore_1[_i];
            scoreC = storageObject.value;
            break;
        }
        var score = CalculatorScore(array3DPlayerFirst, dataPlayer.NumberLine, dataPlayer.NumberTile, scoreC.ScoreF);
        scoreC.ScoreF = score;
        dataPlayer.Score = scoreC.ScoreF;
        var storageWriteRequests = [{
                collection: CollectionUser,
                key: "Score",
                userId: message.sender.userId,
                value: scoreC
            }];
        ScoreFirst = scoreC.ScoreF;
        nakama.storageWrite(storageWriteRequests);
        logger.info(dataPlayer.Score + " Score");
        var resultTile = CalculatorArray2D(array3DPlayerSecend, dataPlayer.NumberLine, dataPlayer.NumberRow, dataPlayer.NumberTile, logger);
        logger.info(resultTile.length + " %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
        if (resultTile.length > 0) {
            dataPlayer.ResultRow = resultTile;
            for (var index = 0; index < resultTile.length; index++) {
                dataPlayer.ResultLine = dataPlayer.NumberLine;
                logger.info(dataPlayer.ResultLine + " /" + resultTile[index]);
                array3DPlayerSecend[dataPlayer.ResultLine][resultTile[index]] = (-1);
            }
            resultTile = [];
        }
        for (var index = 0; index < 3; index++) {
            for (var index1 = 0; index1 < 3; index1++) {
                logger.info(array3DPlayerFirst[index][index1].toString());
            }
        }
        dataPlayer.EndGame = ActionWinPlayer(array3DPlayerFirst);
        if (dataPlayer.EndGame == true) {
            if (dataPlayer.Score > scoreSecend) {
                dataPlayer.PlayerWin = message.sender.userId;
            }
            else {
                dataPlayer.PlayerWin = gameState.players[1].presence.userId;
            }
            var storageDelete = [{
                    key: "Score",
                    userId: message.sender.userId,
                    collection: CollectionUser
                }];
            nakama.storageDelete(storageDelete);
            var storageDelete1 = [{
                    key: "Score",
                    userId: gameState.players[1].presence.userId,
                    collection: CollectionUser
                }];
            nakama.storageDelete(storageDelete1);
        }
    }
    else {
        array3DPlayerSecend[dataPlayer.NumberLine][dataPlayer.NumberRow] = (dataPlayer.NumberTile);
        logger.info(dataPlayer.NumberLine + " " + dataPlayer.NumberRow);
        var resultTile2 = CalculatorArray2D(array3DPlayerFirst, dataPlayer.NumberLine, dataPlayer.NumberRow, dataPlayer.NumberTile, logger);
        logger.info(resultTile2 + " resultTile2");
        if (resultTile2.length > 0) {
            dataPlayer.ResultRow = resultTile2;
            for (var index = 0; index < resultTile2.length; index++) {
                dataPlayer.ResultLine = dataPlayer.NumberLine;
                logger.info(dataPlayer.ResultLine + " /" + resultTile2[index]);
                array3DPlayerFirst[dataPlayer.ResultLine][resultTile2[index]] = (-1);
            }
            resultTile2 = [];
        }
        var storagReadRequestsFirst = [{
                collection: CollectionUser,
                key: "Score",
                userId: message.sender.userId,
            }];
        scoreSecend = scoreC.ScoreF;
        var resultScore = nakama.storageRead(storagReadRequestsFirst);
        for (var _a = 0, resultScore_2 = resultScore; _a < resultScore_2.length; _a++) {
            var storageObject = resultScore_2[_a];
            scoreC = storageObject.value;
            break;
        }
        var score_1 = CalculatorScore(array3DPlayerSecend, dataPlayer.NumberLine, dataPlayer.NumberTile, scoreC.ScoreF);
        scoreC.ScoreF = score_1;
        dataPlayer.Score = scoreC.ScoreF;
        var storageWriteRequests2 = [{
                collection: CollectionUser,
                key: "Score",
                userId: message.sender.userId,
                value: scoreC
            }];
        nakama.storageWrite(storageWriteRequests2);
        for (var index = 0; index < 3; index++) {
            for (var index1 = 0; index1 < 3; index1++) {
                logger.info(array3DPlayerSecend[index][index1].toString());
            }
        }
        dataPlayer.ResultLine = dataPlayer.NumberLine;
        dataPlayer.EndGame = ActionWinPlayer(array3DPlayerSecend);
        if (dataPlayer.EndGame == true) {
            if (dataPlayer.Score > ScoreFirst) {
                dataPlayer.PlayerWin = message.sender.userId;
            }
            else {
                dataPlayer.PlayerWin = gameState.players[0].presence.userId;
            }
            var storageDelete = [{
                    key: "Score",
                    userId: message.sender.userId,
                    collection: CollectionUser
                }];
            nakama.storageDelete(storageDelete);
            var storageDelete1 = [{
                    key: "Score",
                    userId: gameState.players[0].presence.userId,
                    collection: CollectionUser
                }];
            nakama.storageDelete(storageDelete1);
        }
    }
    var dataSendToClint = JSON.stringify(dataPlayer);
    dispatcher.broadcastMessage(message.opCode, dataSendToClint, null, message.sender);
    // dataPlayer.Score = 0;
    //    if(arrayResult.length>0){
    //        arrayResult.splice(0,arrayResult.length);
    //    }
}
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
function CalculatorArray2D(array1, x, y, input, logger) {
    var arrayResult = [];
    array1[x].forEach(function (element, index) {
        if (element === input) {
            logger.info(index + " " + element + " " + input + " " + x + "FFFFFFFFFFFFFFFFFFFFEEE");
            arrayResult.push(index);
        }
    });
    if (arrayResult.length > 0) {
        return arrayResult;
    }
    arrayResult = [];
    return [];
}
function CalculatorScore(array1, x, input, scoreSaved) {
    var countNumber = 0;
    var powScore = 0;
    array1[x].forEach(function (element) {
        if (element == input) {
            countNumber++;
        }
    });
    if (countNumber > 0) {
        powScore = Math.pow(input + 1, countNumber);
        return powScore + scoreSaved;
    }
    return scoreSaved + (input + 1);
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
var MessagesLogic = {
    7: ChooseTurnPlayer
};
