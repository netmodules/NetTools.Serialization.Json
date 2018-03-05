using System;
using System.Globalization;
using System.Text;
using System.Xml;

namespace reblGreen.Serialization.Serializers
{
    [KnownObject(typeof(DateTime))]
    public class DateTimeSerializer : IStringSerializer
    {
        public virtual object FromString(string obj)
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

        public virtual string ToString(object obj)
        {
            // datetime format standard : yyyy-MM-dd HH:mm:ss
            if (obj is DateTime dt)
            {
                var sb = new StringBuilder();
                dt = dt.ToUniversalTime();

                sb.Append(dt.Year.ToString("0000", NumberFormatInfo.InvariantInfo));
                sb.Append('-');
                sb.Append(dt.Month.ToString("00", NumberFormatInfo.InvariantInfo));
                sb.Append('-');
                sb.Append(dt.Day.ToString("00", NumberFormatInfo.InvariantInfo));
                sb.Append('T'); // strict ISO date compliance 
                sb.Append(dt.Hour.ToString("00", NumberFormatInfo.InvariantInfo));
                sb.Append(':');
                sb.Append(dt.Minute.ToString("00", NumberFormatInfo.InvariantInfo));
                sb.Append(':');
                sb.Append(dt.Second.ToString("00", NumberFormatInfo.InvariantInfo));
                sb.Append('.');
                sb.Append(dt.Millisecond.ToString("000", NumberFormatInfo.InvariantInfo));
                sb.Append('Z');

                return sb.ToString();
            }

            return null;
        }
    }
}
