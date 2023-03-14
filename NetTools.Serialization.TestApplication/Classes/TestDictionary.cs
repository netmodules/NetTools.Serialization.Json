using System;
using System.Collections.Generic;
using System.Text;

namespace NetTools.Serialization.TestApplication
{
    class TestDictionary : Dictionary<string, string>, TestInterface
    {
        bool IsDictionary()
        {
            return typeof(IDictionary<,>).IsAssignableFrom(this.GetType());
        }
    }
}
