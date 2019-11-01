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
using Fathym;
using LCU.State.API.NapkinIDE.Setup.Models;
using LCU.State.API.NapkinIDE.Setup.Services;

namespace LCU.State.API.NapkinIDE.Setup
{
    [Serializable]
    [DataContract]
    public class ConfigureInfrastructureRequest
    {
        [DataMember]
        public virtual string InfrastructureType { get; set; }

        [DataMember]
        public virtual MetadataModel Settings { get; set; }

        [DataMember]
        public virtual bool UseDefaultSettings { get; set; }
    }

    public static class ConfigureInfrastructure
    {
        [FunctionName("ConfigureInfrastructure")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
			return await req.Manage<ConfigureInfrastructureRequest, NapkinIDESetupState, NapkinIDESetupStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Configuring Infrastructure: {reqData.InfrastructureType} {reqData.UseDefaultSettings} {reqData.Settings}");

                return await mgr.ConfigureInfrastructure(reqData.InfrastructureType, reqData.UseDefaultSettings, reqData.Settings);
            });
        }
    }
}
