// ─── Match Lifecycle ──────────────────────────────────────────────────────────

let matchInit: nkruntime.MatchInitFunction = function (
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama,
    params: { [key: string]: string }
) {
    let value = "";
    for (let key in params) {
        value = params[key];
    }

    const label: MatchLabel = { open: true, game_mode: value };
    const [arrayFirst, arraySecond, vertical] = buildGrids(value);

    const gameState: GameState = {
        players: [],
        playersWins: [],
        roundDeclaredWins: [[]],
        roundDeclaredDraw: [],
        scene: Scene.Lobby,
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
        // Card/shield state
        shields: [0, 0],
        counterActive: [false, false],
    };

    return {
        state: gameState,
        tickRate: TickRate,
        label: JSON.stringify(label),
    };
};

function buildGrids(mode: string): [any[][], any[][], boolean] {
    let rowFirst: any[][] = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    let rowSecond: any[][] = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    let vertical = false;

    if (mode === "VerticalAndHorizontal") {
        vertical = true;
        rowFirst  = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
        rowSecond = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    } else if (mode === "FourByThree") {
        rowFirst  = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
        rowSecond = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    } else if (mode === "ThreeByThree") {
        rowFirst  = [[-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1]];
        rowSecond = [[-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1], [-1, -1, -1, -1]];
    }
    return [rowFirst, rowSecond, vertical];
}

let matchJoinAttempt: nkruntime.MatchJoinAttemptFunction = function (
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher,
    tick: number,
    state: nkruntime.MatchState,
    presence: nkruntime.Presence,
    metadata: { [key: string]: any }
) {
    const gameState = state as GameState;
    return {
        state: gameState,
        accept: gameState.scene === Scene.Lobby,
    };
};

let matchJoin: nkruntime.MatchJoinFunction = function (
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher,
    tick: number,
    state: nkruntime.MatchState,
    presences: nkruntime.Presence[]
) {
    const gameState = state as GameState;
    if (gameState.scene !== Scene.Lobby) return { state: gameState };

    const existingPresences: nkruntime.Presence[] = [];
    gameState.players.forEach(player => {
        if (player && !player.isBot) existingPresences.push(player.presence);
    });

    for (const presence of presences) {
        const account: nkruntime.Account = nakama.accountGetId(presence.userId);
        const player: Player = {
            presence: presence,
            displayName: account.user.displayName,
            ScorePlayer: 0,
        };
        const slot = getNextPlayerNumber(gameState.players);
        gameState.players[slot] = player;
        gameState.playersWins[slot] = 0;

        dispatcher.broadcastMessage(OperationCode.PlayerJoined, JSON.stringify(player), existingPresences);
        existingPresences.push(presence);
    }

    dispatcher.broadcastMessage(OperationCode.Players, JSON.stringify(gameState.players), presences);
    if (gameState.players[0]) {
        dispatcher.broadcastMessage(OperationCode.TurnMe, JSON.stringify(gameState.players[0].presence.userId));
    }

    gameState.countdown = DurationLobby * TickRate;
    return { state: gameState };
};

let matchLoop: nkruntime.MatchLoopFunction = function (
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher,
    tick: number,
    state: nkruntime.MatchState,
    messages: nkruntime.MatchMessage[]
) {
    const gameState = state as GameState;
    processMessages(messages, gameState, dispatcher, nakama, logger);
    processMatchLoop(gameState, nakama, dispatcher, logger);
    return gameState.endMatch ? null : { state: gameState };
};

let matchLeave: nkruntime.MatchLeaveFunction = function (
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher,
    tick: number,
    state: nkruntime.MatchState,
    presences: nkruntime.Presence[]
) {
    const gameState = state as GameState;
    for (const presence of presences) {
        const playerNumber = getPlayerNumber(gameState.players, presence.sessionId);
        if (playerNumber === PlayerNotFound) continue;

        const name = JSON.stringify(gameState.players[playerNumber].displayName);
        if (!gameState.BeforeEndGame) {
            dispatcher.broadcastMessage(9, name);
        }
        delete gameState.players[playerNumber];
    }
    return { state: gameState };
};

