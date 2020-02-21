using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fathym.Business.Models;

namespace LCU.State.API.NapkinIDE.Setup.Models
{
    [DataContract]
    public class AccessRequestEmail
    {

        [DataMember]
        public virtual string User { get; set; }

        [DataMember]
        public virtual string EnterpriseID { get; set; }

        [DataMember]
        public virtual string Subject { get; set; }

        [DataMember]
        public virtual string Content { get; set; }

        [DataMember]
        public virtual string EmailTo { get; set; }

        [DataMember]
        public virtual string EmailFrom { get; set; }


    }
}
