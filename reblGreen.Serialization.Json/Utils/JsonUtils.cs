using System;
using System.Linq;
using System.Collections.Generic;
using reblGreen.Serialization.Attributes;
using System.Text;

namespace reblGreen.Serialization
{
    internal static class JsonUtils
    {
        static readonly Dictionary<Type, List<JsonProperty>> JsonPropertyCache = new Dictionary<Type, List<JsonProperty>>();
        static readonly object Padlock = new object();


        public static List<JsonProperty> GetJsonProperties<T>(this T type) where T : class
        {
            lock (Padlock)
            {
                Type t;
                Console.WriteLine(typeof(T).ToString());
                //if (type != null)
                //{
                    t = type.GetType();
                //}
                //else
                //{
                //    t = typeof(T);
                //}

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

                var jsonName = member.GetMemberAttributes<JsonName>().FirstOrDefault()?.ToString();


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

                jsonProps.Add(new JsonProperty() { Name = jsonName, Member = member });
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
