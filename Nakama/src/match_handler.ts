// ─── Match Lifecycle ──────────────────────────────────────────────────────────

let matchInit: nkruntime.MatchInitFunction = function (
    context, logger, nakama,
    params: { [key: string]: string }
) {
    let value = "";
    for (let key in params) value = params[key];

    const label: MatchLabel = { open: true, game_mode: value };
    const [arrayFirst, arraySecond, vertical] = buildGrids(value);

    const gameState: GameState = {
        players: [], playersWins: [], roundDeclaredWins: [[]], roundDeclaredDraw: [],
        scene: Scene.Lobby,
        countdown: DurationLobby * TickRate,
        endMatch: false,
        CountTurnPlayer1: 0, CountTurnPlayer2: 0,
        namesForrematch: [],
        BeforeEndGame: false,
        VerticalMode: vertical,
        array3DPlayerFirst: arrayFirst, array3DPlayerSecend: arraySecond,
        ModeText: value,
        hasBot: false, botDifficulty: 0, botNeedsToMove: false, botThinkTick: 0,
    };

    return { state: gameState, tickRate: TickRate, label: JSON.stringify(label) };
};

function buildGrids(mode: string): [any[][], any[][], boolean] {
    let rowFirst: any[][]  = [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
    let rowSecond: any[][] = [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
    let vertical = false;

    if (mode === "VerticalAndHorizontal") {
        vertical = true;
    } else if (mode === "FourByThree") {
        rowFirst  = [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
        rowSecond = [[-1,-1,-1],[-1,-1,-1],[-1,-1,-1],[-1,-1,-1]];
    } else if (mode === "ThreeByThree") {
        rowFirst  = [[-1,-1,-1,-1],[-1,-1,-1,-1],[-1,-1,-1,-1],[-1,-1,-1,-1]];
        rowSecond = [[-1,-1,-1,-1],[-1,-1,-1,-1],[-1,-1,-1,-1],[-1,-1,-1,-1]];
    }
    return [rowFirst, rowSecond, vertical];
}

// ─── Join ─────────────────────────────────────────────────────────────────────

let matchJoinAttempt: nkruntime.MatchJoinAttemptFunction = function (
    context, logger, nakama, dispatcher, tick, state,
    presence, metadata
) {
    const gameState = state as GameState;
    if (gameState.scene !== Scene.Lobby) return { state: gameState, accept: false };

    // Check entry fee balance before accepting
    const league = LEAGUES[gameState.ModeText];
    if (league) {
        try {
            const account = nakama.accountGetId(presence.userId);
            const wallet  = account.wallet || {};
            if ((wallet["coins"] || 0) < league.entryFee) {
                logger.info(`Join rejected — insufficient coins for ${gameState.ModeText}: userId=${presence.userId}`);
                return { state: gameState, accept: false };
            }
        } catch (e) {
            logger.warn("matchJoinAttempt wallet check failed: " + e);
            return { state: gameState, accept: false };
        }
    }

    return { state: gameState, accept: true };
};

let matchJoin: nkruntime.MatchJoinFunction = function (
    context, logger, nakama, dispatcher, tick, state, presences
) {
    const gameState = state as GameState;
    if (gameState.scene !== Scene.Lobby) return { state: gameState };

    const existingPresences: nkruntime.Presence[] = [];
    gameState.players.forEach(p => { if (p && !p.isBot) existingPresences.push(p.presence); });

    for (const presence of presences) {
        const account: nkruntime.Account = nakama.accountGetId(presence.userId);
        const resolvedName = account.user.displayName && account.user.displayName.trim().length > 0
            ? account.user.displayName.trim()
            : (presence.username || account.user.username || "Player");

        // Deduct entry fee
        const league = LEAGUES[gameState.ModeText];
        if (league) {
            try {
                nakama.walletUpdate(presence.userId, { coins: -league.entryFee },
                    { source: "entry_fee", league: gameState.ModeText }, true);
            } catch (e) {
                logger.warn(`Entry fee deduction failed for ${presence.userId}: ${e}`);
            }
        }

        const player: Player = {
            presence, displayName: resolvedName, ScorePlayer: 0,
        };
        const slot = getNextPlayerNumber(gameState.players);
        gameState.players[slot]     = player;
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

// ─── Match Loop ───────────────────────────────────────────────────────────────

let matchLoop: nkruntime.MatchLoopFunction = function (
    context, logger, nakama, dispatcher, tick, state, messages
) {
    const gameState = state as GameState;
    processMessages(messages, gameState, dispatcher, nakama, logger);
    processMatchLoop(gameState, nakama, dispatcher, logger);
    return gameState.endMatch ? null : { state: gameState };
};

let matchLeave: nkruntime.MatchLeaveFunction = function (
    context, logger, nakama, dispatcher, tick, state, presences
) {
    const gameState = state as GameState;
    for (const presence of presences) {
        const num = getPlayerNumber(gameState.players, presence.sessionId);
        if (num === PlayerNotFound) continue;
        const name = JSON.stringify(gameState.players[num].displayName);
        if (!gameState.BeforeEndGame) dispatcher.broadcastMessage(9, name);
        delete gameState.players[num];
    }
    return { state: gameState };
};

let matchTerminate: nkruntime.MatchTerminateFunction = function (
    context, logger, nakama, dispatcher, tick, state, graceSeconds
) { return { state }; };

let matchSignal: nkruntime.MatchSignalFunction = function (
    context, logger, nk, dispatcher, tick, state, data
) { return { state }; };

// ─── Message Routing ──────────────────────────────────────────────────────────

function processMessages(
    messages: nkruntime.MatchMessage[], gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama, logger: nkruntime.Logger
): void {
    for (const message of messages) {
        if (MessagesLogic.hasOwnProperty(message.opCode)) {
            MessagesLogic[message.opCode](message, gameState, dispatcher, nakama, logger);
        }
    }
}

function processMatchLoop(
    gameState: GameState, nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher, logger: nkruntime.Logger
): void {
    switch (gameState.scene) {
        case Scene.Lobby:        matchLoopLobby(gameState, nakama, dispatcher, logger); break;
        case Scene.Battle:       matchLoopBattle(gameState, nakama, dispatcher, logger); break;
        case Scene.RoundResults: matchLoopRoundResults(gameState, nakama, dispatcher);   break;
    }
}

// ─── Lobby ────────────────────────────────────────────────────────────────────

function matchLoopLobby(
    gameState: GameState, nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher, logger: nkruntime.Logger
): void {
    if (getPlayersCount(gameState.players) === 0) return;
    if (gameState.countdown <= 0) return;
    gameState.countdown--;
    if (gameState.countdown > 0) return;

    if (getPlayersCount(gameState.players) >= 2) {
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
    gameState: GameState, dispatcher: nkruntime.MatchDispatcher, logger: nkruntime.Logger
): void {
    const botName = BOT_NAMES[Math.floor(Math.random() * BOT_NAMES.length)];
    const diff    = BOT_DIFFICULTIES[Math.floor(Math.random() * BOT_DIFFICULTIES.length)];

    const botPresence = {
        userId: "bot_" + generateId(), sessionId: "bot_" + generateId(),
        username: botName, node: "server", status: "",
    } as nkruntime.Presence;

    gameState.players[1]     = { presence: botPresence, displayName: botName, ScorePlayer: 0, isBot: true };
    gameState.playersWins[1] = 0;
    gameState.hasBot          = true;
    gameState.botDifficulty   = diff;

    logger.info(`Bot added: name=${botName} difficulty=${diff}`);

    dispatcher.broadcastMessage(OperationCode.Players, JSON.stringify(gameState.players));
    dispatcher.broadcastMessage(OperationCode.TurnMe,  JSON.stringify(gameState.players[0].presence.userId));
    startBattle(gameState, dispatcher);
}

function generateId(): string {
    return Math.random().toString(36).substring(2, 10);
}

// ─── Battle ───────────────────────────────────────────────────────────────────

function matchLoopBattle(
    gameState: GameState, nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher, logger: nkruntime.Logger
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
    gameState: GameState, nakama: nkruntime.Nakama, dispatcher: nkruntime.MatchDispatcher
): void {
    if (gameState.countdown <= 0) return;
    gameState.countdown--;
    if (gameState.countdown > 0) return;

    const winner = getWinner(gameState.playersWins, gameState.players);
    if (winner !== null) {
        if (!winner.isBot) {
            const read = nakama.storageRead([{ collection: CollectionUser, key: KeyTrophies, userId: winner.presence.userId }]);
            let td: TrophiesData = { amount: 0 };
            for (const obj of read) { td = obj.value as TrophiesData; break; }
            td.amount++;
            nakama.storageWrite([{ collection: CollectionUser, key: KeyTrophies, userId: winner.presence.userId, value: td }]);
        }
        gameState.endMatch = true;
        gameState.scene    = Scene.FinalResults;
    } else {
        gameState.scene = Scene.Battle;
    }
    dispatcher.broadcastMessage(OperationCode.ChangeScene, JSON.stringify(gameState.scene));
}

// ─── Turn ─────────────────────────────────────────────────────────────────────

function ChooseTurnPlayer(
    message: nkruntime.MatchMessage, gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama, logger: nkruntime.Logger
): void {
    const dataPlayer: DataPlayer = JSON.parse(nakama.binaryToString(message.data));
    dataPlayer.MinesScore = false;
    gameState.BeforeEndGame = false;

    const isPlayer0 = message.sender.userId === gameState.players[0].presence.userId;

    if (isPlayer0) {
        processTurn(dataPlayer, gameState.array3DPlayerFirst,  gameState.array3DPlayerSecend, gameState, 0, true,  nakama, logger);
    } else {
        processTurn(dataPlayer, gameState.array3DPlayerSecend, gameState.array3DPlayerFirst,  gameState, 1, false, nakama, logger);
    }

    const wasEndGame = dataPlayer.EndGame;
    const dataSend   = JSON.stringify(dataPlayer);

    if (wasEndGame && gameState.hasBot) {
        dispatcher.broadcastMessage(message.opCode, dataSend);
    } else {
        dispatcher.broadcastMessage(message.opCode, dataSend, null, message.sender);
    }

    dataPlayer.EndGame = false;

    if (gameState.hasBot && isPlayer0 && !wasEndGame) {
        gameState.botNeedsToMove = true;
        gameState.botThinkTick = BotThinkMinTicks + Math.floor(Math.random() * (BotThinkMaxTicks - BotThinkMinTicks));
    }
}

function processTurn(
    dataPlayer: DataPlayer,
    moverGrid: any[][], targetGrid: any[][],
    gameState: GameState, moverIndex: number, isMaster: boolean,
    nakama: nkruntime.Nakama, logger: nkruntime.Logger
): void {
    const { NumberLine: line, NumberRow: row, NumberTile: tile } = dataPlayer;

    dataPlayer.master     = isMaster;
    dataPlayer.MinesScore = false;
    dataPlayer.ValueMines = 0;

    moverGrid[line][row] = tile;

    if (moverIndex === 0) gameState.CountTurnPlayer1++;
    else                   gameState.CountTurnPlayer2++;

    dataPlayer.Score = TotalScore(moverGrid, logger, gameState.VerticalMode);
    gameState.players[moverIndex].ScorePlayer = dataPlayer.Score;

    const opponentIndex = 1 - moverIndex;
    let mineCount = 0;

    // ── Vertical mines (VerticalAndHorizontal mode only) ────────────────────────
    if (gameState.VerticalMode) {
        const hits = CalculatorArray2DWithVertical(targetGrid, line, row, tile, logger);
        for (const hitRow of hits) { targetGrid[hitRow][row] = -1; mineCount++; }
        if (mineCount > 0) {
            applyMineResult(dataPlayer, mineCount, tile, targetGrid, gameState, opponentIndex, logger);
        }
        mineCount = 0;
    }

    // ── Horizontal mines (always checked in all modes) ────────────────────────
    {
        const hits = CalculatorArray2D(targetGrid, line, row, tile, logger);
        for (const hitCol of hits) { targetGrid[line][hitCol] = -1; mineCount++; }
        if (mineCount > 0) {
            if (dataPlayer.MinesScore) {
                // Vertical mines already fired — accumulate horizontal damage on top
                dataPlayer.ValueMines += (tile + 1) * mineCount * mineCount;
                dataPlayer.ScoreOtherPlayer = TotalScore(targetGrid, logger, gameState.VerticalMode);
                gameState.players[opponentIndex].ScorePlayer = dataPlayer.ScoreOtherPlayer;
            } else {
                applyMineResult(dataPlayer, mineCount, tile, targetGrid, gameState, opponentIndex, logger);
            }
        }
    }

    dataPlayer.Array2DTilesPlayer      = moverGrid;
    dataPlayer.Array2DTilesOtherPlayer = targetGrid;

    const moverFull  = ActionWinPlayer(moverGrid);
    const targetFull = ActionWinPlayer(targetGrid);
    const turnsEqual = parseInt(gameState.CountTurnPlayer1) === parseInt(gameState.CountTurnPlayer2);

    if ((moverFull || targetFull) && turnsEqual) {
        const s0 = gameState.players[0].ScorePlayer;
        const s1 = gameState.players[1].ScorePlayer;

        if      (s0 > s1) dataPlayer.PlayerWin = gameState.players[0].presence.userId;
        else if (s1 > s0) dataPlayer.PlayerWin = gameState.players[1].presence.userId;
        else               dataPlayer.PlayerWin = "";

        dataPlayer.EndGame      = true;
        gameState.BeforeEndGame = true;

        awardMatchResult(gameState, nakama, dataPlayer.PlayerWin, logger);
    }
}

function applyMineResult(
    dataPlayer: DataPlayer, mineCount: number, tile: number,
    targetGrid: any[][], gameState: GameState, opponentIndex: number,
    logger: nkruntime.Logger
): void {
    const valuMines = tile + 1;
    dataPlayer.ValueMines      = (valuMines * mineCount) * mineCount;
    dataPlayer.ScoreOtherPlayer = TotalScore(targetGrid, logger, gameState.VerticalMode);
    gameState.players[opponentIndex].ScorePlayer = dataPlayer.ScoreOtherPlayer;
    dataPlayer.MinesScore = true;
}

// ─── Economy ──────────────────────────────────────────────────────────────────

function awardMatchResult(
    gameState: GameState, nakama: nkruntime.Nakama, winnerId: string, logger: nkruntime.Logger
): void {
    const league = LEAGUES[gameState.ModeText];
    if (!league) return;

    const updates: nkruntime.WalletUpdate[] = [];

    for (let i = 0; i < gameState.players.length; i++) {
        const player = gameState.players[i];
        if (!player || player.isBot) continue;

        if (winnerId === "") {
            // Draw — return 50%
            updates.push({
                userId: player.presence.userId,
                changeset: { coins: league.drawRefund },
                metadata: { source: "draw_refund" },
            });
        } else if (player.presence.userId === winnerId) {
            // Win
            updates.push({
                userId: player.presence.userId,
                changeset: { coins: league.winnerReward },
                metadata: { source: "match_win", league: gameState.ModeText },
            });
            // Record rank points in both leaderboards
            try {
                nakama.leaderboardRecordWrite(LeaderboardWeekly,  player.presence.userId, player.displayName, league.rankPoints);
                nakama.leaderboardRecordWrite(LeaderboardMonthly, player.presence.userId, player.displayName, league.rankPoints);
            } catch (e) {
                logger.warn("leaderboardRecordWrite failed: " + e);
            }
        }
        // Lose: entry fee already deducted on join, nothing extra
    }

    if (updates.length > 0) {
        try { nakama.walletsUpdate(updates, false); }
        catch (e) { logger.warn("walletsUpdate failed: " + e); }
    }
}

// ─── Leaderboard Reset Hook ───────────────────────────────────────────────────

let onLeaderboardReset: nkruntime.LeaderboardResetFunction = function (
    ctx, logger, nk, leaderboard, reset
): void {
    const isWeekly  = leaderboard.id === LeaderboardWeekly;
    const rewards   = isWeekly ? WEEKLY_REWARDS : MONTHLY_REWARDS;
    const pendingKey = isWeekly ? KeyPendingRewardWeekly : KeyPendingRewardMonthly;
    const typeLabel  = isWeekly ? "weekly" : "monthly";

    let records: nkruntime.LeaderboardRecord[] = [];
    try {
        const result = nk.leaderboardRecordsList(leaderboard.id, [], rewards.length, "", 0);
        records = result.records || [];
    } catch (e) {
        logger.warn("leaderboardRecordsList failed during reset: " + e);
        return;
    }

    const walletUpdates: nkruntime.WalletUpdate[] = [];
    const storageWrites: nkruntime.StorageWriteRequest[] = [];

    for (let i = 0; i < records.length && i < rewards.length; i++) {
        const record = records[i];
        const reward = rewards[i];

        walletUpdates.push({
            userId: record.ownerId,
            changeset: { coins: reward },
            metadata: { source: "leaderboard_reward", type: typeLabel, rank: i + 1 },
        });

        const pendingData: PendingRewardData = { rank: i + 1, reward: reward, type: typeLabel, claimed: false };
        storageWrites.push({
            collection: CollectionSeason,
            key: pendingKey,
            userId: record.ownerId,
            value: pendingData,
            permissionRead: 1,
            permissionWrite: 0,
        });
    }

    if (walletUpdates.length > 0) {
        try { nk.walletsUpdate(walletUpdates, false); }
        catch (e) { logger.warn("Reward distribution failed: " + e); }
    }
    if (storageWrites.length > 0) {
        try { nk.storageWrite(storageWrites); }
        catch (e) { logger.warn("Pending reward storage failed: " + e); }
    }

    logger.info(`${typeLabel} leaderboard reset: distributed rewards to ${records.length} players`);
};

// ─── Sticker & Rematch ────────────────────────────────────────────────────────

function StickersManager(
    message: nkruntime.MatchMessage, gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama, logger: nkruntime.Logger
): void {
    const data: StickerData = JSON.parse(nakama.binaryToString(message.data));
    dispatcher.broadcastMessage(OperationCode.Sticker, JSON.stringify(data));
}

function Rematch(
    message: nkruntime.MatchMessage, gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama, logger: nkruntime.Logger
): void {
    const dataPlayer: IReMatch = JSON.parse(nakama.binaryToString(message.data));

    if (gameState.hasBot) {
        if (dataPlayer.Answer === "no") {
            gameState.endMatch = true;
            dispatcher.broadcastMessage(message.opCode, JSON.stringify(dataPlayer), null, message.sender);
            return;
        }
        if (dataPlayer.Answer === "send" || dataPlayer.Answer === "yes") {
            // Deduct entry fee again for rematch
            const league = LEAGUES[gameState.ModeText];
            if (league) {
                try {
                    nakama.walletUpdate(gameState.players[0].presence.userId,
                        { coins: -league.entryFee }, { source: "entry_fee_rematch" }, true);
                } catch (e) {
                    logger.warn("Rematch entry fee failed: " + e);
                }
            }
            resetGameForRematch(gameState);
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
            // Deduct entry fee for both players
            const league = LEAGUES[gameState.ModeText];
            if (league) {
                const feeUpdates: nkruntime.WalletUpdate[] = [];
                for (const p of gameState.players) {
                    if (p && !p.isBot) {
                        feeUpdates.push({ userId: p.presence.userId, changeset: { coins: -league.entryFee }, metadata: { source: "entry_fee_rematch" } });
                    }
                }
                try { nakama.walletsUpdate(feeUpdates, true); }
                catch (e) { logger.warn("Rematch entry fees failed: " + e); }
            }
            resetGameForRematch(gameState);
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

function resetGameForRematch(gameState: GameState): void {
    gameState.endMatch         = false;
    gameState.BeforeEndGame    = false;
    gameState.botNeedsToMove   = false;
    gameState.CountTurnPlayer1 = 0;
    gameState.CountTurnPlayer2 = 0;
    gameState.namesForrematch  = [];

    for (let i = 0; i < gameState.array3DPlayerFirst.length; i++) {
        for (let j = 0; j < gameState.array3DPlayerFirst[i].length; j++) {
            gameState.array3DPlayerFirst[i][j]  = -1;
            gameState.array3DPlayerSecend[i][j] = -1;
        }
    }

    for (const player of gameState.players) {
        if (player) player.ScorePlayer = 0;
    }
}

// ─── Bot AI ───────────────────────────────────────────────────────────────────

function executeBotTurn(
    gameState: GameState, nakama: nkruntime.Nakama,
    dispatcher: nkruntime.MatchDispatcher, logger: nkruntime.Logger
): void {
    gameState.botNeedsToMove = false;

    const realPlayer = gameState.players[0];
    const botPlayer  = gameState.players[1];
    if (!realPlayer || !botPlayer) return;

    const move = generateBotMove(gameState, logger);
    if (!move) { logger.warn("Bot could not find a valid move"); return; }

    const dataPlayer: DataPlayer = {
        UserId: botPlayer.presence.userId,
        Score: 0, NumberTile: move.tile, NameTile: move.tile.toString(),
        NumberLine: move.line, NumberRow: move.col,
        EndGame: false, PlayerWin: "", ScoreOtherPlayer: 0,
        MinesScore: false, ValueMines: 0, sumRow1: [], sumRow2: [],
        master: false, Array2DTilesPlayer: [], Array2DTilesOtherPlayer: [],
    };

    processTurn(dataPlayer, gameState.array3DPlayerSecend, gameState.array3DPlayerFirst,
        gameState, 1, false, nakama, logger);

    dispatcher.broadcastMessage(OperationCode.ChosseTurn, JSON.stringify(dataPlayer), [realPlayer.presence], null);
}

interface BotMove { line: number; col: number; tile: number; }

function generateBotMove(gameState: GameState, logger: nkruntime.Logger): BotMove | null {
    const botGrid    = gameState.array3DPlayerSecend;
    const playerGrid = gameState.array3DPlayerFirst;
    const numRows    = botGrid.length;
    const numCols    = botGrid[0].length;
    const maxTile    = numCols - 1;
    const difficulty = gameState.botDifficulty;

    const emptyCells: { line: number; col: number }[] = [];
    for (let i = 0; i < numRows; i++)
        for (let j = 0; j < numCols; j++)
            if (botGrid[i][j] === -1) emptyCells.push({ line: i, col: j });

    if (emptyCells.length === 0) return null;

    if (difficulty === 0 || (difficulty === 1 && Math.random() < 0.5)) {
        const cell = emptyCells[Math.floor(Math.random() * emptyCells.length)];
        return { line: cell.line, col: cell.col, tile: Math.floor(Math.random() * (maxTile + 1)) };
    }

    let bestScore = -1;
    let bestMove: BotMove = { line: emptyCells[0].line, col: emptyCells[0].col, tile: Math.floor(Math.random() * (maxTile + 1)) };

    for (const cell of emptyCells) {
        for (let tile = 0; tile <= maxTile; tile++) {
            const tempBot = botGrid.map((r: any[]) => r.slice());
            tempBot[cell.line][cell.col] = tile;

            let mineHits = (playerGrid[cell.line] as number[]).filter((v: number) => v === tile).length;
            if (gameState.VerticalMode) {
                for (let r = 0; r < numRows; r++) if (playerGrid[r][cell.col] === tile) mineHits++;
            }
            const moveScore = simulateTotalScore(tempBot, gameState.VerticalMode) + mineHits * 6;
            if (moveScore > bestScore) { bestScore = moveScore; bestMove = { line: cell.line, col: cell.col, tile }; }
        }
    }
    return bestMove;
}

function simulateTotalScore(grid: any[][], verticalMode: boolean): number {
    let score = 0;
    for (let i = 0; i < grid.length; i++) score += scoreRow(grid[i]);
    if (verticalMode)
        for (let col = 0; col < grid[0].length; col++)
            score += scoreRow(grid.map((r: any[]) => r[col]));
    return score;
}

function scoreRow(arr: any[]): number {
    const counts: { [k: string]: number } = {};
    for (const v of arr) { if (v === -1) continue; counts[v] = (counts[v] || 0) + 1; }
    let sum = 0;
    for (const k of Object.keys(counts)) {
        const key = Number(k); const count = counts[k];
        if (count === 4)      return (key + 1) * 16;
        else if (count === 3) sum += (key + 1) * 9;
        else if (count === 2) sum += (key + 1) * 4;
        else                   sum += (key + 1);
    }
    return sum;
}

// ─── Score / Grid Helpers ─────────────────────────────────────────────────────

function TotalScore(array2D: number[][], logger: nkruntime.Logger, mode: boolean): number {
    let score = 0;
    for (let i = 0; i < array2D.length; i++) score += CalculatorArray(array2D[i], logger);
    if (mode) for (let col = 0; col < array2D[0].length; col++)
        score += CalculatorArray(array2D.map(d => d[col]), logger);
    return score;
}

function CalculatorArray(arrayInput: any[], logger: nkruntime.Logger): number {
    const counts: { [k: string]: number } = {};
    for (const v of arrayInput) { if (v === -1) continue; const k = String(v); counts[k] = (counts[k] || 0) + 1; }
    let sum = 0;
    for (const k of Object.keys(counts)) {
        const key = Number(k); const count = counts[k];
        if (count === 4)      return (key + 1) * 16;
        else if (count === 3) sum += (key + 1) * 9;
        else if (count === 2) sum += (key + 1) * 4;
        else                   sum += (key + 1);
    }
    return sum;
}

function CalculatorArray2D(array1: number[][], x: number, y: number, input: number, logger: nkruntime.Logger): number[] {
    const result: number[] = [];
    array1[x].forEach((element, index) => { if (element === input) result.push(index); });
    return result;
}

function CalculatorArray2DWithVertical(array1: number[][], x: number, y: number, input: number, logger: nkruntime.Logger): number[] {
    const result: number[] = [];
    array1.map(r => r[y]).forEach((element, index) => { if (element === input) result.push(index); });
    return result;
}

function ActionWinPlayer(array1: number[][]): boolean {
    for (let i = 0; i < array1.length; i++)
        for (let j = 0; j < array1[i].length; j++)
            if (array1[i][j] === -1) return false;
    return true;
}

// ─── Player Utilities ─────────────────────────────────────────────────────────

function getPlayersCount(players: Player[]): number {
    let count = 0;
    for (let i = 0; i < MaxPlayers; i++) if (players[i] !== undefined) count++;
    return count;
}

function getNextPlayerNumber(players: Player[]): number {
    for (let i = 0; i < MaxPlayers; i++) if (!players[i]) return i;
    return PlayerNotFound;
}

function getPlayerNumber(players: Player[], sessionId: string): number {
    for (let i = 0; i < MaxPlayers; i++)
        if (players[i] && players[i].presence.sessionId === sessionId) return i;
    return PlayerNotFound;
}

function getWinner(playersWins: number[], players: Player[]): Player | null {
    for (let i = 0; i < MaxPlayers; i++)
        if (playersWins[i] === NecessaryWins) return players[i];
    return null;
}

function playerWon(
    message: nkruntime.MatchMessage, gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama
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
    message: nkruntime.MatchMessage, gameState: GameState,
    dispatcher: nkruntime.MatchDispatcher, nakama: nkruntime.Nakama, logger: nkruntime.Logger
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

class ScoreClass { ScoreF: number = 0; }
class CountWin   { win: number = 0; }