let matchTerminate: nkruntime.MatchTerminateFunction = function (
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher,
    tick: number,
    state: nkruntime.MatchState,
    graceSeconds: number
) {
    return { state };
};

let matchSignal: nkruntime.MatchSignalFunction = function (
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher,
    tick: number,
    state: nkruntime.MatchState,
    data: string
) {
    return { state };
};

// ─── Message Routing ──────────────────────────────────────────────────────────

function processMessages(
    messages: nkruntime.MatchMessage[],
    gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher,
    nakama: nkruntime.Nakama,
    logger: nkruntime.Logger
): void {
    for (const message of messages) {
        if (MessagesLogic.hasOwnProperty(message.opCode)) {
            MessagesLogic[message.opCode](message, gameState, dispatcher, nakama, logger);
        }
    }
}

// ─── Match Loop Phases ────────────────────────────────────────────────────────

function processMatchLoop(
    gameState: GameState,
    nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher,
    logger: nkruntime.Logger
): void {
    switch (gameState.scene) {
        case Scene.Lobby:        matchLoopLobby(gameState, nakama, dispatcher, logger); break;
        case Scene.Battle:       matchLoopBattle(gameState, nakama, dispatcher, logger); break;
        case Scene.RoundResults: matchLoopRoundResults(gameState, nakama, dispatcher); break;
    }
}

function matchLoopLobby(
    gameState: GameState,
    nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher,
    logger: nkruntime.Logger
): void {
    const playerCount = getPlayersCount(gameState.players);
    if (playerCount === 0) return;

    if (gameState.countdown <= 0) return;
    gameState.countdown--;
    if (gameState.countdown > 0) return;

    if (playerCount >= 2) {
        startBattle(gameState, dispatcher);
    } else {
        addBotAndStartBattle(gameState, dispatcher, logger);
    }
}

function startBattle(gameState: GameState, dispatcher: nkruntime.MatchDispatcher): void {
    gameState.scene = Scene.Battle;
    dispatcher.broadcastMessage(OperationCode.ChangeScene, JSON.stringify(gameState.scene));
    dispatcher.matchLabelUpdate(JSON.stringify({ open: false }));
}

function addBotAndStartBattle(
    gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher,
    logger: nkruntime.Logger
): void {
    const nameIndex = Math.floor(Math.random() * BOT_NAMES.length);
    const botName = BOT_NAMES[nameIndex];

    const diffIndex = Math.floor(Math.random() * BOT_DIFFICULTIES.length);
    const difficulty = BOT_DIFFICULTIES[diffIndex];

    const botUserId    = "bot_" + generateId();
    const botSessionId = "bot_" + generateId();
    const botPresence  = {
        userId:    botUserId,
        sessionId: botSessionId,
        username:  botName,
        node:      "server",
        status:    "",
    } as nkruntime.Presence;

    const botPlayer: Player = {
        presence:    botPresence,
        displayName: botName,
        ScorePlayer: 0,
        isBot:       true,
    };

    gameState.players[1]     = botPlayer;
    gameState.playersWins[1] = 0;
    gameState.hasBot         = true;
    gameState.botDifficulty  = difficulty;

    logger.info(`Bot added: name=${botName} difficulty=${difficulty}`);

    dispatcher.broadcastMessage(OperationCode.Players, JSON.stringify(gameState.players));
    dispatcher.broadcastMessage(OperationCode.TurnMe, JSON.stringify(gameState.players[0].presence.userId));

    startBattle(gameState, dispatcher);
}

function generateId(): string {
    return Math.random().toString(36).substring(2, 10);
}

