let gameMode: string = "";

let joinOrCreateMatch: nkruntime.RpcFunction = function (
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama,
    payload: string
): string {
    const label: MatchLabel = { open: true, game_mode: payload };

    const matches = nakama.matchList(1, true, JSON.stringify(label), 1, MaxPlayers);
    if (matches.length > 0) {
        return matches[0].matchId;
    }

    const persons: { [key: string]: string } = { mode: payload };
    return nakama.matchCreate(MatchModuleName, persons);
};

function findCardById(id: number): CardDefinition | null {
    for (let i = 0; i < CARD_DEFINITIONS.length; i++) {
        if (CARD_DEFINITIONS[i].id === id) return CARD_DEFINITIONS[i];
    }
    return null;
}

let purchaseCardRpc: nkruntime.RpcFunction = function (
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama,
    payload: string
): string {
    const userId = context.userId;
    if (!userId) throw new Error("User not authenticated");

    const data = JSON.parse(payload) as { cardId: number };
    const card = findCardById(data.cardId);
    if (!card) throw new Error("Invalid card id: " + data.cardId);

    // Deduct coins — nakama.walletUpdate throws if balance goes negative
    try {
        nakama.walletUpdate(userId, { coins: -card.cost }, { source: "card_purchase", cardId: data.cardId }, true);
    } catch (e) {
        throw new Error("Insufficient coins to purchase this card.");
    }

    // Increment card count in storage
    const stored = nakama.storageRead([{
        collection: CollectionCards,
        key: KeyOwnedCards,
        userId: userId,
    }]);
    let owned: OwnedCardsData = { cards: {} };
    if (stored.length > 0) {
        owned = stored[0].value as OwnedCardsData;
    }
    const key = String(data.cardId);
    owned.cards[key] = (owned.cards[key] || 0) + 1;

    nakama.storageWrite([{
        collection: CollectionCards,
        key: KeyOwnedCards,
        userId: userId,
        value: owned,
        permissionRead: 1,
        permissionWrite: 0,
    }]);

    return JSON.stringify({ success: true, cardId: data.cardId, newCount: owned.cards[key] });
};

let getCardsRpc: nkruntime.RpcFunction = function (
    context: nkruntime.Context,
    logger: nkruntime.Logger,
    nakama: nkruntime.Nakama,
    payload: string
): string {
    const userId = context.userId;
    if (!userId) throw new Error("User not authenticated");

    const stored = nakama.storageRead([{
        collection: CollectionCards,
        key: KeyOwnedCards,
        userId: userId,
    }]);
    let owned: OwnedCardsData = { cards: {} };
    if (stored.length > 0) {
        owned = stored[0].value as OwnedCardsData;
    }
    return JSON.stringify(owned);
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
