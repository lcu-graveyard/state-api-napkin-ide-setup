using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Fathym;
using Fathym.API;
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
        public virtual async Task<NapkinIDESetupState> AcceptTerms(DateTimeOffset acceptedOn, string termsVersion)
        {
            logger.LogInformation("Accepting Terms");

            //  TODO:  Write and call persona to accept terms

            state.TermsAccepted = Status.Success;

            await SetNapkinIDESetupStep(NapkinIDESetupStepTypes.Review);

            return state;
        }

        public virtual async Task<NapkinIDESetupState> BootEnterprise()
        {
            logger.LogInformation("Booting Enterprise");

            await HasDevOpsOAuth();

            if (state.HasDevOpsOAuth)
            {
                if (state.NewEnterpriseAPIKey.IsNullOrEmpty())
                {
                    var entRes = await entArch.CreateEnterprise(new CreateEnterpriseRequest()
                    {
                        Description = state.OrganizationDescription,
                        Host = state.Host,
                        Name = state.OrganizationName
                    }, details.EnterpriseAPIKey, details.Username);

                    state.NewEnterpriseAPIKey = entRes.Model.PrimaryAPIKey;
                }

                if (!state.NewEnterpriseAPIKey.IsNullOrEmpty())
                {
                    var envResp = await devOpsArch.SetupEnvironment(new SetupEnvironmentRequest()
                    {
                        EnvSettings = state.EnvSettings,
                        Template = "fathym\\daf-state-setup",
                        // Template = "fathym\\daf-iot-setup",
                        OrganizationLookup = state.OrganizationLookup
                    }, state.NewEnterpriseAPIKey, details.Username, devOpsEntApiKey: details.EnterpriseAPIKey);

                    state.EnvironmentLookup = envResp.Model?.Lookup;

                    state.EnterpriseBooted = envResp.Status;
                }
            }

            return state;
        }

        public virtual async Task<NapkinIDESetupState> CanFinalize()
        {
            state.CanFinalize = false;

            if (!state.NewEnterpriseAPIKey.IsNullOrEmpty() && !state.EnvironmentLookup.IsNullOrEmpty())
            {
                var canFinalize = await entMgr.EnsureInfraBuiltAndReleased(state.NewEnterpriseAPIKey, details.Username, state.EnvironmentLookup, details.EnterpriseAPIKey);

                state.CanFinalize = canFinalize.Status == Status.Success;
            }

            return state;
        }

        public virtual async Task<NapkinIDESetupState> ConfigureInfrastructure(string infraType, bool useDefaultSettings, MetadataModel settings)
        {
            logger.LogInformation("Configuring Infrastructure.");

            var envLookup = $"{state.OrganizationLookup}-prd";

            state.EnvSettings = settings;

            await SetNapkinIDESetupStep(NapkinIDESetupStepTypes.HostConfig);

            await SetHostFlow(HostFlowTypes.shared);

            return state;
        }

        public virtual async Task<NapkinIDESetupState> Finalize()
        {
            logger.LogInformation("Finalizing Napkin IDE Setup");

            await HasDevOpsOAuth();

            if (state.HasDevOpsOAuth)
            {
                var authAppEnsured = await entArch.EnsureHostAuthApp(state.NewEnterpriseAPIKey, state.Host, state.EnvironmentLookup);

                if (authAppEnsured.Status)
                {
                    var hostEnsured = await entArch.EnsureHost(new EnsureHostRequest()
                    {
                        EnviromentLookup = state.EnvironmentLookup
                    }, state.NewEnterpriseAPIKey, state.Host, state.EnvironmentLookup, details.EnterpriseAPIKey);

                    if (hostEnsured.Status)
                    {
                        var sslEnsured = await entArch.EnsureHostsSSL(new EnsureHostsSSLRequest()
                        {
                            Hosts = new List<string>() { state.Host }
                        }, state.NewEnterpriseAPIKey, state.EnvironmentLookup, details.EnterpriseAPIKey);

                        if (sslEnsured.Status)
                        {
                            var runtimeEnsured = await entArch.EnsureLCURuntime(state.NewEnterpriseAPIKey, state.EnvironmentLookup);

                            if (runtimeEnsured.Status)
                            {
                                logger.LogInformation("Setting up final steps");

                                await devOpsArch.SetEnvironmentInfrastructure(new Personas.DevOps.SetEnvironmentInfrastructureRequest()
                                {
                                    Template = "fathym\\daf-iot-setup"
                                }, state.NewEnterpriseAPIKey, state.EnvironmentLookup, details.Username, devOpsEntApiKey: details.EnterpriseAPIKey);

                                await appDev.ConfigureNapkinIDEForDataApps(state.NewEnterpriseAPIKey, state.Host);

                                await appDev.ConfigureNapkinIDEForDataFlows(state.NewEnterpriseAPIKey, state.Host);

                                state.Step = NapkinIDESetupStepTypes.Complete;
                            }
                        }
                    }
                }
            }

            return state;
        }

        public virtual async Task<NapkinIDESetupState> GetTerms()
        {
            logger.LogInformation("Getting Terms");

            //  TODO:  Write and call persona to get terms

            state.Terms = "<div _ngcontent-trf-c16=\"\" class=\"ng-star-inserted\">" +
                "<h1 _ngcontent-trf-c16=\"\" class=\"mat-display-1 title margin-bottom-4\" fxlayoutalign=\"center center\"" +
                "ng-reflect-fx-layout-align=\"center center\"" +
                "style=\"place-content: center; align-items: center; flex-direction: row; box-sizing: border-box; display: flex;\">" +
                "Fathym Enterprise Agreement </h1>" +
                "<div _ngcontent-trf-c16=\"\" fxlayout=\"row\" fxlayoutalign=\"space-around start\" fxlayoutgap=\"50px\"" +
                "ng-reflect-fx-layout=\"row\" ng-reflect-fx-layout-gap=\"50px\" ng-reflect-fx-layout-align=\"space-around start\"" +
                "style=\"flex-direction: row; box-sizing: border-box; display: flex; place-content: flex-start space-around; align-items: flex-start;\">" +
                "<div _ngcontent-trf-c16=\"\" fxflex=\"50%\" ng-reflect-fx-flex=\"50%\"" +
                "style=\"margin-right: 50px; flex: 1 1 100%; box-sizing: border-box; max-width: 50%;\">" +
                "<p _ngcontent-trf-c16=\"\">By continuting through this step and accepting, you agree to enter into and be " +
                "bound by the Enterprise Agreement located at:</p><a _ngcontent-trf-c16=\"\"" +
                "href=\"https://fathym.com/enterprise-agreement/\" target=\"blank\"><button _ngcontent-trf-c16=\"\"" +
                "class=\"mat-full-width mat-button mat-button-base mat-warn\" color=\"warn\" mat-button=\"\"" +
                "ng-reflect-color=\"warn\"><span class=\"mat-button-wrapper\"> Enterprise Agreement </span>" +
                "<div class=\"mat-button-ripple mat-ripple\" matripple=\"\" ng-reflect-centered=\"false\"" +
                "ng-reflect-disabled=\"false\" ng-reflect-trigger=\"[object HTMLButtonElement]\"></div>" +
                "<div class=\"mat-button-focus-overlay\"></div>" +
                "</button></a>" +
                "</div><br _ngcontent-trf-c16=\"\" style=\"margin-right: 50px;\">" +
                "<div _ngcontent-trf-c16=\"\" fxflex=\"50%\" ng-reflect-fx-flex=\"50%\"" +
                "style=\"flex: 1 1 100%; box-sizing: border-box; max-width: 50%;\">" +
                "<p _ngcontent-trf-c16=\"\">By clicking Accept you also accept Fathym's Terms and Conditions: </p><a " +
                "_ngcontent-trf-c16=\"\" href=\"https://fathym.com/terms-of-services/\" target=\"blank\"><button " +
                "_ngcontent-trf-c16=\"\" class=\"mat-full-width mat-button mat-button-base mat-warn\" color=\"warn\"" +
                "mat-button=\"\" ng-reflect-color=\"warn\"><span class=\"mat-button-wrapper\"> Terms &amp; Conditions" +
                "</span>" +
                "<div class=\"mat-button-ripple mat-ripple\" matripple=\"\" ng-reflect-centered=\"false\"" +
                "ng-reflect-disabled=\"false\" ng-reflect-trigger=\"[object HTMLButtonElement]\"></div>" +
                "<div class=\"mat-button-focus-overlay\"></div>" +
                "</button></a>" +
                "</div>" +
                "</div>" +
                "</div>";

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

            await SetNapkinIDESetupStep(NapkinIDESetupStepTypes.Terms);

            await GetTerms();

            return state;
        }

        public virtual async Task<NapkinIDESetupState> SetHostFlow(HostFlowTypes hostFlow)
        {
            logger.LogInformation("Setting host flow");

            if (state.HostOptions.IsNullOrEmpty())
            {
                var regHosts = await entMgr.ListRegistrationHosts(details.EnterpriseAPIKey);

                state.HostOptions = regHosts.Model;
            }

            state.HostFlow = hostFlow;

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
            logger.LogInformation("Setting Napkin IDE Setup Step");

            await HasDevOpsOAuth();

            state.Step = step;

            return state;
        }

        public virtual async Task<NapkinIDESetupState> SetupDevOpsOAuth(string devOpsAppId, string devOpsClientSecret, string devOpsScopes)
        {
            logger.LogInformation("Setting Up DevOps OAuth");

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