function matchLoopBattle(
    gameState: GameState,
    nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher,
    logger: nkruntime.Logger
): void {
    if (gameState.countdown > 0) {
        gameState.countdown--;
        if (gameState.countdown === 0) {
            gameState.roundDeclaredWins = [];
            gameState.roundDeclaredDraw = [];
            gameState.countdown = DurationRoundResults * TickRate;
            gameState.scene = Scene.RoundResults;
            dispatcher.broadcastMessage(OperationCode.ChangeScene, JSON.stringify(gameState.scene));
        }
        return;
    }

    if (gameState.hasBot && gameState.botNeedsToMove) {
        if (gameState.botThinkTick > 0) {
            gameState.botThinkTick--;
        } else {
            executeBotTurn(gameState, nakama, dispatcher, logger);
        }
    }
}

function matchLoopRoundResults(
    gameState: GameState,
    nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher
): void {
    if (gameState.countdown <= 0) return;
    gameState.countdown--;
    if (gameState.countdown > 0) return;

    const winner = getWinner(gameState.playersWins, gameState.players);
    if (winner !== null) {
        if (!winner.isBot) {
            const storageRead: nkruntime.StorageReadRequest[] = [{
                collection: CollectionUser,
                key: KeyTrophies,
                userId: winner.presence.userId,
            }];
            const result: nkruntime.StorageObject[] = nakama.storageRead(storageRead);
            let trophiesData: TrophiesData = { amount: 0 };
            for (const obj of result) {
                trophiesData = obj.value as TrophiesData;
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
        gameState.scene = Scene.FinalResults;
    } else {
        gameState.scene = Scene.Battle;
    }
    dispatcher.broadcastMessage(OperationCode.ChangeScene, JSON.stringify(gameState.scene));
}

// ─── Turn Handlers ────────────────────────────────────────────────────────────

function ChooseTurnPlayer(
    message: nkruntime.MatchMessage,
    gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher,
    nakama: nkruntime.Nakama,
    logger: nkruntime.Logger
): void {
    const dataPlayer: DataPlayer = JSON.parse(nakama.binaryToString(message.data));
    dataPlayer.MinesScore = false;
    gameState.BeforeEndGame = false;

    const isPlayer0 = message.sender.userId === gameState.players[0].presence.userId;

    if (isPlayer0) {
        processTurn(
            dataPlayer,
            gameState.array3DPlayerFirst,
            gameState.array3DPlayerSecend,
            gameState,
            0,
            true,
            nakama,
            logger
        );
    } else {
        processTurn(
            dataPlayer,
            gameState.array3DPlayerSecend,
            gameState.array3DPlayerFirst,
            gameState,
            1,
            false,
            nakama,
            logger
        );
    }

    const wasEndGame = dataPlayer.EndGame;
    const dataSend = JSON.stringify(dataPlayer);

    if (wasEndGame && gameState.hasBot) {
        dispatcher.broadcastMessage(message.opCode, dataSend);
    } else {
        dispatcher.broadcastMessage(message.opCode, dataSend, null, message.sender);
    }

    dataPlayer.EndGame = false;

    if (gameState.hasBot && isPlayer0 && !wasEndGame) {
        gameState.botNeedsToMove = true;
        gameState.botThinkTick = BotThinkMinTicks +
            Math.floor(Math.random() * (BotThinkMaxTicks - BotThinkMinTicks));
    }
}

function processTurn(
    dataPlayer: DataPlayer,
    moverGrid: any[][],
    targetGrid: any[][],
    gameState: GameState,
    moverIndex: number,
    isMaster: boolean,
    nakama: nkruntime.Nakama,
    logger: nkruntime.Logger
): void {
    const { NumberLine: line, NumberRow: row, NumberTile: tile } = dataPlayer;

    dataPlayer.master = isMaster;
    dataPlayer.MinesScore = false;
    dataPlayer.ValueMines = 0;
    dataPlayer.shieldBlocked = false;

    moverGrid[line][row] = tile;

    if (moverIndex === 0) {
        gameState.CountTurnPlayer1++;
    } else {
        gameState.CountTurnPlayer2++;
    }

    dataPlayer.Score = TotalScore(moverGrid, logger, gameState.VerticalMode);
    gameState.players[moverIndex].ScorePlayer = dataPlayer.Score;

    const opponentIndex = 1 - moverIndex;

    // Check shields before applying mine damage
    const shieldActive = gameState.shields[opponentIndex] > 0;

    let valuMines = 0;
    let mineCount = 0;

    if (gameState.VerticalMode) {
        const verticalHits = CalculatorArray2DWithVertical(targetGrid, line, row, tile, logger);
        if (verticalHits.length > 0) {
            if (shieldActive) {
                gameState.shields[opponentIndex]--;
                dataPlayer.shieldBlocked = true;
            } else {
                for (const hitRow of verticalHits) {
                    targetGrid[hitRow][row] = -1;
                    mineCount++;
                }
                if (mineCount > 0) {
                    valuMines = tile + 1;
                    dataPlayer.ValueMines = (valuMines * mineCount) * mineCount;
                    gameState.players[opponentIndex].ScorePlayer = TotalScore(targetGrid, logger, gameState.VerticalMode);
                    dataPlayer.ScoreOtherPlayer = gameState.players[opponentIndex].ScorePlayer;
                    dataPlayer.MinesScore = true;
                }
            }
            mineCount = 0;
        }
    }

    if (!dataPlayer.MinesScore && !dataPlayer.shieldBlocked) {
        const horizontalHits = CalculatorArray2D(targetGrid, line, row, tile, logger);
        if (horizontalHits.length > 0) {
            if (shieldActive) {
                gameState.shields[opponentIndex]--;
                dataPlayer.shieldBlocked = true;
            } else {
                for (const hitCol of horizontalHits) {
                    targetGrid[line][hitCol] = -1;
                    mineCount++;
                }
                if (mineCount > 0) {
                    valuMines = tile + 1;
                    dataPlayer.ValueMines = (valuMines * mineCount) * mineCount;
                    gameState.players[opponentIndex].ScorePlayer = TotalScore(targetGrid, logger, gameState.VerticalMode);
                    dataPlayer.ScoreOtherPlayer = gameState.players[opponentIndex].ScorePlayer;
                    dataPlayer.MinesScore = true;
                }
            }
        }
    }

    dataPlayer.Array2DTilesPlayer = moverGrid;
    dataPlayer.Array2DTilesOtherPlayer = targetGrid;

    const moverGridFull  = ActionWinPlayer(moverGrid);
    const targetGridFull = ActionWinPlayer(targetGrid);
    const turnsEqual = parseInt(gameState.CountTurnPlayer1) === parseInt(gameState.CountTurnPlayer2);

    if ((moverGridFull || targetGridFull) && turnsEqual) {
        const score0 = gameState.players[0].ScorePlayer;
        const score1 = gameState.players[1].ScorePlayer;

        if (score0 > score1) {
            dataPlayer.PlayerWin = gameState.players[0].presence.userId;
        } else if (score1 > score0) {
            dataPlayer.PlayerWin = gameState.players[1].presence.userId;
        } else {
            dataPlayer.PlayerWin = "";
        }
        dataPlayer.EndGame = true;
        gameState.BeforeEndGame = true;

        // Award coins to real players
        awardCoins(gameState, nakama, dataPlayer.PlayerWin, logger);
    }
}

// ─── Currency Award ───────────────────────────────────────────────────────────

function awardCoins(
    gameState: GameState,
    nakama: nkruntime.Nakama,
    winnerId: string,
    logger: nkruntime.Logger
): void {
    const updates: nkruntime.WalletUpdate[] = [];

    for (let i = 0; i < gameState.players.length; i++) {
        const player = gameState.players[i];
        if (!player || player.isBot) continue;

        let coins = COINS_LOSE;
        if (winnerId === "") {
            coins = COINS_DRAW;
        } else if (player.presence.userId === winnerId) {
            coins = COINS_WIN;
        }

        updates.push({
            userId: player.presence.userId,
            changeset: { coins: coins },
            metadata: { source: "match_result" },
        });
    }

    if (updates.length > 0) {
        try {
            nakama.walletsUpdate(updates, false);
        } catch (e) {
            logger.warn("walletsUpdate failed: " + e);
        }
    }
}

// ─── Card Handler ─────────────────────────────────────────────────────────────

function UseCard(
    message: nkruntime.MatchMessage,
    gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher,
    nakama: nkruntime.Nakama,
    logger: nkruntime.Logger
): void {
    const data: UseCardMessage = JSON.parse(nakama.binaryToString(message.data));
    const senderIndex   = message.sender.userId === gameState.players[0]?.presence.userId ? 0 : 1;
    const opponentIndex = 1 - senderIndex;

    // Counter Pulse intercepts the card
    if (gameState.counterActive[opponentIndex]) {
        gameState.counterActive[opponentIndex] = false;
        data.countered = true;
        dispatcher.broadcastMessage(OperationCode.UseCard, JSON.stringify(data), null, message.sender);
        return;
    }

    // Apply server-side card effects
    switch (data.cardId) {
        case 2: // GuardianShield
            gameState.shields[senderIndex]++;
            break;

        case 6: { // Breaker — clear a row in opponent's grid
            const targetGrid = opponentIndex === 0 ? gameState.array3DPlayerFirst : gameState.array3DPlayerSecend;
            const line = data.targetLine !== undefined ? data.targetLine : -1;
            if (line >= 0 && line < targetGrid.length) {
                for (let c = 0; c < targetGrid[line].length; c++) {
                    targetGrid[line][c] = -1;
                }
            }
            break;
        }

        case 11: { // DiceThief — remove opponent's highest-value filled cell
            const targetGrid = opponentIndex === 0 ? gameState.array3DPlayerFirst : gameState.array3DPlayerSecend;
            removeBestTile(targetGrid);
            break;
        }

        case 12: // CounterPulse — arm a counter for the sender
            gameState.counterActive[senderIndex] = true;
            break;
    }

    // Broadcast to opponent so their client can react
    const realPresences = gameState.players
        .filter(p => p && !p.isBot)
        .map(p => p.presence);

    dispatcher.broadcastMessage(OperationCode.UseCard, JSON.stringify(data), realPresences, message.sender);
}

function removeBestTile(grid: any[][]): void {
    let bestVal  = -1;
    let bestLine = -1;
    let bestCol  = -1;
    for (let i = 0; i < grid.length; i++) {
        for (let j = 0; j < grid[i].length; j++) {
            if (grid[i][j] > bestVal) {
                bestVal  = grid[i][j];
                bestLine = i;
                bestCol  = j;
            }
        }
    }
    if (bestLine >= 0) grid[bestLine][bestCol] = -1;
}

// ─── Bot AI ───────────────────────────────────────────────────────────────────

function executeBotTurn(
    gameState: GameState,
    nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher,
    logger: nkruntime.Logger
): void {
    gameState.botNeedsToMove = false;

    const realPlayer = gameState.players[0];
    const botPlayer  = gameState.players[1];
    if (!realPlayer || !botPlayer) return;

    const move = generateBotMove(gameState, logger);
    if (!move) {
        logger.warn("Bot could not find a valid move");
        return;
    }

    const dataPlayer: DataPlayer = {
        UserId:                  botPlayer.presence.userId,
        Score:                   0,
        NumberTile:              move.tile,
        NameTile:                move.tile.toString(),
        NumberLine:              move.line,
        NumberRow:               move.col,
        EndGame:                 false,
        PlayerWin:               "",
        ScoreOtherPlayer:        0,
        MinesScore:              false,
        ValueMines:              0,
        sumRow1:                 [],
        sumRow2:                 [],
        master:                  false,
        Array2DTilesPlayer:      [],
        Array2DTilesOtherPlayer: [],
    };

    processTurn(
        dataPlayer,
        gameState.array3DPlayerSecend,
        gameState.array3DPlayerFirst,
        gameState,
        1,
        false,
        nakama,
        logger
    );

    dispatcher.broadcastMessage(OperationCode.ChosseTurn, JSON.stringify(dataPlayer), [realPlayer.presence], null);
}

interface BotMove {
    line: number;
    col: number;
    tile: number;
}

function generateBotMove(gameState: GameState, logger: nkruntime.Logger): BotMove | null {
    const botGrid    = gameState.array3DPlayerSecend;
    const playerGrid = gameState.array3DPlayerFirst;
    const numRows    = botGrid.length;
    const numCols    = botGrid[0].length;
    const maxTile    = numCols - 1;
    const difficulty = gameState.botDifficulty;

    const emptyCells: { line: number; col: number }[] = [];
    for (let i = 0; i < numRows; i++) {
        for (let j = 0; j < numCols; j++) {
            if (botGrid[i][j] === -1) {
                emptyCells.push({ line: i, col: j });
            }
        }
    }
    if (emptyCells.length === 0) return null;

    if (difficulty === 0) {
        const cell = emptyCells[Math.floor(Math.random() * emptyCells.length)];
        const tile = Math.floor(Math.random() * (maxTile + 1));
        return { line: cell.line, col: cell.col, tile };
    }

    if (difficulty === 1 && Math.random() < 0.5) {
        const cell = emptyCells[Math.floor(Math.random() * emptyCells.length)];
        const tile = Math.floor(Math.random() * (maxTile + 1));
        return { line: cell.line, col: cell.col, tile };
    }

    let bestScore = -1;
    let bestMove: BotMove = {
        line: emptyCells[0].line,
        col:  emptyCells[0].col,
        tile: Math.floor(Math.random() * (maxTile + 1)),
    };

    for (const cell of emptyCells) {
        for (let tile = 0; tile <= maxTile; tile++) {
            const tempBot = botGrid.map((r: any[]) => r.slice());
            tempBot[cell.line][cell.col] = tile;

            let ownScore = simulateTotalScore(tempBot, gameState.VerticalMode);

            let mineHits = 0;
            const playerRow = playerGrid[cell.line] as number[];
            mineHits += playerRow.filter((v: number) => v === tile).length;
            if (gameState.VerticalMode) {
                for (let r = 0; r < numRows; r++) {
                    if (playerGrid[r][cell.col] === tile) mineHits++;
                }
            }

            const moveScore = ownScore + mineHits * 6;
            if (moveScore > bestScore) {
                bestScore = moveScore;
                bestMove  = { line: cell.line, col: cell.col, tile };
            }
        }
    }

    return bestMove;
}

function simulateTotalScore(grid: any[][], verticalMode: boolean): number {
    let score = 0;
    for (let i = 0; i < grid.length; i++) {
        score += scoreRow(grid[i]);
    }
    if (verticalMode) {
        for (let col = 0; col < grid[0].length; col++) {
            const column = grid.map((r: any[]) => r[col]);
            score += scoreRow(column);
        }
    }
    return score;
}

function scoreRow(arr: any[]): number {
    const counts: { [k: string]: number } = {};
    for (const v of arr) {
        if (v === -1) continue;
        counts[v] = (counts[v] || 0) + 1;
    }
    let sum = 0;
    for (const k of Object.keys(counts)) {
        const key   = Number(k);
        const count = counts[k];
        if (count === 4)      return (key + 1) * 16;
        else if (count === 3) sum += (key + 1) * 9;
        else if (count === 2) sum += (key + 1) * 4;
        else                  sum += (key + 1);
    }
    return sum;
}

// ─── Sticker & Rematch ────────────────────────────────────────────────────────

function StickersManager(
    message: nkruntime.MatchMessage,
    gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher,
    nakama: nkruntime.Nakama,
    logger: nkruntime.Logger
): void {
    const data: StickerData = JSON.parse(nakama.binaryToString(message.data));
    dispatcher.broadcastMessage(OperationCode.Sticker, JSON.stringify(data));
}

function Rematch(
    message: nkruntime.MatchMessage,
    gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher,
    nakama: nkruntime.Nakama,
    logger: nkruntime.Logger
): void {
    const dataPlayer: IReMatch = JSON.parse(nakama.binaryToString(message.data));

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
            dispatcher.broadcastMessage(OperationCode.TurnMe, JSON.stringify(gameState.players[0].presence.userId));
        }
        return;
    }

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
            dispatcher.broadcastMessage(OperationCode.TurnMe, JSON.stringify(gameState.players[0].presence.userId));
        }
    }

    if (dataPlayer.Answer === "send") {
        dataPlayer.userId = message.sender.userId;
        dataPlayer.Answer = "req";
        dispatcher.broadcastMessage(message.opCode, JSON.stringify(dataPlayer), null, message.sender);
    }
}

