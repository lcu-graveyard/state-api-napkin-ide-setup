using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LCU.State.API.NapkinIDE.Setup.Models;
using LCU.State.API.NapkinIDE.Setup.Harness;
using Microsoft.WindowsAzure.Storage;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace LCU.State.API.NapkinIDE.Setup
{
    // [DataContract]
    // public class GrantAccessRequest
    // {
    //     [DataMember]
    //     public virtual string AccessToken { get; set; }

    // }

    public static class GrantAccess
    {
        [FunctionName("GrantUserAccess")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            return await req.Manage<dynamic, UserManagementState, UserManagementStateHarness>(log, async (mgr, reqData) =>
            {
                log.LogInformation($"Requesting user access...");

                await mgr.GrantAccess(req.Query["token"].ToString());

                return await mgr.WhenAll(
                );
            });
        }
    }
}
