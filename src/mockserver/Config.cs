using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
 

namespace MockServer
{
   public class Config
    {
     public  static bool ENABLE_CONVERT_SERVICE = bool.Parse(ConfigurationManager.AppSettings["EnableConvertService"]);
     public static Uri CONVERT_REQ_SERVICE_URL = new Uri(ConfigurationManager.AppSettings["ConvertReqServiceUrl"]);
     public static Uri CONVERT_RESP_SERVICE_URL = new Uri(ConfigurationManager.AppSettings["ConvertRespServiceUrl"]);
     public static ConvertMode CONVERT_MODE;
     static Config() 
     {
         switch (ConfigurationManager.AppSettings["ConvertMode"].ToString().ToLower()) 
         { 
             case "both":
             {
                 CONVERT_MODE = ConvertMode.Both;
                 break;
             }
             case "reqonly": 
             {
                 CONVERT_MODE = ConvertMode.ReqOnly;
                 break;
             }
             case "responly":
             {
                 CONVERT_MODE = ConvertMode.RespOnly;
                 break;
             }
                 
         }
     }
    }
    public enum ConvertMode{None,Both,ReqOnly,RespOnly}
}
