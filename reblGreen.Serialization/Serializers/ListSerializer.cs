using System;
using System.Collections.Generic;

namespace reblGreen.Serialization.Serializers
{
    [KnownObject(typeof(List<>))]
    public class ListSerializer : IJsonSerializer
    {
        public virtual object Deserialize(object obj)
        {
            if (obj is string)
            {
                return obj;
            }

            return obj.ToString();
        }

        public virtual object Serialize(object obj)
        {
            if (obj is string)
            {
                return obj;
            }

            return obj.ToString();
        }
    }
}
