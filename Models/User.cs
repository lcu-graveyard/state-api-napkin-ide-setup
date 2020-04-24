using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fathym.Business.Models;

namespace LCU.State.API.NapkinIDE.Setup.Models
{
    [DataContract]
    public class User : BusinessModel<Guid>
    {
        [DataMember]
        public virtual string Email { get; set; }

    }
}