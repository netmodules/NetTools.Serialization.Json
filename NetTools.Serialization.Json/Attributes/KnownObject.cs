using System;
using System.Reflection;

namespace NetTools.Serialization.Attributes
{
    /// <summary>
    /// This attribute is used to tell the serializer what object type should be used to deserialize into.
    /// This is usefull for deserializing into interface types where an instance of interface cannot be created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class KnownObject : Attribute
    {
        /// <summary>
        /// Returns the Type of the KnownObject
        /// </summary>
        public readonly Type KnownType;

        ///// <summary>
        ///// Returns the TypeInfo of the KnownObject
        ///// </summary>
        //public readonly TypeInfo KnownTypeInfo;

        /// <summary>
        /// This attribute is used to tell the serializer what object type should be used to deserialize into.
        /// This is usefull for deserializing into interface types where an instance of interface cannot be created.
        /// </summary>
        public KnownObject(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "You must specify a type for this KnownObjectAttribute.");
            }

            KnownType = type;
            //KnownTypeInfo = type.GetTypeInfo();
        }
    }
}
