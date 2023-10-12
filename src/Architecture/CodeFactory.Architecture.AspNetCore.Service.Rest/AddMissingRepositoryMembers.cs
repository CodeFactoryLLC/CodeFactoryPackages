using CodeFactory.Automation.NDF.Logic;
using CodeFactory.Automation.NDF.Logic.Data.Sql;
using CodeFactory.Automation.NDF.Logic.Data.Sql.EF;
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

namespace CodeFactory.Architecture.AspNetCore.Service.Rest
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class AddMissingRepositoryMembers : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Add Missing Repository Members";
        private static readonly string commandDescription = "Adds missing contract interface members to the repository implementation.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public AddMissingRepositoryMembers(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(AddMissingRepositoryMembers).FullName;


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
        public static string RepositoryPrefix = "RepositoryPrefix";
        

        /// <summary>
        /// Repositories name suffix.
        /// </summary>
        public static string RepositorySuffix = "RepositorySuffix";

        /// <summary>
        /// The name of the entity framework context to use with repository methods.
        /// </summary>
        public static string ContextName = "ContextName";

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            var config = new ConfigCommand{ CommandType = Type, Name = commandTitle, Category = "Repository",Guidance = "Command is used when updating missing members from a repository implementation." }
            .UpdateExecutionProject
            (
                new ConfigProject
                { 
                    Name = ExecutionProject,
                    Guidance = "The project where the repository class file resides in."
                }
                .AddFolder
                (
                    new ConfigFolder
                    { 
                      Name = ExecutionFolder,
                      Required = false,
                      Guidance = "The target folder the respoitory class will be found in."
                    }
                )
                 .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = ContextName,
                        Guidance = "The name of the entity framework context to use with repository methods."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = RepositoryPrefix,
                        Guidance = "Optional, checks to makes sure the class starts with the provided prefix before considering it a repository."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = RepositorySuffix,
                        Guidance = "Optional, checks to makes sure the class starts with the provided suffix before considering it a repository."
                    }
                )
               
            );

            return config;
        }
        #endregion

        #region Overrides of VsCommandBase<IVsCSharpDocument>

        /// <summary>
        /// Validation Repository that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsCSharpSource result)
        {
            //Result that determines if the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
               var repoClass = result?.SourceCode?.Classes.FirstOrDefault();

               isEnabled = repoClass != null;

                ConfigCommand command = null;

                if( isEnabled ) 
                {
                    command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                    isEnabled = command != null;
                }

                if(isEnabled ) 
                {
                    var repoPrefix = command.ExecutionProject.ParameterValue(RepositoryPrefix);
                    var repoSuffix = command.ExecutionProject.ParameterValue(RepositorySuffix);
                    isEnabled = IsRepositoryClass(repoClass,repoPrefix,repoSuffix);
                }

                if(isEnabled ) isEnabled = repoClass.GetMissingInterfaceMembers().Any();
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
                var repoClass = result.SourceCode?.Classes.FirstOrDefault();

                if(repoClass == null) return;

                var missingMembers = repoClass.GetMissingInterfaceMembers();

                if( !missingMembers.Any() ) return;


                var command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                if(command == null)return;

                var efContextName = command.ExecutionProject.ParameterValue(ContextName);

                if (string.IsNullOrEmpty(efContextName))
                    throw new CodeFactoryException("The entity framework context name could not be loaded from the configuration cannot add missing repository members.");
                
                var loggerBlock = new LoggerBlockNDFLogger("_logger");

                var catchBlocks = new List<ICatchBlock>
                { 
                    new CatchBlockManagedExceptionNDFException(loggerBlock),
                    new CatchBlockDBUpdateExceptionNDFException(loggerBlock),
                    new CatchBlockSqlExceptionNDFException(loggerBlock),
                    new CatchBlockExceptionNDFException(loggerBlock)
                };

                var boundChecks = new List<IBoundsCheckBlock>
                { 
                  
                    new BoundsCheckBlockNullNDFException(true,loggerBlock),
                    new BoundsCheckBlockStringNDFException(true,loggerBlock)
                };

                var tryBlock = new TryBlockRepositoryEF(efContextName,loggerBlock,catchBlocks);

                var updatedRepoClass = await VisualStudioActions.AddClassMissingMembersAsync(result.SourceCode,repoClass,false,loggerBlock,Microsoft.Extensions.Logging.LogLevel.Information,boundChecks,tryBlock,missingMembers);
                
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
        /// Checks the name of the class to confirm it conforms to the repository class naming standard.
        /// </summary>
        /// <param name="repoClass">Class to be checked.</param>
        /// <param name="repoPrefix">Expected prefix to be implemented, this can be null.</param>
        /// <param name="repoSuffix">Expected suffix to be implemented, this can be null.</param>
        /// <returns>True if valid class name, false if not.</returns>
        private bool IsRepositoryClass(CsClass repoClass,string repoPrefix, string repoSuffix)
        { 
            
            bool isRepoClass = false;

            if (repoClass != null) isRepoClass = true;

            if(isRepoClass & repoPrefix != null) isRepoClass = repoClass.Name.StartsWith(repoPrefix);

            if(isRepoClass & repoSuffix != null) isRepoClass = repoClass.Name.EndsWith(repoSuffix);
            
            return isRepoClass;
        }

        #endregion
    }

}
