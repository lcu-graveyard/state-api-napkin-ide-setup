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
using Newtonsoft.Json.Converters;

namespace LCU.State.API.NapkinIDE.Setup
{
    [Serializable]
    [DataContract]
    public class AcceptTermsRequest
    {
        [DataMember]
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public virtual DateTimeOffset AcceptedOn { get; set; }
        
        [DataMember]
        public virtual string Version { get; set; }
    }

    public static class AcceptTerms
    {
        [FunctionName("AcceptTerms")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<AcceptTermsRequest, NapkinIDESetupState, NapkinIDESetupStateHarness>(log, async (mgr, reqData) =>
            {
                return await mgr.AcceptTerms(reqData.AcceptedOn, reqData.Version);
            });
        }
    }
}
