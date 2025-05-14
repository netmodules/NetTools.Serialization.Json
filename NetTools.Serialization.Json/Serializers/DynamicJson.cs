using System;
using System.Globalization;
using System.Text;
using NetTools.Serialization.Attributes;
using NetTools.Serialization.Objects;

namespace NetTools.Serialization.Serializers
{
    /// <summary>
    /// This internal custom serializer implements IStringSerializer interface and will serialize or deserialize a NetTools.Serialization.Objects.DynamicJson object.
    /// </summary>
    [KnownObject(typeof(DynamicJson))]
    public class DynamicJsonSerializer : IStringSerializer
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual string ToString(object obj)
        {
            if (obj is DynamicJson dj)
            {
                return dj.ToString();
            }

            // Technically we should never get to here as only obj with a typeof(DynamicJson) should be passed to this method by StringSerializerFactory.
            return null;
        }
    }
}
