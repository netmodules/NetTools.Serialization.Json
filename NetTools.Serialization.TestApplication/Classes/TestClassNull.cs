using System;
using System.Collections.Generic;
using NetTools.Serialization.Attributes;

namespace NetTools.Serialization.TestApplication
{
    public class TestClassNull
    {
        public TestClass TestClass { get; set; } = new TestClass();
        public string NullString { get; set; } = null;
    }
}
