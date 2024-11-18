using System;
using System.Collections;
using System.Collections.Generic;
using NetTools.Serialization.JsonTools;
using NetTools.Serialization.Objects;
using NetTools.Serialization.Serializers;

namespace NetTools.Serialization
{
    /// <summary>
    /// NetTools.Serialization.Json is a small, fast JSON serialization library that was originally a fork of "Really simple JSON parser in ~300 lines"
    /// </summary>
    public static class Json
    {
        internal static JsonWriter Writer = new JsonWriter();
        internal static JsonReader Reader = new JsonReader();
        internal static Dictionary<Type, List<string>> NonSerialized = new Dictionary<Type, List<string>>();


        /// <summary>
        /// This value is true by default and enforces camelCasing convention when serializing and deserializing objects to JSON string representations. AutoCamelCase
        /// is overridden by JsonNameAttribute which allows you to give properties and fields your own naming convention.
        /// </summary>
        public static bool AutoCamelCase = true;


        /// <summary>
        /// This value is true by default and ignores character casing when deserializing fields and properties by name in .NET objects from a JSON string. This will match
        /// a JSON key to an object property or field name regardless of any upper or lowercase difference and is useful when deserializing lowercase key names into global
        /// property names where the global property names start with an uppercase character.
        /// </summary>
        public static bool IgnoreCase = true;


        /// <summary>
        /// This value is true by default and allows the JSON parser to try parsing incomplete JSON objects. This could be a partially downloaded JSON object for example.
        /// An incomplete JSON object is expected to be valid JSON up to the point of incompletion.
        /// </summary>
        public static bool ParseBroken = true;


        /// <summary>
        /// SerializationFactory allows you to easily inject a custom <see cref="IStringSerializer"/> to control the way a specific object type is serialized or deserialized.
        /// </summary>
        public static readonly StringSerializerFactory SerializationFactory = new StringSerializerFactory();


        /// <summary>
        /// NetTools.Serialisation.Json will exclude properties or fields that are decorated witn a NonSerializedAttribute, a JsonIgnoreAttribute, or are added as
        /// NonSerialized members using this method before the type's members are cached. Due to caching, it is best to use this method (where required) before first
        /// serialize/deserialize of the object type you wish to exclude serialization of members for.
        /// </summary>
        public static void AddNonSerializedMember(Type type, string memberName)
        {
            if (NonSerialized.TryGetValue(type, out var members))
            {
                members.Add(memberName);
                
            }
            else
            {
                NonSerialized[type] = new List<string> { memberName };
            }
        }


        /// <summary>
        /// NetTools.Serialisation.Json returns a new initialized object of type T which has its properties and fields populated from a valid formatted JSON object string.
        /// </summary>
        public static object ObjectFromJson(this object @this, string jsonString, bool includePrivates = false)
        {
            if (@this == null)
            {
                throw new NullReferenceException("Input object can not be null.");
            }

            return Reader.FromJson(@this.GetType(), jsonString, SerializationFactory, includePrivates, ParseBroken);
        }


        /// <summary>
        /// NetTools.Serialisation.Json returns a new initialized object of type T which has its properties and fields populated from a valid formatted JSON object string.
        /// </summary>
        public static object TypeFromJson(this Type @this, string jsonString, bool includePrivates = false)
        {
            if (@this == null)
            {
                throw new NullReferenceException("Input type can not be null.");
            }

            return Reader.FromJson(@this, jsonString, SerializationFactory, includePrivates, ParseBroken);
        }


        /// <summary>
        /// NetTools.Serialisation.Json returns a new initialized object of type T which has its properties and fields populated from a valid formatted JSON object string.
        /// </summary>
        public static T FromJson<T>(this T @this, string jsonString, bool includePrivates = false)
        {
            return Reader.FromJson<T>(jsonString, SerializationFactory, includePrivates, ParseBroken);
            //return (T)Reader.FromJson(@this.GetType(), jsonString, SerializationFactory);
        }


        /// <summary>
        /// NetTools.Serialization.Json returns a JSON object string representation of a .NET object.
        /// </summary>
        public static string ToJson<T>(this T @this, bool serializeEmptyFields = false, bool includePrivates = false)
        {
            return Writer.ToJson(@this, SerializationFactory, NonSerialized, serializeEmptyFields, includePrivates);
        }


        /// <summary>
        /// NetTools.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
        /// a new .NET object of type T and returns the newly initialized object.
        /// </summary>
        public static object FromDictionary(this object @this, Dictionary<string, object> dictionary, bool includePrivates = false)
        {
            var json = ToJson(dictionary, false, includePrivates);
            return FromJson(@this, json, includePrivates);
        }

        /// <summary>
        /// NetTools.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
        /// a new .NET object of type T and returns the newly initialized object.
        /// </summary>
        public static object ObjectFromDictionary(this object @this, Dictionary<string, object> dictionary, bool includePrivates = false)
        {
            if (@this == null)
            {
                throw new NullReferenceException("Input object can not be null.");
            }

