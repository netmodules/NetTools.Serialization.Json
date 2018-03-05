using System;
using System.Collections.Generic;
using reblGreen.Serialization.JsonTools;

namespace reblGreen.Serialization
{
    public static class Json
    {
        static JsonWriter Writer = new JsonWriter();
        static JsonReader Reader = new JsonReader();

        public static StringSerializationFactory SerializationFactory = new StringSerializationFactory();

        public static T FromJson<T>(this T @this, string jsonString) where T: class
        {
            return Reader.FromJson<T>(jsonString, SerializationFactory);
        }

        public static string ToJson<T>(this T @this) where T: class
        {
            return Writer.ToJson(@this, SerializationFactory);
        }

        public static T FromDictionary<T>(this T @this, Dictionary<string, object> dictionary) where T : class
        {
            var json = ToJson(dictionary);
            return (null as T).FromJson(json);
        }

        public static Dictionary<string, object> ToDictionary<T>(this T @this) where T : class
        {
            var json = ToJson(@this);
            return (null as Dictionary<string, object>).FromJson(json);
        }



        public static T FromJson<T>(string jsonString) where T : class
        {
            return Reader.FromJson<T>(jsonString, SerializationFactory);
        }

        public static T FromDictionary<T>(Dictionary<string, object> dictionary) where T : class
        {
            var json = ToJson(dictionary);
            return (null as T).FromJson(json);
        }
    }
}
