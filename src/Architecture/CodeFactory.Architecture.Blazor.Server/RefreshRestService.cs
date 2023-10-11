using CodeFactory.Automation.NDF.Logic.AspNetCore.Service.Rest.Json;
using CodeFactory.WinVs;
using CodeFactory.WinVs.Commands;
using CodeFactory.WinVs.Commands.SolutionExplorer;
using CodeFactory.WinVs.Logging;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Architecture.Blazor.Server
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class RefreshRestService : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Refresh Rest Service";
        private static readonly string commandDescription = "Refreshes the implementation of a rest service from a contract definition.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public RefreshRestService(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The name of the command the configuration is tied to.
        /// </summary>
        public static string Type = typeof(RefreshRestService).FullName;

        /// <summary>
        /// The execution project that contains the definition of the logic contract to implement as a service.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// The execution project folder the contract is stored in, this is optional if logic contracts are in a target sub folder of the project.
        /// </summary>
        public static string ExecutionFolder = "ExecutionFolder";

        /// <summary>
        /// The project where service implementation is hosted.
        /// </summary>
        public static string ServiceProject = "ServiceProject";

        /// <summary>
        /// The target project folder where services are implemented, this required.
        /// </summary>
        public static string ServiceFolder = "ServiceFolder";

        /// <summary>
        /// The project that holds the service models.
        /// </summary>
        public static string ModelProject = "ModelProject";

        /// <summary>
        /// The target folder where service models are to be hosted, this is optional.
        /// </summary>
        public static string ModelFolder = "ModelFolder";

        /// <summary>
        /// The target project that the abstraction logic will be hosted.
        /// </summary>
        public static string AbstractionProject = "AbstractionProject";

        /// <summary>
        /// The target folder where abstraction logic will be hosted, this is optional.
        /// </summary>
        public static string AbstractionFolder = "AbstractionFolder";

        /// <summary>
        /// The target project where abstraction contracts will be created.
        /// </summary>
        public static string ContractProject = "ContractProject";

        /// <summary>
        /// The target folder where abstraction contracts will be created, this is optional
        /// </summary>
        public static string ContractFolder = "ContractFolder";

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            var command = new ConfigCommand
            { Category = "JsonRestService", Name = nameof(RefreshRestService), CommandType = Type }
                .UpdateExecutionProject
                (
                    new ConfigProject
                    {
                        Name = ExecutionProject,
                        Guidance = "Enter the fully project name for the logic contracts project."
                    }
                        .AddFolder
            (
                            new ConfigFolder
                            {
                                Name = ExecutionFolder,
                                Required = false,
                                Guidance =
                                    "Optional, set the relative path from the root of the project. If it is more then one directory deep then use forward slash instead of back slashes."
                            }
                        )
                )
                .AddProject
                (
                    new ConfigProject
                    {
                        Name = ServiceProject,
                        Guidance =
                                "Enter the full project name for the project that hosts the WebAPI service implementation of the logic contract."
                    }
                        .AddFolder
            (
                            new ConfigFolder
                            {
                                Name = ServiceFolder,
                                Required = true,
                                Path = "Controllers",
                                Guidance =
                                    "Required, set the relative path from the root of the project where service controllers are hosted. If it is more then one directory deep then use forward slash instead of back slashes."
                            }
                        )
                )
                .AddProject
                (
                    new ConfigProject
                    {
                        Name = ModelProject,
                        Guidance =
                                "Enter the full project name for the project that hosts the rest service models used by the services."
                    }
                        .AddFolder
            (
                            new ConfigFolder
                            {
                                Name = ModelFolder,
                                Required = false,
                                Guidance =
                                    "Optional, set the relative path from the root of the project. If it is more then one directory deep then use forward slash instead of back slashes."
                            }
                        )
                )
                .AddProject
                (
                    new ConfigProject
                    {
                        Name = AbstractionProject,
                        Guidance =
                                "Enter the full project name for the project that hosts the abstraction implementation of the service."
                    }
                        .AddFolder
                        (
                            new ConfigFolder
                            {
                                Name = AbstractionFolder,
                                Required = false,
                                Guidance =
                                    "Optional, set the relative path from the root of the project. If it is more then one directory deep then use forward slah instead of back slashes."
                            }
                        )
                )
                .AddProject
                (
                    new ConfigProject
                    {
                        Name = ContractProject,
                        Guidance =
                                "Enter the full project name for the project that hosts interface contracts for the abstraction implementation."
                    }
                        .AddFolder
                        (
                            new ConfigFolder
                            {
                                Name = ContractFolder,
                                Required = false,
                                Guidance =
                                    "Optional, set the relative path from the root of the project. If it is more then one directory deep then use '/' instead of back slashes."
                            }
                        )
                );

            return command;
        }
        #endregion

        #region Overrides of VsCommandBase<IVsCSharpDocument>

        /// <summary>
        /// Validation logic that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsCSharpSource result)
        {
            //Result that determines if the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
                var command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                isEnabled = command != null;
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while checking if the solution explorer C# document command {commandTitle} is enabled. ",
                    unhandledError);
                isEnabled = false;
            }

            return isEnabled;
        }

        /// <summary>
        /// Code factory framework calls this method when the command has been executed. 
        /// </summary>
        /// <param name="result">The code factory model that has generated and provided to the command to process.</param>
        public override async Task ExecuteCommandAsync(VsCSharpSource result)
        {
            try
            {
                var command = (await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result))
                              ?? throw new CodeFactoryException("Could not load the command configuration, cannot refresh the service.");

                var logicContract =
                    result?.SourceCode?.Interfaces?.FirstOrDefault()
                    ?? throw new CodeFactoryException("Cannot load the logic contract, cannot refresh the service.");

                var serviceProject =
                    await VisualStudioActions.GetProjectFromConfigAsync(command.Project(ServiceProject))
                    ?? throw new CodeFactoryException("Cannot load the service project, cannot refresh the service.");

                var serviceFolder =
                    await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(ServiceProject), ServiceFolder)
                    ?? throw new CodeFactoryException("Cannot load the service project folder, cannot refresh the service.");

                var modelProject =
                    await VisualStudioActions.GetProjectFromConfigAsync(command.Project(ModelProject))
                    ?? throw new CodeFactoryException("Cannot load the model project, cannot refresh the service.");

                var modelFolder =
                    await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(ModelProject), ModelFolder);

                var abstractionProject =
                    await VisualStudioActions.GetProjectFromConfigAsync(command.Project(AbstractionProject))
                    ?? throw new CodeFactoryException("Cannot load the abstraction project, cannot refresh the service.");

                var abstractionFolder =
                    await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(AbstractionProject), AbstractionFolder);

                var contractProject =
                    await VisualStudioActions.GetProjectFromConfigAsync(command.Project(ContractProject))
                    ?? throw new CodeFactoryException("Cannot load the abstraction contract project, cannot refresh the service.");

                var contractFolder =
                    await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(ContractProject), ContractFolder);

                var serviceClass = await VisualStudioActions.RefreshJsonRestService(logicContract, serviceProject, serviceFolder,
                    modelProject, abstractionProject, contractProject, modelFolder, abstractionFolder, contractFolder);


            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while executing the solution explorer C# document command {commandTitle}. ",
                    unhandledError);

            }

        }

        #endregion
    }
}
