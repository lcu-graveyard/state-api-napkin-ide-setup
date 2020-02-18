using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LCU.State.API.NapkinIDE.Setup.Models;
using Fathym;
using Fathym.API;
using Fathym.Design.Singleton;
using LCU.Graphs;
using LCU.Graphs.Registry.Enterprises;
using LCU.StateAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using LCU.Presentation;
using Microsoft.AspNetCore.WebUtilities;
using LCU.Personas.Client.Applications;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using LCU.Personas.Client.Enterprises;
using LCU.Personas.Client.Security;
using LCU.Personas.Security;
using LCU;
using Newtonsoft.Json;
using LCU.Personas.Client.Identity;

namespace LCU.State.API.NapkinIDE.Setup.Harness
{
    public class UserManagementStateHarness : LCUStateHarness<UserManagementState>
    {
        #region Fields

        protected readonly ApplicationManagerClient appMgr;

        protected readonly string enterpriseId;

        protected readonly EnterpriseManagerClient entMgr;

        protected readonly SecurityManagerClient secMgr;

        protected readonly IdentityManagerClient idMgr;

        #endregion
        public UserManagementStateHarness(HttpRequest req, ILogger log, UserManagementState state)
            : base(req, log, state)
        {
            appMgr = req.ResolveClient<ApplicationManagerClient>(logger);

            entMgr = req.ResolveClient<EnterpriseManagerClient>(logger);

            secMgr = req.ResolveClient<SecurityManagerClient>(logger);

            idMgr = req.ResolveClient<IdentityManagerClient>(logger);

            var enterprise = entMgr.GetEnterprise(details.EnterpriseAPIKey).GetAwaiter().GetResult();

            enterpriseId = enterprise.Model.ID.ToString();

            appMgr.RegisterApplicationProfile(details.ApplicationID, new LCU.ApplicationProfile()
            {
                DatabaseClientMaxPoolConnections = Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-MAX-POOL-CONNS").As<int>(32),
                DatabaseClientPoolSize = Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-POOL-SIZE").As<int>(4),
                DatabaseClientTTLMinutes = Environment.GetEnvironmentVariable("LCU-DATABASE-CLIENT-TTL").As<int>(60)
            });
        }

        public virtual async Task<UserManagementState> RequestAuthorization(string userID, string hostName)
        {
            // Create an access request
            var accessRequest = new AccessRequest()
            {
                User = userID,
                EnterpriseID = enterpriseId,
                AccessConfigurationType = "LCU"
            };

            // Create JToken to attached to metadata model
            var model = new MetadataModel();
            model.Metadata.Add(new KeyValuePair<string, JToken>("AccessRequest", JToken.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(accessRequest))));

            // Create token model - is including the access request payload redundant?? 
            var tokenModel = new CreateTokenModel()
            {
                Payload = model,
                UserEmail = userID,
                OrganizationID = enterpriseId,
                Encrypt = true
            };

            // Encrypt user email and enterpries ID, generate token
            var response = await secMgr.CreateToken("RequestAccessToken", tokenModel);

            // Query graph for admins of enterprise ID
            var admins = idMgr.ListAdmins(enterpriseId);

            // Build grant/deny links and text body
            if (response != null)
            {
                string grantLink = $"<a href=\"{hostName}/grant/token?={response.Model}\">Grant Access</a>";
                string denyLink = $"<a href=\"{hostName}/deny/token?={response.Model}\">Deny Access</a>";
                string emailHtml = $"A user has requested access to this Organization : {grantLink} {denyLink}";

                // Send email from app manager client 
                foreach (string admin in admins.Result.Model)
                {
                    var email = new AccessRequestEmail()
                    {
                        Content = emailHtml,
                        EmailFrom = "admin@fathym.com",
                        EmailTo = admin,
                        User = userID,
                        Subject = "Access authorization requested",
                        EnterpriseID = enterpriseId
                    };

                    var emailModel = new MetadataModel();
                    model.Metadata.Add(new KeyValuePair<string, JToken>("AccessRequestEmail", JToken.Parse(JsonConvert.SerializeObject(email))));

                    await appMgr.SendAccessRequestEmail(model, details.EnterpriseAPIKey);
                }
            }

            // If successful, adjust state to reflect that a request was sent for this enterprise by this user
            return state;
        }


        public virtual async Task<UserManagementState> GrantAccess(string token)
        {
            var response = await appMgr.GrantAccess(token, enterpriseId);            
            
            return state;
        }

        public virtual async Task<UserManagementState> DenyAccess(string token)
        {
            var response = await appMgr.DenyAccess(token, enterpriseId);

            return state;
        }

    }

}