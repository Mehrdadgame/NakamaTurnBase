"use strict";
var JoinOrCreateMatchRpc = "JoinOrCreateMatchRpc";
var JoinTutorialMatchRpc = "JoinTutorialMatchRpc";
var CheckPendingRewardsRpc = "CheckPendingRewardsRpc";
var GetLeaderboardRpc = "GetLeaderboardRpc";
var GetProfileRpc = "GetProfileRpc";
var UpdateProfileRpc = "UpdateProfileRpc";
var SaveProgressionRpc = "SaveProgressionRpc";
var SelectAvatarRpc = "SelectAvatarRpc";
var GetAppVersionRpc = "GetAppVersionRpc";
var SendContactMessageRpc = "SendContactMessageRpc";
var ClaimChestRpc = "ClaimChestRpc";
var GetChestStatusRpc = "GetChestStatusRpc";
var GetChatConfigRpc = "GetChatConfigRpc";
var AdminStatsRpc = "AdminStatsRpc";
var SetConfigRpc = "SetConfigRpc";
var VerifyCoinPurchaseRpc = "VerifyCoinPurchaseRpc";
var LogicLoadedLoggerInfo = "Custom logic loaded.";
var MatchModuleName = "match";
var CollectionMissionDefinitions = "MissionDefinitions";
var KeyMissionDefinitions = "mission_definitions";
function InitModule(ctx, logger, nk, initializer) {
    // Create leaderboards (idempotent — skips if already exist)
    CreateLeaderboards(ctx, logger, nk);
    // Register RPCs
    initializer.registerRpc(JoinOrCreateMatchRpc, joinOrCreateMatch);
    initializer.registerRpc(JoinTutorialMatchRpc, joinTutorialMatchRpc);
    initializer.registerRpc(CheckPendingRewardsRpc, checkPendingRewardsRpc);
    initializer.registerRpc(GetLeaderboardRpc, getLeaderboardRpc);
    initializer.registerRpc(GetProfileRpc, getProfileRpc);
    initializer.registerRpc(SaveProgressionRpc, saveProgressionRpc);
    initializer.registerRpc(UpdateProfileRpc, updateProfileRpc);
    initializer.registerRpc(SelectAvatarRpc, selectAvatarRpc);
    initializer.registerRpc(GetAppVersionRpc, getAppVersionRpc);
    initializer.registerRpc(VerifyCoinPurchaseRpc, verifyCoinPurchaseRpc);
    initializer.registerRpc(ClaimChestRpc, claimChestRpc);
    initializer.registerRpc(GetChestStatusRpc, getChestStatusRpc);
    initializer.registerRpc(SendContactMessageRpc, sendContactMessageRpc);
    initializer.registerRpc(GetChatConfigRpc, getChatConfigRpc);
    initializer.registerRpc(AdminStatsRpc, adminStatsRpc);
    initializer.registerRpc(SetConfigRpc, setConfigRpc);
    // Seed default app version config if it doesn't exist yet
    var existing = nk.storageRead([{ collection: CollectionConfig, key: KeyAppVersion, userId: SystemUserId }]);
    if (existing.length === 0) {
        nk.storageWrite([{
                collection: CollectionConfig,
                key: KeyAppVersion,
                userId: SystemUserId,
                value: { requiredVersion: "", updateUrl: "" },
                permissionRead: 2,
                permissionWrite: 0,
            }]);
        logger.info("App version config seeded with defaults.");
    }
    // Leaderboard reset → distribute rewards
    initializer.registerLeaderboardReset(onLeaderboardReset);
    // Match handler
    initializer.registerMatch(MatchModuleName, {
        matchInit: matchInit,
        matchJoinAttempt: matchJoinAttempt,
        matchJoin: matchJoin,
        matchLeave: matchLeave,
        matchLoop: matchLoop,
        matchTerminate: matchTerminate,
        matchSignal: matchSignal,
    });
    logger.info(LogicLoadedLoggerInfo);
}
var joinOrCreateMatch = function (context, logger, nakama, payload) {
    var label = { open: true, game_mode: payload };
    var matches = nakama.matchList(1, true, JSON.stringify(label), 1, MaxPlayers);
    if (matches.length > 0)
        return matches[0].matchId;
    return nakama.matchCreate(MatchModuleName, { mode: payload });
};
// Creates a private tutorial match (bot-only, no entry fee, no rewards).
// Lobby countdown is 3 s so the client has time to load the game scene before ChangeScene fires.
var joinTutorialMatchRpc = function (context, logger, nakama, payload) {
    var matchId = nakama.matchCreate(MatchModuleName, {
        mode: "ThreeByThree",
        tutorial: "true",
    });
    logger.info("Tutorial match created: " + matchId + " for userId=" + context.userId);
    return matchId;
};
function createDefaultProfile() {
    return {
        email: "", phone: "",
        emailLocked: false, phoneLocked: false,
        emailBonusClaimed: false, phoneBonusClaimed: false,
        avatarId: "avatar_0",
        ownedAvatars: [],
        welcomeBonusClaimed: false,
    };
}
function createDefaultMissionDefinitions() {
    return [
        {
            missionId: "play_first_match",
            title: "اولین مبارزه",
            description: "یک مسابقه انجام بده.",
            goalType: "PlayMatches",
            target: 1,
            rewardXp: 50,
            isRepeatable: false,
        },
        {
            missionId: "win_first_match",
            title: "اولین پیروزی",
            description: "یک مسابقه را ببرید.",
            goalType: "WinMatches",
            target: 1,
            rewardXp: 100,
            isRepeatable: false,
        },
        {
            missionId: "place_20_tiles",
            title: "یک بازیکن پرتلاش",
            description: "۲۰ خانه روی صفحه قرار بده.",
            goalType: "PlaceTiles",
            target: 20,
            rewardXp: 80,
            isRepeatable: true,
        },
        {
            missionId: "clear_12_tiles",
            title: "پاکسازی تاس",
            description: "۱۲ خانه از صفحه بردار.",
            goalType: "ClearTiles",
            target: 12,
            rewardXp: 70,
            isRepeatable: true,
        },
    ];
}
function loadOrSeedMissionDefinitions(nakama, logger) {
    var stored = nakama.storageRead([
        { collection: CollectionMissionDefinitions, key: KeyMissionDefinitions, userId: SystemUserId },
    ]);
    if (stored.length > 0) {
        var storedValue = stored[0].value;
        var definitions = Array.isArray(storedValue)
            ? storedValue
            : (storedValue && Array.isArray(storedValue.missions) ? storedValue.missions : null);
        if (definitions && definitions.length > 0)
            return definitions;
    }
    if (stored.length > 0 && stored[0].value)
        logger.warn("Mission definitions storage record is not a non-empty array; reseeding defaults.");
    var definitions = createDefaultMissionDefinitions();
    nakama.storageWrite([{
            collection: CollectionMissionDefinitions,
            key: KeyMissionDefinitions,
            userId: SystemUserId,
            value: { missions: definitions },
            permissionRead: 2,
            permissionWrite: 0,
        }]);
    logger.info("Seeded default mission definitions.");
    return definitions;
}
function grantFirstLoginBonusIfNeeded(userId, profileObj, profile, logger, nakama) {
    if (profile.welcomeBonusClaimed)
        return profile;
    var updatedProfile = {
        email: profile.email,
        phone: profile.phone,
        emailLocked: profile.emailLocked,
        phoneLocked: profile.phoneLocked,
        emailBonusClaimed: profile.emailBonusClaimed,
        phoneBonusClaimed: profile.phoneBonusClaimed,
        avatarId: profile.avatarId,
        ownedAvatars: profile.ownedAvatars,
        welcomeBonusClaimed: true,
    };
    var writeRequest = {
        collection: CollectionProfile,
        key: KeyProfileData,
        userId: userId,
        value: updatedProfile,
        permissionRead: 1,
        permissionWrite: 0,
    };
    // Compare-and-swap prevents duplicate bonus on concurrent first-login RPC calls.
    writeRequest.version = profileObj && profileObj.version ? profileObj.version : "*";
    try {
        nakama.storageWrite([writeRequest]);
        nakama.walletUpdate(userId, { coins: FirstLoginBonusCoins }, { source: "first_login_bonus" }, true);
        logger.info("First login bonus granted: userId=" + userId + " coins=" + FirstLoginBonusCoins);
        return updatedProfile;
    }
    catch (e) {
        // Another concurrent request may have already claimed the bonus.
        logger.info("First login bonus skipped (already claimed or conflict): " + e);
        var latest = nakama.storageRead([{ collection: CollectionProfile, key: KeyProfileData, userId: userId }]);
        if (latest.length > 0)
            return latest[0].value;
        return updatedProfile;
    }
}
// Called on login — returns unclaimed weekly/monthly rewards and marks them claimed.
// Coins were already added to the wallet during the leaderboard reset hook.
var checkPendingRewardsRpc = function (context, logger, nakama, payload) {
    var userId = context.userId;
    if (!userId)
        throw new Error("Not authenticated");
    var stored = nakama.storageRead([
        { collection: CollectionSeason, key: KeyPendingRewardWeekly, userId: userId },
        { collection: CollectionSeason, key: KeyPendingRewardMonthly, userId: userId },
    ]);
    var weekly = null;
    var monthly = null;
    for (var _i = 0, stored_1 = stored; _i < stored_1.length; _i++) {
        var obj = stored_1[_i];
        var val = obj.value;
        if (!val.claimed) {
            if (obj.key === KeyPendingRewardWeekly)
                weekly = val;
            if (obj.key === KeyPendingRewardMonthly)
                monthly = val;
        }
    }
    // Mark as claimed so popup only shows once
    var toWrite = [];
    if (weekly) {
        weekly.claimed = true;
        toWrite.push({ collection: CollectionSeason, key: KeyPendingRewardWeekly, userId: userId, value: weekly, permissionRead: 1, permissionWrite: 0 });
    }
    if (monthly) {
        monthly.claimed = true;
        toWrite.push({ collection: CollectionSeason, key: KeyPendingRewardMonthly, userId: userId, value: monthly, permissionRead: 1, permissionWrite: 0 });
    }
    if (toWrite.length > 0)
        nakama.storageWrite(toWrite);
    return JSON.stringify({ weekly: weekly, monthly: monthly });
};
// Returns top-N records for weekly or monthly leaderboard + caller's own record.
// Each record includes avatarId fetched from profile storage.
var getLeaderboardRpc = function (context, logger, nakama, payload) {
    var _a;
    var data = JSON.parse(payload || "{}");
    var isMonthly = data.type === "monthly";
    var lbId = isMonthly ? LeaderboardMonthly : LeaderboardWeekly;
    var rewards = isMonthly ? MONTHLY_REWARDS : WEEKLY_REWARDS;
    var limit = data.limit || 100;
    var userId = context.userId;
    var records = [];
    var ownRecord = null;
    try {
        var result = nakama.leaderboardRecordsList(lbId, [], limit, "", 0);
        records = result.records || [];
        if (userId) {
            var own = nakama.leaderboardRecordsList(lbId, [userId], 1, "", 0);
            ownRecord = (own.ownerRecords && own.ownerRecords.length > 0) ? own.ownerRecords[0] : null;
        }
    }
    catch (e) {
        logger.warn("getLeaderboardRpc failed: " + e);
    }
    // Batch-read profiles to get avatarId for each player
    var storageReads = [];
    var seenIds = {};
    for (var _i = 0, records_1 = records; _i < records_1.length; _i++) {
        var r = records_1[_i];
        if (r.ownerId && !seenIds[r.ownerId]) {
            storageReads.push({ collection: CollectionProfile, key: KeyProfileData, userId: r.ownerId });
            seenIds[r.ownerId] = true;
        }
    }
    if (ownRecord && ownRecord.ownerId && !seenIds[ownRecord.ownerId]) {
        storageReads.push({ collection: CollectionProfile, key: KeyProfileData, userId: ownRecord.ownerId });
    }
    var profileMap = {};
    if (storageReads.length > 0) {
        try {
            var profiles = nakama.storageRead(storageReads);
            for (var _b = 0, profiles_1 = profiles; _b < profiles_1.length; _b++) {
                var obj = profiles_1[_b];
                profileMap[obj.userId] = obj.value;
            }
        }
        catch (e) {
            logger.warn("Profile batch read failed: " + e);
        }
    }
    var enriched = records.map(function (r) {
        var _a;
        return ({
            ownerId: r.ownerId,
            username: r.username,
            score: r.score,
            rank: r.rank,
            avatarId: ((_a = profileMap[r.ownerId]) === null || _a === void 0 ? void 0 : _a.avatarId) || "avatar_0",
        });
    });
    var enrichedOwn = ownRecord ? {
        ownerId: ownRecord.ownerId,
        username: ownRecord.username,
        score: ownRecord.score,
        rank: ownRecord.rank,
        avatarId: ((_a = profileMap[ownRecord.ownerId]) === null || _a === void 0 ? void 0 : _a.avatarId) || "avatar_0",
    } : null;
    return JSON.stringify({ records: enriched, ownRecord: enrichedOwn, rewards: rewards });
};
// ─── Avatar ───────────────────────────────────────────────────────────────────
var selectAvatarRpc = function (context, logger, nakama, payload) {
    var userId = context.userId;
    if (!userId)
        throw new Error("Not authenticated");
    var input = JSON.parse(payload || "{}");
    if (!input.avatarId)
        return JSON.stringify({ success: false, error: "Missing avatarId" });
    var price = AVATAR_PRICES.hasOwnProperty(input.avatarId)
        ? AVATAR_PRICES[input.avatarId]
        : -1;
    if (price < 0)
        return JSON.stringify({ success: false, error: "Unknown avatar id" });
    var stored = nakama.storageRead([
        { collection: CollectionProfile, key: KeyProfileData, userId: userId },
    ]);
    var profile = createDefaultProfile();
    if (stored.length > 0)
        profile = stored[0].value;
    var ownedAvatars = profile.ownedAvatars || [];
    var alreadyOwned = ownedAvatars.indexOf(input.avatarId) >= 0;
    if (price > 0 && !alreadyOwned) {
        var account = nakama.accountGetId(userId);
        var wallet = account.wallet || {};
        if ((wallet["coins"] || 0) < price)
            return JSON.stringify({ success: false, error: "Insufficient coins" });
        try {
            nakama.walletUpdate(userId, { coins: -price }, { source: "avatar_purchase", avatarId: input.avatarId }, true);
        }
        catch (e) {
            logger.warn("Avatar purchase wallet deduct failed: " + e);
            return JSON.stringify({ success: false, error: "Payment failed" });
        }
        ownedAvatars.push(input.avatarId);
    }
    profile.ownedAvatars = ownedAvatars;
    profile.avatarId = input.avatarId;
    nakama.storageWrite([{
            collection: CollectionProfile,
            key: KeyProfileData,
            userId: userId,
            value: profile,
            permissionRead: 1,
            permissionWrite: 0,
        }]);
    logger.info("Avatar selected: userId=" + userId + " avatarId=" + input.avatarId + " price=" + price + " alreadyOwned=" + alreadyOwned);
    return JSON.stringify({ success: true, avatarId: input.avatarId, ownedAvatars: ownedAvatars, error: "" });
};
var getProfileRpc = function (context, logger, nakama, payload) {
    var userId = context.userId;
    if (!userId)
        throw new Error("Not authenticated");
    var account = nakama.accountGetId(userId);
    var displayName = account.user.displayName || account.user.username || "";
    var stored = nakama.storageRead([
        { collection: CollectionProfile, key: KeyProfileData, userId: userId },
    ]);
    var profileObj = stored.length > 0 ? stored[0] : null;
    var profile = profileObj
        ? profileObj.value
        : createDefaultProfile();
    profile = grantFirstLoginBonusIfNeeded(userId, profileObj, profile, logger, nakama);
    var ownedAvatars = profile.ownedAvatars || [];
    var effectiveOwned = [];
    var allIds = Object.keys(AVATAR_PRICES);
    for (var _i = 0, allIds_1 = allIds; _i < allIds_1.length; _i++) {
        var id = allIds_1[_i];
        if (AVATAR_PRICES[id] === 0)
            effectiveOwned.push(id);
    }
    for (var _a = 0, ownedAvatars_1 = ownedAvatars; _a < ownedAvatars_1.length; _a++) {
        var id = ownedAvatars_1[_a];
        if (effectiveOwned.indexOf(id) < 0)
            effectiveOwned.push(id);
    }
    var avatarPriceList = allIds.map(function (id) { return ({
        id: id,
        price: AVATAR_PRICES[id],
    }); });
    return JSON.stringify({
        displayName: displayName,
        email: profile.email,
        phone: profile.phone,
        emailLocked: profile.emailLocked,
        phoneLocked: profile.phoneLocked,
        avatarId: profile.avatarId || "avatar_0",
        ownedAvatars: effectiveOwned,
        avatarPrices: avatarPriceList,
        missionDefinitions: loadOrSeedMissionDefinitions(nakama, logger),
        progression: profile.progression || { currentXp: 0, currentLevel: 1, missions: [] },
    });
};
var saveProgressionRpc = function (context, logger, nakama, payload) {
    var userId = context.userId;
    if (!userId)
        throw new Error("Not authenticated");
    var input = JSON.parse(payload || "{}");
    var stored = nakama.storageRead([
        { collection: CollectionProfile, key: KeyProfileData, userId: userId },
    ]);
    var profile = createDefaultProfile();
    if (stored.length > 0)
        profile = stored[0].value;
    var definitions = loadOrSeedMissionDefinitions(nakama, logger);
    var definitionsById = {};
    for (var i = 0; i < definitions.length; i++) {
        if (definitions[i] && definitions[i].missionId)
            definitionsById[definitions[i].missionId] = definitions[i];
    }
    var missions = [];
    var seenMissionIds = {};
    if (Array.isArray(input.missions)) {
        for (var j = 0; j < input.missions.length; j++) {
            var incoming = input.missions[j] || {};
            var missionId = stringField(incoming.missionId);
            var definition = definitionsById[missionId];
            if (!definition || seenMissionIds[missionId])
                continue;
            seenMissionIds[missionId] = true;
            var target = Math.max(1, Math.floor(numberField(definition.target, 1)));
            var progress = Math.max(0, Math.min(target, Math.floor(numberField(incoming.currentProgress, 0))));
            missions.push({
                missionId: missionId,
                currentProgress: progress,
                isCompleted: Boolean(incoming.isCompleted) && (progress >= target || definition.isRepeatable),
            });
        }
    }
    profile.progression = {
        currentXp: Math.max(0, Math.min(100000000, Math.floor(numberField(input.currentXp, 0)))),
        currentLevel: Math.max(1, Math.min(1000, Math.floor(numberField(input.currentLevel, 1)))),
        missions: missions,
    };
    nakama.storageWrite([{
            collection: CollectionProfile,
            key: KeyProfileData,
            userId: userId,
            value: profile,
            permissionRead: 1,
            permissionWrite: 0,
        }]);
    return JSON.stringify({ success: true, progression: profile.progression });
};
var updateProfileRpc = function (context, logger, nakama, payload) {
    var userId = context.userId;
    if (!userId)
        throw new Error("Not authenticated");
    var input = JSON.parse(payload || "{}");
    // Load existing profile
    var stored = nakama.storageRead([
        { collection: CollectionProfile, key: KeyProfileData, userId: userId },
    ]);
    var profile = createDefaultProfile();
    if (stored.length > 0)
        profile = stored[0].value;
    // ── Display name ────────────────────────────────────────────────────────
    if (input.displayName && input.displayName.trim().length > 0) {
        try {
            nakama.accountUpdateId(userId, null, input.displayName.trim(), null, null, null, null, null);
        }
        catch (e) {
            logger.warn("displayName update failed: " + e);
        }
    }
    var coinsAwarded = 0;
    // ── Email (locked once set) ─────────────────────────────────────────────
    if (input.email && input.email.trim().length > 0 && !profile.emailLocked) {
        profile.email = input.email.trim();
        profile.emailLocked = true;
        if (!profile.emailBonusClaimed) {
            coinsAwarded += 100;
            profile.emailBonusClaimed = true;
        }
    }
    // ── Phone (locked once set) ─────────────────────────────────────────────
    if (input.phone && input.phone.trim().length > 0 && !profile.phoneLocked) {
        profile.phone = input.phone.trim();
        profile.phoneLocked = true;
        if (!profile.phoneBonusClaimed) {
            coinsAwarded += 100;
            profile.phoneBonusClaimed = true;
        }
    }
    // ── Award coins ─────────────────────────────────────────────────────────
    if (coinsAwarded > 0) {
        try {
            nakama.walletUpdate(userId, { coins: coinsAwarded }, { source: "profile_bonus" }, true);
        }
        catch (e) {
            logger.warn("Profile bonus wallet update failed: " + e);
        }
    }
    // ── Save profile ────────────────────────────────────────────────────────
    nakama.storageWrite([{
            collection: CollectionProfile,
            key: KeyProfileData,
            userId: userId,
            value: profile,
            permissionRead: 1,
            permissionWrite: 0,
        }]);
    var updated = nakama.accountGetId(userId);
    return JSON.stringify({
        displayName: updated.user.displayName || updated.user.username || "",
        email: profile.email,
        phone: profile.phone,
        emailLocked: profile.emailLocked,
        phoneLocked: profile.phoneLocked,
        coinsAwarded: coinsAwarded,
        error: "",
    });
};
// ─── Coin Shop (IAP) ──────────────────────────────────────────────────────────
var COIN_PACKS = {
    "SmallCoin_TasZan": 3500,
    "Standard_TasZan": 8000,
    "Large_TasZan": 13000,
    "VIP_TasZan": 18000,
    "King_TasZan": 28000,
    "Legend_TasZan": 40000,
    "Diamond_TasZan": 55000,
    "Empire_TasZan": 100000, // امپراتور
};
var PaymentCollection = "payment";
var KeyMyketToken = "myket_iap_token";
var KeyCafeBazaarToken = "cafebazaar_iap_token";
var KeyBazaarToken = "bazaar_iap_token";
var MarketplaceHttpTimeoutMs = 5000;
var MyketValidationBaseUrl = "https://developer.myket.ir/api/applications";
var CafeBazaarValidationBaseUrl = "https://pardakht.cafebazaar.ir/devapi/v2/api/validate";
function normalizePaymentStore(rawStore) {
    var value = (rawStore || "myket").toLowerCase().replace(/[-_\s]/g, "");
    if (value === "myket")
        return "myket";
    if (value === "bazaar" || value === "cafebazaar")
        return "cafebazaar";
    return null;
}
function envFirst(context, keys) {
    for (var _i = 0, keys_1 = keys; _i < keys_1.length; _i++) {
        var key = keys_1[_i];
        var value = context.env[key];
        if (value && value.length > 0)
            return value;
    }
    return "";
}
function configFirst(nakama, keys) {
    var reads = [];
    for (var i = 0; i < keys.length; i++)
        reads.push({ collection: CollectionConfig, key: keys[i], userId: SystemUserId });
    try {
        var records = nakama.storageRead(reads);
        for (var i = 0; i < records.length; i++) {
            var value = records[i].value || {};
            var token = stringField(value.token || value.accessToken || value.apiToken || value.secret);
            if (token)
                return token;
        }
    }
    catch (e) { }
    return "";
}
function parseJsonObject(json) {
    if (!json || json.length === 0)
        return null;
    try {
        var parsed = JSON.parse(json);
        if (parsed && typeof parsed === "object")
            return parsed;
    }
    catch (e) { }
    return null;
}
function stringField(value) {
    if (value === null || value === undefined)
        return "";
    return String(value);
}
function numberField(value, fallback) {
    if (value === null || value === undefined || value === "")
        return fallback;
    var parsed = Number(value);
    return isNaN(parsed) ? fallback : parsed;
}
function getInputDeveloperPayload(input) {
    if (input.developerPayload && input.developerPayload.length > 0)
        return input.developerPayload;
    var purchaseJson = parseJsonObject(input.originalJson);
    if (!purchaseJson)
        return "";
    return stringField(purchaseJson["developerPayload"] || purchaseJson["payload"]);
}
function getInputPackageName(input) {
    if (input.packageName && input.packageName.length > 0)
        return input.packageName;
    var purchaseJson = parseJsonObject(input.originalJson);
    return purchaseJson ? stringField(purchaseJson["packageName"]) : "";
}
function getMarketplacePackageName(context, store, input) {
    var envPackage = store === "cafebazaar"
        ? envFirst(context, ["CAFEBAZAAR_PACKAGE_NAME", "BAZAAR_PACKAGE_NAME", "APP_PACKAGE_NAME"])
        : envFirst(context, ["MYKET_PACKAGE_NAME", "APP_PACKAGE_NAME"]);
    var inputPackage = getInputPackageName(input);
    var packageName = envPackage || inputPackage;
    if (!packageName)
        return { packageName: "", error: "Missing package name config" };
    if (envPackage && inputPackage && envPackage !== inputPackage)
        return { packageName: packageName, error: "Package name mismatch" };
    return { packageName: packageName, error: "" };
}
function httpGetJson(nakama, url, headers) {
    var response = nakama.httpRequest(url, "get", headers, undefined, MarketplaceHttpTimeoutMs);
    var raw = {};
    if (response.body && response.body.length > 0) {
        try {
            raw = JSON.parse(response.body);
        }
        catch (e) {
            return { code: response.code, raw: response.body, error: "Invalid marketplace JSON response" };
        }
    }
    if (response.code < 200 || response.code >= 300)
        return { code: response.code, raw: raw, error: marketplaceError(raw, response.code) };
    return { code: response.code, raw: raw, error: "" };
}
function marketplaceError(raw, code) {
    if (raw && typeof raw === "object") {
        var error = stringField(raw["error"]);
        var description = stringField(raw["error_description"] || raw["error_desciption"]);
        if (error || description)
            return ("Marketplace validation failed (" + code + "): " + error + " " + description).trim();
    }
    return "Marketplace validation failed: HTTP " + code;
}
function validateCafeBazaarPurchase(context, nakama, input) {
    var token = configFirst(nakama, [KeyCafeBazaarToken, KeyBazaarToken]) ||
        envFirst(context, ["CAFEBAZAAR_PISHKHAN_API_SECRET", "BAZAAR_PISHKHAN_API_SECRET", "CAFEBAZAAR_API_SECRET"]);
    var packageResult = getMarketplacePackageName(context, "cafebazaar", input);
    if (!token)
        return invalidValidation("cafebazaar", packageResult.packageName, "Missing config/cafebazaar_iap_token or CAFEBAZAAR_PISHKHAN_API_SECRET", 0, {});
    if (packageResult.error)
        return invalidValidation("cafebazaar", packageResult.packageName, packageResult.error, 0, {});
    var url = CafeBazaarValidationBaseUrl + "/" + encodeURIComponent(packageResult.packageName) + "/inapp/" + encodeURIComponent(input.productId || "") + "/purchases/" + encodeURIComponent(input.purchaseToken || "") + "/";
    var response = httpGetJson(nakama, url, {
        "Accept": "application/json",
        "CAFEBAZAAR-PISHKHAN-API-SECRET": token,
    });
    if (response.error)
        return invalidValidation("cafebazaar", packageResult.packageName, response.error, response.code, response.raw);
    var purchaseState = numberField(response.raw["purchaseState"], -1);
    var consumptionState = numberField(response.raw["consumptionState"], -1);
    if (purchaseState !== 0)
        return invalidValidation("cafebazaar", packageResult.packageName, "Purchase is not successful", response.code, response.raw);
    return {
        valid: true,
        store: "cafebazaar",
        packageName: packageResult.packageName,
        responseCode: response.code,
        raw: response.raw,
        developerPayload: stringField(response.raw["developerPayload"]),
        purchaseTime: numberField(response.raw["purchaseTime"], 0),
        purchaseState: purchaseState,
        consumptionState: consumptionState,
        error: "",
    };
}
function validateMyketPurchase(context, nakama, input) {
    var token = configFirst(nakama, [KeyMyketToken]) ||
        envFirst(context, ["MYKET_ACCESS_TOKEN", "MYKET_API_TOKEN"]);
    var packageResult = getMarketplacePackageName(context, "myket", input);
    if (!token)
        return invalidValidation("myket", packageResult.packageName, "Missing config/myket_iap_token or MYKET_ACCESS_TOKEN", 0, {});
    if (packageResult.error)
        return invalidValidation("myket", packageResult.packageName, packageResult.error, 0, {});
    var url = MyketValidationBaseUrl + "/" + encodeURIComponent(packageResult.packageName) + "/purchases/products/" + encodeURIComponent(input.productId || "") + "/tokens/" + encodeURIComponent(input.purchaseToken || "");
    var response = httpGetJson(nakama, url, {
        "Accept": "application/json",
        "X-Access-Token": token,
    });
    if (response.error)
        return invalidValidation("myket", packageResult.packageName, response.error, response.code, response.raw);
    var purchaseState = numberField(response.raw["purchaseState"], -1);
    var consumptionState = numberField(response.raw["consumptionState"], -1);
    if (purchaseState !== 0)
        return invalidValidation("myket", packageResult.packageName, "Purchase is not successful", response.code, response.raw);
    return {
        valid: true,
        store: "myket",
        packageName: packageResult.packageName,
        responseCode: response.code,
        raw: response.raw,
        developerPayload: stringField(response.raw["developerPayload"]),
        purchaseTime: numberField(response.raw["purchaseTime"], 0),
        purchaseState: purchaseState,
        consumptionState: consumptionState,
        error: "",
    };
}
function invalidValidation(store, packageName, error, responseCode, raw) {
    return {
        valid: false,
        store: store,
        packageName: packageName,
        responseCode: responseCode,
        raw: raw,
        developerPayload: "",
        purchaseTime: 0,
        purchaseState: -1,
        consumptionState: -1,
        error: error,
    };
}
function validateDeveloperPayload(userId, input, marketplacePayload) {
    var inputPayload = getInputDeveloperPayload(input);
    if (inputPayload && marketplacePayload && inputPayload !== marketplacePayload)
        return "Developer payload mismatch";
    var payload = parseJsonObject(inputPayload || marketplacePayload);
    if (!payload)
        return "";
    var payloadUserId = stringField(payload["userId"]);
    var payloadProductId = stringField(payload["productId"]);
    if (payloadUserId && payloadUserId !== userId)
        return "Developer payload user mismatch";
    if (payloadProductId && payloadProductId !== input.productId)
        return "Developer payload product mismatch";
    return "";
}
function paymentStorageKey(nakama, store, input) {
    var source = input.orderId && input.orderId.length > 0 ? input.orderId : (input.purchaseToken || "");
    return store + "_" + nakama.sha256Hash(source);
}
var verifyCoinPurchaseRpc = function (context, logger, nakama, payload) {
    var userId = context.userId;
    if (!userId)
        throw new Error("Not authenticated");
    var input = JSON.parse(payload || "{}");
    var store = normalizePaymentStore(input.store);
    if (!store)
        return JSON.stringify({ success: false, error: "Unknown store: " + input.store });
    if (!input.productId || !input.purchaseToken)
        return JSON.stringify({ success: false, error: "Missing purchase data" });
    var coins = COIN_PACKS[input.productId];
    if (!coins)
        return JSON.stringify({ success: false, error: "Unknown product: " + input.productId });
    var storageKey = paymentStorageKey(nakama, store, input);
    // Idempotency — reject duplicate tokens
    var existingReads = [
        { collection: PaymentCollection, key: storageKey, userId: SystemUserId },
    ];
    var legacyKey = (input.orderId && input.orderId.length > 0) ? input.orderId : "";
    if (legacyKey && legacyKey.length <= 128)
        existingReads.push({ collection: PaymentCollection, key: legacyKey, userId: userId });
    var existing = nakama.storageRead(existingReads);
    if (existing.length > 0)
        return JSON.stringify({ success: false, error: "Already processed" });
    var validation = store === "cafebazaar"
        ? validateCafeBazaarPurchase(context, nakama, input)
        : validateMyketPurchase(context, nakama, input);
    if (!validation.valid) {
        logger.warn("IAP validation failed: userId=" + userId + " store=" + store + " product=" + input.productId + " error=" + validation.error);
        return JSON.stringify({ success: false, error: validation.error || "Invalid purchase" });
    }
    var payloadError = validateDeveloperPayload(userId, input, validation.developerPayload);
    if (payloadError) {
        logger.warn("IAP payload validation failed: userId=" + userId + " store=" + store + " product=" + input.productId + " error=" + payloadError);
        return JSON.stringify({ success: false, error: payloadError });
    }
    // Log payment record
    nakama.storageWrite([{
            collection: PaymentCollection,
            key: storageKey,
            userId: SystemUserId,
            value: {
                store: store,
                userId: userId,
                packageName: validation.packageName,
                productId: input.productId,
                purchaseToken: input.purchaseToken,
                orderId: input.orderId,
                purchaseState: validation.purchaseState,
                purchaseTime: validation.purchaseTime,
                consumptionState: validation.consumptionState,
                developerPayload: validation.developerPayload || getInputDeveloperPayload(input),
                dataSignature: input.dataSignature,
                originalJson: input.originalJson,
                validation: validation.raw,
                coinsAwarded: coins,
                timestamp: Date.now(),
            },
            permissionRead: 0,
            permissionWrite: 0,
        }]);
    // Award coins
    nakama.walletUpdate(userId, { coins: coins }, { source: "iap", store: store, productId: input.productId, orderId: input.orderId }, true);
    logger.info("IAP purchase: userId=" + userId + " store=" + store + " product=" + input.productId + " coins=" + coins);
    return JSON.stringify({ success: true, coinsAwarded: coins });
};
// ─── Force Update ─────────────────────────────────────────────────────────────
// Returns { requiredVersion, updateUrl } stored under the system config collection.
// Admin updates the value via Nakama console → Storage → collection "config", key "app_version".
// ΓöÇΓöÇΓöÇ Chest Reward System ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
var CHEST_COOLDOWN_SEC = 3 * 60 * 60; // 3 hours
// Reward table ΓÇö weights sum to 100
var CHEST_REWARDS = [
    { coins: 50,   weight: 38 },
    { coins: 100,  weight: 25 },
    { coins: 200,  weight: 17 },
    { coins: 300,  weight: 10 },
    { coins: 500,  weight: 6  },
    { coins: 750,  weight: 3  },
    { coins: 1000, weight: 1  },
];
function rollChest() {
    var roll = Math.random() * 100;
    var cumulative = 0;
    for (var i = 0; i < CHEST_REWARDS.length; i++) {
        cumulative += CHEST_REWARDS[i].weight;
        if (roll < cumulative) return CHEST_REWARDS[i].coins;
    }
    return 50;
}
var getChestStatusRpc = function (context, logger, nakama, payload) {
    var userId = context.userId;
    if (!userId) throw new Error("Not authenticated");
    var now = Date.now();
    var records = nakama.storageRead([{ collection: "chest", key: "last_claim", userId: userId }]);
    var lastClaimAt = records.length > 0 ? (records[0].value.lastClaimAt || 0) : 0;
    var elapsed = now - lastClaimAt;
    var cooldownMs = CHEST_COOLDOWN_SEC * 1000;
    var remainingSec = elapsed >= cooldownMs ? 0 : Math.ceil((cooldownMs - elapsed) / 1000);
    return JSON.stringify({ remainingSeconds: remainingSec, ready: remainingSec <= 0 });
};
var claimChestRpc = function (context, logger, nakama, payload) {
    var userId = context.userId;
    if (!userId) throw new Error("Not authenticated");
    var now = Date.now();
    var cooldownMs = CHEST_COOLDOWN_SEC * 1000;
    var records = nakama.storageRead([{ collection: "chest", key: "last_claim", userId: userId }]);
    var lastClaimAt = records.length > 0 ? (records[0].value.lastClaimAt || 0) : 0;
    var elapsed = now - lastClaimAt;
    if (elapsed < cooldownMs) {
        var remainingSec = Math.ceil((cooldownMs - elapsed) / 1000);
        return JSON.stringify({ success: false, error: "not_ready", remainingSeconds: remainingSec });
    }
    var coins = rollChest();
    nakama.walletUpdate(userId, { coins: coins }, { source: "chest" }, true);
    nakama.storageWrite([{
        collection: "chest",
        key: "last_claim",
        userId: userId,
        value: { lastClaimAt: now, lastReward: coins },
        permissionRead: 1,
        permissionWrite: 0,
    }]);
    logger.info("[Chest] userId=" + userId + " won=" + coins + " coins");
    return JSON.stringify({ success: true, coinsAwarded: coins, remainingSeconds: CHEST_COOLDOWN_SEC });
};

