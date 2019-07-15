using System;
using System.Linq;
using System.Collections.Generic;
using reblGreen.Serialization.Attributes;

namespace reblGreen.Serialization
{
    internal static class JsonUtils
    {
        static Dictionary<Type, List<JsonProperty>> JsonPropertyCache = new Dictionary<Type, List<JsonProperty>>();
        static readonly object Padlock = new object();


        public static List<JsonProperty> GetJsonProperties<T>(this T type) where T : class
        {
            lock (Padlock)
            {
                var t = type.GetType(); // typeof(T);
                if (JsonPropertyCache.ContainsKey(t))
                {
                    return JsonPropertyCache[t];
                }
                else
                {
                    var props = FetchJsonProperties(t);
                    JsonPropertyCache.Add(t, props);
                    return props;
                }
            }
        }


        internal static Dictionary<string, JsonProperty> GetJsonPropertiesDictionary<T>(this T type) where T : class
        {
            var dictionary = new Dictionary<string, JsonProperty>();
            var jsonProps = GetJsonProperties(type);

            foreach(var p in jsonProps)
            {
                dictionary.Add(p.Name, p);
            }

            return dictionary;
        }


        internal static List<JsonProperty> FetchJsonProperties(Type type)
        {
            var jsonProps = new List<JsonProperty>();
            var typeProps = type.GetProperties();
            var typeFields = type.GetFields();

            foreach (var member in typeProps)
            {
                if (member.GetMemberAttributes<JsonIgnore>().Any())
                {
                    continue;
                }

                if (!member.IsPublic() || !member.IsReadable() || !member.IsWritable())
                {
                    continue;
                }

                var jName = member.GetMemberAttributes<JsonName>().FirstOrDefault()?.ToString();


                if (string.IsNullOrEmpty(jName))
                {
                    jName = member.Name;
                }

                jsonProps.Add(new JsonProperty() { Name = jName, Member = member });
            }

            foreach (var member in typeFields)
            {
                // Additional Checks for compiler generated fields. Compiler generated fields are linked to auto get/setter properties.
                if (member.GetMemberAttributes<JsonIgnore>().Any() || member.GetMemberAttributes<System.Runtime.CompilerServices.CompilerGeneratedAttribute>().Any())
                {
                    continue;
                }

                if (!member.IsPublic() || !member.IsReadable() || !member.IsWritable() || member.IsStatic)
                {
                    continue;
                }

                var jsonName = member.GetMemberAttributes<JsonName>().FirstOrDefault()?.ToString();


                if (string.IsNullOrEmpty(jsonName))
                {
                    jsonName = member.Name;
                }

                jsonProps.Add(new JsonProperty() { Name = jsonName, Member = member });
            }

            return jsonProps;
        }
    }
}
