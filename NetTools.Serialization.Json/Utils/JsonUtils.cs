using System;
using System.Linq;
using System.Collections.Generic;
using NetTools.Serialization.Attributes;
using System.Text;
using System.Reflection;
using System.Collections;

namespace NetTools.Serialization
{
    internal static class JsonUtils
    {
        static readonly Dictionary<string, List<JsonProperty>> JsonPropertyCache = new Dictionary<string, List<JsonProperty>>();
        //static readonly object Padlock = new object();


        public static List<JsonProperty> GetJsonProperties<T>(this T type, bool includePrivates) where T : class
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

                if (JsonPropertyCache.TryGetValue(t.FullName + includePrivates, out var cache))
                {
                    return cache;
                }
                else
                {
                    var props = FetchJsonProperties(t, includePrivates);
                    JsonPropertyCache.TryAdd(t.FullName + includePrivates, props);
                    return props;
                }
            //}
        }

        internal static Dictionary<string, JsonProperty> GetJsonPropertiesDictionary<T>(this T type, bool includePrivates) where T : class
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

            var jsonProps = GetJsonProperties(type, includePrivates);

            foreach(var p in jsonProps)
            {
                dictionary.Add(p.Name, p);
            }

            return dictionary;
        }

        internal static List<JsonProperty> FetchJsonProperties(Type type, bool includePrivates)
        {
            var jsonProps = new List<JsonProperty>();
            var typeProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var typeFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var member in typeProps)
            {
                if (member.GetMemberAttributes<JsonIgnore>().Any())
                {
                    continue;
                }

                if ((!includePrivates && !member.IsPublic()) || !member.IsReadable())
                {
                    continue;
                }

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

                jsonProps.Add(new JsonProperty() { Name = jsonName, Member = member, Path = jsonPath });
            }

            foreach (var member in typeFields)
            {
                // Additional Checks for compiler generated fields. Compiler generated fields are linked to auto get/setter properties.
                if (member.GetMemberAttributes<JsonIgnore>().Any() || member.GetMemberAttributes<System.Runtime.CompilerServices.CompilerGeneratedAttribute>().Any())
                {
                    continue;
                }

                if ((!includePrivates && member.IsPrivate) || !member.IsReadable() || member.IsStatic)
                {
                    continue;
                }

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

                jsonProps.Add(new JsonProperty() { Name = jsonName, Member = member, Path = jsonPath });
            }

            return jsonProps;
        }
    }
}