function resetGameForRematch(gameState: GameState, nakama: nkruntime.Nakama): void {
    gameState.endMatch         = false;
    gameState.BeforeEndGame    = false;
    gameState.botNeedsToMove   = false;
    gameState.CountTurnPlayer1 = 0;
    gameState.CountTurnPlayer2 = 0;
    gameState.namesForrematch  = [];
    gameState.shields          = [0, 0];
    gameState.counterActive    = [false, false];

    for (let i = 0; i < gameState.array3DPlayerFirst.length; i++) {
        for (let j = 0; j < gameState.array3DPlayerFirst[i].length; j++) {
            gameState.array3DPlayerFirst[i][j]  = -1;
            gameState.array3DPlayerSecend[i][j] = -1;
        }
    }

    const s = new ScoreClass();
    for (const player of gameState.players) {
        if (player && !player.isBot) {
            SaveScore(player.presence.userId, 0, nakama, s);
        }
    }
}

// ─── Score / Grid Helpers ─────────────────────────────────────────────────────

function TotalScore(array2D: number[][], logger: nkruntime.Logger, mode: boolean): number {
    let score = 0;
    for (let i = 0; i < array2D.length; i++) {
        score += CalculatorArray(array2D[i], logger);
    }
    if (mode) {
        for (let col = 0; col < array2D[0].length; col++) {
            score += CalculatorArray(array2D.map(d => d[col]), logger);
        }
    }
    return score;
}

