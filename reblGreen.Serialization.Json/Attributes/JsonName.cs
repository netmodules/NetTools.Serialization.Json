using System;
namespace reblGreen.Serialization.Attributes
{
    /// <summary>
    /// Add this attribute to a property or field to tell the serializer to serialize or deserialize the member using this name.
    /// This enables you to use casing conventions including cammelCase, dash-case and underscore_case.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class JsonName : Attribute
    {
        readonly string Value;

        public JsonName(string name)
        {
            Value = name;
        }

        #region Overrides.

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is JsonName name)
            {
                return Value == name.Value;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator string(JsonName s)
        {
            return s.Value;
        }

        public static implicit operator JsonName(string s)
        {
            return new JsonName(s);
        }

        public static bool operator ==(JsonName x, JsonName y)
        {
            return x.Value == y.Value;
        }

        public static bool operator !=(JsonName x, JsonName y)
        {
            return x.Value != y.Value;
        }

        #endregion
    }
}
