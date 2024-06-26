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
    public class RefreshContractImplementation : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Refresh Contract Implementation";
        private static readonly string commandDescription = "Refreshes the class implementation for the target interface contract.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public RefreshContractImplementation(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(RefreshContractImplementation).FullName;

        /// <summary>
        /// Execution project for the command.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// Execution folder for the command.
        /// </summary>
        public static string ExecutionFolder = "ExecutionFolder";

        /// <summary>
        /// The implement project that holds the implementation class to be refreshed.
        /// </summary>
        public static string ImplementationProject = "ImplementationProject";

        /// <summary>
        /// Optional, folder where the implementation class it be refreshed is located.
        /// </summary>
        public static string ImplementationFolder = "ImplementationFolder";

        /// <summary>
        /// Optional, comma separated value list of prefixes to be removed from the implementation contracts name.
        /// </summary>
        public static string RemoveContractPrefixes = "RemoveContractPrefixes";

        /// <summary>
        /// Optional, comma separated value list of the suffixes to be removed from the implementation contract name.
        /// </summary>
        public static string RemoveContractSuffixes = "RemoveContractSuffixes";

        /// <summary>
        /// Optional, the prefix to append to the implementation class name.
        /// </summary>
        public static string ImplementationPrefix = "ImplementationPrefix";

        /// <summary>
        /// Optional, the suffix to append to the implementation class name.
        /// </summary>
        public static string ImplementationSuffix = "ImplementationSuffix";

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            return new ConfigCommand { Name = commandTitle, Category = "Implementation", CommandType = Type,  }
            .UpdateExecutionProject
            (
                new ConfigProject
                {
                    Name = ExecutionProject, 
                    Guidance = "Project that contains the interface that triggers the automation."
                }
                .AddFolder
                (
                    new ConfigFolder
                    {
                        Name = ExecutionFolder,
                        Required = false,
                        Guidance = "Optional, project folder that contains the interface that triggers the automation."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    {
                        Name = RemoveContractPrefixes,
                        Guidance = "Optional, comma separated value list of prefix values to remove from the contract interface name."   
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    {
                        Name = RemoveContractSuffixes,
                        Guidance = "Optional, comma separated value list of suffix values to remove from the contract interface name."   
                    }
                )

            )
            .AddProject
            (
                new ConfigProject
                { 
                    Name = ImplementationProject,
                    Guidance = "Name of the project that hosts the implementation class to be refreshed."
                }
                .AddFolder
                (
                    new ConfigFolder
                    {
                        Name = ImplementationFolder,
                        Required = false,
                        Guidance = "Optional, The project folder the implementation class is hosted in."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = ImplementationPrefix,
                        Guidance = "Optional, the prefix to append to the implementation class name."

                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = ImplementationSuffix,
                        Guidance = "Optional, the suffix to append to the implementation class name."

                    }
                )
            );

            
        }

        /// <summary>
        /// Registers the default configuration with the configuration manager in CodeFactory.
        /// </summary>
        public static void RegisterDefaultConfiguration()
        {
            var command = new RefreshContractImplementation(null, null);
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
                 
                var implementationContract = result.SourceCode?.Interfaces.FirstOrDefault();

                isEnabled = implementationContract != null;

                ConfigCommand command = null;

                if( isEnabled ) 
                {
                    command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                    isEnabled = command != null;
                }

                if(isEnabled) isEnabled = await ImplementationNeedsUpdatesAsync(command,implementationContract);
                
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
               var implementationContract = result.SourceCode?.Interfaces.FirstOrDefault()
                    ?? throw new CodeFactoryException("Could not load the source interface cannot refresh the implementation.");

                var commandConfig = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result)
                              ?? throw new CodeFactoryException("Could not load the configuratin, cannot refresh the implementation");

                var implementationName = GenerateImplementationClassName(commandConfig,implementationContract)
                    ?? throw new CodeFactoryException("Could not determine the implementation class name, cannot refresh the implementation");

                var contractProject = await VisualStudioActions.GetProjectFromConfigAsync(commandConfig.ExecutionProject)
                    ?? throw new CodeFactoryException("Cannot load the contract interface project, cannot refresh the implementation");
                   
                var contractProjectFolder = await VisualStudioActions.GetProjectFolderFromConfigAsync(commandConfig.ExecutionProject,ExecutionFolder);

                var implementationProject = await VisualStudioActions.GetProjectFromConfigAsync(commandConfig.Project(ImplementationProject))
                    ?? throw new CodeFactoryException("Could not load the implementation project, cannot refresh the implementation.");

                var implementationProjectFolder = await VisualStudioActions.GetProjectFolderFromConfigAsync(commandConfig.Project(ImplementationProject),ImplementationFolder);

                await VisualStudioActions.RefreshImplementationAsync(implementationName,implementationContract.Name,implementationProject,contractProject,implementationFolder:implementationProjectFolder,contractFolder: contractProjectFolder);

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
        /// Helper method that determines if the command should be activated. Checks to make sure all the conditions are met to refresh the implementation.
        /// </summary>
        /// <param name="command">The commands configuration.</param>
        /// <param name="interfaceContract">The source interface that is being used to refresh the implementation.</param>
        /// <returns>True if the command should be enabled, false if not.</returns>
        private async Task<bool> ImplementationNeedsUpdatesAsync(ConfigCommand command,CsInterface interfaceContract)
        { 

            var implementationProject = await VisualStudioActions.GetProjectFromConfigAsync(command.Project(ImplementationProject));

            if(implementationProject == null) return false;

            var implementationFolder = await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(ImplementationProject),ImplementationFolder);

            var implementationClassName = GenerateImplementationClassName(command,interfaceContract);

            if(implementationClassName == null) return false;

            var implementationSource = implementationFolder != null
                ? await implementationFolder.FindCSharpSourceByClassNameAsync(implementationClassName,false)
                : await implementationProject.FindCSharpSourceByClassNameAsync(implementationClassName,false);

            if(implementationSource == null) return true;

            var implementationClass = implementationSource?.SourceCode?.Classes?.FirstOrDefault();

            if(implementationClass == null) return false;

            return implementationClass.GetMissingInterfaceMembers().Any();
            
        }

        /// <summary>
        /// Generates the name of the implementation class to be refreshed.
        /// </summary>
        /// <param name="command">The command configuration to use to generate the class name.</param>
        /// <param name="interfaceContract">The interface that implements the implementation class to refresh.</param>
        /// <returns>Formatted name or null if the name cannot be determined.</returns>
        private string GenerateImplementationClassName(ConfigCommand command,CsInterface interfaceContract)
        { 
            if(command == null)  return null;   

            if(interfaceContract == null) return null;

            var removePrefixes = command.ExecutionProject.ParameterValue(RemoveContractPrefixes);
            var removeSuffixes = command.ExecutionProject.ParameterValue(RemoveContractSuffixes);
            var implementationPrefix = command.Project(ImplementationProject).ParameterValue(ImplementationPrefix);
            var implementationSuffix = command.Project(ImplementationProject).ParameterValue(ImplementationSuffix);

            return NameManagement.Init(removePrefixes,removeSuffixes, implementationPrefix, implementationSuffix).FormatName(interfaceContract.Name.GenerateCSharpFormattedClassName());

        }
    }
}
