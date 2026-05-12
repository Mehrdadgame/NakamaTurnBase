let joinOrCreateMatch: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const label: MatchLabel = { open: true, game_mode: payload };
    const matches = nakama.matchList(1, true, JSON.stringify(label), 1, MaxPlayers);
    if (matches.length > 0) return matches[0].matchId;
    return nakama.matchCreate(MatchModuleName, { mode: payload });
};

// Creates a private tutorial match (bot-only, no entry fee, no rewards).
// Lobby countdown is 3 s so the client has time to load the game scene before ChangeScene fires.
let joinTutorialMatchRpc: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const matchId = nakama.matchCreate(MatchModuleName, {
        mode:     "ThreeByThree",
        tutorial: "true",
    });
    logger.info("Tutorial match created: " + matchId + " for userId=" + context.userId);
    return matchId;
};

function createDefaultProfile(): ProfileData {
    return {
        email: "", phone: "",
        emailLocked: false, phoneLocked: false,
        emailBonusClaimed: false, phoneBonusClaimed: false,
        avatarId: "avatar_0",
        ownedAvatars: [],
        welcomeBonusClaimed: false,
    };
}

function grantFirstLoginBonusIfNeeded(
    userId: string,
    profileObj: nkruntime.StorageObject | null,
    profile: ProfileData,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama
): ProfileData {
    if (profile.welcomeBonusClaimed)
        return profile;

    const updatedProfile: ProfileData = {
        email:             profile.email,
        phone:             profile.phone,
        emailLocked:       profile.emailLocked,
        phoneLocked:       profile.phoneLocked,
        emailBonusClaimed: profile.emailBonusClaimed,
        phoneBonusClaimed: profile.phoneBonusClaimed,
        avatarId:          profile.avatarId,
        ownedAvatars:      profile.ownedAvatars,
        welcomeBonusClaimed: true,
    };

    const writeRequest: nkruntime.StorageWriteRequest = {
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
        nakama.walletUpdate(
            userId,
            { coins: FirstLoginBonusCoins },
            { source: "first_login_bonus" },
            true
        );
        logger.info(`First login bonus granted: userId=${userId} coins=${FirstLoginBonusCoins}`);
        return updatedProfile;
    } catch (e) {
        // Another concurrent request may have already claimed the bonus.
        logger.info("First login bonus skipped (already claimed or conflict): " + e);
        const latest = nakama.storageRead([{ collection: CollectionProfile, key: KeyProfileData, userId }]);
        if (latest.length > 0)
            return latest[0].value as ProfileData;
        return updatedProfile;
    }
}

// Called on login — returns unclaimed weekly/monthly rewards and marks them claimed.
// Coins were already added to the wallet during the leaderboard reset hook.
let checkPendingRewardsRpc: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const userId = context.userId;
    if (!userId) throw new Error("Not authenticated");

    const stored = nakama.storageRead([
        { collection: CollectionSeason, key: KeyPendingRewardWeekly,  userId },
        { collection: CollectionSeason, key: KeyPendingRewardMonthly, userId },
    ]);

    let weekly:  PendingRewardData | null = null;
    let monthly: PendingRewardData | null = null;

    for (const obj of stored) {
        const val = obj.value as PendingRewardData;
        if (!val.claimed) {
            if (obj.key === KeyPendingRewardWeekly)  weekly  = val;
            if (obj.key === KeyPendingRewardMonthly) monthly = val;
        }
    }

    // Mark as claimed so popup only shows once
    const toWrite: nkruntime.StorageWriteRequest[] = [];
    if (weekly)  { weekly.claimed  = true; toWrite.push({ collection: CollectionSeason, key: KeyPendingRewardWeekly,  userId, value: weekly,  permissionRead: 1, permissionWrite: 0 }); }
    if (monthly) { monthly.claimed = true; toWrite.push({ collection: CollectionSeason, key: KeyPendingRewardMonthly, userId, value: monthly, permissionRead: 1, permissionWrite: 0 }); }
    if (toWrite.length > 0) nakama.storageWrite(toWrite);

    return JSON.stringify({ weekly, monthly });
};

