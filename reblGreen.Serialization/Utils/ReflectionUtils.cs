using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace reblGreen.Serialization
{
    public static class ReflectionUtils
    {
        static Dictionary<Type, List<FieldInfo>> FieldCache = new Dictionary<Type, List<FieldInfo>>();
        static Dictionary<Type, List<PropertyInfo>> PropertyCache = new Dictionary<Type, List<PropertyInfo>>();
        static object Padlock = new object();


        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <returns>The attributes.</returns>
        /// <param name="member">Member.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        /// <typeparam name="M">The 2nd type parameter.</typeparam>
        public static List<T> GetMemberAttributes<T>(this MemberInfo @member) where T : Attribute
        {
            var attributes = (IEnumerable<T>)@member.GetCustomAttributes(typeof(T), true);

            if (!attributes.Any())
            {
                return new List<T>();
            }

            //return attributes.Cast<T>().ToList();
            return attributes.ToList();
        }

        public static List<FieldInfo> GetFields(this Type @type)
        {
            lock (Padlock)
            {
                if (FieldCache.ContainsKey(@type))
                {
                    return FieldCache[@type];
                }
                else
                {
                    var members = @type.GetRuntimeFields().ToList();
                    FieldCache.Add(@type, members);
                    return members;
                }
            }
        }

        public static List<PropertyInfo> GetProperties(this Type @type)
        {
            lock (Padlock)
            {
                if (PropertyCache.ContainsKey(@type))
                {
                    return PropertyCache[@type];
                }
                else
                {
                    var members = @type.GetRuntimeProperties().ToList();
                    PropertyCache.Add(@type, members);
                    return members;
                }
            }
        }

        public static bool IsReadable(this MemberInfo @member)
        {
            if (@member is PropertyInfo p && p.CanRead)
            {
                return true;
            }

            if (@member is FieldInfo)
            {
                return true;
            }

            return false;
        }

        public static bool IsWritable(this MemberInfo @member)
        {
            if (@member is PropertyInfo p && p.CanWrite)
            {
                return true;
            }

            if (@member is FieldInfo f && !f.IsLiteral && !f.IsInitOnly)
            {
                return true;
            }

            return false;
        }

        public static bool IsPublic(this MemberInfo @member)
        {
            if (@member is PropertyInfo p)
            {
                if ((bool)p.GetMethod?.IsPublic || (bool)p.SetMethod?.IsPublic)
                {
                    return true;
                }
            }

            if (@member is FieldInfo f && !f.IsPublic)
            {
                return true;
            }

            return false;
        }

        public static bool Set(this MemberInfo @member, object obj, object value)
        {
            if (@member.IsWritable())
            {
                if (@member is PropertyInfo p)
                {
                    p.SetValue(obj, value);
                    return true;
                }

                if (@member is FieldInfo f)
                {
                    f.SetValue(obj, value);
                    return true;
                }
            }

            return false;
        }

        public static object Get(this MemberInfo @member, object obj)
        {
            if (@member.IsReadable())
            {
                if (@member is PropertyInfo p)
                {
                    return p.GetValue(obj);

                }

                if (@member is FieldInfo f)
                {
                    return f.GetValue(obj);
                }
            }

            return null;
        }

        public static bool Set<T>(this T obj, MemberInfo member, object value) where T : class
        {
            return Set(member, obj, value);
        }

        public static object Get<T>(this T obj, MemberInfo member) where T : class
        {
            return Get(member, obj);
        }
    }
}
