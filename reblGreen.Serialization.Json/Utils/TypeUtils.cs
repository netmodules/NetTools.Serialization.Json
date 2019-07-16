using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace reblGreen.Serialization
{
    internal static class TypeUtils
    {
        static Dictionary<Type, TypeInfo> TypeInfoCache = new Dictionary<Type, TypeInfo>();
        static readonly object Padlock = new object();

        /// <summary>
        /// Gets the inheritance hierarchy.
        /// </summary>
        /// <returns>The inheritance hierarchy.</returns>
        /// <param name="type">Type.</param>
        internal static List<Type> GetInheritanceHierarchy(this Type @type)
        {
            var info = type.GetTypeInfoCached();
            return info.GetInheritanceHierarchy().Select(i => i.AsType()).ToList();
        }

        /// <summary>
        /// Gets the inheritance hierarchy.
        /// </summary>
        /// <returns>The inheritance hierarchy.</returns>
        /// <param name="type">Type.</param>
        internal static List<TypeInfo> GetInheritanceHierarchy(this TypeInfo @type)
        {
            var types = new List<TypeInfo>();

            for (var current = type; current != null; current = current.BaseType?.GetTypeInfoCached()) // != null ? current.BaseType.GetTypeInfo() : null)
            {
                types.Add(current);
            }

            return types;
        }


        /// <summary>
        /// Gets the type info.
        /// </summary>
        /// <returns>The type info.</returns>
        /// <param name="type">Type.</param>
        internal static TypeInfo GetTypeInfoCached(this Type @type)
        {
            lock(Padlock)
            {
                if (TypeInfoCache.ContainsKey(@type))
                {
                    return TypeInfoCache[@type];
                }
                else
                {
                    var info = @type.GetTypeInfo();
                    TypeInfoCache.Add(@type, info);
                    return info;
                }
            }
        }



        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <returns>The attributes.</returns>
        /// <param name="type">Type.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        internal static List<T> GetAttributes<T>(this Type @type) where T : Attribute
        {
            var info = type.GetTypeInfoCached();
            return info.GetAttributes<T>();
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <returns>The attributes.</returns>
        /// <param name="this">This.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        internal static List<T> GetAttributes<T>(this TypeInfo @type) where T : Attribute
        {
            var attributes = (IEnumerable<T>)@type.GetCustomAttributes(typeof(T), true);

            if (attributes.Count() == 0)
            {
                return new List<T>();
            }

            //return attributes.Cast<T>().ToList();
            return attributes.ToList();
        }
    }
}
