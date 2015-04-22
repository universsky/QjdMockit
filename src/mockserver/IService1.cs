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
    // NOTE: If you change the interface name "IService1" here, you must also update the reference to "IService1" in Web.config.

    [ServiceContract]
    public interface IService1
    {   //test http://.../Service1.svc/GetData?q=aaa
        [WebInvoke(Method="*",UriTemplate = "*")]
        [OperationContract]
        string GetData();
       
    }
}