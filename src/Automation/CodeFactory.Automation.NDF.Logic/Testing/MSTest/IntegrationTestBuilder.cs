using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.Automation.Standard.Logic;
namespace CodeFactory.Automation.NDF.Logic.Testing.MSTest
{
    /// <summary>
    /// Automation logic that supports integration testing using the MSTest unit test framework.
    /// </summary>
    public static class IntegrationTestBuilder
    {
        /// <summary>
        /// Automation to refresh the integration test implementation.
        /// </summary>
        /// <param name="source">CodeFactory automation access to Visual Studio.</param>
        /// <param name="testName">The name of the test class to be refreshed.</param>
        /// <param name="contract">The target contract to implement testing for.</param>
        /// <param name="testProject">The target project the  target logic is implemented in.</param>
        /// <returns></returns>
        public static async Task RefreshMSTestIntegrationTestAsync(this IVsActions source,string testName, CsInterface contract, VsProject testProject)
        {
            if (source == null) throw new CodeFactoryException("Could not access the CodeFactory automation for visual studio cannot refresh the integration test.");

            if(string.IsNullOrEmpty(testName)) throw new CodeFactoryException("No test name was provided cannot update the integration test.");

            if (contract == null) throw new CodeFactoryException("No contract was provided cannot update the integration test.");

            if (testProject == null) throw new CodeFactoryException($"No test project was provided cannot refresh the integration tests that support the contract '{contract.Name}'");

            var isTestProject = await testProject.TestProjectIsConfiguredAsync(true);

            await testProject.CreateTestLoaderAsync();

            var testClassName = testName;

            if (string.IsNullOrEmpty(testClassName)) throw new CodeFactoryException("Could not load the test class name. Cannot refresh the integration tests.");

            var testSource = await testProject.FindCSharpSourceByClassNameAsync(testClassName, false);

            if (testSource == null) await source.CreateTestAsync(contract, testProject, testClassName);
            else await source.UpdateTestAsync(contract, testSource.SourceCode);

        }

        /// <summary>
        ///  Creates a new integration test;
        /// </summary>
        /// <param name="source">CodeFactory automation for Visual Studio.</param>
        /// <param name="contract">The target interface to be tested.</param>
        /// <param name="testProject">The target project the test should be created in.</param>
        /// <param name="testClassName">The name of the target test class.</param>
        /// <exception cref="CodeFactoryException">Raised if required data is missing.</exception>
        public static async Task CreateTestAsync(this IVsActions source, CsInterface contract, VsProject testProject, string testClassName)
        {
            if (source == null) throw new CodeFactoryException("Could not access the CodeFactory automation for visual studio cannot refresh the tests.");

            if (contract == null) throw new CodeFactoryException("No contract was provided cannot create the tests.");

            if (testProject == null) throw new CodeFactoryException($"No test project was provided cannot create the tests that support the contract '{contract.Name}'");

            if (string.IsNullOrEmpty(testClassName)) throw new CodeFactoryException($"The test class name was not provided cannot create the  tests that support the contract '{contract.Name}'");

            SourceFormatter testFormatter = new SourceFormatter();

            testFormatter.AppendCodeLine(0, "using System;");
            testFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            testFormatter.AppendCodeLine(0, "using System.Linq;");
            testFormatter.AppendCodeLine(0, "using System.Linq.Expressions;");
            testFormatter.AppendCodeLine(0, "using System.Text;");
            testFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            testFormatter.AppendCodeLine(0, "using Microsoft.VisualStudio.TestTools.UnitTesting;");
            testFormatter.AppendCodeLine(0, $"using {contract.Namespace};");
            testFormatter.AppendCodeLine(0);

            testFormatter.AppendCodeLine(0, $"namespace {testProject.DefaultNamespace}");
            testFormatter.AppendCodeLine(0, "{");

            testFormatter.AppendCodeLine(1, "/// <summary>");
            testFormatter.AppendCodeLine(1, $"/// Integration test class that tests the contract <see cref=\"{contract.Name}\"/>");
            testFormatter.AppendCodeLine(1, "/// </summary>");
            testFormatter.AppendCodeLine(1, "[TestClass]");
            testFormatter.AppendCodeLine(1, $"public class {testClassName}");
            testFormatter.AppendCodeLine(1, "{");

            testFormatter.AppendCodeLine(2, "/// <summary>");
            testFormatter.AppendCodeLine(2, $"/// The contract <see cref=\"{contract.Name}\"/> being tested.");
            testFormatter.AppendCodeLine(2, "/// </summary>");
            testFormatter.AppendCodeLine(2, $"private readonly {contract.Name} _contract;");
            testFormatter.AppendCodeLine(2);
            testFormatter.AppendCodeLine(2, "/// <summary>");
            testFormatter.AppendCodeLine(2, $"/// Creates a new instances of the intergration test class for testing.");
            testFormatter.AppendCodeLine(2, "/// </summary>");
            testFormatter.AppendCodeLine(2, $"public {testClassName}()");
            testFormatter.AppendCodeLine(2, "{");
            testFormatter.AppendCodeLine(3, $"_contract = TestLoader.GetRequiredService<{contract.Name}>();");
            testFormatter.AppendCodeLine(2, "}");
            testFormatter.AppendCodeLine(2);

            testFormatter.AppendCodeLine(1, "}");

            testFormatter.AppendCodeLine(0, "}");

            var doc = await testProject.AddDocumentAsync($"{testClassName}.cs", testFormatter.ReturnSource());

            var testSourceCode = await doc.GetCSharpSourceModelAsync();

            await source.UpdateTestAsync(contract, testSourceCode);
        }

