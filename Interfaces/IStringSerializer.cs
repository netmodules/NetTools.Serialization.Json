using System;
namespace reblGreen.Serialization
{
    public interface IStringSerializer
    {
        object FromString(string obj);
        string ToString(object obj);
    }
}
