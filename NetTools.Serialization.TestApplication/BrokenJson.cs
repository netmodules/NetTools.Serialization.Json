using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTools.Serialization.TestApplication
{
    internal static class BrokenJson
    {
        internal static string Text = @"{
  ""rows"": [
    {
      ""keys"": [
        ""travel lodge london"",
        ""https://www.travelodge.co.uk/uk/london/index.html"",
        ""aus"",
        ""DESKTOP""
      ],
      ""clicks"": 62,
      ""impressions"": 184,
      ""ctr"": 0.33695652173913043,
      ""position"": 1.9891304347826086
    },
    {
      ""keys"": [
        ""travel lodge london doc";
    }
}
