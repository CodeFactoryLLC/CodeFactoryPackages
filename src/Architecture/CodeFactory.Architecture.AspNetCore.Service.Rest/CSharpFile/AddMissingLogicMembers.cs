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

namespace CodeFactory.Architecture.AspNetCore.Service.Rest.CSharpFile
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class AddMissingLogicMembers : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Add Missing Logic Members";
        private static readonly string commandDescription = "Adds missing contract interface members to the Logic implementation.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public AddMissingLogicMembers(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(AddMissingLogicMembers).FullName;


        /// <summary>
        /// Exection project for the command.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// Execution folder for the command.
        /// </summary>
        public static string ExecutionFolder = "ExecutionFolder";

        /// <summary>
        /// Repositories name prefix
        /// </summary>
        public static string LogicPrefix = "LogicPrefix";
        

        /// <summary>
        /// Repositories name suffix.
        /// </summary>
        public static string LogicSuffix = "LogicSuffix";

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            var config = new ConfigCommand{ CommandType = Type, Name = commandTitle, Category = "Logic",Guidance = "Command is used when updating missing members from a logic implementation." }
            .UpdateExecutionProject
            (
                new ConfigProject
                { 
                    Name = ExecutionProject,
                    Guidance = "The project where the logic class file resides in."
                }
                .AddFolder
                (
                    new ConfigFolder
                    { 
                      Name = ExecutionFolder,
                      Required = false,
                      Guidance = "The target folder the logic class will be found in."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = LogicPrefix,
                        Guidance = "Optional, checks to makes sure the class starts with the provided prefix before considering it a logic."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = LogicSuffix,
                        Guidance = "Optional, checks to makes sure the class starts with the provided suffix before considering it a logic."
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
               var logicClass = result?.SourceCode?.Classes.FirstOrDefault();

               isEnabled = logicClass != null;

                ConfigCommand command = null;

                if( isEnabled ) 
                {
                    command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                    isEnabled = command != null;
                }

                if(isEnabled ) 
                {
                    var logicPrefix = command.ExecutionProject.ParameterValue(LogicPrefix);
                    var logicSuffix = command.ExecutionProject.ParameterValue(LogicSuffix);
                    isEnabled = IsLogicClass(logicClass,logicPrefix,logicSuffix);
                }

                if(isEnabled ) isEnabled = logicClass.GetMissingInterfaceMembers().Any();
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
               
                var logicSource = result.SourceCode;

                if (logicSource == null) return;

                logicSource = await logicSource.AddUsingStatementAsync("Microsoft.Extensions.Logging");
                logicSource = await logicSource.AddUsingStatementAsync("CodeFactory.NDF");

                var logicClass = logicSource.Classes.FirstOrDefault();
                if(logicClass == null) return;

                var missingMembers = logicClass.GetMissingInterfaceMembers();

                if( !missingMembers.Any() ) return;


                string loggerFieldName = "_logger";

                if (!logicClass.Fields.Any(f=>f.Name == loggerFieldName))
                { 
                    SourceFormatter formatter = new SourceFormatter();

                    formatter.AppendCodeLine(2,"/// <summary>");
                    formatter.AppendCodeLine(2,"/// Logger for the class");
                    formatter.AppendCodeLine(2,"/// </summary>");
                    formatter.AppendCodeLine(2,$"private readonly ILogger {loggerFieldName};");
                    formatter.AppendCodeLine(2);
                    logicSource = await logicClass.AddToBeginningAsync(formatter.ReturnSource());
                    logicClass = logicSource.Classes.FirstOrDefault();
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

                var updatedLogicClass = await VisualStudioActions.AddClassMissingMembersAsync(result.SourceCode,logicClass,false,loggerBlock,Microsoft.Extensions.Logging.LogLevel.Information,boundChecks,tryBlock,missingMembers);
                
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
        /// Validation check to make sure the logic class is formatted to correct name.
        /// </summary>
        /// <param name="logicClass">Class to check.</param>
        /// <param name="logicPrefix">The prefix the logic class should start with, this can be null.</param>
        /// <param name="logicSuffix">The suffix the logic class should end with, this can be null.</param>
        /// <returns>True class name is formatted correctly, false if not.</returns>
        private bool IsLogicClass(CsClass logicClass,string logicPrefix, string logicSuffix)
        { 
            
            bool islogicClass = false;

            if (logicClass != null) islogicClass = true;

            if(islogicClass & logicPrefix != null) islogicClass = logicClass.Name.StartsWith(logicPrefix);

            if(islogicClass & logicSuffix != null) islogicClass = logicClass.Name.EndsWith(logicSuffix);
            
            return islogicClass;
        }

        #endregion
    }
}
