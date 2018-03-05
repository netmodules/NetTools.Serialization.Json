using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using reblGreen.Serialization.Attributes;

namespace reblGreen.Serialization
{
    class JsonProperty
    {
        public string Name;
        //public bool Ignore;
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

                if (knownType != null)
                {
                    MemberType = knownType;
                }
                else if (Member is PropertyInfo p)
                {
                    MemberType = p.PropertyType;
                }
                else if (Member is FieldInfo f)
                {
                    MemberType = f.FieldType;
                }
            }

            return MemberType;
        }
    }
}