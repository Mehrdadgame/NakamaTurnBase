using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nakama.Helpers
{
    public static class JSONExtensions
    {
        #region BEHAVIORS

       /// <summary>
       /// It takes an object and returns a string
       /// </summary>
       /// <param name="obj">The object to serialize.</param>
       /// <returns>
       /// A string
       /// </returns>
        public static string Serialize(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

      /// <summary>
      /// It takes a string and returns an object of the type specified by the generic parameter
      /// </summary>
      /// <param name="json">The JSON string to deserialize.</param>
      /// <returns>
      /// The deserialized object.
      /// </returns>
        public static T Deserialize<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static Dictionary<string, string> Deserialize(this string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        #endregion
    }
}
