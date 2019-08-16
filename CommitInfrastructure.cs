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
    public class CommitInfrastructureRequest
    {
        [DataMember]
        public virtual string Template { get; set; }
    }

    public static class CommitInfrastructure
    {
        [FunctionName("CommitInfrastructure")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
			return await req.Manage<CommitInfrastructureRequest, NapkinIDESetupState, NapkinIDESetupStateHarness>(log, async (mgr, reqData) =>
            {
                return await mgr.CommitInfrastructure(reqData.Template);
            });
        }
    }
}
