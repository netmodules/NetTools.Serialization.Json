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

            //var testClass = json.ToDictionary();

            var jProps = tc.GetJsonProperties();

            foreach (var p in jProps)
            {
                Console.WriteLine(p.GetMemberType());
            }

            Console.WriteLine(jProps.Count);
        }
    }
}
