using Newtonsoft.Json;

namespace NinjaBattle.Game
{
  /* It's a class that contains a single property, SessionId, which is a string */
  
    public class PresenceData
    {
        #region FIELDS

        private const string SessionIdKey = "sessionId";

        #endregion

        #region PROPERTIES

        [JsonProperty(SessionIdKey)] public string SessionId { get; private set; }

        #endregion

        #region CONSTRUCTORS

        public PresenceData(string sessionId)
        {
            SessionId = sessionId;
        }

        #endregion
    }
}
