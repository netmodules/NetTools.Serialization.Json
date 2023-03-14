using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using NetTools.Serialization.Attributes;
using System.Runtime.Serialization;

namespace NetTools.Serialization
{
    public class JsonProperty
    {
        public string Name;
        public MemberInfo Member;

        Type MemberType;

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="value">Value.</param>
        public void SetValue(object obj, object value)
        {
            obj.Set(Member, value);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="obj">Object.</param>
        public object GetValue(object obj)
        {
            return obj.Get(Member);
        }


        /// <summary>
        /// Gets the type of the member.
        /// </summary>
        /// <returns>The member type.</returns>
        /// <param name="obj">Object.</param>
        public Type GetMemberType(object obj = null)
        {
            if (obj != null)
            {
                var currentVal = GetValue(obj);

                if (currentVal != null)
                {
                    return currentVal.GetType();
                }
            }

            if (MemberType == null)
            {
                var knownType = Member.GetMemberAttributes<KnownObject>().FirstOrDefault()?.KnownType;

                if (knownType == null)
                {
                    // Attempt to grab the known object type from class-level...
                    knownType = Member.GetType().GetAttributes<KnownObject>().FirstOrDefault()?.KnownType;
                }

                if (knownType == null)
                {
                    // Attempt to use the System.Runtime.Serialization attribute...
                    knownType = Member.GetType().GetAttributes<KnownTypeAttribute>().FirstOrDefault()?.Type;
                }

                if (Member is PropertyInfo p)
                {
                    if (knownType != null && p.PropertyType.IsAssignableFrom(knownType))
                    {
                        MemberType = knownType;
                    }
                    else
                    {
                        MemberType = p.PropertyType;
                    }
                }
                else if (Member is FieldInfo f)
                {
                    if (knownType != null && f.FieldType.IsAssignableFrom(knownType))
                    {
                        MemberType = knownType;
                    }
                    else
                    {
                        MemberType = f.FieldType;
                    }
                }
            }

            return MemberType;
        }
    }
}