using System;
namespace NetTools.Serialization.Attributes
{
    /// <summary>
    /// Add this attribute to a property or field to tell the serializer to fetch the value from a nested object or array.
    /// This allows you to bring a nested JSON property value into the top-level property of a .NET object.
    /// E.g. if you want to set your property or field value to the value of a nested JSON property called "key", and array
    /// item number 4 of "value" in "key", you would use [JsonPath("key.value[3]")]. If you would like to set your value to
    /// the entire array (list) use you would use [JsonPath("key.value")]. The property type must match or setting its value
    /// may fail.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class JsonPath : Attribute
    {
        readonly string Value;

        public JsonPath(string path)
        {
            Value = path;
        }

        #region Overrides.

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is JsonPath path)
            {
                return Value == path.Value;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator string(JsonPath s)
        {
            return s.Value;
        }

        public static implicit operator JsonPath(string s)
        {
            return new JsonPath(s);
        }

        public static bool operator ==(JsonPath x, JsonPath y)
        {
            return x.Value == y.Value;
        }

        public static bool operator !=(JsonPath x, JsonPath y)
        {
            return x.Value != y.Value;
        }

        #endregion
    }
}
