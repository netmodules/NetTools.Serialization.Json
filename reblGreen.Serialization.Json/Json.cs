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
        public static object ObjectFromJson(this object @this, string jsonString, bool includePrivates = false)
        {
            if (@this == null)
            {
                throw new NullReferenceException("Input object can not be null.");
            }

            return Reader.FromJson(@this.GetType(), jsonString, SerializationFactory, includePrivates);
        }

        ///// <summary>
        ///// reblGreen.Serialisation.Json returns a new initialized object of type T which has its properties and fields populated from a valid formatted JSON object string.
        ///// </summary>
        public static T FromJson<T>(this T @this, string jsonString, bool includePrivates = false)
        {
            return Reader.FromJson<T>(jsonString, SerializationFactory, includePrivates);
            //return (T)Reader.FromJson(@this.GetType(), jsonString, SerializationFactory);
        }

        /// <summary>
        /// reblGreen.Serialization.Json returns a JSON object string representation of a .NET object.
        /// </summary>
        public static string ToJson<T>(this T @this, bool serializeEmptyFields = false, bool includePrivates = false)
        {
            return Writer.ToJson(@this, SerializationFactory, serializeEmptyFields, includePrivates);
        }


        /// <summary>
        /// reblGreen.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
        /// a new .NET object of type T and returns the newly initialized object.
        /// </summary>
        public static object FromDictionary(this object @this, Dictionary<string, object> dictionary, bool includePrivates = false)
        {
            var json = ToJson(dictionary, false, includePrivates);
            return FromJson(@this, json, includePrivates);
        }

        ///// <summary>
        ///// reblGreen.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
        ///// a new .NET object of type T and returns the newly initialized object.
        ///// </summary>
        public static T FromDictionary<T>(this T @this, Dictionary<string, object> dictionary, bool includePrivates = false) where T : class
        {
            var json = ToJson(dictionary, false, includePrivates);
            return (null as T).FromJson(json, includePrivates);
        }


        /// <summary>
        /// reblGreen.Serialization.Json wrapper method which can deserialize a valid JSON object string representation or serialize a .NET object of type T to a JSON string
        /// representation and then deserializes the string into a new Dictionary{string, object} and returns the Dictionary{string, object}. If the string is not valid JSON
        /// this invokation will return null.
        /// </summary>
        public static Dictionary<string, object> ToDictionary<T>(this T @this, bool includePrivates = false) where T : class
        {
            if (typeof(T) == typeof(string))
            {
                return FromJson<Dictionary<string, object>>(@this as string, includePrivates);
            }

            var json = ToJson(@this, false, includePrivates);
            return FromJson<Dictionary<string, object>>(json, includePrivates);
        }


        /// <summary>
        /// reblGreen.Serialization.Json wrapper method which works in the same way as <see cref="ToDictionary{T}(T)"/> but instead returns a
        /// <see cref="DynamicJson"/> object. DynamicJson inherits from <see cref="System.Dynamic.DynamicObject"/> type.
        /// </summary>
        public static DynamicJson ToDynamic<T>(this T @this, bool includePrivates = false) where T : class
        {
            if (typeof(T) == typeof(string))
            {
                return new DynamicJson(@this as string);
            }

            var json = ToJson(@this, false, includePrivates);
            return new DynamicJson(json);
        }


        /// <summary>
        /// reblGreen.Serialisation.Json returns a new initialized object of type T which has its properties and fields populated from a valid formatted JSON object string.
        /// </summary>
        public static T FromJson<T>(string jsonString, bool includePrivates = false) where T : class
        {
            return Reader.FromJson<T>(jsonString, SerializationFactory, includePrivates);
        }

        
        /// <summary>
        /// reblGreen.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
        /// a new .NET object of type T and returns the newly initialized object.
        /// </summary>
        public static T FromDictionary<T>(Dictionary<string, object> dictionary, bool includePrivates = false) where T : class
        {
            var json = ToJson(dictionary, false, includePrivates);

            if (typeof(T) == typeof(string))
            {
                return json as T;
            }


            return FromJson<T>(json, includePrivates);
        }


        /// <summary>
        /// reblGreen.Serialization.Json method which outputs a human-readable indented json string.
        /// </summary>
        public static string BeautifyJson(this string jsonString)
        {
            return JsonFormatter.PrettyPrint(jsonString);
        }


        /// <summary>
        /// reblGreen.Serialization.Json method for removing standard single line and multiline comments
        /// out of a JSON object. This will also remove any whitespace characters, essentially minifying
        /// the JSON object. If you need to reinsert whitespace characters use <see cref="Json.BeautifyJson(string)"/>.
        /// </summary>
        public static string MinifyJson(this string jsonString)
        {
            return JsonFormatter.RemoveCommentsAndWhiteSpace(jsonString);
        }


        /// <summary>
        /// reblGreen.Serialization.Json method which adds quotes to the start and end of a string. Given the input of "string", the output would be "\"string\"".
        /// </summary>
        public static string AddDoubleQuotes(this string s)
        {
            return '"' + s + '"';
        }


        /// <summary>
        /// reblGreen.Serialization.Json method which removes quotes from the beginning and end of a string. Given the input of "\"string\"", the output would be "string".
        /// </summary>
        public static string RemoveDoubleQuotes(this string s)
        {
            var start = s[0] == '"' ? 1 : 0;
            var end = s[s.Length - 1] == '"' ? s.Length - 1 : s.Length;

            return s.Substring(start, end - start);
        }


        /// <summary>
        /// reblGreen.Serialization.Json string extension method that will attempt to parse unicode escaped charaters within a string (Eg. \014c).
        /// </summary>
        public static string DecodeUnicodeCharacters(this string s)
        {
            return Uri.UnescapeDataString(s);
        }

    }
}