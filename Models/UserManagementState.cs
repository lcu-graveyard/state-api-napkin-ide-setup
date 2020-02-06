
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using LCU.Graphs.Registry.Enterprises;

namespace LCU.State.API.NapkinIDE.Setup.Models
{
    [DataContract]
    public class UserManagementState
    {
        [DataMember]
        public virtual List<Enterprise> Enterprises { get; set; }


    }
}