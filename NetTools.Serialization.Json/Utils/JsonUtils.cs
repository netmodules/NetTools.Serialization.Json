using System;
using System.Linq;
using System.Collections.Generic;
using NetTools.Serialization.Attributes;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Concurrent;

namespace NetTools.Serialization
{
    internal static class JsonUtils
    {
        static readonly ConcurrentDictionary<string, List<JsonProperty>> JsonPropertyCache = new ConcurrentDictionary<string, List<JsonProperty>>();
        //static readonly object Padlock = new object();


        public static List<JsonProperty> GetJsonProperties<T>(this T type, bool includePrivates, Dictionary<Type, List<string>> nonSerialized) where T : class
        {
            //lock (Padlock)
            //{
            Type t;
                
            if (type != null)
            {
                t = type.GetType();
            }
            else
            {
                t = typeof(T);
            }

            try
            {
                if (JsonPropertyCache.TryGetValue(t.FullName + includePrivates, out var cache))
                {
                    return cache;
                }
                else
                {
                    var props = FetchJsonProperties(t, includePrivates, nonSerialized);
                    JsonPropertyCache.TryAdd(t.FullName + includePrivates, props);
                    return props;
                }
            }
            catch
            {
                // Likely JsonPropertyCache was modified while invoking TryGetValue, ignore exception and return uncached properties.
                var props = FetchJsonProperties(t, includePrivates, nonSerialized);
                try
                {
                    JsonPropertyCache.TryAdd(t.FullName + includePrivates, props);
                }
                finally { }
                return props;
            }
            //}
        }

        internal static Dictionary<string, JsonProperty> GetJsonPropertiesDictionary<T>(this T type, bool includePrivates, Dictionary<Type, List<string>> nonSerialized = null) where T : class
        {
            Dictionary<string, JsonProperty> dictionary = null;

            if (Json.IgnoreCase)
            {
                dictionary = new Dictionary<string, JsonProperty>(StringComparer.InvariantCultureIgnoreCase);
            }
            else
            {
                dictionary = new Dictionary<string, JsonProperty>();
            }

            var jsonProps = GetJsonProperties(type, includePrivates, nonSerialized);

            foreach(var p in jsonProps)
            {
                dictionary.Add(p.Name, p);
            }

            return dictionary;
        }


        internal static List<JsonProperty> FetchJsonProperties(Type type, bool includePrivates, Dictionary<Type, List<string>> nonSerialized)
        {
            var jsonProps = new List<JsonProperty>();
            var typeProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var typeFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var ignore = new List<string>();

            if (nonSerialized != null)
            {
                var non = nonSerialized.Where(x => x.Key.IsAssignableFrom(type)).SelectMany(x => x.Value);
                
                if (non.Count() > 0)
                {
                    ignore = non.ToList();
                }
            }

            foreach (var member in typeProps)
            {
                if ((!includePrivates && !member.IsPublic()) || !member.IsReadable())
                {
                    continue;
                }

                if (member.GetMemberAttributes<JsonIgnore>().Any()
                    || member.GetMemberAttributes<System.Text.Json.Serialization.JsonIgnoreAttribute>().Any()
                    || member.GetMemberAttributes<NonSerializedAttribute>().Any())
                {
                    continue;
                }

                if (ignore.Contains(member.Name))
                {
                    continue;
                }


                var serializer = member.GetMemberAttributes<JsonSerializer>().FirstOrDefault();


                var jsonName = member.GetMemberAttributes<JsonName>().FirstOrDefault()?.ToString();
                var jsonPath = member.GetMemberAttributes<JsonPath>().FirstOrDefault()?.ToString();


                if (string.IsNullOrEmpty(jsonName))
                {
                    jsonName = member.Name;

                    if (Json.AutoCamelCase)
                    {
                        StringBuilder sb = new StringBuilder(jsonName);
                        sb[0] = char.ToLower(sb[0]);
                        jsonName = sb.ToString();
                    }
                }

                jsonProps.Add(new JsonProperty() { Serializer = serializer, Name = jsonName, Member = member, Path = jsonPath });
            }

            foreach (var member in typeFields)
            {

                if ((!includePrivates && member.IsPrivate) || !member.IsReadable() || member.IsStatic)
                {
                    continue;
                }

                // Additional Checks for compiler generated fields. Compiler generated fields are linked to auto get/setter properties.
                if (member.GetMemberAttributes<JsonIgnore>().Any()
                    || member.GetMemberAttributes<System.Runtime.CompilerServices.CompilerGeneratedAttribute>().Any()
                    || member.GetMemberAttributes<System.Text.Json.Serialization.JsonIgnoreAttribute>().Any()
                    || member.GetMemberAttributes<NonSerializedAttribute>().Any())
                {
                    continue;
                }

                if (ignore.Contains(member.Name))
                {
                    continue;
                }

                
                var serializer = member.GetMemberAttributes<JsonSerializer>().FirstOrDefault();


                var jsonName = member.GetMemberAttributes<JsonName>().FirstOrDefault()?.ToString();
                var jsonPath = member.GetMemberAttributes<JsonPath>().FirstOrDefault()?.ToString();


                if (string.IsNullOrEmpty(jsonName))
                {
                    jsonName = member.Name;

                    if (Json.AutoCamelCase)
                    {
                        StringBuilder sb = new StringBuilder(jsonName);
                        sb[0] = char.ToLower(sb[0]);
                        jsonName = sb.ToString();
                    }
                }

                jsonProps.Add(new JsonProperty() { Serializer = serializer, Name = jsonName, Member = member, Path = jsonPath });
            }

            return jsonProps;
        }
    }
}
