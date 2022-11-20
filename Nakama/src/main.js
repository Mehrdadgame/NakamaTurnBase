"use strict";
var JoinOrCreateMatchRpc = "JoinOrCreateMatchRpc";
var LogicLoadedLoggerInfo = "Custom logic loaded.";
var MatchModuleName = "match";
/**
 * "Register the RPC and the match module with the initializer."
 *
 * The first thing we do is register the RPC. We do this by calling the `registerRpc` function on the
 * initializer. This function takes two parameters: the name of the RPC and the function that will be
 * called when the RPC is invoked
 * @param ctx - The context of the module.
 * @param logger - The logger object that you can use to log messages.
 * @param nk - The Nakama server instance.
 * @param initializer - This is the object that allows you to register your RPCs and match modules.
 */
function InitModule(ctx, logger, nk, initializer) {
    initializer.registerRpc(JoinOrCreateMatchRpc, joinOrCreateMatch);
    initializer.registerBeforeAuthenticateCustom(BeforeAuthenticateCustom);
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
