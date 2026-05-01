const JoinOrCreateMatchRpc  = "JoinOrCreateMatchRpc";
const PurchaseCardRpc       = "PurchaseCardRpc";
const GetCardsRpc           = "GetCardsRpc";
const LogicLoadedLoggerInfo = "Custom logic loaded.";
const MatchModuleName       = "match";

function InitModule(
    ctx: nkruntime.Context,
    logger: nkruntime.Logger,
    nk: nkruntime.Nakama,
    initializer: nkruntime.Initializer
) {
    initializer.registerRpc(JoinOrCreateMatchRpc, joinOrCreateMatch);
    initializer.registerRpc(PurchaseCardRpc,      purchaseCardRpc);
    initializer.registerRpc(GetCardsRpc,          getCardsRpc);

    initializer.registerMatch(MatchModuleName, {
        matchInit,
        matchJoinAttempt,
        matchJoin,
        matchLeave,
        matchLoop,
        matchTerminate,
        matchSignal
    });

    logger.info(LogicLoadedLoggerInfo);
}
