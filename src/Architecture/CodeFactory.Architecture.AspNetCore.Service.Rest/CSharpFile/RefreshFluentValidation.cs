using CodeFactory.Automation.Standard.Logic.FluentValidation;
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
    public class RefreshFluentValidation : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Refresh Validation";
        private static readonly string commandDescription = "Refreshes the fluent validation class that supports the class implemented in this source file.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public RefreshFluentValidation(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(RefreshFluentValidation).FullName;

        /// <summary>
        /// The execution project that contains the definition of the model to refresh validation in.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// The execution project folder the definition of the mode to refresh validation is in, this is optional.
        /// </summary>
        public static string ExecutionFolder = "ExecutionFolder";


        /// <summary>
        /// The prefix to assign to the name of a models validation class.
        /// </summary>
        public static string ModelValidatorPrefix = "ModelValidatorPrefix";

        /// <summary>
        /// The suffix to assign to the name of a models validation class.
        /// </summary>
        public static string ModelValidatorSuffix = "ModelValidatorSuffix";
        
        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            return new ConfigCommand
                    { Category = "ModelValidation", Name = nameof(RefreshFluentValidation), CommandType = Type }
                .UpdateExecutionProject
                (
                    new ConfigProject
                        {
                            Name = ExecutionProject,
                            Guidance = "Enter the fully project name where models have validation to refresh."
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
                                Name= ModelValidatorPrefix,
                                Guidance = "The prefix to assign to the name of a models validation class."
                            }
                        )
                        .AddParameter
                        (
                            new ConfigParameter
                            {
                                Name= ModelValidatorSuffix,
                                Guidance = "The suffix to assign to the name of a models validation class."
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
                isEnabled = result.IsLoaded;

                if (isEnabled) isEnabled = result.SourceCode.Classes.Any();

                if (isEnabled) 
                {
                    var command = (await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                               ?? await ConfigManager.LoadCommandByProjectAsync(Type, result))
                              ?? throw new CodeFactoryException("Could not load the command configuration, cannot refresh the validation.");

                    isEnabled = command != null;
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
                var sourceClass = result.SourceCode.Classes.FirstOrDefault();

                if(sourceClass == null) return;

                var command = (await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                               ?? await ConfigManager.LoadCommandByProjectAsync(Type, result))
                              ?? throw new CodeFactoryException("Could not load the command configuration, cannot refresh the validation.");

                var sourceProject =
                    await VisualStudioActions.GetProjectFromConfigAsync(command.ExecutionProject)
                    ?? throw new CodeFactoryException("Cannot load the execution project, cannot refresh the validation.");

                var sourceFolder =
                    await VisualStudioActions.GetProjectFolderFromConfigAsync(command.ExecutionProject, ExecutionFolder);

                var validatorPrefix = command.ExecutionProject.ParameterValue(ModelValidatorPrefix);

                var validatorSuffix = command.ExecutionProject.ParameterValue(ModelValidatorSuffix);

                string removePrefixes = null;
                string removeSuffixes = null;

                NameManagement nameManagement = null;

                if(!string.IsNullOrEmpty(validatorPrefix) | !string.IsNullOrEmpty(validatorSuffix)) nameManagement = NameManagement.Init(removePrefixes,removeSuffixes, validatorPrefix, validatorSuffix);

                var validationClass = VisualStudioActions.RefreshValidationAsync(sourceClass, sourceProject,sourceFolder,nameManagement);
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
    }

}
