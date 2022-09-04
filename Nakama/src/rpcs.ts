let joinOrCreateMatch: nkruntime.RpcFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, payload: string): string
{
    let matches: nkruntime.Match[];
    const MatchesLimit = 1;
    const MinimumPlayers = 1;
    var label: MatchLabel = { open: true }
    matches = nakama.matchList(MatchesLimit, true, JSON.stringify(label), MinimumPlayers, MaxPlayers );
    if (matches.length > 0)
        return matches[0].matchId;

    return nakama.matchCreate(MatchModuleName);
}
let turnManager: nkruntime.RpcFunction = function (context: nkruntime.Context, logger: nkruntime.Logger, nakama: nkruntime.Nakama, payload: string):string{
var users : string [] = new Array(2);
if(users.some(e=> e === payload)){
    return "";
    
}
    users.push( payload);
   
    logger.info(payload+ " $$$$$$$$$$$$$$$$$$$$$$$$$$$$$");
if(users.length>=2){
   
    var User =  users.pop()!;
    users.length =0;
    logger.info(User+ " User Name Send");
    return User;
  

}
return "";
    
}
