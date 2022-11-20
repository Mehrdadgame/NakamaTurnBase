

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
interface Claims {
  id: string,
  username: string
}

const BeforeAuthenticateCustom: nkruntime.BeforeHookFunction<nkruntime.AuthenticateCustomRequest> = function (ctx: nkruntime.Context, logger: nkruntime.Logger, nk: nkruntime.Nakama, data: nkruntime.AuthenticateCustomRequest): nkruntime.AuthenticateCustomRequest | void {

if(data.account?.id==null)
return data;

 
    
    var secretKey= ctx.env["MIIEvwIBADANBgkqhkiG9w0BAQEFAASCBKkwggSlAgEAAoIBAQC7VJTUt9Us8cKjMzEfYyjiWA4R4/M2bS1GB4t7NXp98C3SC6dVMvDuictGeurT8jNbvJZHtCSuYEvuNMoSfm76oqFvAp8Gy0iz5sxjZmSnXyCdPEovGhLa0VzMaQ8s+CLOyS56YyCFGeJZqgtzJ6GR3eqoYSW9b9UMvkBpZODSctWSNGj3P7jRFDO5VoTwCQAWbFnOjDfH5Ulgp2PKSQnSJP3AJLQNFNe7br1XbrhV//eO+t51mIpGSDCUv3E0DDFcWDTH9cXDTTlRZVEiR2BwpZOOkE/Z0/BVnhZYL71oZV34bKfWjQIt6V/isSMahdsAASACp4ZTGtwiVuNd9tybAgMBAAECggEBAKTmjaS6tkK8BlPXClTQ2vpz/N6uxDeS35mXpqasqskVlaAidgg/sWqpjXDbXr93otIMLlWsM+X0CqMDgSXKejLS2jx4GDjI1ZTXg++0AMJ8sJ74pWzVDOfmCEQ/7wXs3+cbnXhKriO8Z036q92Qc1+N87SI38nkGa0ABH9CN83HmQqt4fB7UdHzuIRe/me2PGhIq5ZBzj6h3BpoPGzEP+x3l9YmK8t/1cN0pqI+dQwYdgfGjackLu/2qH80MCF7IyQaseZUOJyKrCLtSD/Iixv/hzDEUPfOCjFDgTpzf3cwta8+oE4wHCo1iI1/4TlPkwmXx4qSXtmw4aQPz7IDQvECgYEA8KNThCO2gsC2I9PQDM/8Cw0O983WCDY+oi+7JPiNAJwv5DYBqEZB1QYdj06YD16XlC/HAZMsMku1na2TN0driwenQQWzoev3g2S7gRDoS/FCJSI3jJ+kjgtaA7Qmzlgk1TxODN+G1H91HW7t0l7VnL27IWyYo2qRRK3jzxqUiPUCgYEAx0oQs2reBQGMVZnApD1jeq7n4MvNLcPvt8b/eU9iUv6Y4Mj0Suo/AU8lYZXm8ubbqAlwz2VSVunD2tOplHyMUrtCtObAfVDUAhCndKaA9gApgfb3xw1IKbuQ1u4IF1FJl3VtumfQn//LiH1B3rXhcdyo3/vIttEk48RakUKClU8CgYEAzV7W3COOlDDcQd935DdtKBFRAPRPAlspQUnzMi5eSHMD/ISLDY5IiQHbIH83D4bvXq0X7qQoSBSNP7Dvv3HYuqMhf0DaegrlBuJllFVVq9qPVRnKxt1Il2HgxOBvbhOT+9in1BzA+YJ99UzC85O0Qz06A+CmtHEy4aZ2kj5hHjECgYEAmNS4+A8Fkss8Js1RieK2LniBxMgmYml3pfVLKGnzmng7H2+cwPLhPIzIuwytXywh2bzbsYEfYx3EoEVgMEpPhoarQnYPukrJO4gwE2o5Te6T5mJSZGlQJQj9q4ZB2Dfzet6INsK0oG8XVGXSpQvQh3RUYekCZQkBBFcpqWpbIEsCgYAnM3DQf3FJoSnXaMhrVBIovic5l0xFkEHskAjFTevO86Fsz1C2aSeRKSqGFoOQ0tmJzBEs1R6KqnHInicDTQrKhArgLXX4v3CddjfTRJkFWDbE/CkvKZNOrcf1nhaGCPspRJj2KUkj1Fhl9Cncdn/RsYEONbwQSjIfMPkvxF+8HQ=="];
    const claims = verifyAndParseJwt(secretKey, data.account.id ,nk);

    
  
    // Update the incoming authenticate request with the user ID and username
    data.account.id = claims.id;
    data.username = claims.username;
  
    return data;
  
}
const verifyAndParseJwt = function (secretKey: string, jwt: string ,nk: nkruntime.Nakama): Claims {

  var decode = nk.base64Decode(jwt);
 var decodejson:Claims = JSON.parse( decode)

  // Use your favourite JWT library to verify the signature and decode the JWT contents
  // Once verified and decoded, return a Claims object accordingly
  return decodejson;
}
