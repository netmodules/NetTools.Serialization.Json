using System;
namespace reblGreen.Serialization
{
    public interface IJsonSerializer
    {
        object Deserialize(object obj);
        object Serialize(object obj);
    }
}