function CalculatorArray(arrayInput: any[], logger: nkruntime.Logger): number {
    const counts: { [k: string]: number } = {};
    for (const v of arrayInput) {
        if (v === -1) continue;
        const key = String(v);
        counts[key] = (counts[key] || 0) + 1;
    }

    let sum = 0;
    for (const k of Object.keys(counts)) {
        const key   = Number(k);
        const count = counts[k];
        if (count === 4)      return (key + 1) * 16;
        else if (count === 3) sum += (key + 1) * 9;
        else if (count === 2) sum += (key + 1) * 4;
        else                  sum += (key + 1);
    }
    return sum;
}

function CalculatorArray2D(
    array1: number[][],
    x: number,
    y: number,
    input: number,
    logger: nkruntime.Logger
): number[] {
    const result: number[] = [];
    array1[x].forEach((element, index) => {
        if (element === input) result.push(index);
    });
    return result;
}

function CalculatorArray2DWithVertical(
    array1: number[][],
    x: number,
    y: number,
    input: number,
    logger: nkruntime.Logger
): number[] {
    const result: number[] = [];
    const column = array1.map(r => r[y]);
    column.forEach((element, index) => {
        if (element === input) result.push(index);
    });
    return result;
}

function ActionWinPlayer(array1: number[][]): boolean {
    for (let i = 0; i < array1.length; i++) {
        for (let j = 0; j < array1[i].length; j++) {
            if (array1[i][j] === -1) return false;
        }
    }
    return true;
}

