//using NetTools.NetCore.Modules.Interfaces;
using NetTools.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTools.Serialization.TestApplication
{
    /// <summary>
    /// This class acts as a dummy event so we can deserialize an incomming event request for the event name only. This is an optimisation as
    /// initial deserialization requires only the event name so we can perform the second deserialization of the complete event once we can
    /// identify the concrete object type
    /// </summary>
    [Serializable]
    internal class DummyEvent //: IEvent
    {
        [JsonName("name")]
        public EventName Name { get; set; }

        [JsonName("meta"), JsonIgnore]
        public Dictionary<string, object> Meta { get; set; }

        [JsonIgnore]
        public bool Handled { get; }

        public Dictionary<string, object> Input { get; set; }

        [JsonPath("partialArray[0].test")]
        public string NestedZero { get; set; }

        [JsonPath("partialArray[0].testDate")]
        public DateTime TestDate { get; set; }
    }
}
