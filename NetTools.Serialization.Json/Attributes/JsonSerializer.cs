using System;
using System.Collections.Generic;
using System.Text;

namespace NetTools.Serialization.Attributes
{
    /// <summary>
    /// This attribute is used to tell the serializer what object type should be used to deserialize into.
    /// This is usefull for deserializing into interface types where an instance of interface cannot be created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class JsonSerializer : Attribute, IStringSerializer
    {
        IStringSerializer Serializer;


        public JsonSerializer(Type serializer)
        {
            if (serializer == null || serializer == this.GetType())
            {
                throw new ArgumentException("serializer");
            }

            if (!typeof(IStringSerializer).IsAssignableFrom(serializer))
            {
                throw new Exception("The type of argument 'serializer' must implement interface NetTools.Serialization.IStringSerializer");
            }

            Serializer = (IStringSerializer)Activator.CreateInstance(serializer);

            if (Serializer == null)
            {
                throw new Exception("The type of argument 'serializer' must implement interface NetTools.Serialization.IStringSerializer and contain an empty constructor.");
            }
        }

        public object FromString(string obj, Type t)
        {
            return Serializer.FromString(obj, t);
        }

        public string ToString(object obj)
        {
            return Serializer.ToString(obj);
        }
    }
}