// ΓöÇΓöÇΓöÇ Force Update ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
// Returns { requiredVersion, updateUrl } stored under the system config collection.
// Admin updates the value via Nakama console ΓåÆ Storage ΓåÆ collection "config", key "app_version".
var getAppVersionRpc = function (context, logger, nakama, payload) {
    var stored = nakama.storageRead([
        { collection: CollectionConfig, key: KeyAppVersion, userId: SystemUserId },
    ]);
    if (stored.length === 0) {
        // Default: no update required — returns empty requiredVersion
        return JSON.stringify({ requiredVersion: "", updateUrl: "" });
    }
    return JSON.stringify(stored[0].value);
};
// Returns { enabled } stored under the system config collection.
// Admin toggles via Nakama console ΓåÆ Storage ΓåÆ collection "config", key "chat_enabled",
// user 00000000-0000-0000-0000-000000000000, value {"enabled": false} to disable chat globally.
var getChatConfigRpc = function (context, logger, nakama, payload) {
    var stored = nakama.storageRead([
        { collection: CollectionConfig, key: KeyChatEnabled, userId: SystemUserId },
    ]);
    if (stored.length === 0) {
        // Default: chat enabled
        return JSON.stringify({ enabled: true });
    }
    return JSON.stringify(stored[0].value);
};
// Server-side authoritative check used by the chat relay.
function isChatEnabled(nakama) {
    try {
        var stored = nakama.storageRead([
            { collection: CollectionConfig, key: KeyChatEnabled, userId: SystemUserId },
        ]);
        if (stored.length === 0)
            return true; // default-on if unseeded
        return stored[0].value.enabled !== false;
    }
    catch (e) {
        return true;
    }
}
// ΓöÇΓöÇΓöÇ Admin analytics ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
// Call from Nakama Console ΓåÆ API Explorer ΓåÆ Run RPC "AdminStatsRpc" (leave user blank),
// or via HTTP with the server http_key. Optional payload: {"days":7,"secret":"..."}.
// Reads the DB directly (sqlQuery) to summarise user behaviour, economy, IAP and fraud signals.
var adminStatsRpc = function (context, logger, nakama, payload) {
    var input = {};
    try { input = JSON.parse(payload || "{}"); } catch (e) {}

    // Admin-only: console/server calls have no userId. A player session is rejected unless it
    // carries the configured admin secret (config ΓåÆ key "admin_secret" value {"secret":"..."}).
    var allowed = !context.userId;
    if (!allowed) {
        try {
            var sec = nakama.storageRead([{ collection: CollectionConfig, key: "admin_secret", userId: SystemUserId }]);
            if (sec.length && sec[0].value && input.secret && input.secret === sec[0].value.secret) allowed = true;
        } catch (e) {}
    }
    if (!allowed) return JSON.stringify({ error: "Admin only" });

    var days = parseInt(input.days, 10);
    if (!days || days <= 0 || days > 365) days = 7;
    var WIN = "INTERVAL '" + days + " days'"; // days is a validated int ΓÇö safe to inline
    var SYS = "'00000000-0000-0000-0000-000000000000'";

    function q(sql) {
        try { return { ok: true, rows: nakama.sqlQuery(sql, []) }; }
        catch (e) { return { ok: false, error: String(e) }; }
    }
    function num(res, col) {
        if (!res.ok || !res.rows || res.rows.length === 0) return 0;
        var n = parseInt(res.rows[0][col], 10);
        return isNaN(n) ? 0 : n;
    }

    var report = { generatedAt: new Date().toISOString(), windowDays: days };

    report.users = {
        total:          num(q("SELECT count(*)::int AS n FROM users WHERE id <> " + SYS), "n"),
        newToday:       num(q("SELECT count(*)::int AS n FROM users WHERE id <> " + SYS + " AND create_time > now() - INTERVAL '1 day'"), "n"),
        newInWindow:    num(q("SELECT count(*)::int AS n FROM users WHERE id <> " + SYS + " AND create_time > now() - " + WIN), "n"),
        activeInWindow: num(q("SELECT count(*)::int AS n FROM users WHERE id <> " + SYS + " AND update_time > now() - " + WIN), "n"),
        activeToday:    num(q("SELECT count(*)::int AS n FROM users WHERE id <> " + SYS + " AND update_time > now() - INTERVAL '1 day'"), "n"),
    };

    var econ = q("SELECT coalesce(metadata->>'source','(none)') AS source, count(*)::int AS n, " +
        "coalesce(sum((changeset->>'coins')::int),0)::int AS coins FROM wallet_ledger GROUP BY 1 ORDER BY 3 DESC");
    report.economy = {
        coinsInWallets: num(q("SELECT coalesce(sum((wallet->>'coins')::int),0)::int AS n FROM users"), "n"),
        bySource: econ.ok ? econ.rows : [],
        error: econ.ok ? undefined : econ.error,
    };

    var iapTot = q("SELECT count(*)::int AS n, coalesce(sum((changeset->>'coins')::int),0)::int AS coins, " +
        "count(DISTINCT user_id)::int AS buyers FROM wallet_ledger WHERE metadata->>'source'='iap'");
    var byProduct = q("SELECT coalesce(metadata->>'productId','?') AS product, count(*)::int AS n, " +
        "coalesce(sum((changeset->>'coins')::int),0)::int AS coins FROM wallet_ledger WHERE metadata->>'source'='iap' GROUP BY 1 ORDER BY 2 DESC");
    report.iap = {
        totalGrants:      num(iapTot, "n"),
        totalCoinsSold:   num(iapTot, "coins"),
        uniqueBuyers:     num(iapTot, "buyers"),
        unverifiedGrants: num(q("SELECT count(*)::int AS n FROM storage WHERE collection='payment' AND value->>'verified'='false'"), "n"),
        byProduct: byProduct.ok ? byProduct.rows : [],
    };

    var topBuyers = q("SELECT user_id, count(*)::int AS grants, coalesce(sum((changeset->>'coins')::int),0)::int AS coins " +
        "FROM wallet_ledger WHERE metadata->>'source'='iap' GROUP BY user_id ORDER BY 2 DESC LIMIT 15");
    var fast = q("SELECT l.user_id, count(*)::int AS grants FROM wallet_ledger l JOIN users u ON u.id = l.user_id " +
        "WHERE l.metadata->>'source'='iap' AND l.create_time < u.create_time + INTERVAL '5 minutes' " +
        "GROUP BY l.user_id HAVING count(*) >= 2 ORDER BY 2 DESC LIMIT 15");
    report.fraudSignals = {
        note: "Purchase records exist only AFTER the IAP security fix was deployed.",
        topBuyers: topBuyers.ok ? topBuyers.rows : [],
        manyPurchasesWithin5minOfSignup: fast.ok ? fast.rows : [],
    };

    report.engagement = {
        profiles:          num(q("SELECT count(*)::int AS n FROM storage WHERE collection='Profile'"), "n"),
        entryFeeCharges:   num(q("SELECT count(*)::int AS n FROM wallet_ledger WHERE metadata->>'source'='entry_fee'"), "n"),
        chestClaimers:     num(q("SELECT count(DISTINCT user_id)::int AS n FROM wallet_ledger WHERE metadata->>'source'='chest'"), "n"),
        cardOwners:        num(q("SELECT count(*)::int AS n FROM storage WHERE collection='Cards'"), "n"),
    };

    // Per-mode play volume ΓÇö answers "most played game mode". The mode is recorded on each
    // entry-fee ledger entry as metadata.league (one row per real-player match join).
    // NOTE: code mode "ThreeByThree" is actually the 4x4 "FourByFour" scene ΓÇö labels disambiguate.
    var MODE_LABELS = {
        "ThreeByThree":          "FourByFour 4x4 (Showdown)",
        "FourByThree":           "FourByThree 4x3 (Dicepunk)",
        "VerticalAndHorizontal": "VerticalAndHorizontal 3x3 (Dice Master)",
    };
    function labelMode(m) { return MODE_LABELS[m] || m; }
    var modeAll = q("SELECT coalesce(metadata->>'league','(unknown)') AS mode, count(*)::int AS matches, " +
        "count(DISTINCT user_id)::int AS players FROM wallet_ledger WHERE metadata->>'source'='entry_fee' GROUP BY 1 ORDER BY 2 DESC");
    var modeWin = q("SELECT coalesce(metadata->>'league','(unknown)') AS mode, count(*)::int AS matches " +
        "FROM wallet_ledger WHERE metadata->>'source'='entry_fee' AND create_time > now() - " + WIN + " GROUP BY 1 ORDER BY 2 DESC");
    if (modeAll.ok) for (var mi = 0; mi < modeAll.rows.length; mi++) modeAll.rows[mi].label = labelMode(modeAll.rows[mi].mode);
    if (modeWin.ok) for (var mj = 0; mj < modeWin.rows.length; mj++) modeWin.rows[mj].label = labelMode(modeWin.rows[mj].mode);
    report.gameModes = {
        note: "Match volume per mode from entry-fee charges. Code mode 'ThreeByThree' = the 4x4 FourByFour scene.",
        mostPlayedAllTime:  (modeAll.ok && modeAll.rows.length) ? labelMode(modeAll.rows[0].mode) : null,
        mostPlayedInWindow: (modeWin.ok && modeWin.rows.length) ? labelMode(modeWin.rows[0].mode) : null,
        breakdownAllTime:   modeAll.ok ? modeAll.rows : [],
        breakdownInWindow:  modeWin.ok ? modeWin.rows : [],
        error: modeAll.ok ? undefined : modeAll.error,
    };

    logger.info("[AdminStats] generated windowDays=" + days);
    return JSON.stringify(report);
};

