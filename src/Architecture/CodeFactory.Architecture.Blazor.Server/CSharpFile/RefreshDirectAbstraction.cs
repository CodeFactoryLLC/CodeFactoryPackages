using CodeFactory.Automation.Standard.Logic;
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
using System.Windows;
using CodeFactory.Automation.NDF.Logic.Abstraction;

namespace CodeFactory.Architecture.Blazor.Server.CSharpFile
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class RefreshDirectAbstraction : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Refresh Direct Abstraction";
        private static readonly string commandDescription = "Refreshes the implementation of an abstraction from a contract definition.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public RefreshDirectAbstraction(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The name of the command the configuration is tied to.
        /// </summary>
        public static string Type = typeof(RefreshDirectAbstraction).FullName;

        /// <summary>
        /// The execution project that contains the definition of the logic contract to implement as a service.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// The execution project folder the contract is stored in, this is optional if logic contracts are in a target sub folder of the project.
        /// </summary>
        public static string ExecutionFolder = "ExecutionFolder";

        /// <summary>
        /// The target project that the abstraction logic will be hosted.
        /// </summary>
        public static string AbstractionProject = "AbstractionProject";

        /// <summary>ContractNameRemovePrefixes
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
        /// Comma separated list of prefixes to remove from the interface contract when creating the abstract name.
        /// </summary>
        public static string ContractNameRemovePrefixes = "ContractNameRemovePrefixes";

        /// <summary>
        /// Comma separated list of suffixes to remove from the logic contract when creating the abstraction name.
        /// </summary>
        public static string ContractNameRemoveSuffixes = "ContractNameRemoveSuffixes";

        /// <summary>
        /// Prefix to start the service client contract name with.
        /// </summary>
        public static string AbstractionContractNameAppendPrefix = "AbstractionContractNameAppendPrefix";

        /// <summary>
        /// Suffix to append to the service client contract name. 
        /// </summary>
        public static string AbstractionContractNameAppendSuffix = "AbstractionContractNameAppendSuffix";

        /// <summary>
        /// Prefix to start the service client name with.
        /// </summary>
        public static string AbstractionNameAppendPrefix = "AbstractionNameAppendPrefix";

        /// <summary>
        /// Suffix to append to the service client name. 
        /// </summary>
        public static string AbstractionNameAppendSuffix = "AbstractionNameAppendSuffix";


        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            var command = new ConfigCommand
            { 
                Category = "Abstraction", 
                Name = nameof(RefreshDirectAbstraction), 
                CommandType = Type,
                Guidance = "Generates an abstraction layer class that calls the selected interface."
            }
                .UpdateExecutionProject
                (
                    new ConfigProject
                    {
                        Name = ExecutionProject,
                        Guidance = "Enter the fully project name for the project that hosts the interface contracts."
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
                        .AddParameter
                        (
                            new ConfigParameter
                            {
                                Name = ContractNameRemovePrefixes,
                                Guidance = "Optional, provide a comma separated value of each prefix to check for to be removed from the contract name when creating the name of the  abstraction."
                            }
                        )
                        .AddParameter
                        (
                            new ConfigParameter
                            {
                                Name = ContractNameRemoveSuffixes,
                                Guidance = "Optional, provide a comma separated value of each suffix to check for to be removed from the contract name when creating the name of the abstraction.",
                                Value = "Logic"
                            }
                        )
                        .AddParameter
                        (
                            new ConfigParameter
                            {
                                Name = AbstractionContractNameAppendPrefix,
                                Guidance = "Optional, provide the prefix to append to the abstraction interface name."
                            }
                        )
                        .AddParameter
                        (
                            new ConfigParameter
                            {
                                Name = AbstractionContractNameAppendSuffix,
                                Guidance = "Optional, provide the suffix to append to the abstraction interface name.",
                                Value = "Client"
                            }
                        )
                        .AddParameter
                        (
                            new ConfigParameter
                            {
                                Name = AbstractionNameAppendPrefix,
                                Guidance = "Optional, provide the prefix to append to the abstraction name."
                            }
                        )
                        .AddParameter
                        (
                            new ConfigParameter
                            {
                                Name = AbstractionNameAppendSuffix,
                                Guidance = "Optional, provide the suffix to append to the abstraction name.",
                                Value = "Client"
                            }
                        )

                )
                .AddProject
                (
                    new ConfigProject
                    {
                        Name = AbstractionProject,
                        Guidance =
                                "Enter the full project name for the project that hosts the abstraction implementation."
                    }
                        .AddFolder
                        (
                            new ConfigFolder
                            {
                                Name = AbstractionFolder,
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

        /// <summary>
        /// Registers the default configuration with the configuration manager in CodeFactory.
        /// </summary>
        public static void RegisterDefaultConfiguration()
        {
            var command = new RefreshDirectAbstraction(null, null);
            var config = command.LoadExternalConfigDefinition();
            config?.RegisterCommandWithDefaultConfiguration();
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
                    ?? throw new CodeFactoryException("Cannot load the contract interface, cannot refresh the service.");

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

                //Execution command parameters.
                var contractNameRemovePrefixes = command.ExecutionProject.ParameterValue(ContractNameRemovePrefixes);
                var contractNameRemoveSuffixes = command.ExecutionProject.ParameterValue(ContractNameRemoveSuffixes);
                var abstractionContractNameAppendPrefix = command.ExecutionProject.ParameterValue(AbstractionContractNameAppendPrefix);
                var abstractionContractNameAppendSuffix = command.ExecutionProject.ParameterValue(AbstractionContractNameAppendSuffix);
                var abstractionNameAppendPrefix = command.ExecutionProject.ParameterValue(AbstractionNameAppendPrefix);
                var abstractionNameAppendSuffix = command.ExecutionProject.ParameterValue(AbstractionNameAppendSuffix);

                //Create the abstraction contract
                var abstractionContractNameManagement = NameManagement.Init(contractNameRemovePrefixes, contractNameRemoveSuffixes, abstractionContractNameAppendPrefix, abstractionContractNameAppendSuffix);
                var abstractionContractName = abstractionContractNameManagement.FormatName(logicContract.Name.GenerateCSharpFormattedClassName());

                var abstractionContract = await VisualStudioActions.RefreshCSharpAbstractionContractAsync($"I{abstractionContractName}", logicContract, contractProject, contractFolder)
                                          ?? throw new CodeFactoryException("Could not refresh the abstraction contract. The abstraction cannot be updated.");

                //Create the abstraction class
                var abstractionNameManagement = NameManagement.Init(contractNameRemovePrefixes, contractNameRemoveSuffixes, abstractionNameAppendPrefix, abstractionNameAppendSuffix);
                var abstractionName = abstractionNameManagement.FormatName(logicContract.Name.GenerateCSharpFormattedClassName());

                var abstractionClass = await VisualStudioActions.RefreshAbstractionClass(abstractionName, abstractionContract, logicContract, abstractionProject, abstractionFolder);


            }
            catch (CodeFactoryException cfException)
            {
                MessageBox.Show(cfException.Message, "Automation Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
