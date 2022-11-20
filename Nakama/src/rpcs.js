"use strict";
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
    matches = nakama.matchList(MatchesLimit, true, JSON.stringify(label), MinimumPlayers, MaxPlayers);
    if (matches.length > 0) {
        return matches[0].matchId;
    }
    //test
    ///
    var persons = {};
    persons = { "mode": payload };
    return nakama.matchCreate(MatchModuleName, persons);
};
function CreateLeaderborad(context, logger, nakama) {
    var id = IdLeaderboard;
    var authoritative = true;
    var sort = "desc" /* DESCENDING */;
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
var BeforeAuthenticateCustom = function (ctx, logger, nk, data) {
    var _a;
    logger.info("jwtdwftrewtg");
    var secretKey = "JWT_SECRET_KEY22545641536456";
    if (((_a = data.account) === null || _a === void 0 ? void 0 : _a.id) == null)
        return data;
    logger.info("jwt" + data.account.id);
    var claims = verifyAndParseJwt(secretKey, data.account.id, logger);
    if (!claims) {
        logger.error("error verifying and parsing jwt");
    }
    // Update the incoming authenticate request with the user ID and username
    data.account.id = claims.id;
    data.username = claims.username;
    return data;
};
var jws = require('jsonwebtoken');
var verifyAndParseJwt = function (secretKey, jwtt, logger) {
    var claims = { id: "", username: "" };
    // Use your favourite JWT library to verify the signature and decode the JWT contents
    jws.sign({ foo: 'bar' }, secretKey, { algorithm: 'RS256' }, function (err, token) {
        logger.info(token + " %%%%%%%%%%%%%%%%%%%%%%");
        claims.id = token.id;
        claims.username = token.username;
    });
    // Once verified and decoded, return a Claims object accordingly
    return claims;
};
