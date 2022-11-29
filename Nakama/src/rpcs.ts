
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
    matches = nakama.matchList(MatchesLimit, true, JSON.stringify(label), MinimumPlayers, MaxPlayers );
    if (matches.length > 0)
    {
        return matches[0].matchId;
    }
    //test
   
    ///
   var persons: { [id: string] : string; } = {};
   persons= { "mode": payload };
    return nakama.matchCreate("match",persons);
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
interface Claims {
  id: string,
  username: string
}



const BeforeAuthenticateCustom: nkruntime.BeforeHookFunction<nkruntime.AuthenticateCustomRequest> = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: nkruntime.AuthenticateCustomRequest): nkruntime.AuthenticateCustomRequest | void {

if(data.account?.id==null)
return data;
logger.debug(data.account.id + " sfgfdgeryerhyerEEEEEEEEEEEEEEEEEEEE");


 var token = nk.jwtGenerate('RS256','eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.cThIIoDvwdueQB468K5xDc5633seEFoqwxjF_xSJyQQ',{'sub':data.account.id});
 nk.rsaSha256Hash("id",)
 
logger.debug(token + " FFFFFFFFFFFFFFFFFFFFFFFFFFF");

    return data;
}
const verifyAndParseJwt = function (secretKey: string, jwt: string ,nk: nkruntime.Nakama): Claims {

  var decode = nk.base64Decode(jwt);
 var decodejson:Claims = JSON.parse( decode)

  // Use your favourite JWT library to verify the signature and decode the JWT contents
  // Once verified and decoded, return a Claims object accordingly
  return decodejson;
}



