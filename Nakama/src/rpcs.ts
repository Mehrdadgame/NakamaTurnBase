/**
 * If there are any open matches, join the first one. If there are no open matches, create a new one.
 * @param context - The context of the RPC call.
 * @param logger - A logger object that can be used to log messages to the server console.
 * @param nakama - nkruntime.Nakama - The Nakama runtime object.
 * @param {string} payload - string - The payload sent from the client.
 * @returns The match ID.
 */
   let gameMode :string="" ;
let joinOrCreateMatch: nkruntime.RpcFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, payload: string): string
{
    let matches: nkruntime.Match[];
    const MatchesLimit = 1;
    const MinimumPlayers = 1;
    var label: MatchLabel = { open: true ,game_mode:payload}
    var query = "+label.open:true +label.game_mode:"+payload;
    matches = nakama.matchList(MatchesLimit, true, JSON.stringify(label), MinimumPlayers, MaxPlayers );
    if (matches.length > 0)
    {
        var s= new ScoreCalss;
        s.ScoreF=0;
        SaveScore(context.userId,0,nakama,s);
        return matches[0].matchId;
    }
    var s= new ScoreCalss;
    s.ScoreF=0;
    SaveScore(context.userId,0,nakama,s);

   // nakama.leaderboardRecordWrite(IdLeaderboard,context.userId,context.username,10)
   //CreateLeaderborad(context,logger,nakama);
   var persons: { [id: string] : string; } = {};
   persons= { "mode": payload };
    return nakama.matchCreate(MatchModuleName,persons);
} 
 function CreateLeaderborad(context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama){ 
 let id =IdLeaderboard;
let authoritative = true;
let sort = nkruntime.SortOrder.DESCENDING;
let operator = nkruntime.Operator.BEST;
let reset = null;
let metadata = {};
try {
  nakama.leaderboardCreate(id, authoritative, sort, operator, reset, metadata);
 
} catch(error) {
  
    // Handle error
}

}
