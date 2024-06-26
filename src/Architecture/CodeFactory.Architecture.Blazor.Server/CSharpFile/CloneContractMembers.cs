using CodeFactory.Automation.NDF.Logic.General;
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

namespace CodeFactory.Architecture.Blazor.Server.CSharpFile
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class CloneContractMembers : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Clone Contract Members";
        private static readonly string commandDescription = "Clones the interface members to a new interface and implements the interface.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public CloneContractMembers(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(CloneContractMembers).FullName;

        /// <summary>
        /// Execution project for the command.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// Execution folder for the command.
        /// </summary>
        public static string ExecutionFolder = "ExecutionFolder";

        /// <summary>
        /// List of prefixes to be removed from the interface definition.
        /// </summary>
        public static string RemoveContractPrefix = "RemoveContractPrefix";

        /// <summary>
        /// List of suffixes to be removed from the interface definition.
        /// </summary>
        public static string RemoveContractSuffix = "ContractSuffix";

        /// <summary>
        /// The project the implementation contract can be found in.
        /// </summary>
        public static string ImplementationContractProject = "ImplementationContractProject";

        /// <summary>
        /// The project folder the implementation contract can be found in.
        /// </summary>
        public static string ImplementationContractProjectFolder = "ImplementationContractProjectFolder";

        /// <summary>
        /// The project the implementation implementation can be found in.
        /// </summary>
        public static string ImplementationProject = "ImplementationProject";

        /// <summary>
        /// The project folder the implementation implementation can be found in.
        /// </summary>
        public static string ImplementationProjectFolder = "ImplementationProjectFolder";

        /// <summary>
        /// Implementation name prefix
        /// </summary>
        public static string ImplementationPrefix = "ImplementationPrefix";
        
        /// <summary>
        /// Implementation name suffix.
        /// </summary>
        public static string ImplementationSuffix = "ImplementationSuffix";

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            var config = new ConfigCommand
            { 
                Name = nameof(CloneContractMembers), 
                CommandType = Type, 
                Category = "Implementation", 
                Guidance = commandDescription};

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
                        Name = RemoveContractPrefix,
                        Guidance = "List of prefixes to be removed from the beginning of the source contracts name."
                        
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = RemoveContractSuffix,
                        Guidance = "List of suffixes to be removed from the end of the source contracts name."
                        
                    }
                )
                
            )
            .AddProject
            (
                new ConfigProject
                { 
                    Name = ImplementationContractProject,
                    Guidance = "The target project the implementation contract is to be located in."
                }
                .AddFolder
                (
                    new ConfigFolder
                    {
                        Name = ImplementationContractProjectFolder,
                        Required = false,
                        Guidance = "The target folder where implementation contract interfaces are located."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = ImplementationPrefix,
                        Guidance = "The prefix to assign to the name of the implementation contract."
                        
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = ImplementationSuffix,
                        Guidance = "The suffix to assign to the name of the implementation contract."
                        
                    }
                )
            )
            .AddProject
            (
                new ConfigProject
                { 
                    Name = ImplementationProject,
                    Guidance = "The target project the implementation  is to be located in."
                }
                .AddFolder
                (
                    new ConfigFolder
                    {
                        Name = ImplementationProjectFolder,
                        Required = false,
                        Guidance = "The target folder where implementation is located."
                    }
                )
            );

            return config;
        }

        /// <summary>
        /// Registers the default configuration with the configuration manager in CodeFactory.
        /// </summary>
        public static void RegisterDefaultConfiguration()
        {
            var command = new CloneContractMembers(null, null);
            var config = command.LoadExternalConfigDefinition();
            config?.RegisterCommandWithDefaultConfiguration();
        }
        #endregion

        #region Overrides of VsCommandBase<IVsCSharpDocument>

        /// <summary>
        /// Validation implementation that will determine if this command should be enabled for execution.
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
                    var repoPrefix = command.ExecutionProject.ParameterValue(RemoveContractPrefix);
                    var repoSuffix = command.ExecutionProject.ParameterValue(RemoveContractSuffix);
                    isEnabled = IsSourceContract(repoInterface,repoPrefix,repoSuffix);
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
                              ?? throw new CodeFactoryException("Could not load the commands configuration cannot update the implementation implementation.");

                var repoContract = result.SourceCode?.Interfaces.FirstOrDefault()
                    ?? throw new CodeFactoryException("Could not load the repository contract cannot update the implementation implementation.");

                var implementationContractProject = await VisualStudioActions.GetProjectFromConfigAsync(command.Project(ImplementationContractProject))
                    ?? throw new CodeFactoryException("The implementation contract project could not be loaded, cannot refresh the implementation class.");

                var implementationContractProjectFolder = await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(ImplementationContractProject),ImplementationContractProjectFolder);

                var implementationProject = await VisualStudioActions.GetProjectFromConfigAsync(command.Project(ImplementationProject))
                    ?? throw new CodeFactoryException("The implementation project could not be loaded, cannot refresh the implementation class.");

                var implementationProjectFolder = await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(ImplementationProject),ImplementationProjectFolder);

                var repoRemovePrefix = command.ExecutionProject.ParameterValue(RemoveContractPrefix);
                var repoRemoveSuffix = command.ExecutionProject.ParameterValue(RemoveContractSuffix);

                var implementationPrefix = command.Project(ImplementationContractProject)?.ParameterValue(ImplementationPrefix);
                var implementationSuffix = command.Project(ImplementationContractProject)?.ParameterValue(ImplementationSuffix);


                var implementationName = NameManagement.Init(repoRemovePrefix,repoRemoveSuffix,implementationPrefix,implementationSuffix).FormatName(repoContract.Name.GenerateCSharpFormattedClassName());
                
                var implementationContract = await VisualStudioActions.CloneInterfaceAsync($"I{implementationName}", repoContract,false,implementationContractProject,
                    implementationContractProjectFolder,"Implementation contract implementation.");

                var implementationClass = await VisualStudioActions.RefreshImplementationAsync(implementationName,$"I{implementationName}",implementationProject,implementationContractProject,implementationFolder:implementationProjectFolder,contractFolder:implementationContractProjectFolder);
                
            }
            catch (CodeFactoryException codeFactoryError)
            {
                MessageBox.Show(codeFactoryError.Message, "Automation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while executing the solution explorer C# document command {commandTitle}. ",
                    unhandledError);

            }

        }

        #endregion

        /// <summary>
        /// Helper method that checks to make sure the source interface meets the implementation standard.
        /// </summary>
        /// <param name="sourceInterface">Interface to check.</param>
        /// <param name="sourcePrefix">source contract prefix to check</param>
        /// <param name="sourceSuffix">source contract suffix to check</param>
        /// <returns>True if a source contract false if not.</returns>
        private bool IsSourceContract(CsInterface sourceInterface,string sourcePrefix, string sourceSuffix)
        { 
            
            bool isContractInterface = false;

            if (sourceInterface != null) isContractInterface = true;

            if(isContractInterface & sourcePrefix != null) isContractInterface = sourceInterface.Name.StartsWith($"I{sourcePrefix}");

            if(isContractInterface & sourceSuffix != null) isContractInterface = sourceInterface.Name.EndsWith(sourceSuffix);
            
            return isContractInterface;
        }
    }

}
