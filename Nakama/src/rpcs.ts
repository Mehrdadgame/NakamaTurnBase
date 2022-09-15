let joinOrCreateMatch: nkruntime.RpcFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, payload: string): string
{
    let matches: nkruntime.Match[];
    const MatchesLimit = 1;
    const MinimumPlayers = 1;
    var label: MatchLabel = { open: true }
    matches = nakama.matchList(MatchesLimit, true, JSON.stringify(label), MinimumPlayers, MaxPlayers );
    if (matches.length > 0){
        var s= new ScoreCalss;
        s.ScoreF=0;
        SaveScore(context.userId,0,nakama,s);
        return matches[0].matchId;
    }
    var s= new ScoreCalss;
    s.ScoreF=0;
    SaveScore(context.userId,0,nakama,s);
    return nakama.matchCreate(MatchModuleName);
} 