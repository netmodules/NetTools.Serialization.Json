using System;
using System.Collections.Generic;
using reblGreen.Serialization.Attributes;

namespace reblGreen.Serialization.TestApplication
{
    public class TestClassNull
    {
        public TestClass TestClass { get; set; } = new TestClass();
        public string NullString { get; set; } = null;
    }
}