// Returns top-N records for weekly or monthly leaderboard + caller's own record.
// Each record includes avatarId fetched from profile storage.
let getLeaderboardRpc: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const data   = JSON.parse(payload || "{}") as { type?: string; limit?: number };
    const lbId   = data.type === "monthly" ? LeaderboardMonthly : LeaderboardWeekly;
    const limit  = data.limit  || 100;
    const userId = context.userId;

    let records: nkruntime.LeaderboardRecord[] = [];
    let ownRecord: nkruntime.LeaderboardRecord | null = null;

    try {
        const result = nakama.leaderboardRecordsList(lbId, [], limit, "", 0);
        records = result.records || [];
        if (userId) {
            const own = nakama.leaderboardRecordsList(lbId, [userId], 1, "", 0);
            ownRecord = (own.ownerRecords && own.ownerRecords.length > 0) ? own.ownerRecords[0] : null;
        }
    } catch (e) {
        logger.warn("getLeaderboardRpc failed: " + e);
    }

    // Batch-read profiles to get avatarId for each player
    const storageReads: nkruntime.StorageReadRequest[] = [];
    const seenIds: { [id: string]: boolean } = {};
    for (const r of records) {
        if (r.ownerId && !seenIds[r.ownerId]) {
            storageReads.push({ collection: CollectionProfile, key: KeyProfileData, userId: r.ownerId });
            seenIds[r.ownerId] = true;
        }
    }
    if (ownRecord && ownRecord.ownerId && !seenIds[ownRecord.ownerId]) {
        storageReads.push({ collection: CollectionProfile, key: KeyProfileData, userId: ownRecord.ownerId });
    }

    const profileMap: { [userId: string]: ProfileData } = {};
    if (storageReads.length > 0) {
        try {
            const profiles = nakama.storageRead(storageReads);
            for (const obj of profiles) {
                profileMap[obj.userId] = obj.value as ProfileData;
            }
        } catch (e) { logger.warn("Profile batch read failed: " + e); }
    }

    const enriched = records.map(r => ({
        ownerId:  r.ownerId,
        username: r.username,
        score:    r.score,
        rank:     r.rank,
        avatarId: (profileMap[r.ownerId]?.avatarId) || "avatar_0",
    }));

    const enrichedOwn = ownRecord ? {
        ownerId:  ownRecord.ownerId,
        username: ownRecord.username,
        score:    ownRecord.score,
        rank:     ownRecord.rank,
        avatarId: (profileMap[ownRecord.ownerId]?.avatarId) || "avatar_0",
    } : null;

    return JSON.stringify({ records: enriched, ownRecord: enrichedOwn });
};

// ─── Avatar ───────────────────────────────────────────────────────────────────

let selectAvatarRpc: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const userId = context.userId;
    if (!userId) throw new Error("Not authenticated");

    const input = JSON.parse(payload || "{}") as { avatarId: string };
    if (!input.avatarId) return JSON.stringify({ success: false, error: "Missing avatarId" });

    const price: number = AVATAR_PRICES.hasOwnProperty(input.avatarId)
        ? AVATAR_PRICES[input.avatarId]
        : -1;

    if (price < 0)
        return JSON.stringify({ success: false, error: "Unknown avatar id" });

    const stored = nakama.storageRead([
        { collection: CollectionProfile, key: KeyProfileData, userId },
    ]);
    let profile: ProfileData = createDefaultProfile();
    if (stored.length > 0) profile = stored[0].value as ProfileData;

    const ownedAvatars = profile.ownedAvatars || [];
    const alreadyOwned = ownedAvatars.indexOf(input.avatarId) >= 0;

    if (price > 0 && !alreadyOwned) {
        const account = nakama.accountGetId(userId);
        const wallet  = account.wallet || {};
        if ((wallet["coins"] || 0) < price)
            return JSON.stringify({ success: false, error: "Insufficient coins" });

        try {
            nakama.walletUpdate(
                userId,
                { coins: -price },
                { source: "avatar_purchase", avatarId: input.avatarId },
                true
            );
        } catch (e) {
            logger.warn("Avatar purchase wallet deduct failed: " + e);
            return JSON.stringify({ success: false, error: "Payment failed" });
        }

        ownedAvatars.push(input.avatarId);
    }

    profile.ownedAvatars = ownedAvatars;
    profile.avatarId = input.avatarId;

    nakama.storageWrite([{
        collection:      CollectionProfile,
        key:             KeyProfileData,
        userId,
        value:           profile,
        permissionRead:  1,
        permissionWrite: 0,
    }]);

    logger.info(`Avatar selected: userId=${userId} avatarId=${input.avatarId} price=${price} alreadyOwned=${alreadyOwned}`);
    return JSON.stringify({ success: true, avatarId: input.avatarId, ownedAvatars, error: "" });
};

