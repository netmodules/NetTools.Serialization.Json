using System;
using System.Globalization;
using System.Text;
using System.Xml;

namespace reblGreen.Serialization.Serializers
{
    [KnownObject(typeof(Guid))]
    public class GuidSerializer : IStringSerializer
    {
        public virtual object FromString(string obj)
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
            // datetime format standard : yyyy-MM-dd HH:mm:ss
            if (obj is Guid g)
            {
                return g.ToString();
            }

            return null;
        }
    }
}
