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
var gameMode = "";
var joinOrCreateMatch = function (context, logger, nakama, payload) {
    var label = { open: true, game_mode: payload };
    // Try to find an existing open match for this game mode (with at least 1 player waiting)
    var matches = nakama.matchList(1, true, JSON.stringify(label), 1, MaxPlayers);
    if (matches.length > 0) {
        return matches[0].matchId;
    }
    // No open match found — create a new one
    var persons = { mode: payload };
    return nakama.matchCreate(MatchModuleName, persons);
};
function CreateLeaderboard(context, logger, nakama) {
    try {
        nakama.leaderboardCreate(IdLeaderboard, true, "descending" /* DESCENDING */, "best" /* BEST */, null, {});
    }
    catch (error) {
        // Already exists — ignore
    }
}
// ─── Match Lifecycle ──────────────────────────────────────────────────────────
var matchInit = function (context, logger, nakama, params) {
    var value = "";
    for (var key in params) {
        value = params[key];
    }
    var label = { open: true, game_mode: value };
    var _a = buildGrids(value), arrayFirst = _a[0], arraySecond = _a[1], vertical = _a[2];
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
        VerticalMode: vertical,
        array3DPlayerFirst: arrayFirst,
        array3DPlayerSecend: arraySecond,
        ModeText: value,
        // Bot state
        hasBot: false,
        botDifficulty: 0,
        botNeedsToMove: false,
        botThinkTick: 0,
    };
    return {
        state: gameState,
        tickRate: TickRate,
        label: JSON.stringify(label),
    };
};
function buildGrids(mode) {
    var rowFirst = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    var rowSecond = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    var vertical = false;
    if (mode === "VerticalAndHorizontal") {
        vertical = true;
        rowFirst = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
        rowSecond = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    }
    else if (mode === "FourByThree") {
        rowFirst = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
        rowSecond = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    }
    else if (mode === "ThreeByThree") {
        rowFirst = [[-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1]];
        rowSecond = [[-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1]];
    }
    return [rowFirst, rowSecond, vertical];
}
var matchJoinAttempt = function (context, logger, nakama, dispatcher, tick, state, presence, metadata) {
    var gameState = state;
    return {
        state: gameState,
        accept: gameState.scene === 3 /* Lobby */,
    };
};
var matchJoin = function (context, logger, nakama, dispatcher, tick, state, presences) {
    var gameState = state;
    if (gameState.scene !== 3 /* Lobby */)
        return { state: gameState };
    // Collect existing presences to notify new joiners about
    var existingPresences = [];
    gameState.players.forEach(function (player) {
        if (player && !player.isBot)
            existingPresences.push(player.presence);
    });
    for (var _i = 0, presences_1 = presences; _i < presences_1.length; _i++) {
        var presence = presences_1[_i];
        var account = nakama.accountGetId(presence.userId);
        var player = {
            presence: presence,
            displayName: account.user.displayName,
            ScorePlayer: 0,
        };
        var slot = getNextPlayerNumber(gameState.players);
        gameState.players[slot] = player;
        gameState.playersWins[slot] = 0;
        // Notify existing players that someone joined
        dispatcher.broadcastMessage(1 /* PlayerJoined */, JSON.stringify(player), existingPresences);
        existingPresences.push(presence);
    }
    // Send full player list and turn info to all new joiners
    dispatcher.broadcastMessage(0 /* Players */, JSON.stringify(gameState.players), presences);
    if (gameState.players[0]) {
        dispatcher.broadcastMessage(6 /* TurnMe */, JSON.stringify(gameState.players[0].presence.userId));
    }
    // Reset countdown so the 10-second window starts fresh on each join
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
        if (playerNumber === PlayerNotFound)
            continue;
        var name_1 = JSON.stringify(gameState.players[playerNumber].displayName);
        if (!gameState.BeforeEndGame) {
            dispatcher.broadcastMessage(9, name_1);
        }
        delete gameState.players[playerNumber];
    }
    return { state: gameState };
};
var matchTerminate = function (context, logger, nakama, dispatcher, tick, state, graceSeconds) {
    return { state: state };
};
var matchSignal = function (context, logger, nk, dispatcher, tick, state, data) {
    return { state: state };
};
// ─── Message Routing ──────────────────────────────────────────────────────────
function processMessages(messages, gameState, dispatcher, nakama, logger) {
    for (var _i = 0, messages_1 = messages; _i < messages_1.length; _i++) {
        var message = messages_1[_i];
        if (MessagesLogic.hasOwnProperty(message.opCode)) {
            MessagesLogic[message.opCode](message, gameState, dispatcher, nakama, logger);
        }
    }
}
// ─── Match Loop Phases ────────────────────────────────────────────────────────
function processMatchLoop(gameState, nakama, dispatcher, logger) {
    switch (gameState.scene) {
        case 3 /* Lobby */:
            matchLoopLobby(gameState, nakama, dispatcher, logger);
            break;
        case 4 /* Battle */:
            matchLoopBattle(gameState, nakama, dispatcher, logger);
            break;
        case 5 /* RoundResults */:
            matchLoopRoundResults(gameState, nakama, dispatcher);
            break;
    }
}
function matchLoopLobby(gameState, nakama, dispatcher, logger) {
    var playerCount = getPlayersCount(gameState.players);
    // Nothing to do until at least one real player is in the match
    if (playerCount === 0)
        return;
    if (gameState.countdown <= 0)
        return;
    gameState.countdown--;
    if (gameState.countdown > 0)
        return;
    // Countdown reached zero
    if (playerCount >= 2) {
        // Two real players — start normally
        startBattle(gameState, dispatcher);
    }
    else {
        // Only one player — add a bot and start
        addBotAndStartBattle(gameState, dispatcher, logger);
    }
}
function startBattle(gameState, dispatcher) {
    gameState.scene = 4 /* Battle */;
    dispatcher.broadcastMessage(5 /* ChangeScene */, JSON.stringify(gameState.scene));
    dispatcher.matchLabelUpdate(JSON.stringify({ open: false }));
}
function addBotAndStartBattle(gameState, dispatcher, logger) {
    // Pick a random human-looking name
    var nameIndex = Math.floor(Math.random() * BOT_NAMES.length);
    var botName = BOT_NAMES[nameIndex];
    // Pick random difficulty
    var diffIndex = Math.floor(Math.random() * BOT_DIFFICULTIES.length);
    var difficulty = BOT_DIFFICULTIES[diffIndex];
    // Build a fake presence (bot never actually connects via socket)
    var botUserId = "bot_" + generateId();
    var botSessionId = "bot_" + generateId();
    var botPresence = {
        userId: botUserId,
        sessionId: botSessionId,
        username: botName,
        node: "server",
        status: "",
    };
    var botPlayer = {
        presence: botPresence,
        displayName: botName,
        ScorePlayer: 0,
        isBot: true,
    };
    gameState.players[1] = botPlayer;
    gameState.playersWins[1] = 0;
    gameState.hasBot = true;
    gameState.botDifficulty = difficulty;
    logger.info("Bot added: name=" + botName + " difficulty=" + difficulty);
    // Broadcast updated player list so the real player sees the bot as opponent
    dispatcher.broadcastMessage(0 /* Players */, JSON.stringify(gameState.players));
    // Real player (index 0) goes first
    dispatcher.broadcastMessage(6 /* TurnMe */, JSON.stringify(gameState.players[0].presence.userId));
    startBattle(gameState, dispatcher);
}
/** Simple random ID for bot session/userId */
function generateId() {
    return Math.random().toString(36).substring(2, 10);
}
function matchLoopBattle(gameState, nakama, dispatcher, logger) {
    // Handle ongoing battle countdown (used for round/ending transitions)
    if (gameState.countdown > 0) {
        gameState.countdown--;
        if (gameState.countdown === 0) {
            gameState.roundDeclaredWins = [];
            gameState.roundDeclaredDraw = [];
            gameState.countdown = DurationRoundResults * TickRate;
            gameState.scene = 5 /* RoundResults */;
            dispatcher.broadcastMessage(5 /* ChangeScene */, JSON.stringify(gameState.scene));
        }
        return;
    }
    // Bot thinking timer
    if (gameState.hasBot && gameState.botNeedsToMove) {
        if (gameState.botThinkTick > 0) {
            gameState.botThinkTick--;
        }
        else {
            executeBotTurn(gameState, dispatcher, logger);
        }
    }
}
function matchLoopRoundResults(gameState, nakama, dispatcher) {
    if (gameState.countdown <= 0)
        return;
    gameState.countdown--;
    if (gameState.countdown > 0)
        return;
    var winner = getWinner(gameState.playersWins, gameState.players);
    if (winner !== null) {
        // Trophy only goes to real players
        if (!winner.isBot) {
            var storageRead = [{
                    collection: CollectionUser,
                    key: KeyTrophies,
                    userId: winner.presence.userId,
                }];
            var result = nakama.storageRead(storageRead);
            var trophiesData = { amount: 0 };
            for (var _i = 0, result_1 = result; _i < result_1.length; _i++) {
                var obj = result_1[_i];
                trophiesData = obj.value;
                break;
            }
            trophiesData.amount++;
            nakama.storageWrite([{
                    collection: CollectionUser,
                    key: KeyTrophies,
                    userId: winner.presence.userId,
                    value: trophiesData,
                }]);
        }
        gameState.endMatch = true;
        gameState.scene = 6 /* FinalResults */;
    }
    else {
        gameState.scene = 4 /* Battle */;
    }
    dispatcher.broadcastMessage(5 /* ChangeScene */, JSON.stringify(gameState.scene));
}
// ─── Turn Handlers ────────────────────────────────────────────────────────────
function ChooseTurnPlayer(message, gameState, dispatcher, nakama, logger) {
    var dataPlayer = JSON.parse(nakama.binaryToString(message.data));
    dataPlayer.MinesScore = false;
    gameState.BeforeEndGame = false;
    var isPlayer0 = message.sender.userId === gameState.players[0].presence.userId;
    if (isPlayer0) {
        processTurn(dataPlayer, gameState.array3DPlayerFirst, gameState.array3DPlayerSecend, gameState, 0, true, logger);
    }
    else {
        processTurn(dataPlayer, gameState.array3DPlayerSecend, gameState.array3DPlayerFirst, gameState, 1, false, logger);
    }
    var wasEndGame = dataPlayer.EndGame;
    var dataSend = JSON.stringify(dataPlayer);
    if (wasEndGame && gameState.hasBot) {
        // In bot game, when EndGame occurs on player0's turn the sender would normally not
        // receive the result — send to everyone so player0 sees the endgame screen.
        dispatcher.broadcastMessage(message.opCode, dataSend);
    }
    else {
        // Normal case: send result to everyone EXCEPT the sender
        dispatcher.broadcastMessage(message.opCode, dataSend, null, message.sender);
    }
    dataPlayer.EndGame = false;
    // If playing against bot and the real player just moved without ending the game
    // → schedule bot response
    if (gameState.hasBot && isPlayer0 && !wasEndGame) {
        gameState.botNeedsToMove = true;
        gameState.botThinkTick = BotThinkMinTicks +
            Math.floor(Math.random() * (BotThinkMaxTicks - BotThinkMinTicks));
    }
}
/**
 * Shared turn logic for both real players and bot.
 * moverGrid = grid of the player making the move
 * targetGrid = grid of the opponent
 */
