using System;
namespace reblGreen.Serialization.Attributes
{
    /// <summary>
    /// Add this attribute to a property or field to tell the serializer to ignore this when serializing to JSON.
    /// By default, only public properties and fields are serialized but this attribute can be added to privates
    /// to ensure that these are not serialized under any configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class JsonName : Attribute
    {
        readonly string Name;

        public JsonName(string name)
        {
            Name = name;
        }

        public string GetName()
        {
            return Name;
        }
    }
}