// ΓöÇΓöÇΓöÇ Admin: set a global config value ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
// Stores under collection "config" / SystemUserId. Use for: myket_iap_token,
// iap_strict, chat_enabled, admin_secret, app_version.
// Call from Nakama Console ΓåÆ API Explorer ΓåÆ Run RPC "SetConfigRpc" (leave user blank).
//   {"key":"myket_iap_token","value":{"token":"<X-Access-Token>"}}
//   {"key":"iap_strict","value":{"enabled":true}}
var setConfigRpc = function (context, logger, nakama, payload) {
    var input = {};
    try { input = JSON.parse(payload || "{}"); } catch (e) { return JSON.stringify({ error: "Bad payload" }); }

    var allowed = !context.userId; // console/server calls have no userId
    if (!allowed) {
        try {
            var sec = nakama.storageRead([{ collection: CollectionConfig, key: "admin_secret", userId: SystemUserId }]);
            if (sec.length && sec[0].value && input.secret && input.secret === sec[0].value.secret) allowed = true;
        } catch (e) {}
    }
    if (!allowed) return JSON.stringify({ error: "Admin only" });

    var key = (input.key || "").trim();
    if (!key) return JSON.stringify({ error: "Missing 'key'" });
    if (input.value === undefined || input.value === null) return JSON.stringify({ error: "Missing 'value'" });

    nakama.storageWrite([{
        collection: CollectionConfig, key: key, userId: SystemUserId,
        value: input.value, permissionRead: 2, permissionWrite: 0,
    }]);
    logger.info("[SetConfig] admin set config key=" + key);
    return JSON.stringify({ success: true, key: key, value: input.value });
};

