using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using LCU.State.API.NapkinIDE.Setup.Models;
using LCU.State.API.NapkinIDE.Setup.Services;

namespace LCU.State.API.NapkinIDE.Setup
{
	[Serializable]
	[DataContract]
	public class SetOrganizationDetailsRequest
	{
		[DataMember]
		public virtual string Description { get; set; }

		[DataMember]
		public virtual string Name { get; set; }

		[DataMember]
		public virtual string Lookup { get; set; }
	}

    public static class SetOrganizationDetails
    {
        [FunctionName("SetOrganizationDetails")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
			return await req.Manage<SetOrganizationDetailsRequest, NapkinIDESetupState, NapkinIDESetupStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Setting Organization Details: {reqData.Name} {reqData.Description} {reqData.Lookup}");

                return await mgr.SetOrganizationDetails(reqData.Name, reqData.Description, reqData.Lookup);
            });
        }
    }
}
