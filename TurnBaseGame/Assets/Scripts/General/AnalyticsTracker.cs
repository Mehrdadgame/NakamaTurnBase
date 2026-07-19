using System.Collections.Generic;
using GameAnalyticsSDK;
using GameAnalyticsSDK.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NinjaBattle.General
{
    public static class AnalyticsTracker
    {
        private static bool _initialized;

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized = true;
            if (!GameAnalytics.Initialized)
                GameAnalytics.Initialize();
        }

        public static void SendDesign(string eventName, float value = 1f, IDictionary<string, object> fields = null)
        {
            EnsureInitialized();

            var safeFields = fields != null ? new Dictionary<string, object>(fields) : new Dictionary<string, object>();

            if (!safeFields.ContainsKey("platform"))
                safeFields["platform"] = Application.platform.ToString();

            if (!safeFields.ContainsKey("scene"))
                safeFields["scene"] = SceneManager.GetActiveScene().name;

            GA_Design.NewEvent(eventName, value, safeFields, false);
        }
    }
}
