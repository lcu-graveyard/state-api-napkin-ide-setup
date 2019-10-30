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
	public class SetHostFlowRequest
	{
		[DataMember]
		public virtual HostFlowTypes HostFlow { get; set; }
	}

    public static class SetHostFlow
    {
        [FunctionName("SetHostFlow")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
			return await req.Manage<SetHostFlowRequest, NapkinIDESetupState, NapkinIDESetupStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Setting Host Flow: {reqData.HostFlow}");

                return await mgr.SetHostFlow(reqData.HostFlow);
            });
        }
    }
}
