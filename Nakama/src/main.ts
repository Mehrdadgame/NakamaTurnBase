const JoinOrCreateMatchRpc     = "JoinOrCreateMatchRpc";
const CheckPendingRewardsRpc   = "CheckPendingRewardsRpc";
const GetLeaderboardRpc        = "GetLeaderboardRpc";
const GetProfileRpc            = "GetProfileRpc";
const UpdateProfileRpc         = "UpdateProfileRpc";
const LogicLoadedLoggerInfo    = "Custom logic loaded.";
const MatchModuleName          = "match";

function InitModule(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    initializer: nkruntime.Initializer
) {
    // Create leaderboards (idempotent — skips if already exist)
    CreateLeaderboards(ctx, logger, nk);

    // Register RPCs
    initializer.registerRpc(JoinOrCreateMatchRpc,   joinOrCreateMatch);
    initializer.registerRpc(CheckPendingRewardsRpc,  checkPendingRewardsRpc);
    initializer.registerRpc(GetLeaderboardRpc,       getLeaderboardRpc);
    initializer.registerRpc(GetProfileRpc,           getProfileRpc);
    initializer.registerRpc(UpdateProfileRpc,        updateProfileRpc);

    // Leaderboard reset → distribute rewards
    initializer.registerLeaderboardReset(onLeaderboardReset);

    // Match handler
    initializer.registerMatch(MatchModuleName, {
        matchInit, matchJoinAttempt, matchJoin, matchLeave,
        matchLoop, matchTerminate, matchSignal,
    });

    logger.info(LogicLoadedLoggerInfo);
}
