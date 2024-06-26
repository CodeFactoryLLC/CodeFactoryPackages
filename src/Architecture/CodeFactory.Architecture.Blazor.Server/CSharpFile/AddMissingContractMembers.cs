using CodeFactory.Automation.NDF.Logic;
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
    public class AddMissingContractMembers : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Add Missing Contract Members";
        private static readonly string commandDescription = "Adds missing contract interface members from the implementation.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public AddMissingContractMembers(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(AddMissingContractMembers).FullName;


        /// <summary>
        /// Exection project for the command.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// Execution folder for the command.
        /// </summary>
        public static string ExecutionFolder = "ExecutionFolder";

        /// <summary>
        /// The project name starts with the following prefix.
        /// </summary>
        public static string ProjectNamePrefix = "ProjectNamePrefix";
        

        /// <summary>
        /// The project name ends with the following suffix.
        /// </summary>
        public static string ProjectNameSuffix = "ProjectNameSuffix";

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            var config = new ConfigCommand
            { CommandType = Type, 
                Name = nameof(AddMissingContractMembers), 
                Category = "Contract Implementation",
                Guidance = "Command is used when updating missing members from a project implementation." 
            }
            .UpdateExecutionProject
            (
                new ConfigProject
                { 
                    Name = ExecutionProject,
                    Guidance = "The project where the project class file resides in."
                }
                .AddFolder
                (
                    new ConfigFolder
                    { 
                      Name = ExecutionFolder,
                      Required = false,
                      Guidance = "The target folder the project class will be found in."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = ProjectNamePrefix,
                        Guidance = "Optional, checks to makes sure the class starts with the provided prefix before adding missing contract members."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = ProjectNameSuffix,
                        Guidance = "Optional, checks to makes sure the class starts with the provided suffix before adding missing contract members."
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
            var command = new AddMissingContractMembers(null, null);
            var config = command.LoadExternalConfigDefinition();
            config?.RegisterCommandWithDefaultConfiguration();
        }
        #endregion

        #region Overrides of VsCommandBase<IVsCSharpDocument>

        /// <summary>
        /// Validation project that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsCSharpSource result)
        {
            //Result that determines if the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
               var projectClass = result?.SourceCode?.Classes.FirstOrDefault();

               isEnabled = projectClass != null;

                ConfigCommand command = null;

                if( isEnabled ) 
                {
                    command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                    isEnabled = command != null;
                }

                if(isEnabled ) 
                {
                    var projectPrefix = command.ExecutionProject.ParameterValue(ProjectNamePrefix);
                    var projectSuffix = command.ExecutionProject.ParameterValue(ProjectNameSuffix);
                    isEnabled = IsProjectClass(projectClass,projectPrefix,projectSuffix);
                }

                if(isEnabled ) isEnabled = projectClass.GetMissingInterfaceMembers().Any();
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
               
                var projectSource = result.SourceCode;

                if (projectSource == null) return;

                projectSource = await projectSource.AddUsingStatementAsync("Microsoft.Extensions.Logging");
                projectSource = await projectSource.AddUsingStatementAsync("CodeFactory.NDF");

                var projectClass = projectSource.Classes.FirstOrDefault();
                if(projectClass == null) return;

                var missingMembers = projectClass.GetMissingInterfaceMembers();

                if( !missingMembers.Any() ) return;


                string loggerFieldName = "_logger";

                if (!projectClass.Fields.Any(f=>f.Name == loggerFieldName))
                { 
                    SourceFormatter formatter = new SourceFormatter();

                    formatter.AppendCodeLine(2,"/// <summary>");
                    formatter.AppendCodeLine(2,"/// Logger for the class");
                    formatter.AppendCodeLine(2,"/// </summary>");
                    formatter.AppendCodeLine(2,$"private readonly ILogger {loggerFieldName};");
                    formatter.AppendCodeLine(2);
                    projectSource = await projectClass.AddToBeginningAsync(formatter.ReturnSource());
                    projectClass = projectSource.Classes.FirstOrDefault();
                }

       
                var command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                if(command == null)return;


                var loggerBlock = new LoggerBlockNDFLogger(loggerFieldName);

                var catchBlocks = new List<ICatchBlock>
                { 
                    new CatchBlockManagedExceptionNDFException(loggerBlock),
                    new CatchBlockExceptionNDFException(loggerBlock)
                };

                var boundChecks = new List<IBoundsCheckBlock>
                { 
                    new BoundsCheckBlockStringNDFException(true,loggerBlock),
                    new BoundsCheckBlockNullNDFException(true,loggerBlock)
                };

                var tryBlock = new TryBlockStandard(loggerBlock,catchBlocks);

                var updatedProjectClass = await VisualStudioActions.AddClassMissingMembersAsync(result.SourceCode,projectClass,false,loggerBlock,Microsoft.Extensions.Logging.LogLevel.Information,boundChecks,tryBlock,missingMembers);
                
            }
            catch (CodeFactoryException cfException)
            { 
                MessageBox.Show(cfException.Message,"CodeFactory Error",MessageBoxButton.OK,MessageBoxImage.Error);    
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while executing the solution explorer C# document command {commandTitle}. ",
                    unhandledError);

            }

        }

        /// <summary>
        /// Validation check to make sure the project class is formatted to correct name.
        /// </summary>
        /// <param name="projectClass">Class to check.</param>
        /// <param name="projectPrefix">The prefix the project class should start with, this can be null.</param>
        /// <param name="projectSuffix">The suffix the project class should end with, this can be null.</param>
        /// <returns>True class name is formatted correctly, false if not.</returns>
        private bool IsProjectClass(CsClass projectClass,string projectPrefix, string projectSuffix)
        { 
            
            bool isprojectClass = false;

            if (projectClass != null) isprojectClass = true;

            if(isprojectClass & projectPrefix != null) isprojectClass = projectClass.Name.StartsWith(projectPrefix);

            if(isprojectClass & projectSuffix != null) isprojectClass = projectClass.Name.EndsWith(projectSuffix);
            
            return isprojectClass;
        }

        #endregion
    }
}
