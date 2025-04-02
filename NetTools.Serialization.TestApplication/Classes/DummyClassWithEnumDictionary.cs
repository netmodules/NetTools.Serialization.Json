//using NetTools.NetCore.Modules.Interfaces;
using NetTools.Serialization.Attributes;
using NetTools.Serialization.TestApplication.Classes;
using NetTools.Serialization.TestApplication.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetTools.Serialization.TestApplication
{
    /// <summary>
    /// This class acts as a dummy event so we can deserialize an incomming event request for the event name only. This is an optimisation as
    /// initial deserialization requires only the event name so we can perform the second deserialization of the complete event once we can
    /// identify the concrete object type
    /// </summary>
    [Serializable]
    //[JsonSerializer(typeof(AttributeSerializer))]
    internal class DummyClassWithEnumDictionary : DummyClassWithNonSerialized
    {
        private class AttributeSerializer : IStringSerializer
        {
            public object FromString(string obj, Type t)
            {
                return Json.ToDictionary(obj).ToDictionary(kv => Enum.Parse<DictionaryKenum>(kv.Key, true), kv => kv.Value);
            }

            public string ToString(object obj)
            {
                return obj.ToString().AddDoubleQuotes();
            }
        }

        public bool NonSerialized { get; set; } = true;

        [JsonSerializer(typeof(AttributeSerializer))]
        public Dictionary<DictionaryKenum, object> Dic { get; set; }
    }
}
