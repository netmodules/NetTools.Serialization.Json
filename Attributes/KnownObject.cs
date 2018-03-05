using System;
using System.Reflection;

namespace reblGreen.Serialization.Attributes
{
    /// <summary>
    /// This attribute is used to tell the serializer what object type should be used to deserialize into.
    /// This is usefull for deserializing into interface types where an instance of interface cannot be created.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    public class KnownObject : Attribute
    {
        /// <summary>
        /// Returns the Type of the KnownObject
        /// </summary>
        public readonly Type KnownType; // { get; private set; }

        /// <summary>
        /// Returns the TypeInfo of the KnownObject
        /// </summary>
        public readonly TypeInfo KnownTypeInfo; // { get; private set; }

        /// <summary>
        /// This attribute is used to tell the serializer what object type should be used to deserialize into.
        /// This is usefull for deserializing into interface types where an instance of interface cannot be created.
        /// </summary>
        public KnownObject(Type type)
        {
            KnownType = type;
            KnownTypeInfo = type.GetTypeInfo();
        }


        //public bool CanCastTo(Type type)
        //{
        //    // If the types are identical no casting is needed which means casting is possible ;)
        //    if (type == KnownType)
        //    {
        //        return true;
        //    }

        //    // If KnownType is a direct subclass the KnownType can downcast and no further checks are required.
        //    if (KnownTypeInfo.IsSubclassOf(type))
        //    {
        //        return true;
        //    }

        //    var typeInfo = type.GetTypeInfo();
        //    var canDowncast = KnownTypeInfo;

        //    if (typeInfo.IsGenericType && canDowncast.IsGenericType)
        //    {
        //        var obj = typeof(object).GetTypeInfo();

        //        while (canDowncast != null && canDowncast != obj)
        //        {
        //            var cur = canDowncast.IsGenericType ? canDowncast.GetGenericTypeDefinition().GetTypeInfo() : canDowncast;

        //            if (typeInfo == cur)
        //            {
        //                return true;
        //            }

        //            canDowncast = canDowncast.BaseType != null ? canDowncast.BaseType.GetTypeInfo() : null;
        //        }

        //        return false;
        //    }
        //}
    }
}