function processTurn(dataPlayer, moverGrid, targetGrid, gameState, moverIndex, isMaster, logger) {
    var line = dataPlayer.NumberLine, row = dataPlayer.NumberRow, tile = dataPlayer.NumberTile;
    dataPlayer.master = isMaster;
    dataPlayer.MinesScore = false;
    dataPlayer.ValueMines = 0;
    // Place tile
    moverGrid[line][row] = tile;
    if (moverIndex === 0) {
        gameState.CountTurnPlayer1++;
    }
    else {
        gameState.CountTurnPlayer2++;
    }
    // Calculate mover's total score
    dataPlayer.Score = TotalScore(moverGrid, logger, gameState.VerticalMode);
    gameState.players[moverIndex].ScorePlayer = dataPlayer.Score;
    // Check for mine triggers on the target's grid
    var valuMines = 0;
    var mineCount = 0;
    if (gameState.VerticalMode) {
        var verticalHits = CalculatorArray2DWithVertical(targetGrid, line, row, tile, logger);
        for (var _i = 0, verticalHits_1 = verticalHits; _i < verticalHits_1.length; _i++) {
            var hitRow = verticalHits_1[_i];
            targetGrid[hitRow][row] = -1;
            mineCount++;
        }
        if (mineCount > 0) {
            valuMines = tile + 1;
            dataPlayer.ValueMines = (valuMines * mineCount) * mineCount;
            var opponentIndex = 1 - moverIndex;
            gameState.players[opponentIndex].ScorePlayer = TotalScore(targetGrid, logger, gameState.VerticalMode);
            dataPlayer.ScoreOtherPlayer = gameState.players[opponentIndex].ScorePlayer;
            dataPlayer.MinesScore = true;
        }
        mineCount = 0;
    }
    if (!dataPlayer.MinesScore) {
        var horizontalHits = CalculatorArray2D(targetGrid, line, row, tile, logger);
        for (var _a = 0, horizontalHits_1 = horizontalHits; _a < horizontalHits_1.length; _a++) {
            var hitCol = horizontalHits_1[_a];
            targetGrid[line][hitCol] = -1;
            mineCount++;
        }
        if (mineCount > 0) {
            valuMines = tile + 1;
            dataPlayer.ValueMines = (valuMines * mineCount) * mineCount;
            var opponentIndex = 1 - moverIndex;
            gameState.players[opponentIndex].ScorePlayer = TotalScore(targetGrid, logger, gameState.VerticalMode);
            dataPlayer.ScoreOtherPlayer = gameState.players[opponentIndex].ScorePlayer;
            dataPlayer.MinesScore = true;
        }
    }
    dataPlayer.Array2DTilesPlayer = moverGrid;
    dataPlayer.Array2DTilesOtherPlayer = targetGrid;
    // Check end-game condition
    var moverGridFull = ActionWinPlayer(moverGrid);
    var targetGridFull = ActionWinPlayer(targetGrid);
    var turnsEqual = parseInt(gameState.CountTurnPlayer1) === parseInt(gameState.CountTurnPlayer2);
    if ((moverGridFull || targetGridFull) && turnsEqual) {
        var score0 = gameState.players[0].ScorePlayer;
        var score1 = gameState.players[1].ScorePlayer;
        if (score0 > score1) {
            dataPlayer.PlayerWin = gameState.players[0].presence.userId;
        }
        else if (score1 > score0) {
            dataPlayer.PlayerWin = gameState.players[1].presence.userId;
        }
        else {
            dataPlayer.PlayerWin = "";
        }
        dataPlayer.EndGame = true;
        gameState.BeforeEndGame = true;
    }
}
// ─── Bot AI ───────────────────────────────────────────────────────────────────
/**
 * Called from matchLoopBattle when botThinkTick reaches 0.
 * Generates the bot's move, processes it, and sends the result to the real player.
 */