let getProfileRpc: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const userId = context.userId;
    if (!userId) throw new Error("Not authenticated");

    const account = nakama.accountGetId(userId);
    const displayName = account.user.displayName || account.user.username || "";

    const stored = nakama.storageRead([
        { collection: CollectionProfile, key: KeyProfileData, userId },
    ]);

    const profileObj = stored.length > 0 ? stored[0] : null;
    let profile: ProfileData = profileObj
        ? profileObj.value as ProfileData
        : createDefaultProfile();

    profile = grantFirstLoginBonusIfNeeded(userId, profileObj, profile, logger, nakama);

    const ownedAvatars = profile.ownedAvatars || [];
    const effectiveOwned: string[] = [];
    const allIds = Object.keys(AVATAR_PRICES);

    for (const id of allIds) {
        if (AVATAR_PRICES[id] === 0)
            effectiveOwned.push(id);
    }

    for (const id of ownedAvatars) {
        if (effectiveOwned.indexOf(id) < 0)
            effectiveOwned.push(id);
    }

    const avatarPriceList = allIds.map(id => ({
        id,
        price: AVATAR_PRICES[id],
    }));

    return JSON.stringify({
        displayName,
        email:       profile.email,
        phone:       profile.phone,
        emailLocked: profile.emailLocked,
        phoneLocked: profile.phoneLocked,
        avatarId: profile.avatarId || "avatar_0",
        ownedAvatars: effectiveOwned,
        avatarPrices: avatarPriceList,
    });
};

let updateProfileRpc: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const userId = context.userId;
    if (!userId) throw new Error("Not authenticated");

    const input = JSON.parse(payload || "{}") as {
        displayName?: string;
        email?: string;
        phone?: string;
    };

    // Load existing profile
    const stored = nakama.storageRead([
        { collection: CollectionProfile, key: KeyProfileData, userId },
    ]);
    let profile: ProfileData = createDefaultProfile();
    if (stored.length > 0) profile = stored[0].value as ProfileData;

    // ── Display name ────────────────────────────────────────────────────────
    if (input.displayName && input.displayName.trim().length > 0) {
        try {
            nakama.accountUpdateId(
                userId, null, input.displayName.trim(),
                null, null, null, null, null
            );
        } catch (e) {
            logger.warn("displayName update failed: " + e);
        }
    }

    let coinsAwarded = 0;

    // ── Email (locked once set) ─────────────────────────────────────────────
    if (input.email && input.email.trim().length > 0 && !profile.emailLocked) {
        profile.email      = input.email.trim();
        profile.emailLocked = true;
        if (!profile.emailBonusClaimed) {
            coinsAwarded += 100;
            profile.emailBonusClaimed = true;
        }
    }

    // ── Phone (locked once set) ─────────────────────────────────────────────
    if (input.phone && input.phone.trim().length > 0 && !profile.phoneLocked) {
        profile.phone      = input.phone.trim();
        profile.phoneLocked = true;
        if (!profile.phoneBonusClaimed) {
            coinsAwarded += 100;
            profile.phoneBonusClaimed = true;
        }
    }

    // ── Award coins ─────────────────────────────────────────────────────────
    if (coinsAwarded > 0) {
        try {
            nakama.walletUpdate(
                userId,
                { coins: coinsAwarded },
                { source: "profile_bonus" },
                true
            );
        } catch (e) {
            logger.warn("Profile bonus wallet update failed: " + e);
        }
    }

    // ── Save profile ────────────────────────────────────────────────────────
    nakama.storageWrite([{
        collection:      CollectionProfile,
        key:             KeyProfileData,
        userId,
        value:           profile,
        permissionRead:  1,
        permissionWrite: 0,
    }]);

    const updated = nakama.accountGetId(userId);
    return JSON.stringify({
        displayName:  updated.user.displayName || updated.user.username || "",
        email:        profile.email,
        phone:        profile.phone,
        emailLocked:  profile.emailLocked,
        phoneLocked:  profile.phoneLocked,
        coinsAwarded,
        error:        "",
    } as UpdateProfileResult);
};

