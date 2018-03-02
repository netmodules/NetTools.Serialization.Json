using System;
using System.Linq;
using System.Collections.Generic;
using reblGreen.Serialization.Attributes;

namespace reblGreen.Serialization
{
    public static class JsonUtils
    {
        static Dictionary<Type, List<JsonProperty>> JsonPropertyCache = new Dictionary<Type, List<JsonProperty>>();
        static object Padlock = new object();

        public static List<JsonProperty> GetJsonProperties<T>(this T @type) where T : class
        {
            lock (Padlock)
            {
                var t = typeof(T);
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

        static List<JsonProperty> FetchJsonProperties(Type @type)
        {
            var jProps = new List<JsonProperty>();
            var props = @type.GetProperties();
            var fields = @type.GetFields();

            foreach (var m in props)
            {
                if (m.GetMemberAttributes<JsonIgnore>().Any())
                {
                    continue;
                }

                if (!m.IsPublic() || !m.IsReadable() || !m.IsWritable())
                {
                    continue;
                }

                var jName = m.GetMemberAttributes<JsonName>().FirstOrDefault()?.GetName();


                if (string.IsNullOrEmpty(jName))
                {
                    jName = m.Name;
                }

                jProps.Add(new JsonProperty() { Name = jName, Member = m });
            }

            foreach (var m in fields)
            {
                // Additional Checks for compiler generated fields. Compiler generated fields are linked to auto get/setter properties.
                if (m.GetMemberAttributes<JsonIgnore>().Any() || m.GetMemberAttributes<System.Runtime.CompilerServices.CompilerGeneratedAttribute>().Any())
                {
                    continue;
                }

                if (!m.IsPublic() || !m.IsReadable() || !m.IsWritable())
                {
                    continue;
                }

                var jName = m.GetMemberAttributes<JsonName>().FirstOrDefault()?.GetName();


                if (string.IsNullOrEmpty(jName))
                {
                    jName = m.Name;
                }

                jProps.Add(new JsonProperty() { Name = jName, Member = m });
            }

            return jProps;
        }
    }
}
