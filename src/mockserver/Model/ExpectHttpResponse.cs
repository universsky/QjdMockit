using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace MockServer.Model
{
   public class ExpectHttpResponse
    {
       public int StatusCode { get; set; }
       public WebHeaderCollection Header { get; set; }
       public byte[] Body { get; set; }
       public TimeSpan AdditionalDelay { get; set; }
     
    }
   public class ExpectHttpResponseMap
   {
      
       public TimeSpan AdditionalDelay { get; set; }
       public MapFileType Type { get; set; }
       public FileInfo[] FileUri { get; set; }
       private bool _enableConverter = false;
       public bool EnableConverter 
       {
           get { return _enableConverter; } 
           set { _enableConverter = value; }
       }
   }
   public enum MapFileType { One, Two,None }
}
