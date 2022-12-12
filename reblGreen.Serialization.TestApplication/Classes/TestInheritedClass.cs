using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace reblGreen.Serialization.TestApplication.Classes
{
    class TestInheritedClass : InheritedClass
    {
        public string NonInheritedProperty { get; set; }
    }




    [KnownType(typeof(TestInheritedClass))]
    class InheritedClass
    {
        public string InheritedProperty { get; set; }
    }
}
