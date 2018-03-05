using System;
using System.Collections.Generic;
using reblGreen.Serialization.Attributes;

namespace reblGreen.Serialization.TestConsole
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


        public static TestClass GetPopulatedTestClass()
        {
            return new TestClass()
            {
                E = TestEnum.One,
                G = Guid.NewGuid(),
                DT = DateTime.UtcNow,
                I = 35565,
                L = new List<string>() { "this", "is", "a", "string" },
                S = "this is a string",
                TS = TimeSpan.FromMilliseconds(987654321)
            };
        }
    }
}
