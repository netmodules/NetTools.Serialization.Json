using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NetTools.Serialization.Attributes;
using NetTools.Serialization.TestApplication.Classes;

namespace NetTools.Serialization.TestApplication
{
    public class TestClass
    {
        public enum TestEnum
        {
            None,
            One,
            Two
        }

        [JsonName("anEnum")]
        public TestEnum E { get; set; }

        [JsonName("string")]
        public string S { get; set; }

        [JsonName("Timespan")]
        public TimeSpan TS { get; set; }

        public DateTime DT { get; set; }

        public Guid G { get; set; }

        public int I { get; set; }

        [JsonIgnore]
        public int IgnoredInt { get; set; }
        private int PrivateInt { get; set; }


        [KnownObject(typeof(List<string>))]
        public ICollection<string> L { get; set; }

        internal InheritedClass InheritedClass { get; set; }

        internal Dictionary<int, bool> IntBoolDictionary { get; set; }

        internal Dictionary<bool, int> BoolIntDictionary { get; set; }

        internal Dictionary<int, short> IntShortDictionary { get; set; }

        internal Dictionary<string, int> StringIntDictionary { get; set; }

        public static TestClass GetPopulatedTestClass()
        {
            return new TestClass()
            {
                E = TestEnum.One,
                G = Guid.NewGuid(),
                DT = DateTime.UtcNow,
                I = 35565,
                L = new List<string>() { "this", "is", "a", "£1 pound sign string \u00a31" },
                S = "this is a string",
                TS = TimeSpan.FromMilliseconds(987654321),
                InheritedClass = new TestInheritedClass { InheritedProperty = "This property is inherited", NonInheritedProperty = "This property is not inherited" },
                IntBoolDictionary = new Dictionary<int, bool>()
                {
                    { 1, true },
                    { 2, false },
                    { 3, true },
                },
                BoolIntDictionary = new Dictionary<bool, int>()
                {
                    { true, 1 },
                    { false, 0 }
                },
                IntShortDictionary = new Dictionary<int, short>()
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { short.MaxValue, short.MaxValue }
                },
                StringIntDictionary = new Dictionary<string, int>()
                {
                    { "1", 1 },
                    { "2", 2 },
                    { "3", 3 },
                }
            };
        }
    }
}
