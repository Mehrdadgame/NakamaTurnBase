using System.Collections.Generic;

// Mirrors server LEAGUES in consts.ts — keep values in sync.
public static class ClientLeagues
{
    public struct Info
    {
        public int entryFee;
        public int winnerReward;
        public int drawRefund;
    }

    public static readonly Dictionary<ModeGame, Info> All = new Dictionary<ModeGame, Info>
    {
        { ModeGame.ThreeByThree,          new Info { entryFee = 750, winnerReward = 1260, drawRefund = 375 } },
        { ModeGame.FourByThree,           new Info { entryFee = 500, winnerReward = 840,  drawRefund = 250 } },
        { ModeGame.VerticalAndHorizontal, new Info { entryFee = 250, winnerReward = 420,  drawRefund = 125 } },
    };

    public static Info Get(ModeGame mode)
    {
        return All.TryGetValue(mode, out var info) ? info : default;
    }
}
