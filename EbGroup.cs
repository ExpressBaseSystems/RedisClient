using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EbRedisClient
{
    public class EbGroup
    {
        public string Name { get; set; }
        public string Pattern { get; set; }

        public EbGroup(string textgroup, string textpattern)
        {
            Name = textgroup;
            Pattern = textpattern;
        }
    }

    public class FindValClass
    {
        public string Key { set; get; }
        public object Obj { set; get; }
        public string Type { set; get; }
    }
}