function executeBotTurn(gameState, dispatcher, logger) {
    gameState.botNeedsToMove = false;
    var realPlayer = gameState.players[0];
    var botPlayer = gameState.players[1];
    if (!realPlayer || !botPlayer)
        return;
    var move = generateBotMove(gameState, logger);
    if (!move) {
        logger.warn("Bot could not find a valid move");
        return;
    }
    var dataPlayer = {
        UserId: botPlayer.presence.userId,
        Score: 0,
        NumberTile: move.tile,
        NameTile: move.tile.toString(),
        NumberLine: move.line,
        NumberRow: move.col,
        EndGame: false,
        PlayerWin: "",
        ScoreOtherPlayer: 0,
        MinesScore: false,
        ValueMines: 0,
        sumRow1: [],
        sumRow2: [],
        master: false,
        Array2DTilesPlayer: [],
        Array2DTilesOtherPlayer: [],
    };
    processTurn(dataPlayer, gameState.array3DPlayerSecend, gameState.array3DPlayerFirst, gameState, 1, false, logger);
    // Send bot's turn result to the real player only
    dispatcher.broadcastMessage(7 /* ChosseTurn */, JSON.stringify(dataPlayer), [realPlayer.presence], null);
}
/**
 * Generate a move for the bot based on difficulty:
 *   0 (Easy)   → fully random
 *   1 (Normal) → 50% random, 50% strategic
 *   2 (Hard)   → always picks the best scored move
 */
