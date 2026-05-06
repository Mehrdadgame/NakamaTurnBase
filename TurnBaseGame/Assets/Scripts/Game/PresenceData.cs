using Newtonsoft.Json;

namespace NinjaBattle.Game
{
    public class PresenceData
    {
        #region FIELDS

        private const string SessionIdKey = "sessionId";
        private const string UserIdKey = "userId";
        private const string UsernameKey = "username";

        #endregion

        #region PROPERTIES

        [JsonProperty(SessionIdKey)] public string SessionId { get; private set; }
        [JsonProperty(UserIdKey)] public string UserId { get; private set; }
        [JsonProperty(UsernameKey)] public string Username { get; private set; }

        #endregion

        #region CONSTRUCTORS

        public PresenceData(string sessionId, string userId = "", string username = "")
        {
            SessionId = sessionId;
            UserId = userId;
            Username = username;
        }

        #endregion
    }
}
