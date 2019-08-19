using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Fathym;
using LCU.Presentation.Personas.Applications;
using LCU.Presentation.Personas.Enterprises;
using LCU.Runtime;
using LCU.State.API.NapkinIDE.Setup.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LCU.State.API.NapkinIDE.Setup.Services
{
    public class NapkinIDESetupStateHarness : LCUStateHarness<NapkinIDESetupState>
    {
        #region Fields
        protected readonly ApplicationManagerClient appMgr;

        protected readonly EnterpriseArchitectClient entArch;

        protected readonly EnterpriseManagerClient entMgr;
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public NapkinIDESetupStateHarness(HttpRequest req, ILogger logger, NapkinIDESetupState state)
            : base(req, logger, state)
        {
            entArch = req.ResolveClient<EnterpriseArchitectClient>(logger);

            entMgr = req.ResolveClient<EnterpriseManagerClient>(logger);
        }
        #endregion

        #region API Methods
        public virtual async Task<NapkinIDESetupState> BootEnterprise()
        {
            await HasDevOpsOAuth();

            if (state.HasDevOpsOAuth)
            {
                //  TODO:  Need a new enterprise api key, what is the first call that does that???  SecureHost?  Can't be anymore, should add a get or create enterprise method or something...

                // var devOpsOAuth = await entMgr.SetupDevOpsOAuthConnection(new Presentation.Personas.Enterprises.SetupDevOpsOAuthConnectionRequest()
                // {
                //     DevOpsAppID = devOpsAppId,
                //     DevOpsClientSecret = devOpsClientSecret,
                //     DevOpsScopes = devOpsScopes
                // }, details.EnterpriseAPIKey);

                // var configured = await devOpsArch.ConfigureInfrastructure(new Presentation.Personas.DevOps.ConfigureInfrastructureRequest()
                // {
                //     EnvSettings = settings,
                //     OrganizationLookup = state.GitHub.SelectedOrg,
                //     InfraType = infraType,
                //     UseDefaultSettings = useDefaultSettings
                // }, details.EnterpriseAPIKey, envLookup, details.Username);

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