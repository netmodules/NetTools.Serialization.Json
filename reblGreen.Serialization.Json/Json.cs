using System;
using System.Collections.Generic;
using reblGreen.Serialization.JsonTools;
using reblGreen.Serialization.Objects;
using reblGreen.Serialization.Serializers;

namespace reblGreen.Serialization
{
    public static class Json
    {
        internal static JsonWriter Writer = new JsonWriter();
        internal static JsonReader Reader = new JsonReader();


        public static StringSerializerFactory SerializationFactory = new StringSerializerFactory();


        /// <summary>
        /// reblGreen.Serialisation.Json returns a new initialized object of type T which has its properties and fields populated from a valid formatted JSON object string.
        /// </summary>
        public static T FromJson<T>(this T @this, string jsonString) where T: class
        {
            return Reader.FromJson<T>(jsonString, SerializationFactory);
        }

        /// <summary>
        /// reblGreen.Serialization.Json returns a JSON object string representation of a .NET object.
        /// </summary>
        public static string ToJson<T>(this T @this) where T: class
        {
            return Writer.ToJson(@this, SerializationFactory);
        }


        /// <summary>
        /// reblGreen.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
        /// a new .NET object of type T and returns the newly initialized object.
        /// </summary>
        public static T FromDictionary<T>(this T @this, Dictionary<string, object> dictionary) where T : class
        {
            var json = ToJson(dictionary);
            return (null as T).FromJson(json);
        }


        /// <summary>
        /// reblGreen.Serialization.Json wrapper method which can deserialize a valid JSON object string representation or serialize a .NET object of type T to a JSON string
        /// representation and then deserializes the string into a new Dictionary{string, object} and returns the Dictionary{string, object}. If the string is not valid JSON
        /// this invokation will return null.
        /// </summary>
        public static Dictionary<string, object> ToDictionary<T>(this T @this) where T : class
        {
            if (typeof(T) == typeof(string))
            {
                return (null as Dictionary<string, object>).FromJson(@this as string);
            }

            var json = ToJson(@this);
            return (null as Dictionary<string, object>).FromJson(json);
        }


        /// <summary>
        /// reblGreen.Serialization.Json wrapper method which works in the same way as <see cref="ToDictionary{T}(T)"/> but instead returns a
        /// <see cref="DynamicJson"/> object. DynamicJson inherits from <see cref="System.Dynamic.DynamicObject"/> type.
        /// </summary>
        public static DynamicJson ToDynamic<T>(this T @this) where T : class
        {
            if (typeof(T) == typeof(string))
            {
                return new DynamicJson(@this as string);
            }

            var json = ToJson(@this);
            return new DynamicJson(json);
        }


        /// <summary>
        /// reblGreen.Serialisation.Json returns a new initialized object of type T which has its properties and fields populated from a valid formatted JSON object string.
        /// </summary>
        public static T FromJson<T>(string jsonString) where T : class
        {
            return Reader.FromJson<T>(jsonString, SerializationFactory);
        }


        /// <summary>
        /// reblGreen.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
        /// a new .NET object of type T and returns the newly initialized object.
        /// </summary>
        public static T FromDictionary<T>(Dictionary<string, object> dictionary) where T : class
        {
            var json = ToJson(dictionary);

            if (typeof(T) == typeof(string))
            {
                return json as T;
            }


            return (null as T).FromJson(json);
        }


        /// <summary>
        /// Adds quotes to the start and end of a string. Given the input of "string", the output would be "\"string\"".
        /// </summary>
        public static string AddDoubleQuotes(this string s)
        {
            return '"' + s + '"';
        }


        /// <summary>
        /// Removes quotes from the beginning and end of a string. Given the input of "\"string\"", the output would be "string".
        /// </summary>
        public static string RemoveDoubleQuotes(this string s)
        {
            var start = s[0] == '"' ? 1 : 0;
            var end = s[s.Length - 1] == '"' ? s.Length - 1 : s.Length;

            return s.Substring(start, end - start);
        }
    }
}