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

            dynamic d = tc3.ToDynamic();

            d.Dynamic = new Dictionary<string, object>();
            d.Dynamic.SomeKey = "SomeValue";


            string dStr = d.ToString();

            var d2 = dStr.ToDynamic();

            var lType = d["L"].GetType();

            Console.WriteLine(d2["a_dynamic_value"]);
            Console.WriteLine(lType);
            Console.WriteLine(json);
            Console.WriteLine(dStr);
            Console.WriteLine(testDic.Count);
        }
    }
}
