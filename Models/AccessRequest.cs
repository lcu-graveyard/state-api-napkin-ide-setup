using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fathym.Business.Models;

namespace LCU.State.API.NapkinIDE.Setup.Models
{
    [DataContract]
    public class AccessRequest
    {

        [DataMember]
        public virtual string User { get; set; }


        [DataMember]
        public virtual string EnterpriseID { get; set; }

 
    }
}