function generateBotMove(gameState, logger) {
    var botGrid = gameState.array3DPlayerSecend;
    var playerGrid = gameState.array3DPlayerFirst;
    var numRows = botGrid.length;
    var numCols = botGrid[0].length;
    var maxTile = numCols - 1;
    var difficulty = gameState.botDifficulty;
    // Collect all empty cells in the bot's grid
    var emptyCells = [];
    for (var i = 0; i < numRows; i++) {
        for (var j = 0; j < numCols; j++) {
            if (botGrid[i][j] === -1) {
                emptyCells.push({ line: i, col: j });
            }
        }
    }
    if (emptyCells.length === 0)
        return null;
    // Easy: fully random
    if (difficulty === 0) {
        var cell = emptyCells[Math.floor(Math.random() * emptyCells.length)];
        var tile = Math.floor(Math.random() * (maxTile + 1));
        return { line: cell.line, col: cell.col, tile: tile };
    }
    // Normal: 50% random, 50% strategic
    if (difficulty === 1 && Math.random() < 0.5) {
        var cell = emptyCells[Math.floor(Math.random() * emptyCells.length)];
        var tile = Math.floor(Math.random() * (maxTile + 1));
        return { line: cell.line, col: cell.col, tile: tile };
    }
    // Hard (or Normal strategic half): pick move with highest combined score
    var bestScore = -1;
    var bestMove = {
        line: emptyCells[0].line,
        col: emptyCells[0].col,
        tile: Math.floor(Math.random() * (maxTile + 1)),
    };
    for (var _i = 0, emptyCells_1 = emptyCells; _i < emptyCells_1.length; _i++) {
        var cell = emptyCells_1[_i];
        var _loop_1 = function (tile) {
            // Simulate mover's score after placing this tile
            var tempBot = botGrid.map(function (r) { return r.slice(); });
            tempBot[cell.line][cell.col] = tile;
            var ownScore = simulateTotalScore(tempBot, gameState.VerticalMode);
            // Count how many opponent tiles this tile would destroy
            var mineHits = 0;
            // Horizontal mines
            var playerRow = playerGrid[cell.line];
            mineHits += playerRow.filter(function (v) { return v === tile; }).length;
            // Vertical mines (only in VerticalAndHorizontal mode)
            if (gameState.VerticalMode) {
                for (var r = 0; r < numRows; r++) {
                    if (playerGrid[r][cell.col] === tile)
                        mineHits++;
                }
            }
            // Combined score: own improvement + weighted mine damage
            var moveScore = ownScore + mineHits * 6;
            if (moveScore > bestScore) {
                bestScore = moveScore;
                bestMove = { line: cell.line, col: cell.col, tile: tile };
            }
        };
        for (var tile = 0; tile <= maxTile; tile++) {
            _loop_1(tile);
        }
    }
    return bestMove;
}
/**
 * Score simulation without a logger (used only for bot AI evaluation).
 */
