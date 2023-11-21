using Modules.Google.SearchConsole.Events.Classes;
using NetTools.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modules.Google.SearchConsole.Classes
{
    public class RootObject
    {
        public List<Row> Rows { get; set; }
        
        public string Error { get; set; }
    } 

}
