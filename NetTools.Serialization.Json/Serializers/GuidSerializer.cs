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
    [KnownObject(typeof(Guid))]
    public class GuidSerializer : IStringSerializer
    {
        public virtual object FromString(string obj, Type t)
        {
            try
            {
                return Guid.Parse(obj);
            }
            catch
            {
                return null;
            }
        }

        public virtual string ToString(object obj)
        {
            if (obj is Guid g)
            {
                return g.ToString().AddDoubleQuotes();
            }

            // Technically we should never get to here as only obj with a typeof(TimeSpan) should be passed to this method by StringSerializerFactory.
            return null;
        }
    }
}
