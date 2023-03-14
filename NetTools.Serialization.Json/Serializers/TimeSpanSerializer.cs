using System;
using System.Xml;
using NetTools.Serialization.Attributes;

namespace NetTools.Serialization.Serializers
{
    /// <summary>
    /// This internal custom serializer implements IStringSerializer interface and will serialize and deserialize System.TimeSpan
    /// to an ISO 8601 Duration. See: <see href="https://en.wikipedia.org/wiki/ISO_8601#Durations">Durations</see>.
    /// </summary>
    [KnownObject(typeof(TimeSpan))]
    public class TimeSpanSerializer : IStringSerializer
    {
        public virtual object FromString(string obj, Type t)
        {
            try
            {
                // ISO 8601 Duration (timespan) starts with P (period) and formatted as P[n]Y[n]M[n]DT[n]H[n]M[n]S (where n is numeric).
                // See: https://en.wikipedia.org/wiki/ISO_8601 -> Durations
                if (obj.StartsWith("p", StringComparison.OrdinalIgnoreCase))
                {
                    return XmlConvert.ToTimeSpan(obj);
                }

                // If not ISO 8601, try parsing using the .NET TimeSpan.Parse method.
                return TimeSpan.Parse(obj);
            }
            catch
            {
                return null;
            }
        }

        public virtual string ToString(object obj)
        {
            if (obj is TimeSpan ts)
            {
                // ISO 8601 Duration (timespan) starts with P (period) and formatted as P[n]Y[n]M[n]DT[n]H[n]M[n]S (where n is numeric).
                // See: https://en.wikipedia.org/wiki/ISO_8601 -> Durations
                return XmlConvert.ToString(ts).AddDoubleQuotes();
            }

            // Technically we should never get to here as only obj with a typeof(TimeSpan) should be passed to this method by StringSerializerFactory.
            return null;
        }
    }
}
