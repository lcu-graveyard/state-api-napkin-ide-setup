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
    public class SetupDevOpsOAuthRequest
    {
        [DataMember]
        public virtual string AppID { get; set; }
        
        [DataMember]
        public virtual string ClientSecret { get; set; }
        
        [DataMember]
        public virtual string Scopes { get; set; }
    }

    public static class SetupDevOpsOAuth
    {
        [FunctionName("SetupDevOpsOAuth")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
			return await req.Manage<SetupDevOpsOAuthRequest, NapkinIDESetupState, NapkinIDESetupStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Setting DevOps OAuth: {reqData.AppID} {reqData.ClientSecret} {reqData.Scopes}");

                return await mgr.SetupDevOpsOAuth(reqData.AppID, reqData.ClientSecret, reqData.Scopes);
            });
        }
    }
}
