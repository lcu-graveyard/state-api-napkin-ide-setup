using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fathym.Business.Models;

namespace LCU.State.API.NapkinIDE.Setup.Models
{
    [DataContract]
    public class AccessCard : BusinessModel<Guid>
    {
        [DataMember]
        public virtual string AccessConfigurationType { get; set; }


        [DataMember]
        public virtual string EnterpriseAPIKey { get; set; }


        [DataMember]
        public virtual string Registry { get; set; }

    }
}