using System;
using System.Globalization;
using System.Text;
using System.Xml;
using NetTools.Serialization.Attributes;

namespace NetTools.Serialization.Serializers
{
    /// <summary>
    /// This internal custom serializer implements IStringSerializer interface and will serialize or deserialize a System.Guid object.
    /// </summary>
    [KnownObject(typeof(Uri))]
    public class UriSerializer : IStringSerializer
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual object FromString(string obj, Type t)
        {
            try
            {
                return new Uri(obj, UriKind.RelativeOrAbsolute);
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
            if (obj is Uri u)
            {
                return u.ToString().AddDoubleQuotes();
            }

            // Technically we should never get to here as only obj with a typeof(Uri) should be passed to this method by StringSerializerFactory.
            return null;
        }
    }
}
