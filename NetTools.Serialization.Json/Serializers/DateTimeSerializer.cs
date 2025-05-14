using System;
using System.Globalization;
using System.Text;
using NetTools.Serialization.Attributes;

namespace NetTools.Serialization.Serializers
{
    /// <summary>
    /// This internal custom serializer implements IStringSerializer interface and will serialize or deserialize System.DateTime to and from an ISO 8601 date and time
    /// representation. When deserializing, other date formats may be supports.
    /// See: <see href="https://en.wikipedia.org/wiki/ISO_8601#Combined_date_and_time_representations">Combined date and time representations</see>.
    /// </summary>
    [KnownObject(typeof(DateTime))]
    public class DateTimeSerializer : IStringSerializer
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual object FromString(string obj, Type t)
        {
            try
            {
                return DateTime.Parse(obj);
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
            // ISO 8601 Combined date and time representation (2007-04-05T14:30Z).
            // See: https://en.wikipedia.org/wiki/ISO_8601 -> Durations
            // datetime format standard : yyyy-MM-dd HH:mm:ss
            if (obj is DateTime dt)
            {
                var sb = new StringBuilder();
                dt = dt.ToUniversalTime();

                // Year...
                sb.Append(dt.Year.ToString("0000", NumberFormatInfo.InvariantInfo));

                // Month...
                sb.Append('-');
                sb.Append(dt.Month.ToString("00", NumberFormatInfo.InvariantInfo));

                // Day...
                sb.Append('-');
                sb.Append(dt.Day.ToString("00", NumberFormatInfo.InvariantInfo));

                // Hour...
                sb.Append('T'); // strict ISO date compliance 
                sb.Append(dt.Hour.ToString("00", NumberFormatInfo.InvariantInfo));

                // Minutes...
                sb.Append(':');
                sb.Append(dt.Minute.ToString("00", NumberFormatInfo.InvariantInfo));

                // Seconds...
                sb.Append(':');
                sb.Append(dt.Second.ToString("00", NumberFormatInfo.InvariantInfo));

                // Milliseconds...
                sb.Append('.');
                sb.Append(dt.Millisecond.ToString("000", NumberFormatInfo.InvariantInfo));

                // UTC...
                sb.Append('Z');

                return sb.ToString().AddDoubleQuotes();
            }

            // Technically we should never get to here as only obj with a typeof(DateTime) should be passed to this method by StringSerializerFactory.
            return null;
        }
    }
}
