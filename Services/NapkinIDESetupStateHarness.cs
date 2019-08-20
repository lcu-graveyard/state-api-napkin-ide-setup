using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Fathym;
using LCU.Personas.Client.Applications;
using LCU.Personas.Client.DevOps;
using LCU.Personas.Client.Enterprises;
using LCU.Personas.Client.Identity;
using LCU.Personas.DevOps;
using LCU.Personas.Enterprises;
using LCU.State.API.NapkinIDE.Setup.Models;
using LCU.StateAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LCU.State.API.NapkinIDE.Setup.Services
{
    public class NapkinIDESetupStateHarness : LCUStateHarness<NapkinIDESetupState>
    {
        #region Fields
        protected readonly ApplicationManagerClient appMgr;

        protected readonly DevOpsArchitectClient devOpsArch;

        protected readonly EnterpriseArchitectClient entArch;

        protected readonly EnterpriseManagerClient entMgr;

        protected readonly IdentityManagerClient idMgr;
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public NapkinIDESetupStateHarness(HttpRequest req, ILogger logger, NapkinIDESetupState state)
            : base(req, logger, state)
        {
            devOpsArch = req.ResolveClient<DevOpsArchitectClient>(logger);

            entArch = req.ResolveClient<EnterpriseArchitectClient>(logger);

            entMgr = req.ResolveClient<EnterpriseManagerClient>(logger);

            idMgr = req.ResolveClient<IdentityManagerClient>(logger);
        }
        #endregion

        #region API Methods
        public virtual async Task<NapkinIDESetupState> BootEnterprise()
        {
            await HasDevOpsOAuth();

            if (state.HasDevOpsOAuth)
            {
                var entRes = await entArch.CreateEnterprise(new CreateEnterpriseRequest()
                {
                    Description = state.OrganizationDescription,
                    Host = state.Host,
                    Name = state.OrganizationName
                }, details.EnterpriseAPIKey, details.Username);

                if (entRes.Status)
                {
                    var setup = await devOpsArch.SetupInfrastructure(new SetupInfrastructureRequest()
                    {
                        EnvSettings = state.EnvSettings,
                        OrganizationLookup = state.OrganizationLookup
                    }, details.EnterpriseAPIKey, details.Username);

                    // if (configured.Status)
                    // {
                    //     state.Environment = configured.Model;

                    //     var envSettings = await entMgr.GetEnvironmentSettings(details.EnterpriseAPIKey, state.Environment.Lookup);

                    //     state.EnvSettings = envSettings.Model;
                    // }
                    // else
                    //     state.Error = configured.Status.Message;

                    // if (state.InfraTemplate.SelectedTemplate.IsNullOrEmpty())
                    //     state.InfraTemplate.SelectedTemplate = selectedTemplate;

                    // var committed = await devOpsArch.CommitInfrastructure(new Presentation.Personas.DevOps.CommitInfrastructureRequest()
                    // {
                    //     EnvironmentLookup = state.Environment.Lookup,
                    //     SelectedTemplate = state.InfraTemplate.SelectedTemplate,
                    // }, details.EnterpriseAPIKey, state.Environment.Lookup, details.Username);

                    // state.Host = host?.ToLower();

                    // var acquired = await entArch.SecureHost(new SecureHostRequest()
                    // {
                    //     OrganizationDescription = state.OrganizationDescription,
                    //     OrganizationName = state.OrganizationName
                    // }, details.EnterpriseAPIKey, state.Host);

                    // state.HostApprovalMessage = null;

                    // if (acquired.Status)
                    // {
                    //     state.Step = StepTypes.Provisioning;

                    //     state.Provisioning = "Sit back and relax while we provision your new organization forge. This will configure things to run at the above domain.";
                    // }
                    // else
                    //     state.HostApprovalMessage = acquired.Status.Message;
                }
            }

            return state;
        }

        public virtual async Task<NapkinIDESetupState> ConfigureInfrastructure(string infraType, bool useDefaultSettings, MetadataModel settings)
        {
            var envLookup = $"{state.OrganizationLookup}-prd";

            state.EnvSettings = settings;

            await SetNapkinIDESetupStep(NapkinIDESetupStepTypes.HostConfig);

            return state;
        }

        public virtual async Task<NapkinIDESetupState> HasDevOpsOAuth()
        {
            var hasDevOps = await entMgr.HasDevOpsOAuth(details.EnterpriseAPIKey, details.Username);

            state.HasDevOpsOAuth = hasDevOps.Status;

            return state;
        }

        public virtual async Task<NapkinIDESetupState> Refresh()
        {
            logger.LogInformation("Refreshing NapkinIDE setup state");

            if (state.OrganizationName.IsNullOrEmpty())
                await SetNapkinIDESetupStep(NapkinIDESetupStepTypes.OrgDetails);

            await HasDevOpsOAuth();

            return state;
        }

        public virtual async Task<NapkinIDESetupState> SecureHost(string host)
        {
            logger.LogInformation("Securing Host");

            state.Host = host;

            await SetNapkinIDESetupStep(NapkinIDESetupStepTypes.Review);

            return state;
        }

        public virtual async Task<NapkinIDESetupState> SetHostFlow(string hostFlow)
        {
            logger.LogInformation("Setting host flow");

            if (state.HostOptions == null || state.HostOptions.Count == 0)
            {
                var regHosts = await entMgr.ListRegistrationHosts(details.EnterpriseAPIKey);

                state.HostOptions = regHosts.Model;
            }

            state.HostFlow = hostFlow?.ToEnum<HostFlowTypes>();

            return state;
        }

        public virtual async Task<NapkinIDESetupState> SetOrganizationDetails(string name, string description, string lookup)
        {
            logger.LogInformation("Setting organization details");

            if (!name.IsNullOrEmpty())
                await SetNapkinIDESetupStep(NapkinIDESetupStepTypes.AzureSetup);
            else
                await SetNapkinIDESetupStep(NapkinIDESetupStepTypes.OrgDetails);

            state.OrganizationName = name;

            state.OrganizationDescription = description;

            state.OrganizationLookup = lookup;

            return state;
        }

        public virtual async Task<NapkinIDESetupState> SetNapkinIDESetupStep(NapkinIDESetupStepTypes step)
        {
            await HasDevOpsOAuth();

            state.Step = step;

            return state;
        }

        public virtual async Task<NapkinIDESetupState> SetupDevOpsOAuth(string devOpsAppId, string devOpsClientSecret, string devOpsScopes)
        {
            await SetNapkinIDESetupStep(NapkinIDESetupStepTypes.HostConfig);

            state.DevOpsAppID = devOpsAppId;

            state.DevOpsClientSecret = devOpsClientSecret;

            state.DevOpsScopes = devOpsScopes;

            return state;
        }
        #endregion

        #region Helpers
        #endregion
    }
}