// ─── Player / Storage Utilities ───────────────────────────────────────────────

function getPlayersCount(players: Player[]): number {
    let count = 0;
    for (let i = 0; i < MaxPlayers; i++) {
        if (players[i] !== undefined) count++;
    }
    return count;
}

function getNextPlayerNumber(players: Player[]): number {
    for (let i = 0; i < MaxPlayers; i++) {
        if (!players[i]) return i;
    }
    return PlayerNotFound;
}

function getPlayerNumber(players: Player[], sessionId: string): number {
    for (let i = 0; i < MaxPlayers; i++) {
        if (players[i] && players[i].presence.sessionId === sessionId) return i;
    }
    return PlayerNotFound;
}

function playerNumberIsUsed(players: Player[], playerNumber: number): boolean {
    return players[playerNumber] !== undefined;
}

function getWinner(playersWins: number[], players: Player[]): Player | null {
    for (let i = 0; i < MaxPlayers; i++) {
        if (playersWins[i] === NecessaryWins) return players[i];
    }
    return null;
}

function playerWon(
    message: nkruntime.MatchMessage,
    gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher,
    nakama: nkruntime.Nakama
): void {
    if (gameState.scene !== Scene.Battle || gameState.countdown > 0) return;

    const data: PlayerWonData = JSON.parse(nakama.binaryToString(message.data));
    const { tick, playerNumber } = data;

    if (!gameState.roundDeclaredWins[tick]) gameState.roundDeclaredWins[tick] = [];
    if (!gameState.roundDeclaredWins[tick][playerNumber]) gameState.roundDeclaredWins[tick][playerNumber] = 0;

    gameState.roundDeclaredWins[tick][playerNumber]++;
    if (gameState.roundDeclaredWins[tick][playerNumber] < getPlayersCount(gameState.players)) return;

    gameState.playersWins[playerNumber]++;
    gameState.countdown = DurationBattleEnding * TickRate;
    dispatcher.broadcastMessage(message.opCode, message.data, null, message.sender);
}

