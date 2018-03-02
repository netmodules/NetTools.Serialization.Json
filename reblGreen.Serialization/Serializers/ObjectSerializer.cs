using System;
namespace reblGreen.Serialization.Serializers
{
    [KnownObject(typeof(string))]
    public class StringSerializer : IJsonSerializer
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
