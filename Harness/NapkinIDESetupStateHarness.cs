using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using LCU.Presentation.Personas.Applications;
using LCU.Runtime;
using LCU.State.API.NapkinIDE.Setup.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LCU.State.API.NapkinIDE.Setup.Harness
{
    public class ForgeAPIAppsStateHarness : LCUStateHarness<NapkinIDESetupState>
    {
        #region Fields
        protected readonly ApplicationManagerClient appMgr;

        #endregion

        #region Properties

        #endregion

        #region Constructors
        public ForgeAPIAppsStateHarness(HttpRequest req, ILogger logger, NapkinIDESetupState state)
            : base(req, logger, state)
        {
            appMgr = req.ResolveClient<ApplicationManagerClient>(logger);
        }
        #endregion

        #region API Methods
        public virtual async Task<NapkinIDESetupState> Refresh()
        {
            return state;
        }
        #endregion

        #region Helpers
        #endregion
    }
}