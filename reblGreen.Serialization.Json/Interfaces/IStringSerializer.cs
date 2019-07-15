using System;
namespace reblGreen.Serialization
{
    /// <summary>
    /// This interface is used to register a custom de/serializer. Your class implementing IStringSerializer must
    /// also contain a KnownObjectAttribute, at class level, stating the Type/s the IStringSerializer can handle.
    /// If the return output of your serializer should be wrapped in double quotes you can use the extension method
    /// <see cref="Json.AddDoubleQuotes"/> in the return of your ToString() implementation.
    /// </summary>
    public interface IStringSerializer
    {
        /// <summary>
        /// This method is invoked by JsonReader before JsonReader attempts to deserialize the object using its own methods.
        /// If your custom de/serializer returns null the JsonReader will continue as normal, otherwise your custom
        /// deserialization is returned.
        /// </summary>
        object FromString(string obj, Type t);


        /// <summary>
        /// This method is invoked by JsonReader before JsonReader attempts to serialize the object using its own methods.
        /// If your custom de/serializer returns null or an empty string the JsonReader will continue as normal, otherwise
        /// your custom serialized string is returned. Considering that numeric objects are represented as unquoted strings
        /// within a JSON object, you must wrap your return string in quotes if quotes are required for valid JSON.
        /// <see cref="Json.AddDoubleQuotes"/> is a string extension method for wrapping a string in double quotes.
        /// </summary>
        string ToString(object obj);
    }
}