        /// <summary>
        /// Updates a test class that performs intergration tests on the target contract.
        /// </summary>
        /// <param name="source">CodeFactory automation for visual studio.</param>
        /// <param name="contract">The target contract that is being tested.</param>
        /// <param name="testClassSource">The target source code that is to be updated.</param>
        /// <exception cref="CodeFactoryException">Throw if required data is missing.</exception>
        public static async Task UpdateTestAsync(this IVsActions source, CsInterface contract, CsSource testClassSource)
        {
            if (source == null) throw new CodeFactoryException("Could not access the CodeFactory automation for visual studio cannot refresh the integration tests.");

            if (contract == null) throw new CodeFactoryException("No contract was provided cannot update the integration tests.");

            if (testClassSource == null) throw new CodeFactoryException($"The test class source was not provided cannot update the integration tests that support the contract '{contract.Name}'");

            var currentSource = testClassSource;

            var testClass = currentSource.Classes.FirstOrDefault();

            if (testClass == null) throw new CodeFactoryException($"The test class could not be loaded from the provided source. cannot update the integrationn tests that support the contract '{contract.Name}'");

            var contractMethods = contract.GetAllInterfaceMethods();

            if (!contractMethods.Any()) return;

            var sourceMethods = testClass.Methods ?? new List<CsMethod>();

            var AddTests = new List<CsMethod>();

            foreach (var contractMethod in contractMethods)
            {
                var testMethodName = contractMethod.FormatTestMethodName();

                if (string.IsNullOrEmpty(testMethodName)) continue;

                if (!sourceMethods.Any(m => m.Name == testMethodName)) AddTests.Add(contractMethod);
            }

            if (!AddTests.Any()) return;

            var sourceManager = new SourceClassManager(currentSource, testClass, source);

            foreach (var addMethod in AddTests)
            {
                bool hasReturnType = !addMethod.IsVoid;
                bool isAsync = false;
                bool hasParameters = addMethod.HasParameters;
                StringBuilder parameterBuilder = new StringBuilder();
                var testMethodFormatter = new SourceFormatter();

                if (hasReturnType)
                {
                    isAsync = addMethod.ReturnType.IsTaskType();

                    if (isAsync) hasReturnType = !addMethod.ReturnType.IsTaskOnlyType();
                }

                testMethodFormatter.AppendCodeLine(2);
                testMethodFormatter.AppendCodeLine(2, "/// <summary>");
                testMethodFormatter.AppendCodeLine(2, $"/// Integration test that tests the contract method \"{addMethod.Name}\"");
                testMethodFormatter.AppendCodeLine(2, "/// </summary>");
                testMethodFormatter.AppendCodeLine(2, "[TestMethod]");

                if (isAsync) testMethodFormatter.AppendCodeLine(2, $"public async Task {addMethod.FormatTestMethodName()}()");
                else testMethodFormatter.AppendCodeLine(2, $"public void {addMethod.FormatTestMethodName()}()");
                testMethodFormatter.AppendCodeLine(2, "{");
                testMethodFormatter.AppendCodeLine(3, "//Arrange");
                testMethodFormatter.AppendCodeLine(3);
                if (hasParameters)
                {
                    bool firstParameter = true;

                    foreach (var testParameter in addMethod.Parameters)
                    {
                        if (firstParameter)
                        {
                            parameterBuilder.Append($"{testParameter.Name}");
                            firstParameter = false;
                        }
                        else parameterBuilder.Append($", {testParameter.Name}");

                        string defaultValue = testParameter.ParameterType.GenerateCSharpDefaultValue();
                        testMethodFormatter.AppendCodeLine(3,
                            defaultValue != null
                            ? $"{testParameter.ParameterType.GenerateCSharpTypeName(sourceManager.NamespaceManager)} {testParameter.Name} = {defaultValue};"
                            : $"{testParameter.ParameterType.GenerateCSharpTypeName(sourceManager.NamespaceManager)} {testParameter.Name};");
                        testMethodFormatter.AppendCodeLine(3);
                    }
                }

                testMethodFormatter.AppendCodeLine(3, "try");
                testMethodFormatter.AppendCodeLine(3, "{");
                testMethodFormatter.AppendCodeLine(4, "//Act");
                if (hasReturnType)
                {
                    var returnType = isAsync ? addMethod.ReturnType.GenericParameters.First().Type : addMethod.ReturnType;

                    var defaultValue = returnType.GenerateCSharpDefaultValue();

                    testMethodFormatter.AppendCodeLine(4, defaultValue != null
                        ? $"{returnType.GenerateCSharpTypeName(sourceManager.NamespaceManager)} result = {defaultValue};"
                        : $"{returnType.GenerateCSharpTypeName(sourceManager.NamespaceManager)} result;");
                    testMethodFormatter.AppendCodeLine(4);

                }

                string awaitStatement = isAsync ? " await " : " ";

                var methodParameters = hasParameters ? parameterBuilder.ToString() : "";

                testMethodFormatter.AppendCodeLine(4, hasReturnType
                        ? $"result ={awaitStatement}_contract.{addMethod.Name}({methodParameters});"
                        : $"{awaitStatement}_contract.{addMethod.Name}({methodParameters});");
                testMethodFormatter.AppendCodeLine(4);

                testMethodFormatter.AppendCodeLine(4, "//Assert");
                testMethodFormatter.AppendCodeLine(4, "throw new NotImplementedException();");
                testMethodFormatter.AppendCodeLine(4);
                testMethodFormatter.AppendCodeLine(3, "}");

                testMethodFormatter.AppendCodeLine(3, "catch (Exception unhandled)");
                testMethodFormatter.AppendCodeLine(3, "{");
                testMethodFormatter.AppendCodeLine(4, "Assert.Fail($\"The following unhandled exception occurred '{unhandled.Message}' \");");
                testMethodFormatter.AppendCodeLine(3, "}");

                testMethodFormatter.AppendCodeLine(3, "finally");
                testMethodFormatter.AppendCodeLine(3, "{");
                testMethodFormatter.AppendCodeLine(4, "//Cleanup");
                testMethodFormatter.AppendCodeLine(4);
                testMethodFormatter.AppendCodeLine(3, "}");
                testMethodFormatter.AppendCodeLine(3);

                testMethodFormatter.AppendCodeLine(2, "}");

                await sourceManager.ConstructorsAddAfterAsync(testMethodFormatter.ReturnSource());

                testMethodFormatter.ResetFormatter();
            }

            return;
        }

