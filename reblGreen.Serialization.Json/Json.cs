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


        /// <summary>
        /// This field is true by default and enforces camelCasing convention when serializing and deserializing objects to JSON string representations. AutoCamelCase
        /// is overridden by JsonNameAttribute which allows you to give properties and fields your own naming convention.
        /// </summary>
        public static bool AutoCamelCase = true;

        
        /// <summary>
        /// SerializationFactory allows you to easily inject a custom <see cref="IStringSerializer"/> to control the way a specific object type is serialized or deserialized.
        /// </summary>
        public static readonly StringSerializerFactory SerializationFactory = new StringSerializerFactory();

        
        /// <summary>
        /// reblGreen.Serialisation.Json returns a new initialized object of type T which has its properties and fields populated from a valid formatted JSON object string.
        /// </summary>
        public static T FromJson<T>(this object @this, string jsonString)
        {
            if (@this == null)
            {
                return Reader.FromJson<T>(jsonString, SerializationFactory);
            }

            return (T)Reader.FromJson(@this.GetType(), jsonString, SerializationFactory);
        }

        ///// <summary>
        ///// reblGreen.Serialisation.Json returns a new initialized object of type T which has its properties and fields populated from a valid formatted JSON object string.
        ///// </summary>
        //public static T FromJson<T>(this T @this, string jsonString)
        //{
        //    //return (T)Reader.FromJson(@this.GetType(), jsonString, SerializationFactory);
        //    return Reader.FromJson<T>(jsonString, SerializationFactory);
        //}

        /// <summary>
        /// reblGreen.Serialization.Json returns a JSON object string representation of a .NET object.
        /// </summary>
        public static string ToJson<T>(this T @this)
        {
            return Writer.ToJson(@this, SerializationFactory);
        }


        /// <summary>
        /// reblGreen.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
        /// a new .NET object of type T and returns the newly initialized object.
        /// </summary>
        public static T FromDictionary<T>(this object @this, Dictionary<string, object> dictionary)
        {
            var json = ToJson(dictionary);
            return FromJson<T>(@this, json);
        }

        ///// <summary>
        ///// reblGreen.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
        ///// a new .NET object of type T and returns the newly initialized object.
        ///// </summary>
        //public static T FromDictionary<T>(this T @this, Dictionary<string, object> dictionary) where T : class
        //{
        //    var json = ToJson(dictionary);
        //    return (null as T).FromJson(json);
        //}


        /// <summary>
        /// reblGreen.Serialization.Json wrapper method which can deserialize a valid JSON object string representation or serialize a .NET object of type T to a JSON string
        /// representation and then deserializes the string into a new Dictionary{string, object} and returns the Dictionary{string, object}. If the string is not valid JSON
        /// this invokation will return null.
        /// </summary>
        public static Dictionary<string, object> ToDictionary<T>(this T @this) where T : class
        {
            if (typeof(T) == typeof(string))
            {
                return FromJson<Dictionary<string, object>>(@this as string);
            }

            var json = ToJson(@this);
            return FromJson<Dictionary<string, object>>(json);
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


            return FromJson<T>(json);
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