            var json = ToJson(dictionary, false, includePrivates);
            return Reader.FromJson(@this.GetType(), json, SerializationFactory, includePrivates, ParseBroken);
        }


        ///// <summary>
        ///// NetTools.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
        ///// a new .NET object of type T and returns the newly initialized object.
        ///// </summary>
        public static T FromDictionary<T>(this T @this, Dictionary<string, object> dictionary, bool includePrivates = false) where T : class
        {
            var json = ToJson(dictionary, false, includePrivates);
            return (null as T).FromJson(json, includePrivates);
        }


        ///// <summary>
        ///// NetTools.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
        ///// a new .NET object of type T and returns the newly initialized object.
        ///// </summary>
        public static object TypeFromDictionary(this Type @this, Dictionary<string, object> dictionary, bool includePrivates = false, bool allowUnterminated = true)
        {
            var json = ToJson(dictionary, false, includePrivates);
            return @this.TypeFromJson(json, includePrivates);
        }


        /// <summary>
        /// NetTools.Serialization.Json wrapper method which can deserialize a valid JSON object string representation or serialize a .NET object of type T to a JSON string
        /// representation and then deserializes the string into a new Dictionary{string, object} and returns the Dictionary{string, object}. If the string is not a valid
        /// JSON object this invokation will return null.
        /// </summary>
        public static Dictionary<string, object> ToDictionary<T>(this T @this, bool includePrivates = false) where T : class
        {
            if (typeof(T) == typeof(string) || @this is string)
            {
                return FromJson<Dictionary<string, object>>(@this as string, includePrivates);
            }

            var json = ToJson(@this, false, includePrivates);
            return FromJson<Dictionary<string, object>>(json, includePrivates);
        }


        /// <summary>
        /// NetTools.Serialization.Json wrapper method which can deserialize a valid JSON object string representation of a JSON array
        /// into a new List{object}. If the string is not a valid JSON array this invokation will return null.
        /// </summary>
        public static List<object> ToList(this string @this)
        {
            return FromJson<List<object>>(@this, false);
        }


        /// <summary>
        /// NetTools.Serialization.Json wrapper method which works in the same way as <see cref="ToDictionary{T}(T, bool)"/> but instead
        /// returns a <see cref="DynamicJson"/> object. <see cref="DynamicJson"/> inherits from <see cref="System.Dynamic.DynamicObject"/>
        /// type.
        /// </summary>
        public static DynamicJson ToDynamic<T>(this T @this, bool includePrivates = false) where T : class
        {
            if (typeof(T) == typeof(string) || @this is string)
            {
                return new DynamicJson(@this as string);
            }

            var json = ToJson(@this, false, includePrivates);
            return new DynamicJson(json);
        }


        /// <summary>
        /// NetTools.Serialisation.Json returns a new initialized object of type T which has its properties and fields populated from a valid formatted JSON object string.
        /// </summary>
        public static T FromJson<T>(string jsonString, bool includePrivates = false) where T : class
        {
            return Reader.FromJson<T>(jsonString, SerializationFactory, includePrivates, ParseBroken);
        }

        
        /// <summary>
        /// NetTools.Serialization.Json wrapper method which serializes a Dictionary{string, object} to a JSON string representation and then deserializes the string into
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
        /// NetTools.Serialization.Json method which outputs a human-readable indented json string.
        /// </summary>
        public static string BeautifyJson(this string jsonString)
        {
            return JsonFormatter.PrettyPrint(jsonString);
        }


        /// <summary>
        /// NetTools.Serialization.Json method for removing standard single line and multiline comments
        /// out of a JSON object. This will also remove any whitespace characters, essentially minifying
        /// the JSON object. If you need to reinsert whitespace characters use <see cref="Json.BeautifyJson(string)"/>.
        /// </summary>
        public static string MinifyJson(this string jsonString)
        {
            return JsonFormatter.RemoveCommentsAndWhiteSpace(jsonString);
        }


        /// <summary>
        /// NetTools.Serialization.Json method which adds quotes to the start and end of a string. Given the input of "string", the output would be "\"string\"".
        /// </summary>
        public static string AddDoubleQuotes(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "\"\"";
            }

            return '"' + s + '"';
        }


        /// <summary>
        /// NetTools.Serialization.Json method which removes quotes from the beginning and end of a string. Given the input of "\"string\"", the output would be "string".
        /// </summary>
        public static string RemoveDoubleQuotes(this string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length < 2)
            {
                return s;
            }

            var start = s[0] == '"' ? 1 : 0;
            var end = s[s.Length - 1] == '"' ? s.Length - 1 : s.Length;

            return s.Substring(start, end - start);
        }


        /// <summary>
        /// NetTools.Serialization.Json string extension method that will attempt to parse unicode escaped charaters within a string (Eg. \014c).
        /// </summary>
        public static string DecodeUnicodeCharacters(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            return Uri.UnescapeDataString(s);
        }

    }
}