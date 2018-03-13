using System;
using System.Globalization;
using System.Text;
using reblGreen.Serialization.Attributes;
using reblGreen.Serialization.Objects;

namespace reblGreen.Serialization.Serializers
{
    [KnownObject(typeof(DynamicJson))]
    public class DynamicJsonSerializer : IStringSerializer
    {
        public virtual object FromString(string obj)
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

        public virtual string ToString(object obj)
        {
            // datetime format standard : yyyy-MM-dd HH:mm:ss
            if (obj is DynamicJson dj)
            {
                return dj.ToString();
            }

            return null;
        }
    }
}
