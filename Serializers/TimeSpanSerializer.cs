using System;
using System.Xml;
using reblGreen.Serialization.Attributes;

namespace reblGreen.Serialization.Serializers
{
    [KnownObject(typeof(TimeSpan))]
    public class TimeSpanSerializer : IStringSerializer
    {
        public virtual object FromString(string obj)
        {
            try
            {
                // ISO 8601 Duration (timespan) starts with P (period) and formatted as P[n]Y[n]M[n]DT[n]H[n]M[n]S (where n is numeric).
                // See: https://en.wikipedia.org/wiki/ISO_8601 -> Durations
                if (obj.StartsWith("p", StringComparison.OrdinalIgnoreCase))
                {
                    return XmlConvert.ToTimeSpan(obj);
                }

                // If not ISO 8601, try parsing .NET standard timespan.
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
                return XmlConvert.ToString(ts);
            }

            // We don't want to write .NET timespan format
            // return obj.ToString();

            return null;
        }
    }
}
