const JoinOrCreateMatchRpc     = "JoinOrCreateMatchRpc";
const CheckPendingRewardsRpc   = "CheckPendingRewardsRpc";
const GetLeaderboardRpc        = "GetLeaderboardRpc";
const GetProfileRpc            = "GetProfileRpc";
const UpdateProfileRpc         = "UpdateProfileRpc";
const SelectAvatarRpc          = "SelectAvatarRpc";
const GetAppVersionRpc         = "GetAppVersionRpc";
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
    initializer.registerRpc(SelectAvatarRpc,         selectAvatarRpc);
    initializer.registerRpc(GetAppVersionRpc,        getAppVersionRpc);

    // Seed default app version config if it doesn't exist yet
    const existing = nk.storageRead([{ collection: CollectionConfig, key: KeyAppVersion, userId: SystemUserId }]);
    if (existing.length === 0) {
        nk.storageWrite([{
            collection:      CollectionConfig,
            key:             KeyAppVersion,
            userId:          SystemUserId,
            value:           { requiredVersion: "", updateUrl: "" } as AppVersionConfig,
            permissionRead:  2,
            permissionWrite: 0,
        }]);
        logger.info("App version config seeded with defaults.");
    }

    // Leaderboard reset → distribute rewards
    initializer.registerLeaderboardReset(onLeaderboardReset);

    // Match handler
    initializer.registerMatch(MatchModuleName, {
        matchInit, matchJoinAttempt, matchJoin, matchLeave,
        matchLoop, matchTerminate, matchSignal,
    });

    logger.info(LogicLoadedLoggerInfo);
}