function draw(
    message: nkruntime.MatchMessage,
    gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher,
    nakama: nkruntime.Nakama,
    logger: nkruntime.Logger
): void {
    if (gameState.scene !== Scene.Battle || gameState.countdown > 0) return;

    const data: DrawData = JSON.parse(nakama.binaryToString(message.data));
    const { tick } = data;

    if (!gameState.roundDeclaredDraw[tick]) gameState.roundDeclaredDraw[tick] = 0;
    gameState.roundDeclaredDraw[tick]++;
    if (gameState.roundDeclaredDraw[tick] < getPlayersCount(gameState.players)) return;

    gameState.countdown = DurationBattleEnding * TickRate;
    dispatcher.broadcastMessage(message.opCode, message.data, null, message.sender);
}

// ─── Storage ──────────────────────────────────────────────────────────────────

function SaveScore(id: string, mines: number, nakama: nkruntime.Nakama, scoreObj: ScoreClass): number {
    scoreObj.ScoreF -= mines;
    nakama.storageWrite([{
        collection: CollectionUser,
        key: "Score",
        userId: id,
        value: scoreObj,
    }]);
    return scoreObj.ScoreF;
}

function ReadScore(id: string, nakama: nkruntime.Nakama): ScoreClass {
    const result = nakama.storageRead([{ collection: CollectionUser, key: "Score", userId: id }]);
    for (const obj of result) return obj.value as ScoreClass;
    return new ScoreClass();
}

function SaveScoreLeaderboard(id: string, nakama: nkruntime.Nakama, scoreLeaderboard: CountWin): void {
    nakama.storageWrite([{
        collection: "Rank",
        key: "leaderboard",
        userId: id,
        value: scoreLeaderboard,
    }]);
}

function ReadScoreLeaderboard(id: string, nakama: nkruntime.Nakama): CountWin {
    const result = nakama.storageRead([{ collection: "Rank", key: "leaderboard", userId: id }]);
    for (const obj of result) return obj.value as CountWin;
    return new CountWin();
}

// ─── Data Classes ─────────────────────────────────────────────────────────────

class ScoreClass {
    ScoreF: number = 0;
}

class CountWin {
    win: number = 0;
}
