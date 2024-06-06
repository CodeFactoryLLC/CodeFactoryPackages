using CodeFactory.Architecture.AspNetCore.Service.Rest.CSharpFile;
using CodeFactory.Automation.NDF.Logic.Testing.MSTest;
using CodeFactory.Automation.NDF.Logic.Testing.XUnit;
using CodeFactory.WinVs;
using CodeFactory.WinVs.Commands;
using CodeFactory.WinVs.Commands.IDE;
using CodeFactory.WinVs.Logging;
using CodeFactory.WinVs.Models.ProjectSystem;
using System;
using System.Threading.Tasks;

namespace CodeFactory.Architecture.AspNetCore.Service.Rest.IDE
{
	/// <summary>
	/// Code factory command that is executed when the solution is loaded. This command only gets called one time on load of the solution.
	/// </summary>
	public class LoadExternalConfigurationCommand : SolutionLoadCommandBase
    {
        private static readonly string commandTitle = "Load External Configuration";
        private static readonly string commandDescription = "Loads the external configuration for automation.";

        #pragma warning disable CS1998
        /// <inheritdoc />
        public LoadExternalConfigurationCommand(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
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
                var refreshEFRepository = new RefreshEFRepositoryCommand(null, null);
                refreshEFRepository.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var refreshRestService = new RefreshRestServiceCommand(null, null);
                refreshRestService.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

				// Load configuration based on what type of test project you have loaded                
				var projects = await result.GetProjectsAsync(false);
				foreach (var project in projects)
				{
					if (await project.TestProjectIsConfiguredMSTestAsync())
					{
						var RefreshMSTestCommand = new RefreshMSTestCommand(null, null);
						RefreshMSTestCommand.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();
					}

					if (await project.TestProjectIsConfiguredXUnitAsync())
					{
						var RefreshXUnitTestCommand = new RefreshXUnitTestCommand(null, null);
						RefreshXUnitTestCommand.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();
					}
				}

				var refreshFluentValidation = new RefreshFluentValidationCommand(null, null);
                refreshFluentValidation.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var addMissingRepositoryMembers = new AddMissingRepositoryMembersCommand(null, null);
                addMissingRepositoryMembers.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var updateLogicImplementation = new UpdateLogicImplementationCommand(null, null);
                updateLogicImplementation.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var addMissingLogicMembers = new AddMissingLogicMembersCommand(null, null);
                addMissingLogicMembers.LoadExternalConfigDefinition().RegisterCommandWithDefaultConfiguration();

                var refreshLogic = new RefreshLogicCommand(null, null);
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
