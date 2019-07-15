using System;
using System.Globalization;
using System.Text;
using reblGreen.Serialization.Attributes;
using reblGreen.Serialization.Objects;

namespace reblGreen.Serialization.Serializers
{
    /// <summary>
    /// This internal custom serializer implements IStringSerializer interface and will serialize or deserialize a reblGreen.Serialization.Objects.DynamicJson object.
    /// </summary>
    [KnownObject(typeof(DynamicJson))]
    public class DynamicJsonSerializer : IStringSerializer
    {
        public virtual object FromString(string obj, Type t)
        {
            try
            {
                return new DynamicJson(obj);
            }
            catch
            {
                return null;
            }
        }

        public virtual string ToString(object obj)
        {
            if (obj is DynamicJson dj)
            {
                return dj.ToString();
            }

            // Technically we should never get to here as only obj with a typeof(TimeSpan) should be passed to this method by StringSerializerFactory.
            return null;
        }
    }
}
