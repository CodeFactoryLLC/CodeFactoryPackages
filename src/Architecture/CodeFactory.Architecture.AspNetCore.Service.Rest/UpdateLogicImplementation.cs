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

namespace CodeFactory.Architecture.AspNetCore.Service.Rest
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class UpdateLogicImplementation : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Update Logic Implementation";
        private static readonly string commandDescription = "Clones changes from the respository contract to the logic contract and refreshes the logic implementation.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public UpdateLogicImplementation(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(UpdateLogicImplementation).FullName;

        /// <summary>
        /// Exection project for the command.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// Execution folder for the command.
        /// </summary>
        public static string ExecutionFolder = "ExecutionFolder";

        /// <summary>
        /// List of prefixes to be removed from the interface definition.
        /// </summary>
        public static string RepoPrefix = "RepoPrefix";

        /// <summary>
        /// List of suffixes to be removed from the interface definition.
        /// </summary>
        public static string RepoSuffix = "RepoSuffix";

        /// <summary>
        /// The project the logic contract can be found in.
        /// </summary>
        public static string LogicContractProject = "LogicContractProject";

        /// <summary>
        /// The project folder the logic contract can be found in.
        /// </summary>
        public static string LogicContractProjectFolder = "LogicContractProjectFolder";

        /// <summary>
        /// The project the logic implementation can be found in.
        /// </summary>
        public static string LogicProject = "LogicProject";

        /// <summary>
        /// The project folder the logic implementation can be found in.
        /// </summary>
        public static string LogicProjectFolder = "LogicProjectFolder";

        /// <summary>
        /// Logic name prefix
        /// </summary>
        public static string LogicPrefix = "LogicPrefix";
        
        /// <summary>
        /// Logic name suffix.
        /// </summary>
        public static string LogicSuffix = "LogicSuffix";

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            var config = new ConfigCommand{ Name = commandTitle, CommandType = Type, Category = "Logic", Guidance = commandDescription};

            config.UpdateExecutionProject
            (
                new ConfigProject
                { 
                    Name = ExecutionProject,
                    Guidance = "The target project the command is executed from."
                }
                .AddFolder
                (
                    new ConfigFolder
                    {
                        Name= ExecutionFolder,
                        Required = false,
                        Guidance = "The target folder where source interface configurations are located."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = RepoPrefix,
                        Guidance = "List of prefixes to be removed from the beginning of the contract name."
                        
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = RepoSuffix,
                        Guidance = "List of suffixes to be removed from the end of the contract name."
                        
                    }
                )
                
            )
            .AddProject
            (
                new ConfigProject
                { 
                    Name = LogicContractProject,
                    Guidance = "The target project the logic contract is to be located in."
                }
                .AddFolder
                (
                    new ConfigFolder
                    {
                        Name = LogicContractProjectFolder,
                        Required = false,
                        Guidance = "The target folder where logic contract interfaces are located."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = LogicPrefix,
                        Guidance = "The prefix to assign to the name of the logic contract."
                        
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = LogicSuffix,
                        Guidance = "The suffix to assign to the name of the logic contract."
                        
                    }
                )
            )
            .AddProject
            (
                new ConfigProject
                { 
                    Name = LogicProject,
                    Guidance = "The target project the logic  is to be located in."
                }
                .AddFolder
                (
                    new ConfigFolder
                    {
                        Name = LogicProjectFolder,
                        Required = false,
                        Guidance = "The target folder where logic is located."
                    }
                )
            );

            return config;
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
                var repoInterface = result?.SourceCode?.Interfaces.FirstOrDefault();

               isEnabled = repoInterface != null;

                ConfigCommand command = null;

                if( isEnabled ) 
                {
                    command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                    isEnabled = command != null;
                }

                if(isEnabled ) 
                {
                    var repoPrefix = command.ExecutionProject.ParameterValue(RepoPrefix);
                    var repoSuffix = command.ExecutionProject.ParameterValue(RepoSuffix);
                    isEnabled = IsRepositoryContract(repoInterface,repoPrefix,repoSuffix);
                }
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
                              ?? throw new CodeFactoryException("Could not load the commands configuration cannot update the logic implementation.");

                var repoContract = result.SourceCode?.Interfaces.FirstOrDefault()
                    ?? throw new CodeFactoryException("Could not load the repository contract cannot update the logic implementation.");

                var logicContractProject = await VisualStudioActions.GetProjectFromConfigAsync(command.Project(LogicContractProject));

                var logicContractProjectFolder = await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(LogicContractProject),LogicContractProjectFolder);

                var repoPrefix = command.ExecutionProject.ParameterValue(RepoPrefix);
                var repoSuffix = command.ExecutionProject.ParameterValue(RepoSuffix);

                var logicPrefix = command.Project(LogicContractProject)?.ParameterValue(LogicPrefix);
                var logicSuffix = command.Project(LogicContractProject)?.ParameterValue(LogicSuffix);

                var removePrefixes = repoPrefix == null ? null: new List<string>(repoPrefix.Split(','));
                var removeSuffixes = repoSuffix == null ? null: new List<string>(repoSuffix.Split(','));
                var nameManagement = NameManagement.Init(removePrefixes,removeSuffixes,logicPrefix,logicSuffix);
                
                var logicContract = await VisualStudioActions.CloneInterfaceAsync(repoContract,false,logicContractProject,
                    logicContractProjectFolder,nameManagement,"Logic contract implementation.");
                
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while executing the solution explorer C# document command {commandTitle}. ",
                    unhandledError);

            }

        }

        #endregion

        /// <summary>
        /// Helper method that checks to make sure the interface meets the implementation standard.
        /// </summary>
        /// <param name="repoInterface">Interface to check.</param>
        /// <param name="repoPrefix">repository prefix to check</param>
        /// <param name="repoSuffix">repository suffix to check</param>
        /// <returns>True if a repository contract false if not.</returns>
        private bool IsRepositoryContract(CsInterface repoInterface,string repoPrefix, string repoSuffix)
        { 
            
            bool isRepoInterface = false;

            if (repoInterface != null) isRepoInterface = true;

            if(isRepoInterface & repoPrefix != null) isRepoInterface = repoInterface.Name.StartsWith($"I{repoPrefix}");

            if(isRepoInterface & repoSuffix != null) isRepoInterface = repoInterface.Name.EndsWith(repoSuffix);
            
            return isRepoInterface;
        }
    }

}
