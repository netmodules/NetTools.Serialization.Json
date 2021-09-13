using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace reblGreen.Serialization
{
    internal static class ReflectionUtils
    {
        static Dictionary<Type, List<FieldInfo>> FieldCache = new Dictionary<Type, List<FieldInfo>>();
        static Dictionary<Type, List<PropertyInfo>> PropertyCache = new Dictionary<Type, List<PropertyInfo>>();
        static readonly object Padlock = new object();


        // Workaround for CoreCLR where FormatterServices.GetUninitializedObject is not public (but might change in RTM so we could remove this then).
        //private static readonly Func<Type, object> GetUninitializedObject =
        //    (Func<Type, object>)
        //        typeof(string)
        //            .GetTypeInfo()
        //            .Assembly
        //            .GetType("System.Runtime.Serialization.FormatterServices")
        //            .GetRuntimeMethod("GetUninitializedObject", new Type[] { typeof(object) }) //, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
        //            .CreateDelegate(typeof(Func<Type, object>));

        static readonly MethodInfo GetUninitializedObject =
                typeof(string)
                    .GetTypeInfo()
                    .Assembly
                    .GetType("System.Runtime.Serialization.FormatterServices")
                    .GetRuntimeMethod("GetUninitializedObject", new Type[] { typeof(object) });


        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <returns>The attributes.</returns>
        /// <param name="member">Member.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        /// <typeparam name="M">The 2nd type parameter.</typeparam>
        internal static List<T> GetMemberAttributes<T>(this MemberInfo @member) where T : Attribute
        {
            var attributes = (IEnumerable<T>)@member.GetCustomAttributes(typeof(T), true);

            if (!attributes.Any())
            {
                return new List<T>();
            }

            //return attributes.Cast<T>().ToList();
            return attributes.ToList();
        }


        /// <summary>
        /// 
        /// </summary>
        internal static List<FieldInfo> GetFields(this Type @type)
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


        /// <summary>
        /// 
        /// </summary>
        internal static List<PropertyInfo> GetProperties(this Type @type)
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


        /// <summary>
        /// 
        /// </summary>
        internal static bool IsReadable(this MemberInfo @member)
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


        /// <summary>
        /// 
        /// </summary>
        internal static bool IsWritable(this MemberInfo @member)
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


        /// <summary>
        /// 
        /// </summary>
        internal static bool IsPublic(this MemberInfo @member)
        {
            if (@member is PropertyInfo p)
            {
                if ((p.GetMethod != null && p.GetMethod.IsPublic) || (p.SetMethod != null && p.SetMethod.IsPublic))
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


        /// <summary>
        /// 
        /// </summary>
        internal static bool Set(this MemberInfo @member, object obj, object value)
        {
            if (@member.IsWritable())
            {
                // Calling SetValue(obj, null) on a value type (non-nullable) is the equivalent of default(type) so we don't need to worry
                // about non-nullables as this is handled for us by the CLR.

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


        /// <summary>
        /// 
        /// </summary>
        internal static object Get(this MemberInfo @member, object obj)
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


        /// <summary>
        /// 
        /// </summary>
        internal static bool Set<T>(this T obj, MemberInfo member, object value) where T : class
        {
            return Set(member, obj, value);
        }


        /// <summary>
        /// 
        /// </summary>
        internal static object Get<T>(this T obj, MemberInfo member) where T : class
        {
            return Get(member, obj);
        }


        /// <summary>
        /// 
        /// </summary>
        internal static object GetInstanceOf(Type type)
        {
            try
            {
                if (type == typeof(string))
                {
                    return Expression.Lambda<Func<object>>(Expression.Constant(string.Empty)).Compile().Invoke();
                }

                if (type.HasDefaultConstructor())
                {
                    return Expression.Lambda<Func<object>>(Expression.New(type)).Compile().Invoke();
                }

                return FormatterServices.GetUninitializedObject(type);

                //if (type != typeof(string) && GetUninitializedObject != null)
                //{
                //    return GetUninitializedObject.Invoke(null, new object[] { type });
                //}
            }
            catch { }

            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }

        public static class New<T>
        {
            public static readonly Func<T> Instance = Creator();

            static Func<T> Creator()
            {
                Type t = typeof(T);
                return () => (T)GetInstanceOf(t);
            }
        }

        public static bool HasDefaultConstructor(this Type t)
        {
            return t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null;
        }

        /// <summary>
        /// Looks for the method in the type matching the name and arguments.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName">
        /// The name of the method to find.
        /// </param>
        /// <param name="args">
        /// The types of the method's arguments to match.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if:
        ///     - The name of the method is not specified.
        /// </exception>
        internal static MethodInfo GetRuntimeMethod(this Type type, string methodName, Type[] args)
        {
            if (ReferenceEquals(type, null))
                throw new NullReferenceException("The type has not been specified.");

            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentNullException("methodName", "The name of the method has not been specified.");


            var methods = type.GetRuntimeMethods().Where(methodInfo => string.Equals(methodInfo.Name, methodName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!methods.Any())
                return null;    //  No methods have the specified name.

            if (methods.Count == 1)
            {
                MethodInfo methodInfo = methods.Single();
                return IsSignatureMatch(methodInfo, args) ? methodInfo : null;
            }

            //  Oh no, don't make me go there.
            throw new NotImplementedException("Resolving overloaded methods is not implemented as of now.");
        }


        /// <summary>
        /// Finds out if the provided arguments matches the specified method's signature.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool IsSignatureMatch(MethodBase methodInfo, Type[] args)
        {
            //  Gets the parameters of the method to analyze.
            ParameterInfo[] parameters = methodInfo.GetParameters();

            int currentArgId = 0;

            foreach (ParameterInfo parameterInfo in parameters)
            {
                if (!ReferenceEquals(args, null) && currentArgId < args.Length)
                {
                    //  Find out if the types matchs.
                    if (parameterInfo.ParameterType == args[currentArgId])
                    {
                        currentArgId++;
                        continue; //  Yeah! Try the next one.
                    }

                    //  Is this a generic parameter?
                    if (parameterInfo.ParameterType.IsGenericParameter)
                    {
                        //  Gets the base type of the generic parameter.
                        Type baseType = parameterInfo.ParameterType.GetTypeInfo().BaseType;


                        //  TODO: This is not good v and works with the most simple situation.
                        //  Does the base type match?  
                        if (args[currentArgId].GetTypeInfo().BaseType == baseType)
                        {
                            currentArgId++;
                            continue; //  Yeah! Go on to the next parameter.
                        }
                    }
                }

                //  Is this parameter optional or does it have a default value?
                if (parameterInfo.IsOptional || parameterInfo.HasDefaultValue)
                    continue; // Uhum. So let's ignore this parameter for now.

                //  No need to go further. It does not match :(
                return false;
            }

            //  Ye!
            return true;
        }


        /// <summary>
        /// 
        /// </summary>
        internal static ConstructorInfo GetConstructor(this Type type, Type[] args)
        {
            var info = type.GetTypeInfoCached();
            return info.DeclaredConstructors
                .Single(constructor => constructor.GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .SequenceEqual(args));
        }
    }
}
