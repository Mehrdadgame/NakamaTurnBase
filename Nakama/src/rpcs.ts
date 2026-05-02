let joinOrCreateMatch: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const label: MatchLabel = { open: true, game_mode: payload };
    const matches = nakama.matchList(1, true, JSON.stringify(label), 1, MaxPlayers);
    if (matches.length > 0) return matches[0].matchId;
    return nakama.matchCreate(MatchModuleName, { mode: payload });
};

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

        // Own rank (ownerRecords query)
        if (userId) {
            const own = nakama.leaderboardRecordsList(lbId, [userId], 1, "", 0);
            ownRecord = (own.ownerRecords && own.ownerRecords.length > 0) ? own.ownerRecords[0] : null;
        }
    } catch (e) {
        logger.warn("getLeaderboardRpc failed: " + e);
    }

    return JSON.stringify({ records, ownRecord });
};

// ─── Avatar ───────────────────────────────────────────────────────────────────

let selectAvatarRpc: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const userId = context.userId;
    if (!userId) throw new Error("Not authenticated");

    const input = JSON.parse(payload || "{}") as { avatarId: string };
    if (!input.avatarId) return JSON.stringify({ success: false, error: "Missing avatarId" });

    // Server-side price lookup — client cannot spoof the price
    const price: number = AVATAR_PRICES.hasOwnProperty(input.avatarId)
        ? AVATAR_PRICES[input.avatarId]
        : -1;

    if (price < 0)
        return JSON.stringify({ success: false, error: "Unknown avatar id" });

    if (price > 0) {
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
    }

    // Load profile, update avatarId, save
    const stored = nakama.storageRead([
        { collection: CollectionProfile, key: KeyProfileData, userId },
    ]);
    let profile: ProfileData = {
        email: "", phone: "",
        emailLocked: false, phoneLocked: false,
        emailBonusClaimed: false, phoneBonusClaimed: false,
        avatarId: "avatar_0",
    };
    if (stored.length > 0) profile = stored[0].value as ProfileData;

    profile.avatarId = input.avatarId;

    nakama.storageWrite([{
        collection:      CollectionProfile,
        key:             KeyProfileData,
        userId,
        value:           profile,
        permissionRead:  1,
        permissionWrite: 0,
    }]);

    logger.info(`Avatar selected: userId=${userId} avatarId=${input.avatarId} price=${price}`);
    return JSON.stringify({ success: true, avatarId: input.avatarId, error: "" });
};

// ─── Profile ──────────────────────────────────────────────────────────────────

let getProfileRpc: nkruntime.RpcFunction = function (
    context, logger, nakama, payload
): string {
    const userId = context.userId;
    if (!userId) throw new Error("Not authenticated");

    const account = nakama.accountGetId(userId);
    const displayName = account.user.displayName || "";

    const stored = nakama.storageRead([
        { collection: CollectionProfile, key: KeyProfileData, userId },
    ]);

    let profile: ProfileData = {
        email: "", phone: "",
        emailLocked: false, phoneLocked: false,
        emailBonusClaimed: false, phoneBonusClaimed: false,
        avatarId: "avatar_0",
    };
    if (stored.length > 0) profile = stored[0].value as ProfileData;

    return JSON.stringify({
        displayName,
        email:       profile.email,
        phone:       profile.phone,
        emailLocked: profile.emailLocked,
        phoneLocked: profile.phoneLocked,
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
    let profile: ProfileData = {
        email: "", phone: "",
        emailLocked: false, phoneLocked: false,
        emailBonusClaimed: false, phoneBonusClaimed: false,
        avatarId: "avatar_0",
    };
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
        displayName:  updated.user.displayName || "",
        email:        profile.email,
        phone:        profile.phone,
        emailLocked:  profile.emailLocked,
        phoneLocked:  profile.phoneLocked,
        coinsAwarded,
        error:        "",
    } as UpdateProfileResult);
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
