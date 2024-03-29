﻿using CodeFactory.Automation.NDF.Logic.Data.Sql.EF;
using CodeFactory.Automation.NDF.Logic;
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
using CodeFactory.Automation.NDF.Logic.Testing.MSTest;
using CodeFactory.Automation.NDF.Logic.General;

namespace CodeFactory.Architecture.Blazor.Server.CSharpFile
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class RefreshEFRepository : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Refresh EF Repository";
        private static readonly string commandDescription = "Refreshes the EF repository and models implementation.";


#pragma warning disable CS1998

        /// <inheritdoc />
        public RefreshEFRepository(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(RefreshEFRepository).FullName;

        /// <summary>
        /// The execution project that contains the definition of the entity framework entity models.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// The execution project folder the entity framework models are stored in. This is optional and only used when entities are stored in a target folder.
        /// </summary>
        public static string ExecutionModelFolder = "ExecutionModelFolder";

        /// <summary>
        /// The project where application level entities will be created and stored.
        /// </summary>
        public static string EntityProject = "EntityProject";

        /// <summary>
        /// The target folder where application entities will be created and stored. This is optional and only used when entities are stored in a target folder.
        /// </summary>
        public static string EntityFolder = "EntityFolder";

        /// <summary>
        /// The repository project where repositories will be created or updated in.
        /// </summary>
        public static string RepoProject = "RepoProject";

        /// <summary>
        /// The target folder where repositories will be created and stored. This is optional and only used when repositories are stored in a target folder.
        /// </summary>
        public static string RepoFolder = "RepoFolder";

        /// <summary>
        /// The target project that will store the interface contract definition for the created and updated repositories. 
        /// </summary>
        public static string RepoContractProject = "RepoContractProject";

        /// <summary>
        /// The target folder where interface contract definitions for repositories is stored. This is optional and only used with interface contracts for repositories stored in the target folder.
        /// </summary>
        public static string RepoContractFolder = "RepoContractFolder";

        /// <summary>
        /// The target project to generate and update integration tests that support the repository.
        /// </summary>
        public static string IntegrationTestProject = "IntegrationTestProject";

        /// <summary>
        /// The target folder where integration tests will be stored. This is optional and only used with integration tests that are to be stored in the target folder.
        /// </summary>
        public static string IntegrationTestFolder = "IntegrationTestFolder";

        /// <summary>
        /// The name of the entity framework context class.
        /// </summary>
        public static string EFContextClassName = "EFContextClassName";

        /// <summary>
        /// Optional, list of prefixes seperated by comma to remove from the name of the entity framework entity.
        /// </summary>
        public static string EFEntityRemovePrefixes = "EFEntityRemovePrefixes";

        /// <summary>
        /// Optional, list of prefixes seperated by comma to remove from the name of the entity framework entity.
        /// </summary>
        public static string EFEntityRemoveSuffixes = "EFEntityRemoveSuffixes";
        
        /// <summary>
        /// Optional, prefix to assign to a repository when creating.
        /// </summary>
        public static string RepositoryPrefix = "RepositoryPrefix";

        /// <summary>
        /// Optional, suffix to assign to a repository when creating.
        /// </summary>
        public static string RepositorySuffix = "RepositorySuffix";

        /// <summary>
        /// Optional, prefix to assign to a application model when creating.
        /// </summary>
        public static string AppModelPrefix ="AppModelPrefix"; 

        /// <summary>
        /// Optional, suffix to assign to a application model when creating.
        /// </summary>
        public static string AppModelSuffix ="AppModelSuffix"; 

        /// <summary>
        /// Optional, prefix to assign to a integration test when creating.
        /// </summary>
        public static string TestPrefix ="TestPrefix"; 

        /// <summary>
        /// Optional, suffix to assign to a integration test when creating.
        /// </summary>
        public static string TestSuffix ="TestSuffix"; 

        /// <summary>
        /// The prefix to assign to the name of a application models validation class.
        /// </summary>
        public static string AppModelValidatorPrefix = "AppModelValidatorPrefix";

        /// <summary>
        /// The suffix to assign to the name of a application models validation class.
        /// </summary>
        public static string AppModelValidatorSuffix = "AppModelValidatorSuffix";

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            var command = new ConfigCommand { Category = "RefreshEFRepository", Name = "EFRepositoryRefresh", CommandType = Type }
                .UpdateExecutionProject
                (
                    new ConfigProject
                    {
                        Name = ExecutionProject,
                        Guidance = "Enter the fully project name for the project that hosts the EF models."
                    }
                    .AddFolder
                    (
                        new ConfigFolder
                        {
                            Name = ExecutionModelFolder,
                            Required = false,
                            Guidance =
                                "Optional, set the relative path from the root of the project. If it is more then one directory deep then use '/' instead of back slashes."
                        }
                    )
                    .AddParameter
                    (
                        new ConfigParameter
                        {
                            Name = EFContextClassName,
                            Guidance = "Enter the class name of the database context used by entity framework."
                        }
                    )
                    .AddParameter
                    (
                        new ConfigParameter
                        { 
                            Name = EFEntityRemovePrefixes,
                            Guidance = "Comma seperated value list of the prefixes in case sensitive format to be removed from the entity framework entity name when creating new objects."
                        }
                    )
                    .AddParameter
                    (
                        new ConfigParameter
                        { 
                            Name = EFEntityRemoveSuffixes,
                            Guidance = "Comma seperated value list of the suffixes in case sensitive format to be removed from the entity framework entity name when creating new objects."
                        }
                    )
                )
                .AddProject
                (
                    new ConfigProject
                    {
                        Name = EntityProject,
                        Guidance =
                            "Enter the full project name for the project that hosts the generated application POCO models that represent the EF Models."
                    }
                    .AddFolder
                    (
                        new ConfigFolder
                        {
                            Name = EntityFolder,
                            Required = false,
                            Guidance =
                                "Optional, set the relative path from the root of the project. If it is more then one directory deep then use '/' instead of back slashes."
                        }
                    )
                    .AddParameter
                    (
                        new ConfigParameter
                        { 
                            Name = AppModelPrefix,
                            Guidance = "Optional, prefix to assign to the application model entity when it is created."
                        }
                    )
                    .AddParameter
                    (
                        new ConfigParameter
                        { 
                            Name = AppModelSuffix,
                            Guidance = "Optional, suffix to assign to the application model entity when it is created.",
                            Value = "AppModel"
                        }
                    )
                     .AddParameter
                    (
                        new ConfigParameter
                        { 
                            Name = AppModelValidatorPrefix,
                            Guidance = "Optional, prefix to assign to the application model entity validator when it is created."
                            
                        }
                    )
                    .AddParameter
                    (
                        new ConfigParameter
                        { 
                            Name = AppModelValidatorSuffix,
                            Guidance = "Optional, suffix to assign to the application model entity validator when it is created.",
                            Value = "Validator"
                        }
                    )
                )
                .AddProject
                (
                    new ConfigProject
                    {
                        Name = RepoProject,
                        Guidance =
                            "Enter the full project name for the project that hosts the generated repositories that represent the EF Models."
                    }
                    .AddFolder
                    (
                        new ConfigFolder()
                        {
                            Name = RepoFolder,
                            Required = false,
                            Guidance =
                                "Optional, set the relative path from the root of the project. If it is more then one directory deep then use '/' instead of back slashes."
                        }
                    )
                    .AddParameter
                    (
                        new ConfigParameter
                        { 
                            Name = RepositoryPrefix,
                            Guidance = "Optional, prefix to assign to the repository when it is created."
                        }
                    )
                    .AddParameter
                    (
                        new ConfigParameter
                        { 
                            Name = RepositorySuffix,
                            Guidance = "Optional, suffix to assign to the repository when it is created.",
                            Value = "Repository"
                        }
                    )
                )
                .AddProject
                (
                    new ConfigProject
                    {
                        Name = RepoContractProject,
                        Guidance =
                            "Enter the full project name for the project that hosts the interface definition for the repository contract."
                    }
                    .AddFolder
                    (
                        new ConfigFolder
                        {
                            Name = RepoContractFolder,
                            Required = false,
                            Guidance =
                                "Optional, set the relative path from the root of the project. If it is more then one directory deep then use '/' instead of back slashes."
                        }
                    )
                )
                .AddProject
                (
                    new ConfigProject
                    {
                        Name = IntegrationTestProject,
                        Guidance =
                            "Enter the full project name for the project that hosts the generated integration test for the repository."
                    }
                    .AddFolder
                    (
                        new ConfigFolder()
                        {
                            Name = IntegrationTestFolder,
                            Required = false,
                            Guidance =
                                "Optional, set the relative path from the root of the project. If it is more then one directory deep then use '/' instead of back slashes."
                        }
                    )
                    .AddParameter
                    (
                        new ConfigParameter
                        { 
                            Name = TestPrefix,
                            Guidance = "Optional, prefix to assign to the integration test when it is created."
                        }
                    )
                    .AddParameter
                    (
                        new ConfigParameter
                        { 
                            Name = TestSuffix,
                            Guidance = "Optional, suffix to assign to the integration test when it is created.",
                            Value = "Test"
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
                isEnabled = (await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionModelFolder, result, FolderLoadType.TargetFolderOnly)) != null;

                if (!isEnabled) isEnabled = (await ConfigManager.LoadCommandByProjectAsync(Type, result)) != null;
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
                var command = (await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionModelFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result))
                              ?? throw new CodeFactoryException("Could not load the automation configuration cannot refresh the EF repository.");

                VsProject efModelProject = await VisualStudioActions.GetProjectFromConfigAsync(command.ExecutionProject)
                    ?? throw new CodeFactoryException("Could load the EF hosting project, cannot refresh the EF repository.");

                VsProjectFolder efModelFolder =
                    await VisualStudioActions.GetProjectFolderFromConfigAsync(command.ExecutionProject, ExecutionModelFolder);

                var contextClassName = command.ExecutionProject.ParameterValue(EFContextClassName);
                var efEntityRemovePrefixs = command.ExecutionProject.ParameterValue(EFEntityRemovePrefixes);
                var efEntityRemoveSuffixes = command.ExecutionProject.ParameterValue(EFEntityRemoveSuffixes);

                VsProject appModelProject = await VisualStudioActions.GetProjectFromConfigAsync(command.Project(EntityProject))
                    ?? throw new CodeFactoryException("Could not load the entity model project, cannot refresh the EF repository.");

                VsProjectFolder appModelFolder =
                    await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(EntityProject), EntityFolder);

                var appModelPrefix = command.Project(EntityProject).ParameterValue(AppModelPrefix);
                var appModelSuffix = command.Project(EntityProject).ParameterValue(AppModelSuffix);
                var appModelValidatorPrefix = command.Project(EntityProject).ParameterValue(AppModelValidatorPrefix);
                var appModelValidatorSuffix = command.Project(EntityProject).ParameterValue(AppModelValidatorSuffix);

                VsProject repoProject = await VisualStudioActions.GetProjectFromConfigAsync(command.Project(RepoProject))
                    ?? throw new CodeFactoryException("Could not load the repository project, cannot refresh the EF repository.");

                VsProjectFolder repoFolder =
                    await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(RepoProject), RepoFolder);

                var repoPrefix = command.Project(RepoProject).ParameterValue(RepositoryPrefix);

                var repoSuffix = command.Project(RepoProject).ParameterValue(RepositorySuffix);

                VsProject contractProject =
                    await VisualStudioActions.GetProjectFromConfigAsync(command.Project(RepoContractProject)) ??
                    throw new CodeFactoryException(
                        "Could not load the repository contract project, cannot refresh the EF repository.");


                VsProjectFolder contractFolder = await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(RepoContractProject), RepoFolder);

                VsProject testProject =
                    await VisualStudioActions.GetProjectFromConfigAsync(command.Project(IntegrationTestProject));

                VsProjectFolder testFolder =
                    await VisualStudioActions.GetProjectFolderFromConfigAsync(command.Project(IntegrationTestProject),
                        IntegrationTestFolder);

                var testPrefix = testProject != null ? command.Project(IntegrationTestProject).ParameterValue(TestPrefix) : null;

                var testSuffix = testProject != null ? command.Project(IntegrationTestProject).ParameterValue(TestSuffix) : null;

                if (string.IsNullOrWhiteSpace(contextClassName))
                    throw new CodeFactoryException(
                        "The entity framework context class name was not provided, cannot refresh the EF repository.");

                 var contextClass = (await efModelProject.FindCSharpSourceByClassNameAsync(contextClassName))
                                     ?.SourceCode?.Classes?.FirstOrDefault()
                                     ?? throw new CodeFactoryException($"The entity framework context class '{contextClassName}' could not be loaded, cannot refresh the EF repository,");

                await VisualStudioActions.RefreshDbContextAsync(contextClassName, efModelProject, efModelFolder);

                bool supportsLogging = await repoProject.SupportsLogging();

                bool supportsNDF = await repoProject.SupportsNDF();

                var efModel = result.SourceCode?.Classes?.FirstOrDefault()
                    ?? throw new CodeFactoryException("The EF entity class could not be loaded, cannot refresh the EF repository.");

                var nameManagement = NameManagement.Init(efEntityRemovePrefixs,efEntityRemoveSuffixes,appModelPrefix,appModelSuffix);

                var appModel = (await VisualStudioActions.RefreshModelAsync(efModel, appModelProject,
                                   EntityModelNamespaces(),nameManagement, appModelFolder, $"Application data model that supports '{efModel.Name}'", true,useSourceProperty: RepositoryBuilder.UseSourceProperty))
                               ?? throw new CodeFactoryException($"Could not load the entity that supports the ef model '{efModel.Name}', cannot refresh the EF repository.");

                string noReplacePrefix = null;
                string noReplaceSuffix = null;

                var modelValidatorNameManagement = NameManagement.Init(noReplacePrefix,noReplaceSuffix,appModelValidatorPrefix,appModelValidatorSuffix);
                
                var validation = (await VisualStudioActions.RefreshValidationClassAsync(appModel,appModelProject,appModelFolder,modelValidatorNameManagement))
                                ?? throw new CodeFactoryException($"Could not refresh the validation for the app model '{appModel.Name}', cannot refresh the EF repository.");

                await VisualStudioActions.RefreshFluentValidationAsync(efModel,appModel,validation);

                await VisualStudioActions.RefreshEntityFrameworkEntityTransform(appModel, efModel, efModelProject,
                    efModelFolder);

                var repositoryName = NameManagement.Init(efEntityRemovePrefixs,efEntityRemoveSuffixes,repoPrefix,repoSuffix).FormatName(efModel.Name);

                var repoClass = await VisualStudioActions.RefreshEFRepositoryAsync(repositoryName,efModel, repoProject, contractProject, appModel,
                    contextClass, supportsNDF, supportsLogging, repoFolder, contractFolder);

               
                if(repoClass != null & testProject != null)
                { 
                    var contractName = $"I{repositoryName}";

                    CsInterface contractInterface = contractFolder != null ? (await contractFolder.FindCSharpSourceByInterfaceNameAsync(contractName))?.SourceCode?.Interfaces?.FirstOrDefault() 
                    : (await contractProject.FindCSharpSourceByInterfaceNameAsync(contractName))?.SourceCode?.Interfaces?.FirstOrDefault();
                    
                    if(contractInterface != null)
                        { 
                            var testName = NameManagement.Init(noReplacePrefix,noReplaceSuffix,testPrefix,testSuffix).FormatName(repositoryName);
                            await VisualStudioActions.RefreshMSTestIntegrationTestAsync(testName, contractInterface,testProject); 
                        }
                }

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
        /// Helper method that creates the default namespaces to add to a new entity class that is crearted.
        /// </summary>
        /// <returns>List of using statements to use.</returns>
        private List<IUsingStatementNamespace> EntityModelNamespaces()
        {
            return new List<IUsingStatementNamespace>
            {
                new ManualUsingStatementNamespace("System"),
                new ManualUsingStatementNamespace("System.Collections.Generic"),
                new ManualUsingStatementNamespace("System.Linq"),
                new ManualUsingStatementNamespace("System.Text")
            };
        }
    }

}
