using System;
using System.Net;
using NetTools.Serialization.Attributes;

namespace NetTools.Serialization.Serializers
{
    /// <summary>
    /// This internal custom serializer implements IStringSerializer interface and will serialize or deserialize a System.Net.IPAddress object.
    /// </summary>
    [KnownObject(typeof(IPAddress))]
    public class IpAddressSerializer : IStringSerializer
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual object FromString(string obj, Type t)
        {
            return IPAddress.Parse(obj);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual string ToString(object obj)
        {
            // We must wrap the output string in quotes for the end JSON to be valid.
            return obj.ToString().AddDoubleQuotes();
        }
    }
}
