using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace WCF_REST
{
    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in Web.config.

    public class Service1 : IService1
    {
        public string GetData()
        {
            return "Hello World";
        }

       
    }
}