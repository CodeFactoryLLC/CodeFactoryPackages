using CodeFactory.Automation.NDF.Logic.Testing.XUnit;
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
    public class RefreshXUnitIntegrationTest : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Refresh XUnit Integration Test";
        private static readonly string commandDescription = "Refreshes an integration test from the target interface that uses XUnit.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public RefreshXUnitIntegrationTest(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(RefreshXUnitIntegrationTest).FullName;

        /// <summary>
        /// Project executing the command
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// Project that hosts the intergration tests
        /// </summary>
        public static string TestProject = "TestProject";

        /// <summary>
        /// Prefix to append to the name of the integration test being created.
        /// </summary>
        public static string TestClassPrefix = "TestClassPrefix";

        /// <summary>
        /// Suffix to append to the name of the integration test being created.
        /// </summary>
        public static string TestClassSuffix = "TestClassSuffix";

        /// <summary>
        /// Prefix(s) that will be used to create mutiple test methods for each contract method being tested.
        /// </summary>
        public static string TestMethodPrefixes = "TestMethodPrefixes";



        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            var config = new ConfigCommand
            {
                CommandType = Type,
                Category = "Testing",
                Name = nameof(RefreshTest),
                Guidance = "Automation command that generates integration tests from a provided interface."
            }
            .UpdateExecutionProject
            (
                    new ConfigProject { Name = ExecutionProject, Guidance = "Enter the name of the project the command will trigger from." }
            )

            .AddProject
            (
                    new ConfigProject { Name = TestProject, Guidance = "Enter the name of the project that hosts the XUnit integration testing." }
            )
            .AddParameter
            (
                new ConfigParameter
                {
                    Name = TestClassPrefix,
                    Guidance = "Optional, prefix to append to the name of the integration test class when being created."
                }
            )
            .AddParameter
            (
                new ConfigParameter
                {
                    Name = TestClassSuffix,
                    Guidance = "Optional, Suffix to append to the name of the integration test class when being created.",
                    Value = "Test"

                }
            ).AddParameter
            (
                new ConfigParameter
                {
                    Name = TestMethodPrefixes,
                    Guidance = "Optional, List of prefixes that will be added to the name of the test methods that test a target contract method. Store as comma separated values between prefixes."
                }
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
                isEnabled = result.SourceCode?.Interfaces?.FirstOrDefault() != null;

                if (isEnabled)
                {
                    var config = await ConfigManager.LoadCommandByProjectAsync(Type, result);

                    isEnabled = config != null;

                    if (isEnabled)
                    {
                        var testProject = await VisualStudioActions.GetProjectFromConfigAsync(config.Project(TestProject));

                        isEnabled = testProject != null;

                        if (isEnabled) isEnabled = await testProject.TestProjectIsConfiguredAsync();
                    }
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
                var config = (await ConfigManager.LoadCommandByProjectAsync(Type, result))
                    ?? throw new CodeFactoryException("Could not load the commands configuration.");

                var testProject = (await VisualStudioActions.GetProjectFromConfigAsync(config.Project(TestProject))) 
                    ?? throw new CodeFactoryException("Could not locate the test project cannot refresh the test.");

                

                var targetInterface = result.SourceCode?.Interfaces?.FirstOrDefault()
                    ?? throw new CodeFactoryException("Could not locate the interface to have tests created from.");

                var testClassPrefix = config.ParameterValue(TestClassPrefix);
                var testClassSuffix = config.ParameterValue(TestClassSuffix);

                var methodPrefixes = config.ParameterValue(TestMethodPrefixes);

                List<string> testMethodPrefixes = null;

                if (methodPrefixes != null)
                {
                    testMethodPrefixes = new List<string>();
                    var prefixeData = methodPrefixes.Split(',');

                    foreach (var prefix in prefixeData)
                    {
                        testMethodPrefixes.Add(prefix.Trim());
                    }
                }

                string noRemove = null;

                var testName = NameManagement.Init(noRemove,noRemove,testClassPrefix,testClassSuffix).FormatName(targetInterface.Name.GenerateCSharpFormattedClassName());


                var test = VisualStudioActions.RefreshXUnitIntegrationTestAsync(testName,targetInterface, testProject,testMethodPrefixes);


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
