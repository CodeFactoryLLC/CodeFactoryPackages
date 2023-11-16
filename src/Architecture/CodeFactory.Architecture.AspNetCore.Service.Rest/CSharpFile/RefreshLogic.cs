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

namespace CodeFactory.Architecture.AspNetCore.Service.Rest.CSharpFile
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class RefreshLogic : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Refresh Logic";
        private static readonly string commandDescription = "Refreshes the logic implementation for the target logic interface.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public RefreshLogic(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(RefreshLogic).FullName;

        /// <summary>
        /// Exection project for the command.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// Execution folder for the command.
        /// </summary>
        public static string ExecutionFolder = "ExecutionFolder";

        /// <summary>
        /// The logic project that holds the logic class to be refreshed.
        /// </summary>
        public static string LogicProject = "LogicProject";

        /// <summary>
        /// Optional, folder where the logic class it be refreshed is located.
        /// </summary>
        public static string LogicFolder = "LogicFolder";

        /// <summary>
        /// Optional, comma seperated value list of prefixes to be removed from the logic contracts name.
        /// </summary>
        public static string RemovePrefixes = "RemovePrefixes";

        /// <summary>
        /// Optional, comma seperated value list of the suffixes to be removed from the logic contract name.
        /// </summary>
        public static string RemoveSuffixes = "RemoveSuffixes";

        /// <summary>
        /// Optional, the prefix to append to the logic class name.
        /// </summary>
        public static string LogicPrefix = "LogicPrefix";

        /// <summary>
        /// Optional, the suffix to append to the logic class name.
        /// </summary>
        public static string LogicSuffix = "LogicSuffix";

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            return new ConfigCommand { Name = commandTitle, Category = "Logic", CommandType = Type,  }
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
                        Name = RemovePrefixes,
                        Guidance = "Optional, comma seperated value list of prefix values to remove from the interface name."   
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    {
                        Name = RemoveSuffixes,
                        Guidance = "Optional, comma seperated value list of suffix values to remove from the interface name."   
                    }
                )

            )
            .AddProject
            (
                new ConfigProject
                { 
                    Name = LogicProject,
                    Guidance = "Name of the project that hosts the logic class to be refreshed."
                }
                .AddFolder
                (
                    new ConfigFolder
                    {
                        Name = LogicFolder,
                        Required = false,
                        Guidance = "Optional, The project folder the logic class is hosted in."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = LogicPrefix,
                        Guidance = "Optional, the prefix to append to the logic class name."

                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = LogicSuffix,
                        Guidance = "Optional, the suffix to append to the logic class name."

                    }
                )
            );

            
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
                 
                var logicContract = result.SourceCode?.Interfaces.FirstOrDefault();

                isEnabled = logicContract != null;

                ConfigCommand command = null;

                if( isEnabled ) 
                {
                    command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                    isEnabled = command != null;
                }

                if(isEnabled) isEnabled = await LogicNeedsUpdatesAsync(command,logicContract);
                
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
               var logicContract = result.SourceCode?.Interfaces.FirstOrDefault()
                    ?? throw new CodeFactoryException("Could not load the source interface cannot refresh the logic.");

                var commandConfig = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result)
                              ?? throw new CodeFactoryException("Could not load the configuratin, cannot refresh the logic");

                var logicName = GenerateLogicClassName(commandConfig,logicContract)
                    ?? throw new CodeFactoryException("Could not determine the logic class name, cannot refresh the logic");

                var contractProject = await VisualStudioActions.GetProjectFromConfigAsync(commandConfig.ExecutionProject)
                    ?? throw new CodeFactoryException("Cannot load the contract interface project, cannot refresh the logic");
                   
                var contractProjectFolder = await VisualStudioActions.GetProjectFolderFromConfigAsync(commandConfig.ExecutionProject,ExecutionFolder);

                var logicProject = await VisualStudioActions.GetProjectFromConfigAsync(commandConfig.Project(LogicProject))
                    ?? throw new CodeFactoryException("Could not load the logic project, cannot refresh the logic.");

                var logicProjectFolder = await VisualStudioActions.GetProjectFolderFromConfigAsync(commandConfig.Project(LogicProject),LogicFolder);

                await VisualStudioActions.RefreshLogicAsync(logicName,logicContract.Name,logicProject,contractProject,logicFolder:logicProjectFolder,contractFolder: contractProjectFolder);

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
        /// Helper method that determines if the command should be activated. Checks to make sure all the conditions are met to refresh the logic.
        /// </summary>
        /// <param name="command">The commands configuration.</param>
        /// <param name="interfaceContract">The source interface that is being used to refresh the logic.</param>
        /// <returns>True if the command should be enabled, false if not.</returns>
        private async Task<bool> LogicNeedsUpdatesAsync(ConfigCommand command,CsInterface interfaceContract)
        { 

            var logicProject = await VisualStudioActions.GetProjectFromConfigAsync(command.Project(LogicProject));

            if(logicProject == null) return false;

            var logicFolder = await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(LogicProject),LogicFolder);

            var logicClassName = GenerateLogicClassName(command,interfaceContract);

            if(logicClassName == null) return false;

            var logicSource = logicFolder != null
                ? await logicFolder.FindCSharpSourceByClassNameAsync(logicClassName,false)
                : await logicProject.FindCSharpSourceByClassNameAsync(logicClassName,false);

            if(logicSource == null) return true;

            var logicClass = logicSource?.SourceCode?.Classes?.FirstOrDefault();

            if(logicClass == null) return false;

            return logicClass.GetMissingInterfaceMembers().Any();
            
        }

        /// <summary>
        /// Generates the name of the logic class to be refreshed.
        /// </summary>
        /// <param name="command">The command configuration to use to generate the class name.</param>
        /// <param name="interfaceContract">The interface that implements the logic class to refresh.</param>
        /// <returns>Formatted name or null if the name cannot be determined.</returns>
        private string GenerateLogicClassName(ConfigCommand command,CsInterface interfaceContract)
        { 
            if(command == null)  return null;   

            if(interfaceContract == null) return null;

            var removePrefixes = command.ExecutionProject.ParameterValue(RemovePrefixes);
            var removeSuffixes = command.ExecutionProject.ParameterValue(RemoveSuffixes);
            var logicPrefix = command.Project(LogicProject).ParameterValue(LogicPrefix);
            var logicSuffix = command.Project(LogicProject).ParameterValue(LogicSuffix);

            return NameManagement.Init(removePrefixes,removeSuffixes, logicPrefix, logicSuffix).FormatName(interfaceContract.Name.GenerateCSharpFormattedClassName());

        }
    }
}