function CreateLeaderboards(context, logger, nakama) {
    var configs = [
        { id: LeaderboardWeekly, reset: "0 0 * * 1" },
        { id: LeaderboardMonthly, reset: "0 0 1 * *" }, // 1st of every month
    ];
    for (var _i = 0, configs_1 = configs; _i < configs_1.length; _i++) {
        var cfg = configs_1[_i];
        try {
            // Use raw strings — Nakama 3.x JS runtime expects "desc"/"asc" and "incr"/"decr"/"best"/"set"
            nakama.leaderboardCreate(cfg.id, true, "desc", "incr", cfg.reset, {});
            logger.info("Leaderboard created: " + cfg.id);
        }
        catch (e) {
            logger.info("Leaderboard " + cfg.id + ": " + e);
        }
    }
}
// ─── Match Lifecycle ──────────────────────────────────────────────────────────
var matchInit = function (context, logger, nakama, params) {
    var value = "";
    for (var key in params)
        value = params[key];
    var label = { open: true, game_mode: value };
    var _a = buildGrids(value), arrayFirst = _a[0], arraySecond = _a[1], vertical = _a[2];
    var gameState = {
        players: [], playersWins: [], roundDeclaredWins: [[]], roundDeclaredDraw: [],
        scene: 3 /* Lobby */,
        countdown: DurationLobby * TickRate,
        endMatch: false,
        CountTurnPlayer1: 0, CountTurnPlayer2: 0,
        namesForrematch: [],
        BeforeEndGame: false,
        VerticalMode: vertical,
        array3DPlayerFirst: arrayFirst, array3DPlayerSecend: arraySecond,
        ModeText: value,
        hasBot: false, botDifficulty: 0, botNeedsToMove: false, botThinkTick: 0,
        isTutorial: params.tutorial === "true",
        tutorialBotMoveIndex: 0,
    };
    return { state: gameState, tickRate: TickRate, label: JSON.stringify(label) };
};
function buildGrids(mode) {
    var rowFirst = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    var rowSecond = [[-1, -1, -1], [-1, -1, -1], [-1, -1, -1]];
    var vertical = false;
    if (mode === "VerticalAndHorizontal") {
        vertical = true;
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
// ─── Join ─────────────────────────────────────────────────────────────────────
var matchJoinAttempt = function (context, logger, nakama, dispatcher, tick, state, presence, metadata) {
    var gameState = state;
    if (gameState.scene !== 3 /* Lobby */)
        return { state: gameState, accept: false };
    // Tutorial mode: skip entry fee check entirely
    if (!gameState.isTutorial) {
        var league = LEAGUES[gameState.ModeText];
        if (league) {
            try {
                var account = nakama.accountGetId(presence.userId);
                var wallet = account.wallet || {};
                if ((wallet["coins"] || 0) < league.entryFee) {
                    logger.info("Join rejected \u2014 insufficient coins for " + gameState.ModeText + ": userId=" + presence.userId);
                    return { state: gameState, accept: false };
                }
            }
            catch (e) {
                logger.warn("matchJoinAttempt wallet check failed: " + e);
                return { state: gameState, accept: false };
            }
        }
    }
    return { state: gameState, accept: true };
};
var matchJoin = function (context, logger, nakama, dispatcher, tick, state, presences) {
    var gameState = state;
    if (gameState.scene !== 3 /* Lobby */)
        return { state: gameState };
    var existingPresences = [];
    gameState.players.forEach(function (p) { if (p && !p.isBot)
        existingPresences.push(p.presence); });
    for (var _i = 0, presences_1 = presences; _i < presences_1.length; _i++) {
        var presence = presences_1[_i];
        var account = nakama.accountGetId(presence.userId);
        var resolvedName = account.user.displayName && account.user.displayName.trim().length > 0
            ? account.user.displayName.trim()
            : (presence.username || account.user.username || "Player");
        // Deduct entry fee (skipped for tutorial)
        if (!gameState.isTutorial) {
            var league = LEAGUES[gameState.ModeText];
            if (league) {
                try {
                    nakama.walletUpdate(presence.userId, { coins: -league.entryFee }, { source: "entry_fee", league: gameState.ModeText }, true);
                }
                catch (e) {
                    logger.warn("Entry fee deduction failed for " + presence.userId + ": " + e);
                }
            }
        }
        var player = {
            presence: presence,
            displayName: resolvedName, avatarId: "avatar_0", ScorePlayer: 0,
        };
        var slot = getNextPlayerNumber(gameState.players);
        gameState.players[slot] = player;
        gameState.playersWins[slot] = 0;
        dispatcher.broadcastMessage(1 /* PlayerJoined */, JSON.stringify(player), existingPresences);
        existingPresences.push(presence);
    }
    dispatcher.broadcastMessage(0 /* Players */, JSON.stringify(gameState.players), presences);
    if (gameState.players[0]) {
        dispatcher.broadcastMessage(6 /* TurnMe */, JSON.stringify(gameState.players[0].presence.userId));
    }
    gameState.countdown = DurationLobby * TickRate;
    return { state: gameState };
};
// ─── Match Loop ───────────────────────────────────────────────────────────────
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
        var num = getPlayerNumber(gameState.players, presence.sessionId);
        if (num === PlayerNotFound)
            continue;
        var name_1 = JSON.stringify(gameState.players[num].displayName);
        if (!gameState.BeforeEndGame)
            dispatcher.broadcastMessage(9, name_1);
        delete gameState.players[num];
    }
    return { state: gameState };
};
var matchTerminate = function (context, logger, nakama, dispatcher, tick, state, graceSeconds) { return { state: state }; };
var matchSignal = function (context, logger, nk, dispatcher, tick, state, data) { return { state: state }; };
// ─── Message Routing ──────────────────────────────────────────────────────────
function processMessages(messages, gameState, dispatcher, nakama, logger) {
    for (var _i = 0, messages_1 = messages; _i < messages_1.length; _i++) {
        var message = messages_1[_i];
        if (MessagesLogic.hasOwnProperty(message.opCode)) {
            MessagesLogic[message.opCode](message, gameState, dispatcher, nakama, logger);
        }
    }
}
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
// ─── Lobby ────────────────────────────────────────────────────────────────────
function matchLoopLobby(gameState, nakama, dispatcher, logger) {
    if (getPlayersCount(gameState.players) === 0)
        return;
    if (gameState.countdown <= 0)
        return;
    gameState.countdown--;
    if (gameState.countdown > 0)
        return;
    // Tutorial: always play against a bot
    if (gameState.isTutorial) {
        addBotAndStartBattle(gameState, dispatcher, logger);
        return;
    }
    if (getPlayersCount(gameState.players) >= 2) {
        startBattle(gameState, dispatcher);
    }
    else {
        addBotAndStartBattle(gameState, dispatcher, logger);
    }
}
function startBattle(gameState, dispatcher) {
    gameState.scene = 4 /* Battle */;
    dispatcher.broadcastMessage(5 /* ChangeScene */, JSON.stringify(gameState.scene));
    dispatcher.matchLabelUpdate(JSON.stringify({ open: false }));
}
function addBotAndStartBattle(gameState, dispatcher, logger) {
    var botName = BOT_NAMES[Math.floor(Math.random() * BOT_NAMES.length)];
    var diff = BOT_DIFFICULTIES[Math.floor(Math.random() * BOT_DIFFICULTIES.length)];
    var botPresence = {
        userId: "bot_" + generateId(), sessionId: "bot_" + generateId(),
        username: botName, node: "server", status: "",
    };
    gameState.players[1] = { presence: botPresence, displayName: botName, avatarId: "avatar_0", ScorePlayer: 0, isBot: true };
    gameState.playersWins[1] = 0;
    gameState.hasBot = true;
    gameState.botDifficulty = diff;
    logger.info("Bot added: name=" + botName + " difficulty=" + diff);
    dispatcher.broadcastMessage(0 /* Players */, JSON.stringify(gameState.players));
    dispatcher.broadcastMessage(6 /* TurnMe */, JSON.stringify(gameState.players[0].presence.userId));
    startBattle(gameState, dispatcher);
}
function generateId() {
    return Math.random().toString(36).substring(2, 10);
}
// ─── Battle ───────────────────────────────────────────────────────────────────
function matchLoopBattle(gameState, nakama, dispatcher, logger) {
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
    if (gameState.hasBot && gameState.botNeedsToMove) {
        if (gameState.botThinkTick > 0) {
            gameState.botThinkTick--;
        }
        else {
            executeBotTurn(gameState, nakama, dispatcher, logger);
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
        if (!winner.isBot) {
            var read = nakama.storageRead([{ collection: CollectionUser, key: KeyTrophies, userId: winner.presence.userId }]);
            var td = { amount: 0 };
            for (var _i = 0, read_1 = read; _i < read_1.length; _i++) {
                var obj = read_1[_i];
                td = obj.value;
                break;
            }
            td.amount++;
            nakama.storageWrite([{ collection: CollectionUser, key: KeyTrophies, userId: winner.presence.userId, value: td }]);
        }
        gameState.endMatch = true;
        gameState.scene = 6 /* FinalResults */;
    }
    else {
        gameState.scene = 4 /* Battle */;
    }
    dispatcher.broadcastMessage(5 /* ChangeScene */, JSON.stringify(gameState.scene));
}
// ─── Turn ─────────────────────────────────────────────────────────────────────
function ChooseTurnPlayer(message, gameState, dispatcher, nakama, logger) {
    var dataPlayer = JSON.parse(nakama.binaryToString(message.data));
    dataPlayer.MinesScore = false;
    gameState.BeforeEndGame = false;
    var isPlayer0 = message.sender.userId === gameState.players[0].presence.userId;
    if (isPlayer0) {
        processTurn(dataPlayer, gameState.array3DPlayerFirst, gameState.array3DPlayerSecend, gameState, 0, true, nakama, logger);
    }
    else {
        processTurn(dataPlayer, gameState.array3DPlayerSecend, gameState.array3DPlayerFirst, gameState, 1, false, nakama, logger);
    }
    var wasEndGame = dataPlayer.EndGame;
    var dataSend = JSON.stringify(dataPlayer);
    if (wasEndGame && gameState.hasBot) {
        dispatcher.broadcastMessage(message.opCode, dataSend);
    }
    else {
        dispatcher.broadcastMessage(message.opCode, dataSend, null, message.sender);
    }
    dataPlayer.EndGame = false;
    if (gameState.hasBot && isPlayer0 && !wasEndGame) {
        gameState.botNeedsToMove = true;
        gameState.botThinkTick = BotThinkMinTicks + Math.floor(Math.random() * (BotThinkMaxTicks - BotThinkMinTicks));
    }
}
function processTurn(dataPlayer, moverGrid, targetGrid, gameState, moverIndex, isMaster, nakama, logger) {
    var line = dataPlayer.NumberLine, row = dataPlayer.NumberRow, tile = dataPlayer.NumberTile;
    dataPlayer.master = isMaster;
    dataPlayer.MinesScore = false;
    dataPlayer.ValueMines = 0;
    moverGrid[line][row] = tile;
    if (moverIndex === 0)
        gameState.CountTurnPlayer1++;
    else
        gameState.CountTurnPlayer2++;
    dataPlayer.Score = TotalScore(moverGrid, logger, gameState.VerticalMode);
    gameState.players[moverIndex].ScorePlayer = dataPlayer.Score;
    var opponentIndex = 1 - moverIndex;
    var mineCount = 0;
    // ── Vertical mines (VerticalAndHorizontal mode only) ────────────────────────
    if (gameState.VerticalMode) {
        var hits = CalculatorArray2DWithVertical(targetGrid, line, row, tile, logger);
        for (var _i = 0, hits_1 = hits; _i < hits_1.length; _i++) {
            var hitRow = hits_1[_i];
            targetGrid[hitRow][row] = -1;
            mineCount++;
        }
        if (mineCount > 0) {
            applyMineResult(dataPlayer, mineCount, tile, targetGrid, gameState, opponentIndex, logger);
        }
        mineCount = 0;
    }
    // ── Horizontal mines (always checked in all modes) ────────────────────────
    {
        var hits = CalculatorArray2D(targetGrid, line, row, tile, logger);
        for (var _a = 0, hits_2 = hits; _a < hits_2.length; _a++) {
            var hitCol = hits_2[_a];
            targetGrid[line][hitCol] = -1;
            mineCount++;
        }
        if (mineCount > 0) {
            if (dataPlayer.MinesScore) {
                // Vertical mines already fired — accumulate horizontal damage on top
                dataPlayer.ValueMines += (tile + 1) * mineCount * mineCount;
                dataPlayer.ScoreOtherPlayer = TotalScore(targetGrid, logger, gameState.VerticalMode);
                gameState.players[opponentIndex].ScorePlayer = dataPlayer.ScoreOtherPlayer;
            }
            else {
                applyMineResult(dataPlayer, mineCount, tile, targetGrid, gameState, opponentIndex, logger);
            }
        }
    }
    dataPlayer.Array2DTilesPlayer = moverGrid;
    dataPlayer.Array2DTilesOtherPlayer = targetGrid;
    var moverFull = ActionWinPlayer(moverGrid);
    var targetFull = ActionWinPlayer(targetGrid);
    var turnsEqual = parseInt(gameState.CountTurnPlayer1) === parseInt(gameState.CountTurnPlayer2);
    if ((moverFull || targetFull) && turnsEqual) {
        var s0 = gameState.players[0].ScorePlayer;
        var s1 = gameState.players[1].ScorePlayer;
        if (s0 > s1)
            dataPlayer.PlayerWin = gameState.players[0].presence.userId;
        else if (s1 > s0)
            dataPlayer.PlayerWin = gameState.players[1].presence.userId;
        else
            dataPlayer.PlayerWin = "";
        dataPlayer.EndGame = true;
        gameState.BeforeEndGame = true;
        awardMatchResult(gameState, nakama, dataPlayer.PlayerWin, logger);
    }
}
function applyMineResult(dataPlayer, mineCount, tile, targetGrid, gameState, opponentIndex, logger) {
    var valuMines = tile + 1;
    dataPlayer.ValueMines = (valuMines * mineCount) * mineCount;
    dataPlayer.ScoreOtherPlayer = TotalScore(targetGrid, logger, gameState.VerticalMode);
    gameState.players[opponentIndex].ScorePlayer = dataPlayer.ScoreOtherPlayer;
    dataPlayer.MinesScore = true;
}
// ─── Economy ──────────────────────────────────────────────────────────────────
function awardMatchResult(gameState, nakama, winnerId, logger) {
    // Tutorial matches: no rewards, no leaderboard updates
    if (gameState.isTutorial)
        return;
    var league = LEAGUES[gameState.ModeText];
    if (!league)
        return;
    var updates = [];
    for (var i = 0; i < gameState.players.length; i++) {
        var player = gameState.players[i];
        if (!player || player.isBot)
            continue;
        if (winnerId === "") {
            // Draw — return 50%
            updates.push({
                userId: player.presence.userId,
                changeset: { coins: league.drawRefund },
                metadata: { source: "draw_refund" },
            });
        }
        else if (player.presence.userId === winnerId) {
            // Win
            updates.push({
                userId: player.presence.userId,
                changeset: { coins: league.winnerReward },
                metadata: { source: "match_win", league: gameState.ModeText },
            });
            // Record rank points in both leaderboards
            try {
                nakama.leaderboardRecordWrite(LeaderboardWeekly, player.presence.userId, player.displayName, league.rankPoints);
                nakama.leaderboardRecordWrite(LeaderboardMonthly, player.presence.userId, player.displayName, league.rankPoints);
            }
            catch (e) {
                logger.warn("leaderboardRecordWrite failed: " + e);
            }
        }
        // Lose: entry fee already deducted on join, nothing extra
    }
    if (updates.length > 0) {
        try {
            nakama.walletsUpdate(updates, false);
        }
        catch (e) {
            logger.warn("walletsUpdate failed: " + e);
        }
    }
}
// ─── Leaderboard Reset Hook ───────────────────────────────────────────────────
var onLeaderboardReset = function (ctx, logger, nk, leaderboard, reset) {
    var isWeekly = leaderboard.id === LeaderboardWeekly;
    var rewards = isWeekly ? WEEKLY_REWARDS : MONTHLY_REWARDS;
    var pendingKey = isWeekly ? KeyPendingRewardWeekly : KeyPendingRewardMonthly;
    var typeLabel = isWeekly ? "weekly" : "monthly";
    var records = [];
    try {
        var result = nk.leaderboardRecordsList(leaderboard.id, [], rewards.length, "", 0);
        records = result.records || [];
    }
    catch (e) {
        logger.warn("leaderboardRecordsList failed during reset: " + e);
        return;
    }
    var walletUpdates = [];
    var storageWrites = [];
    for (var i = 0; i < records.length && i < rewards.length; i++) {
        var record = records[i];
        var reward = rewards[i];
        walletUpdates.push({
            userId: record.ownerId,
            changeset: { coins: reward },
            metadata: { source: "leaderboard_reward", type: typeLabel, rank: i + 1 },
        });
        var pendingData = { rank: i + 1, reward: reward, type: typeLabel, claimed: false };
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
        try {
            nk.walletsUpdate(walletUpdates, false);
        }
        catch (e) {
            logger.warn("Reward distribution failed: " + e);
        }
    }
    if (storageWrites.length > 0) {
        try {
            nk.storageWrite(storageWrites);
        }
        catch (e) {
            logger.warn("Pending reward storage failed: " + e);
        }
    }
    logger.info(typeLabel + " leaderboard reset: distributed rewards to " + records.length + " players");
};
// ─── Sticker & Rematch ────────────────────────────────────────────────────────
function StickersManager(message, gameState, dispatcher, nakama, logger) {
    var data = JSON.parse(nakama.binaryToString(message.data));
    dispatcher.broadcastMessage(10 /* Sticker */, JSON.stringify(data));
}
function Rematch(message, gameState, dispatcher, nakama, logger) {
    var dataPlayer = JSON.parse(nakama.binaryToString(message.data));
    if (gameState.hasBot) {
        if (dataPlayer.Answer === "no") {
            gameState.endMatch = true;
            dispatcher.broadcastMessage(message.opCode, JSON.stringify(dataPlayer), null, message.sender);
            return;
        }
        if (dataPlayer.Answer === "send" || dataPlayer.Answer === "yes") {
            // Deduct entry fee again for rematch
            var league = LEAGUES[gameState.ModeText];
            if (league) {
                try {
                    nakama.walletUpdate(gameState.players[0].presence.userId, { coins: -league.entryFee }, { source: "entry_fee_rematch" }, true);
                }
                catch (e) {
                    logger.warn("Rematch entry fee failed: " + e);
                }
            }
            resetGameForRematch(gameState);
            dataPlayer.Answer = "yes";
            dispatcher.broadcastMessage(message.opCode, JSON.stringify(dataPlayer), null, message.sender);
            dispatcher.broadcastMessage(6 /* TurnMe */, JSON.stringify(gameState.players[0].presence.userId));
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
            var league = LEAGUES[gameState.ModeText];
            if (league) {
                var feeUpdates = [];
                for (var _i = 0, _a = gameState.players; _i < _a.length; _i++) {
                    var p = _a[_i];
                    if (p && !p.isBot) {
                        feeUpdates.push({ userId: p.presence.userId, changeset: { coins: -league.entryFee }, metadata: { source: "entry_fee_rematch" } });
                    }
                }
                try {
                    nakama.walletsUpdate(feeUpdates, true);
                }
                catch (e) {
                    logger.warn("Rematch entry fees failed: " + e);
                }
            }
            resetGameForRematch(gameState);
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
function resetGameForRematch(gameState) {
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
    for (var _i = 0, _a = gameState.players; _i < _a.length; _i++) {
        var player = _a[_i];
        if (player)
            player.ScorePlayer = 0;
    }
}
// ─── Bot AI ───────────────────────────────────────────────────────────────────
function executeBotTurn(gameState, nakama, dispatcher, logger) {
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
        Score: 0, NumberTile: move.tile, NameTile: move.tile.toString(),
        NumberLine: move.line, NumberRow: move.col,
        EndGame: false, PlayerWin: "", ScoreOtherPlayer: 0,
        MinesScore: false, ValueMines: 0, sumRow1: [], sumRow2: [],
        master: false, Array2DTilesPlayer: [], Array2DTilesOtherPlayer: [],
    };
    processTurn(dataPlayer, gameState.array3DPlayerSecend, gameState.array3DPlayerFirst, gameState, 1, false, nakama, logger);
    dispatcher.broadcastMessage(7 /* ChosseTurn */, JSON.stringify(dataPlayer), [realPlayer.presence], null);
}
// Scripted bot moves for tutorial mode (tile is 0-indexed dice value)
var TUTORIAL_BOT_MOVES = [
    { line: 0, col: 0, tile: 1 }, // places value 2 at row-0 col-0
];
function generateBotMove(gameState, logger) {
    // Tutorial: play scripted moves first, then fall through to random
    if (gameState.isTutorial) {
        var idx = gameState.tutorialBotMoveIndex;
        if (idx < TUTORIAL_BOT_MOVES.length) {
            gameState.tutorialBotMoveIndex++;
            logger.info("Tutorial bot scripted move " + idx + ": " + JSON.stringify(TUTORIAL_BOT_MOVES[idx]));
            return TUTORIAL_BOT_MOVES[idx];
        }
    }
    var botGrid = gameState.array3DPlayerSecend;
    var playerGrid = gameState.array3DPlayerFirst;
    var numRows = botGrid.length;
    var numCols = botGrid[0].length;
    var maxTile = numCols - 1;
    var difficulty = gameState.botDifficulty;
    var emptyCells = [];
    for (var i = 0; i < numRows; i++)
        for (var j = 0; j < numCols; j++)
            if (botGrid[i][j] === -1)
                emptyCells.push({ line: i, col: j });
    if (emptyCells.length === 0)
        return null;
    if (difficulty === 0 || (difficulty === 1 && Math.random() < 0.5)) {
        var cell = emptyCells[Math.floor(Math.random() * emptyCells.length)];
        return { line: cell.line, col: cell.col, tile: Math.floor(Math.random() * (maxTile + 1)) };
    }
    var bestScore = -1;
    var bestMove = { line: emptyCells[0].line, col: emptyCells[0].col, tile: Math.floor(Math.random() * (maxTile + 1)) };
    for (var _i = 0, emptyCells_1 = emptyCells; _i < emptyCells_1.length; _i++) {
        var cell = emptyCells_1[_i];
        var _loop_1 = function (tile) {
            var tempBot = botGrid.map(function (r) { return r.slice(); });
            tempBot[cell.line][cell.col] = tile;
            var mineHits = playerGrid[cell.line].filter(function (v) { return v === tile; }).length;
            if (gameState.VerticalMode) {
                for (var r = 0; r < numRows; r++)
                    if (playerGrid[r][cell.col] === tile)
                        mineHits++;
            }
            var moveScore = simulateTotalScore(tempBot, gameState.VerticalMode) + mineHits * 6;
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
function simulateTotalScore(grid, verticalMode) {
    var score = 0;
    for (var i = 0; i < grid.length; i++)
        score += scoreRow(grid[i]);
    if (verticalMode) {
        var _loop_2 = function (col) {
            score += scoreRow(grid.map(function (r) { return r[col]; }));
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
// ─── Score / Grid Helpers ─────────────────────────────────────────────────────
function TotalScore(array2D, logger, mode) {
    var score = 0;
    for (var i = 0; i < array2D.length; i++)
        score += CalculatorArray(array2D[i], logger);
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
        var k = String(v);
        counts[k] = (counts[k] || 0) + 1;
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
    array1[x].forEach(function (element, index) { if (element === input)
        result.push(index); });
    return result;
}
function CalculatorArray2DWithVertical(array1, x, y, input, logger) {
    var result = [];
    array1.map(function (r) { return r[y]; }).forEach(function (element, index) { if (element === input)
        result.push(index); });
    return result;
}
function ActionWinPlayer(array1) {
    for (var i = 0; i < array1.length; i++)
        for (var j = 0; j < array1[i].length; j++)
            if (array1[i][j] === -1)
                return false;
    return true;
}
// ─── Player Utilities ─────────────────────────────────────────────────────────
function getPlayersCount(players) {
    var count = 0;
    for (var i = 0; i < MaxPlayers; i++)
        if (players[i] !== undefined)
            count++;
    return count;
}
function getNextPlayerNumber(players) {
    for (var i = 0; i < MaxPlayers; i++)
        if (!players[i])
            return i;
    return PlayerNotFound;
}
function getPlayerNumber(players, sessionId) {
    for (var i = 0; i < MaxPlayers; i++)
        if (players[i] && players[i].presence.sessionId === sessionId)
            return i;
    return PlayerNotFound;
}
function getWinner(playersWins, players) {
    for (var i = 0; i < MaxPlayers; i++)
        if (playersWins[i] === NecessaryWins)
            return players[i];
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
var LEAGUES = {
    "ThreeByThree": {
        displayName: "SHOWDOWN DICE",
        entryFee: 50,
        winnerReward: 80,
        drawRefund: 25,
        rankPoints: 50,
    },
    "FourByThree": {
        displayName: "DICEPUNK LEAGUE",
        entryFee: 150,
        winnerReward: 250,
        drawRefund: 75,
        rankPoints: 120,
    },
    "VerticalAndHorizontal": {
        displayName: "DICE MASTER",
        entryFee: 250,
        winnerReward: 420,
        drawRefund: 125,
        rankPoints: 250,
    },
};
// ─── Leaderboards ─────────────────────────────────────────────────────────────
var CollectionContactMessages = "ContactMessages";
var sendContactMessageRpc = function (context, logger, nakama, payload) {
    var userId = context.userId;
    if (!userId)
        throw new Error("Not authenticated");
    var data = JSON.parse(payload || "{}");
    var subject = (data.subject || "").toString().trim().substring(0, 100);
    var message = (data.message || "").toString().trim().substring(0, 2000);
    if (message.length === 0)
        throw new Error("Message cannot be empty");
    var username = context.username || userId;
    var sentAt = Math.floor(Date.now() / 1000);
    var key = sentAt + "_" + userId.replace(/-/g, "").substring(0, 8);
    var entry = {
        fromUserId: userId,
        fromUsername: username,
        subject: subject,
        message: message,
        sentAt: sentAt,
        platform: data.platform || "mobile",
    };
    try {
        nakama.storageWrite([{
                collection: CollectionContactMessages,
                key: key,
                userId: SystemUserId,
                value: entry,
                permissionRead: 0,
                permissionWrite: 0,
            }]);
        logger.info("[ContactUs] message saved key=" + key + " from=" + username + "(" + userId + ")");
        return JSON.stringify({ success: true });
    }
    catch (e) {
        logger.warn("[ContactUs] storageWrite failed: " + e);
        throw new Error("Failed to save message");
    }
};
var LeaderboardWeekly = "weekly_leaderboard";
var LeaderboardMonthly = "monthly_leaderboard";
// Top-10 reward tables (index 0 = rank 1)
var WEEKLY_REWARDS = [1000, 500, 250, 150, 125, 100, 75, 50, 25, 10];
var MONTHLY_REWARDS = [5000, 2500, 1000, 750, 500, 300, 200, 100, 50, 10];
var CollectionSeason = "Season";
var CollectionProfile = "Profile";
var KeyProfileData = "profile_data";
var FirstLoginBonusCoins = 3500;
// Avatar catalog — prices validated server-side (client cannot lie about price)
// id must match AvatarLibrary ScriptableObject ids in Unity client
var AVATAR_PRICES = {
    "avatar_0": 0,
    "avatar_1": 0,
    "avatar_2": 250,
    "avatar_3": 300,
    "avatar_4": 500,
    "avatar_5": 600,
    "avatar_6": 700,
    "avatar_7": 720,
    "avatar_8": 750,
    "avatar_9": 770,
    "avatar_10": 800,
    "avatar_11": 820,
    "avatar_12": 830,
    "avatar_13": 840,
    "avatar_14": 860,
    "avatar_15": 880,
    "avatar_16": 900,
    "avatar_17": 920,
    "avatar_18": 940,
    "avatar_20": 980,
    "avatar_21": 1000,
    "avatar_22": 1050,
    "avatar_23": 1100,
    "avatar_24": 1200,
};
var KeyPendingRewardWeekly = "pending_weekly";
var KeyPendingRewardMonthly = "pending_monthly";
// ─── App Version (Force Update) ───────────────────────────────────────────────
var CollectionConfig = "config";
var KeyAppVersion = "app_version";
var KeyChatEnabled = "chat_enabled";
// System user ID — used to store global config readable by all authenticated users
var SystemUserId = "00000000-0000-0000-0000-000000000000";
// ─── Bot ──────────────────────────────────────────────────────────────────────
var BotThinkMinTicks = TickRate * 1;
var BotThinkMaxTicks = TickRate * 3;
var BOT_NAMES = [
    "Ali_K99", "Daniel_P", "Sara_GG", "xX_Cobra_Xx", "Reza_77",
    "ProGamer88", "IronWolf7", "NightStar", "Champion_K", "Shadow_X9",
    "MasterPlay", "Kamran_Ace", "DarkFire", "QuickShot9", "EliteKing",
    "Arash_Pro", "Ninja_Storm", "CoolBreeze", "FlashPlayer", "TitanFist"
];
var BOT_DIFFICULTIES = [0, 1, 2];
// ─── Message routing ──────────────────────────────────────────────────────────
var MessagesLogic = {
    7: ChooseTurnPlayer,
    8: Rematch,
    10: StickersManager,
};
