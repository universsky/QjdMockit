using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MockServer.Model
{
    class RuleKey
    {
        public string MapUri { get; set; }
        public string MapBodySubStr { get; set; }
        public string Verb { get; set; }
        public URLMapType MapType { get; set; }
     
        
       
    }
    public enum URLMapType{Exact,StartWith,Contains}
}
