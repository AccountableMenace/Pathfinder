using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pathfinder
{
    public class FileListStruct
    {
        public string name { get; set; }
        public string fullPath { get; set; }
        public override string ToString()
        {
            return string.Format(name);
        }
    }
    
}
