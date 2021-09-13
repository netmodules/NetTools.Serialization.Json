using System;
using System.Collections.Generic;
using System.Text;

namespace reblGreen.Serialization.TestApplication
{
    public class TestNoDefaultConstructor
    {
        public string S { get; private set; }
        public bool B { get; private set; }
        public int I { get; private set; }


        public TestNoDefaultConstructor(string s, bool b, int i)
        {
            Console.WriteLine("Non-default constructor invoked.");
            S = s;
            B = b;
            I = i;
        }
    }
}
