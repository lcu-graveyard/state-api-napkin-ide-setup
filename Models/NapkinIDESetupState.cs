using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LCU.State.API.NapkinIDE.Setup.Models
{
    [DataContract]
    public class NapkinIDESetupState
    {
        [DataMember]
        public virtual bool Loading { get; set; }
    }
}