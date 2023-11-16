using CodeFactory.Architecture.AspNetCore.Service.Rest.CSharpFile;
using CodeFactory.WinVs;
using CodeFactory.WinVs.Commands;
using CodeFactory.WinVs.Commands.IDE;
using CodeFactory.WinVs.Logging;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Architecture.AspNetCore.Service.Rest.IDE
{
    /// <summary>
    /// Code factory command that is executed when the solution is loaded. This command only gets called one time on load of the solution.
    /// </summary>
    public class LoadExternalConfiguration : SolutionLoadCommandBase
    {
        private static readonly string commandTitle = "Load External Configuration";
        private static readonly string commandDescription = "Loads the external configuration for automation.";

        #pragma warning disable CS1998
        /// <inheritdoc />
        public LoadExternalConfiguration(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Code factory framework calls this method when the command has been executed. 
        /// </summary>
        /// <param name="result">The code factory model that has generated and provided to the command to process.</param>
        public override async Task ExecuteCommandAsync(VsSolution result)
        {

            try
            {
                var refreshEFRepository = new RefreshEFRepository(null, null);
                refreshEFRepository.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var refreshRestService = new RefreshRestService(null, null);
                refreshRestService.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var refreshTest = new RefreshTest(null, null);
                refreshTest.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var refreshFluentValidation = new RefreshFluentValidation(null, null);
                refreshFluentValidation.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var addMissingRepositoryMembers = new AddMissingRepositoryMembers(null, null);
                addMissingRepositoryMembers.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var updateLogicImplementation = new UpdateLogicImplementation(null, null);
                updateLogicImplementation.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var addMissingLogicMembers = new AddMissingLogicMembers(null, null);
                addMissingLogicMembers.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var refreshLogic = new RefreshLogic(null, null);
                refreshLogic.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                ConfigManager.LoadConfiguration(result, "Automation", VisualStudioActions);
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while executing the solution explorer solution command {commandTitle}. ",
                    unhandledError);

            }

        }
    }
}
