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

namespace LCU.State.API.NapkinIDE.Setup.Services
{
	[Serializable]
	[DataContract]
	public class SetOrganizationDetailsRequest
	{
		[DataMember]
		public virtual NapkinIDESetupStepTypes Step { get; set; }
	}
    
    public static class SetNapkinIDESetupStep
    {
        [FunctionName("SetNapkinIDESetupStep")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
			return await req.Manage<SetOrganizationDetailsRequest, NapkinIDESetupState, NapkinIDESetupStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Setting Napkin IDE Setup Step: {reqData.Step}");

                return await mgr.SetNapkinIDESetupStep(reqData.Step);
            });
        }
    }
}