// ─── Coin Shop (Cafebazaar IAP) ───────────────────────────────────────────────

const COIN_PACKS: { [id: string]: number } = {
    "SmallCoin_TasZan": 3500,    // کوچک
    "Standard_TasZan": 8000,    // استاندارد
    "Large_TasZan": 13000,   // بزرگ
    "VIP_TasZan": 18000,   // ویژه
    "King_TasZan": 28000,   // شاهانه
    "Legend_TasZan": 40000,   // اضافه‌ای
    "Diamond_TasZan": 55000,   // الماس
    "Empire_TasZan": 100000,  // امپراتور
};

let verifyCoinPurchaseRpc: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const userId = context.userId;
    if (!userId) throw new Error("Not authenticated");

    const input = JSON.parse(payload || "{}") as {
        productId: string;
        purchaseToken: string;
        orderId: string;
        dataSignature: string;
        originalJson: string;
    };

    if (!input.productId || !input.purchaseToken)
        return JSON.stringify({ success: false, error: "Missing purchase data" });

    const coins = COIN_PACKS[input.productId];
    if (!coins)
        return JSON.stringify({ success: false, error: "Unknown product: " + input.productId });

    // Use orderId as storage key; fall back to purchaseToken if missing
    const storageKey = (input.orderId && input.orderId.length > 0) ? input.orderId : input.purchaseToken;

    // Idempotency — reject duplicate tokens
    const existing = nakama.storageRead([{ collection: "payment", key: storageKey, userId }]);
    if (existing.length > 0)
        return JSON.stringify({ success: false, error: "Already processed" });

    // Log payment record
    nakama.storageWrite([{
        collection: "payment",
        key: storageKey,
        userId,
        value: {
            productId:     input.productId,
            purchaseToken: input.purchaseToken,
            orderId:       input.orderId,
            dataSignature: input.dataSignature,
            originalJson:  input.originalJson,
            coinsAwarded:  coins,
            timestamp:     Date.now(),
        },
        permissionRead:  1,
        permissionWrite: 0,
    }]);

    // Award coins
    nakama.walletUpdate(
        userId,
        { coins },
        { source: "iap", productId: input.productId, orderId: input.orderId },
        true
    );

    logger.info(`IAP purchase: userId=${userId} product=${input.productId} coins=${coins}`);
    return JSON.stringify({ success: true, coinsAwarded: coins });
};

// ─── Force Update ─────────────────────────────────────────────────────────────

// Returns { requiredVersion, updateUrl } stored under the system config collection.
// Admin updates the value via Nakama console → Storage → collection "config", key "app_version".
let getAppVersionRpc: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const stored = nakama.storageRead([
        { collection: CollectionConfig, key: KeyAppVersion, userId: SystemUserId },
    ]);

    if (stored.length === 0) {
        // Default: no update required — returns empty requiredVersion
        return JSON.stringify({ requiredVersion: "", updateUrl: "" } as AppVersionConfig);
    }

    return JSON.stringify(stored[0].value as AppVersionConfig);
};

function CreateLeaderboards(
    context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama
): void {
    const configs: Array<{ id: string; reset: string }> = [
        { id: LeaderboardWeekly,  reset: "0 0 * * 1" },   // every Monday 00:00 UTC
        { id: LeaderboardMonthly, reset: "0 0 1 * *" },   // 1st of every month
    ];
    for (const cfg of configs) {
        try {
            // Use raw strings — Nakama 3.x JS runtime expects "desc"/"asc" and "incr"/"decr"/"best"/"set"
            nakama.leaderboardCreate(cfg.id, true, "desc" as any, "incr" as any, cfg.reset, {});
            logger.info("Leaderboard created: " + cfg.id);
        } catch (e) {
            logger.info("Leaderboard " + cfg.id + ": " + e);
        }
    }
}

