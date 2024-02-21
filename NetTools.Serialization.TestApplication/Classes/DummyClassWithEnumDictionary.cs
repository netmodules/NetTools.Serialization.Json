//using NetTools.NetCore.Modules.Interfaces;
using NetTools.Serialization.Attributes;
using NetTools.Serialization.TestApplication.Enums;
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
    internal class DummyClassWithEnumDictionary //: IEvent
    {
        public Dictionary<DictionaryKenum, object> Dic { get; set; }
    }
}
