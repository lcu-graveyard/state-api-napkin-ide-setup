using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fathym;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LCU.State.API.NapkinIDE.Setup.Models
{
    [DataContract]
    public class UserManagementState
    {
        [DataMember]
        public virtual List<Enterprise> Enterprises { get; set; }


    }
}