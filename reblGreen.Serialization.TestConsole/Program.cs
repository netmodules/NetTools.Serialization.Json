using System;
using System.Collections.Generic;
using reblGreen.Serialization;

namespace reblGreen.Serialization.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var tc = TestClass.GetPopulatedTestClass();

            var json = tc.ToJson();

            var testDic = json.ToDictionary();

            TestClass tc2 = Json.FromJson<TestClass>(json);

            var tc3 = Json.FromDictionary<TestClass>(testDic);

            Console.WriteLine(json);
            Console.WriteLine(testDic.Count);
        }
    }
}