function simulateTotalScore(grid, verticalMode) {
    var score = 0;
    for (var i = 0; i < grid.length; i++) {
        score += scoreRow(grid[i]);
    }
    if (verticalMode) {
        var _loop_2 = function (col) {
            var column = grid.map(function (r) { return r[col]; });
            score += scoreRow(column);
        };
        for (var col = 0; col < grid[0].length; col++) {
            _loop_2(col);
        }
    }
    return score;
}
function scoreRow(arr) {
    var counts = {};
    for (var _i = 0, arr_1 = arr; _i < arr_1.length; _i++) {
        var v = arr_1[_i];
        if (v === -1)
            continue;
        counts[v] = (counts[v] || 0) + 1;
    }
    var sum = 0;
    for (var _a = 0, _b = Object.keys(counts); _a < _b.length; _a++) {
        var k = _b[_a];
        var key = Number(k);
        var count = counts[k];
        if (count === 4)
            return (key + 1) * 16;
        else if (count === 3)
            sum += (key + 1) * 9;
        else if (count === 2)
            sum += (key + 1) * 4;
        else
            sum += (key + 1);
    }
    return sum;
}
// ─── Sticker & Rematch ────────────────────────────────────────────────────────
function StickersManager(message, gameState, dispatcher, nakama, logger) {
    var data = JSON.parse(nakama.binaryToString(message.data));
    dispatcher.broadcastMessage(10 /* Sticker */, JSON.stringify(data));
}
function Rematch(message, gameState, dispatcher, nakama, logger) {
    var dataPlayer = JSON.parse(nakama.binaryToString(message.data));
    // In a bot game the bot never sends a rematch request, so auto-confirm immediately
    if (gameState.hasBot) {
        if (dataPlayer.Answer === "no") {
            gameState.endMatch = true;
            dispatcher.broadcastMessage(message.opCode, JSON.stringify(dataPlayer), null, message.sender);
            return;
        }
        if (dataPlayer.Answer === "send" || dataPlayer.Answer === "yes") {
            resetGameForRematch(gameState, nakama);
            dataPlayer.Answer = "yes";
            dispatcher.broadcastMessage(message.opCode, JSON.stringify(dataPlayer), null, message.sender);
            dispatcher.broadcastMessage(6 /* TurnMe */, JSON.stringify(gameState.players[0].presence.userId));
        }
        return;
    }
    // Normal 2-player rematch flow
    gameState.namesForrematch.push(dataPlayer.userId);
    if (getPlayersCount(gameState.players) === 1) {
        dataPlayer.Answer = "left";
        dispatcher.broadcastMessage(message.opCode, JSON.stringify(dataPlayer), null, message.sender);
        return;
    }
    if (gameState.namesForrematch.length > 1) {
        if (dataPlayer.Answer === "no") {
            gameState.endMatch = true;
            dispatcher.broadcastMessage(message.opCode, JSON.stringify(dataPlayer), null, message.sender);
            return;
        }
        if (dataPlayer.Answer === "yes" || dataPlayer.Answer === "send") {
            resetGameForRematch(gameState, nakama);
            dataPlayer.Answer = "yes";
            dispatcher.broadcastMessage(message.opCode, JSON.stringify(dataPlayer), null, message.sender);
            dispatcher.broadcastMessage(6 /* TurnMe */, JSON.stringify(gameState.players[0].presence.userId));
        }
    }
    if (dataPlayer.Answer === "send") {
        dataPlayer.userId = message.sender.userId;
        dataPlayer.Answer = "req";
        dispatcher.broadcastMessage(message.opCode, JSON.stringify(dataPlayer), null, message.sender);
    }
}
function resetGameForRematch(gameState, nakama) {
    gameState.endMatch = false;
    gameState.BeforeEndGame = false;
    gameState.botNeedsToMove = false;
    gameState.CountTurnPlayer1 = 0;
    gameState.CountTurnPlayer2 = 0;
    gameState.namesForrematch = [];
    for (var i = 0; i < gameState.array3DPlayerFirst.length; i++) {
        for (var j = 0; j < gameState.array3DPlayerFirst[i].length; j++) {
            gameState.array3DPlayerFirst[i][j] = -1;
            gameState.array3DPlayerSecend[i][j] = -1;
        }
    }
    var s = new ScoreClass();
    for (var _i = 0, _a = gameState.players; _i < _a.length; _i++) {
        var player = _a[_i];
        if (player && !player.isBot) {
            SaveScore(player.presence.userId, 0, nakama, s);
        }
    }
}
// ─── Score / Grid Helpers ─────────────────────────────────────────────────────
function TotalScore(array2D, logger, mode) {
    var score = 0;
    for (var i = 0; i < array2D.length; i++) {
        score += CalculatorArray(array2D[i], logger);
    }
    if (mode) {
        var _loop_3 = function (col) {
            score += CalculatorArray(array2D.map(function (d) { return d[col]; }), logger);
        };
        for (var col = 0; col < array2D[0].length; col++) {
            _loop_3(col);
        }
    }
    return score;
}
function CalculatorArray(arrayInput, logger) {
    var counts = {};
    for (var _i = 0, arrayInput_1 = arrayInput; _i < arrayInput_1.length; _i++) {
        var v = arrayInput_1[_i];
        if (v === -1)
            continue;
        var key = String(v);
        counts[key] = (counts[key] || 0) + 1;
    }
    var sum = 0;
    for (var _a = 0, _b = Object.keys(counts); _a < _b.length; _a++) {
        var k = _b[_a];
        var key = Number(k);
        var count = counts[k];
        if (count === 4)
            return (key + 1) * 16;
        else if (count === 3)
            sum += (key + 1) * 9;
        else if (count === 2)
            sum += (key + 1) * 4;
        else
            sum += (key + 1);
    }
    return sum;
}
function CalculatorArray2D(array1, x, y, input, logger) {
    var result = [];
    array1[x].forEach(function (element, index) {
        if (element === input)
            result.push(index);
    });
    return result;
}
function CalculatorArray2DWithVertical(array1, x, y, input, logger) {
    var result = [];
    var column = array1.map(function (r) { return r[y]; });
    column.forEach(function (element, index) {
        if (element === input)
            result.push(index);
    });
    return result;
}
function ActionWinPlayer(array1) {
    for (var i = 0; i < array1.length; i++) {
        for (var j = 0; j < array1[i].length; j++) {
            if (array1[i][j] === -1)
                return false;
        }
    }
    return true;
}
// ─── Player / Storage Utilities ───────────────────────────────────────────────
function getPlayersCount(players) {
    var count = 0;
    for (var i = 0; i < MaxPlayers; i++) {
        if (players[i] !== undefined)
            count++;
    }
    return count;
}
function getNextPlayerNumber(players) {
    for (var i = 0; i < MaxPlayers; i++) {
        if (!players[i])
            return i;
    }
    return PlayerNotFound;
}
function getPlayerNumber(players, sessionId) {
    for (var i = 0; i < MaxPlayers; i++) {
        if (players[i] && players[i].presence.sessionId === sessionId)
            return i;
    }
    return PlayerNotFound;
}
function playerNumberIsUsed(players, playerNumber) {
    return players[playerNumber] !== undefined;
}
function getWinner(playersWins, players) {
    for (var i = 0; i < MaxPlayers; i++) {
        if (playersWins[i] === NecessaryWins)
            return players[i];
    }
    return null;
}
function playerWon(message, gameState, dispatcher, nakama) {
    if (gameState.scene !== 4 /* Battle */ || gameState.countdown > 0)
        return;
    var data = JSON.parse(nakama.binaryToString(message.data));
    var tick = data.tick, playerNumber = data.playerNumber;
    if (!gameState.roundDeclaredWins[tick])
        gameState.roundDeclaredWins[tick] = [];
    if (!gameState.roundDeclaredWins[tick][playerNumber])
        gameState.roundDeclaredWins[tick][playerNumber] = 0;
    gameState.roundDeclaredWins[tick][playerNumber]++;
    if (gameState.roundDeclaredWins[tick][playerNumber] < getPlayersCount(gameState.players))
        return;
    gameState.playersWins[playerNumber]++;
    gameState.countdown = DurationBattleEnding * TickRate;
    dispatcher.broadcastMessage(message.opCode, message.data, null, message.sender);
}
function draw(message, gameState, dispatcher, nakama, logger) {
    if (gameState.scene !== 4 /* Battle */ || gameState.countdown > 0)
        return;
    var data = JSON.parse(nakama.binaryToString(message.data));
    var tick = data.tick;
    if (!gameState.roundDeclaredDraw[tick])
        gameState.roundDeclaredDraw[tick] = 0;
    gameState.roundDeclaredDraw[tick]++;
    if (gameState.roundDeclaredDraw[tick] < getPlayersCount(gameState.players))
        return;
    gameState.countdown = DurationBattleEnding * TickRate;
    dispatcher.broadcastMessage(message.opCode, message.data, null, message.sender);
}
// ─── Storage ──────────────────────────────────────────────────────────────────
function SaveScore(id, mines, nakama, scoreObj) {
    scoreObj.ScoreF -= mines;
    nakama.storageWrite([{
            collection: CollectionUser,
            key: "Score",
            userId: id,
            value: scoreObj,
        }]);
    return scoreObj.ScoreF;
}
function ReadScore(id, nakama) {
    var result = nakama.storageRead([{ collection: CollectionUser, key: "Score", userId: id }]);
    for (var _i = 0, result_2 = result; _i < result_2.length; _i++) {
        var obj = result_2[_i];
        return obj.value;
    }
    return new ScoreClass();
}
function SaveScoreLeaderboard(id, nakama, scoreLeaderboard) {
    nakama.storageWrite([{
            collection: "Rank",
            key: "leaderboard",
            userId: id,
            value: scoreLeaderboard,
        }]);
}
function ReadScoreLeaderboard(id, nakama) {
    var result = nakama.storageRead([{ collection: "Rank", key: "leaderboard", userId: id }]);
    for (var _i = 0, result_3 = result; _i < result_3.length; _i++) {
        var obj = result_3[_i];
        return obj.value;
    }
    return new CountWin();
}
// ─── Data Classes ─────────────────────────────────────────────────────────────
var ScoreClass = /** @class */ (function () {
    function ScoreClass() {
        this.ScoreF = 0;
    }
    return ScoreClass;
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
var IdLeaderboard = "b7c182b36521Win";
// Bot configuration
var BotThinkMinTicks = TickRate * 1; // min 1 second thinking delay
var BotThinkMaxTicks = TickRate * 3; // max 3 seconds thinking delay
var BOT_NAMES = [
    "Ali_K99", "Daniel_P", "Sara_GG", "xX_Cobra_Xx", "Reza_77",
    "ProGamer88", "IronWolf7", "NightStar", "Champion_K", "Shadow_X9",
    "MasterPlay", "Kamran_Ace", "DarkFire", "QuickShot9", "EliteKing",
    "Arash_Pro", "Ninja_Storm", "CoolBreeze", "FlashPlayer", "TitanFist"
];
// 0=Easy, 1=Normal, 2=Hard
var BOT_DIFFICULTIES = [0, 1, 2];
var MessagesLogic = {
    7: ChooseTurnPlayer,
    8: Rematch,
    10: StickersManager,
};
