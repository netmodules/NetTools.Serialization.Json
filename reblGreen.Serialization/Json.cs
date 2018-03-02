using System;
using System.Collections.Generic;

namespace reblGreen.Serialization
{
    public static class Json
    {
        public static T FromJson<T>(this T @this, string jsonString) where T: class
        {
            return default(T);   
        }

        public static string ToJson<T>(this T @this) where T: class
        {
            return JsonSerializationFactory.SerializeJson(@this);
            //return String.Empty;
        }

        public static T FromDictionary<T>(this T @this, Dictionary<string, object> dictionary) where T : class
        {
            return default(T);
        }

        public static Dictionary<string, object> ToDictionary<T>(this T @this) where T : class
        {
            return new Dictionary<string, object>();
        }
    }
}
