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
    [KnownObject(typeof(byte[]))]
    public class ByteArraySerializer : IStringSerializer
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual object FromString(string obj, Type t)
        {
            try
            {
                var split = obj.Trim('[', ']').Split(',', StringSplitOptions.RemoveEmptyEntries);
                var arr = new byte[split.Length];

                for (var i = 0; i < split.Length; i++)
                {
                    arr[i] = byte.Parse(split[i].Trim());
                }

                return arr;
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
            if (obj is byte[] bytes)
            {
                var builder = new StringBuilder("[", bytes.Length);
                builder.Append(string.Join(",", bytes));
                builder.Append(']');
                return builder.ToString();
            }

            // Technically we should never get to here as only obj with a correct type should be passed to this method by StringSerializerFactory.
            return null;
        }
    }
}
