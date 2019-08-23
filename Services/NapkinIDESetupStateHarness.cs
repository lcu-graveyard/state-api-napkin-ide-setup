using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Fathym;
using LCU.Graphs.Registry.Enterprises.Provisioning;
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
                    state.NewEnterpriseAPIKey = entRes.Model.PrimaryAPIKey;
                    
                    var doProj = await devOpsArch.EnsureDevOpsProject(state.NewEnterpriseAPIKey, details.Username, details.EnterpriseAPIKey);

                    var envResp = await devOpsArch.EnsureEnvironment(new EnsureEnvironmentRequest()
                    {
                        EnvSettings = state.EnvSettings,
                        OrganizationLookup = state.OrganizationLookup
                    }, state.NewEnterpriseAPIKey);

                    var env = envResp.Model;

                    state.EnvironmentLookup = env.Lookup;

                    var infraRepoResp = await devOpsArch.EnsureInfrastructureRepo(state.NewEnterpriseAPIKey, details.Username, env.Lookup, details.EnterpriseAPIKey, doProj.Model);

                    var lcuFeedResp = await devOpsArch.EnsureLCUFeed(new EnsureLCUFeedRequest()
                    {
                        EnvironmentLookup = env.Lookup
                    }, state.NewEnterpriseAPIKey, details.Username, details.EnterpriseAPIKey);

                    var taskLibraryResp = await devOpsArch.EnsureTaskTlibrary(state.NewEnterpriseAPIKey, details.Username, env.Lookup, details.EnterpriseAPIKey, doProj.Model);

                    var buildReleaseResp = await devOpsArch.EnsureInfrastructureBuildAndRelease(state.NewEnterpriseAPIKey, details.Username, env.Lookup, details.EnterpriseAPIKey,
                        doProj.Model);

                    var envInfra = await devOpsArch.SetEnvironmentInfrastructure(new SetEnvironmentInfrastructureRequest()
                    {
                        Template = "fathym\\daf-setup"
                    }, state.NewEnterpriseAPIKey, env.Lookup, details.Username, details.EnterpriseAPIKey);

                    state.EnterpriseBooted = envInfra.Status;
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

        public virtual async Task<NapkinIDESetupState> Finalize()
        {
            await HasDevOpsOAuth();

            if (state.HasDevOpsOAuth)
            {
                var ensured = await entArch.EnsureHost(new EnsureHostRequest()
                {
                    EnviromentLookup = state.EnvironmentLookup
                }, state.NewEnterpriseAPIKey, state.Host, state.EnvironmentLookup, details.EnterpriseAPIKey);

                //  TODO:  Release LCU Runtime and Web Job to the Web App via a persona - webApp.WarDeploy i think, or one of the other deploy methods

                if (ensured.Status)
                {
                    // await SetNapkinIDESetupStep(NapkinIDESetupStepTypes.Complete);
                }
            }

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

            state.Host = host?.ToLower();

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