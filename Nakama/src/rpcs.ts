let gameMode: string = "";

let joinOrCreateMatch: nkruntime.RpcFunction = function (
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama,
    payload: string
): string {
    const label: MatchLabel = { open: true, game_mode: payload };

    // Try to find an existing open match for this game mode (with at least 1 player waiting)
    const matches = nakama.matchList(1, true, JSON.stringify(label), 1, MaxPlayers);
    if (matches.length > 0) {
        return matches[0].matchId;
    }

    // No open match found — create a new one
    const persons: { [key: string]: string } = { mode: payload };
    return nakama.matchCreate(MatchModuleName, persons);
};

function CreateLeaderboard(
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama
): void {
    try {
        nakama.leaderboardCreate(
            IdLeaderboard,
            true,
            nkruntime.SortOrder.DESCENDING,
            nkruntime.Operator.BEST,
            null,
            {}
        );
    } catch (error) {
        // Already exists — ignore
    }
}
