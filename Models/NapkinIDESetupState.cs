using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Fathym;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LCU.State.API.NapkinIDE.Setup.Models
{
    [DataContract]
    public class NapkinIDESetupState
    {
        [DataMember]
        public virtual string DevOpsAppID { get; set; }

        [DataMember]
        public virtual string DevOpsClientSecret { get; set; }

        [DataMember]
        public virtual string DevOpsScopes { get; set; }

        [DataMember]
        public virtual bool EnterpriseBooted { get; set; }

        [DataMember]
        public virtual string EnvironmentLookup { get; set; }

        [DataMember]
        public virtual MetadataModel EnvSettings { get; set; }
        
		[DataMember]
        public virtual bool HasDevOpsOAuth { get; set; }

		[DataMember]
        public virtual string Host { get; set; }

		[DataMember]
		[JsonConverter(typeof(StringEnumConverter))]
		public virtual HostFlowTypes? HostFlow { get; set; }

		[DataMember]
		public virtual List<string> HostOptions { get; set; }

        [DataMember]
        public virtual bool Loading { get; set; }

        [DataMember]
        public virtual string NewEnterpriseAPIKey { get; set; }

        [DataMember]
        public virtual string OrganizationDescription { get; set; }

        [DataMember]
        public virtual string OrganizationLookup { get; set; }

        [DataMember]
        public virtual string OrganizationName { get; set; }

        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public virtual NapkinIDESetupStepTypes Step { get; set; }
    }

	[DataContract]
	public enum HostFlowTypes
	{
		[EnumMember]
		@private,

		[EnumMember]
		shared
	}

    [DataContract]
    public enum NapkinIDESetupStepTypes
    {
        [EnumMember]
        OrgDetails,

        [EnumMember]
        AzureSetup,

        [EnumMember]
        HostConfig,

        [EnumMember]
        Review,

        [EnumMember]
        Complete
    }

    [DataContract]
    public class AzureInfaSettings
    {
        [DataMember]
        public virtual string AzureTenantID { get; set; }
        
        [DataMember]
        public virtual string AzureSubID { get; set; }
        
        [DataMember]
        public virtual string AzureAppID { get; set; }
        
        [DataMember]
        public virtual string AzureAppAuthKey { get; set; }
    }
}