        /// <summary>
        /// Formats the name of the test method for the contract.
        /// </summary>
        /// <param name="source">Method model to generate the test method name from.</param>
        /// <returns>Formatted method name or null if it is not found.</returns>
        private static string FormatTestMethodName(this CsMethod source)
        {
            if (source == null) return null;

            if (!source.HasParameters) return source.Name;

            StringBuilder testMethodBuilder = new StringBuilder();

            testMethodBuilder.Append($"{source.Name}By");

            foreach (var parameter in source.Parameters)
                testMethodBuilder.Append(parameter.Name.GenerateCSharpProperCase());

            return testMethodBuilder.ToString();
        }

        /// <summary>
        /// Creates a new instance of the 'TestLoader' class.
        /// </summary>
        /// <param name="testProject">Target project to add the test loader to.</param>
        public static async Task CreateTestLoaderAsync(this VsProject testProject)
        {
            var sourceFile = await testProject.FindCSharpSourceByClassNameAsync("TestLoader", false);

            if (sourceFile != null) return;

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Configuration;");
            sourceFormatter.AppendCodeLine(0, "using Microsoft.Extensions.DependencyInjection;");
            sourceFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            sourceFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging.Abstractions;");
            sourceFormatter.AppendCodeLine(0, "using System;");
            sourceFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            sourceFormatter.AppendCodeLine(0, "using System.Linq;");
            sourceFormatter.AppendCodeLine(0, "using System.Text;");
            sourceFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            sourceFormatter.AppendCodeLine(0);
            sourceFormatter.AppendCodeLine(0, $"namespace {testProject.DefaultNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, "/// Loader class that is used by testing to load required services.");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, "public static class TestLoader");
            sourceFormatter.AppendCodeLine(1, "{");
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Backing field for the <see cref=\"Configuration\"/> property.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "private static readonly IConfiguration _configuration;");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Backing field for the <see cref=\"ServiceProvider\"/> property.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "private static readonly IServiceProvider _serviceProvider;");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Logging factory used for testing");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "private static readonly ILoggerFactory _loggerFactory;");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Constructor that gets called when the class is accessed for the first time.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "static TestLoader()");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "//Loading the configuration");
            sourceFormatter.AppendCodeLine(3, "var configuration = new ConfigurationBuilder()");
            sourceFormatter.AppendCodeLine(3, "    .AddJsonFile(\"appsettings.local.json\", true)");
            sourceFormatter.AppendCodeLine(3, "    .AddEnvironmentVariables().Build();");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "//Setting the config property.");
            sourceFormatter.AppendCodeLine(3, "_configuration = configuration;");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "//Creating the service container");
            sourceFormatter.AppendCodeLine(3, "var services = new ServiceCollection();");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "//Adding access to configuration from the service container");
            sourceFormatter.AppendCodeLine(3, "services.AddSingleton<IConfiguration>(Configuration);");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "//Loading the libraries into the service collection.");
            sourceFormatter.AppendCodeLine(3, "LoadLibraries(services, _configuration);");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "_loggerFactory = new NullLoggerFactory();");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "services.AddSingleton(_loggerFactory);");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "services.AddLogging();");
            sourceFormatter.AppendCodeLine(3, "//Building the service provider.");
            sourceFormatter.AppendCodeLine(3, "_serviceProvider = services.BuildServiceProvider();");
            sourceFormatter.AppendCodeLine(3);

            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// The loaded configuration to be used with testing.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public static IConfiguration Configuration => _configuration;");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Service provider for the loaded dependency configuration.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public static IServiceProvider ServiceProvider => _serviceProvider;");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Gets the required service to use with testing.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <typeparam name=\"T\">Type of the service to be loaded.</typeparam>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>Instance of the service</returns>");
            sourceFormatter.AppendCodeLine(2, "public static T GetRequiredService<T>() where T : notnull");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "return _serviceProvider.GetRequiredService<T>();");

            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Loads the libraries into service collection");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"services\">Service collection to load.</param>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"configuration\">The configuration to be provided to services.</param>");
            sourceFormatter.AppendCodeLine(2, "public static void LoadLibraries(IServiceCollection services, IConfiguration configuration)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "///TODO: Load dependency injection librariers here.");
            sourceFormatter.AppendCodeLine(2, "}");


            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");

            await testProject.AddDocumentAsync("TestLoader.cs", sourceFormatter.ReturnSource());
        }

        /// <summary>
        /// Helper method that checks a project to make sure all required project references exist before building a test.
        /// </summary>
        /// <param name="project">Project to check.</param>
        /// <param name="throwError">Optional flag that determines if an exception should be thrown if the project is not configred, default is false.</param>
        /// <returns>True if configured false if not.</returns>
        /// <exception cref="CodeFactoryException">Thrown if required data is missing.</exception>
        public static async Task<bool> TestProjectIsConfiguredAsync(this VsProject project, bool throwError = false)
        {
            if (project == null) throw new CodeFactoryException("No test project was provided cannot build integration tests.");

            var projectRefs = await project.GetProjectReferencesAsync();

            if (!projectRefs.Any(r => r.Name == "Microsoft.Extensions.Configuration"))
            {

                if (throwError) throw new CodeFactoryException("The test project must reference 'Microsoft.Extensions.Configuration'");
                return false;
            }
            if (!projectRefs.Any(r => r.Name == "Microsoft.Extensions.Configuration.Json"))
            {
                if (throwError) throw new CodeFactoryException("The test project must reference 'Microsoft.Extensions.Configuration.Json'");
                return false;
            }
            if (!projectRefs.Any(r => r.Name == "Microsoft.Extensions.Configuration.EnvironmentVariables"))
            {
                if (throwError) throw new CodeFactoryException("The test project must reference 'Microsoft.Extensions.Configuration.EnvironmentVariables'");
                return false;
            }
            if (!projectRefs.Any(r => r.Name == "Microsoft.Extensions.DependencyInjection"))
            {
                if (throwError) throw new CodeFactoryException("The test project must reference 'Microsoft.Extensions.DependencyInjection'");
                return false;
            }

            if (!projectRefs.Any(r => r.Name == "Microsoft.Extensions.Logging"))
            {
                if (throwError) throw new CodeFactoryException("The test project must reference 'Microsoft.Extensions.Logging'");
                return false;
            }
            if (!projectRefs.Any(r => r.Name == "Microsoft.VisualStudio.TestPlatform.TestFramework"))
            {
                if (throwError) throw new CodeFactoryException("The test project must reference 'MSTest.TestFramework'");
                return false;
            }
            return true;
        }
    }
}
