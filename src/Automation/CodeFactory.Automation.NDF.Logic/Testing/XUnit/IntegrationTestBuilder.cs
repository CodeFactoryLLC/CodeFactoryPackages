using CodeFactory.Automation.NDF.Logic.AspNetCore.Service.Rest.Json;
using CodeFactory.Automation.Standard.Logic.Extensions;
using CodeFactory.WinVs;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.ProjectSystem;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic.Testing.XUnit
{
	/// <summary>
	/// Automation logic that supports integration testing using the XUnit unit test framework.
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
		public static async Task RefreshIntegrationTestAsync(this IVsActions source, string testName, CsInterface contract, VsProject testProject)
		{
			if (source == null) throw new CodeFactoryException("Could not access the CodeFactory automation for visual studio cannot refresh the integration test.");

			if (string.IsNullOrEmpty(testName)) throw new CodeFactoryException("No test name was provided cannot update the integration test.");

			if (contract == null) throw new CodeFactoryException("No contract was provided cannot update the integration test.");

			if (testProject == null) throw new CodeFactoryException($"No test project was provided cannot refresh the integration tests that support the contract '{contract.Name}'");

			var isTestProject = await testProject.TestProjectIsConfiguredXUnitAsync(true);

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
			testFormatter.AppendCodeLine(0, "using Xunit;");
			testFormatter.AppendCodeLine(0, "using CodeFactory.NDF;");
			testFormatter.AppendCodeLine(0, $"using {contract.Namespace};");
			testFormatter.AppendCodeLine(0);

			testFormatter.AppendCodeLine(0, $"namespace {testProject.DefaultNamespace}");
			testFormatter.AppendCodeLine(0, "{");

			testFormatter.AppendCodeLine(1, "/// <summary>");
			testFormatter.AppendCodeLine(1, $"/// Integration test class that tests the contract <see cref=\"{contract.Name}\"/>");
			testFormatter.AppendCodeLine(1, "/// </summary>");
			testFormatter.AppendCodeLine(1, $"public class {testClassName}");
			testFormatter.AppendCodeLine(1, "{");

			testFormatter.AppendCodeLine(2, "/// <summary>");
			testFormatter.AppendCodeLine(2, $"/// The contract <see cref=\"{contract.Name}\"/> being tested.");
			testFormatter.AppendCodeLine(2, "/// </summary>");
			testFormatter.AppendCodeLine(2, $"private readonly {contract.Name} _contract;");
			testFormatter.AppendCodeLine(0);
			testFormatter.AppendCodeLine(2, "/// <summary>");
			testFormatter.AppendCodeLine(2, $"/// Creates a new instances of the intergration test class for testing.");
			testFormatter.AppendCodeLine(2, "/// </summary>");
			testFormatter.AppendCodeLine(2, $"public {testClassName}()");
			testFormatter.AppendCodeLine(2, "{");
			testFormatter.AppendCodeLine(3, $"_contract = TestLoader.GetRequiredService<{contract.Name}>();");
			testFormatter.AppendCodeLine(2, "}");
			testFormatter.AppendCodeLine(0);

			testFormatter.AppendCodeLine(1, "}");

			testFormatter.AppendCodeLine(0, "}");

			var doc = await testProject.AddDocumentAsync($"{testClassName}.cs", testFormatter.ReturnSource().TrimStartEndLines());

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
				StringBuilder parameterBuilder = new StringBuilder();
				var testMethodFormatter = new SourceFormatter();

				testMethodFormatter.AppendCodeLine(0);
				testMethodFormatter.AppendCodeLine(2, "/// <summary>");
				testMethodFormatter.AppendCodeLine(2, $"/// Integration test that tests the contract method \"{addMethod.Name}\"");
				testMethodFormatter.AppendCodeLine(2, "/// </summary>");
				testMethodFormatter.AppendCodeLine(2, "[Fact]");

				if (addMethod.IsAsync()) testMethodFormatter.AppendCodeLine(2, $"public async Task {addMethod.FormatTestMethodName()}()");
				else testMethodFormatter.AppendCodeLine(2, $"public void {addMethod.FormatTestMethodName()}()");
				testMethodFormatter.AppendCodeLine(2, "{");

				// Build Arrange Section.				
				testMethodFormatter.AppendCode(ArrangeSectionBuilder(addMethod, AddTests));

				testMethodFormatter.AppendCodeLine(3, "try");
				testMethodFormatter.AppendCodeLine(3, "{");

				// Build Act Section
				testMethodFormatter.AppendCode(ActSectionBuilder(addMethod, AddTests));
				testMethodFormatter.AppendCodeLine(0);

				// Build Assert Section.
				testMethodFormatter.AppendCode(AssertSectionBuilder(addMethod, AddTests));

				testMethodFormatter.AppendCodeLine(3, "}");
				testMethodFormatter.AppendCodeLine(3, "catch (Exception unhandled)");
				testMethodFormatter.AppendCodeLine(3, "{");
				testMethodFormatter.AppendCodeLine(4, "Assert.Fail($\"The following unhandled exception occurred '{unhandled.Message}' \");");
				testMethodFormatter.AppendCodeLine(3, "}");

				testMethodFormatter.AppendCodeLine(3, "finally");
				testMethodFormatter.AppendCodeLine(3, "{");

				// Build Finally Clause section.
				testMethodFormatter.AppendCode(FinallyClauseSectionBuilder(addMethod, AddTests));

				testMethodFormatter.AppendCodeLine(3, "}");
				testMethodFormatter.AppendCodeLine(0);

				testMethodFormatter.AppendCodeLine(2, "}");

				await sourceManager.ConstructorsAddAfterAsync(testMethodFormatter.ReturnSource());

				testMethodFormatter.ResetFormatter();
			}

			return;
		}

		/// <summary>
		/// Builds the Arange body contents of a test.
		/// </summary>
		/// <param name="source">Source formatter of the test.</param>
		/// <returns>Formatted test contents.</returns>
		private static string ArrangeSectionBuilder(CsMethod method, List<CsMethod> allMethods)
		{
			SourceFormatter source = new SourceFormatter();
			if (method == null) return source.ReturnSource();

			var addMethod = allMethods.FirstOrDefault(x => x.Name.ToLower() == "addasync");

			// Initialize parameter variables
			source.AppendCodeLine(3, "// Arrange");
			source = method.GenerateNewTestObjectsFromParameters(source);

			// Initialize objects depending on CRUD methods.
			if (method.Name.ToLower().StartsWith("getasync"))
			{
				// If AddAsync exist, new up an object so that we can use it later as a parameter to add a test record.
				if (addMethod != null)
				{
					source = addMethod.GenerateNewTestObjectsFromParameters(source);
				}
			}

			// Initialize the result object
			if (method.HasReturnType())
			{
				var defaultValue = method.GetReturnType().GenerateCSharpDefaultValue();

				source.AppendCodeLine(3, defaultValue != null
					? $"{method.GetReturnType().GenerateCSharpTypeName()}? result = new {method.GetReturnType().GenerateCSharpTypeName()}();"
					: $"{method.GetReturnType().GenerateCSharpTypeName()} result;");
				source.AppendCodeLine(4);
			}

			return source.ReturnSource().TrimEndLines();
		}

		/// <summary>
		/// Builds the Act body contents of a test.
		/// </summary>
		/// <param name="source">Source formatter of the test.</param>
		/// <returns>Formatted test contents.</returns>
		private static string ActSectionBuilder(CsMethod method, List<CsMethod> allMethods)
		{
			SourceFormatter source = new SourceFormatter();
			if (method == null) return source.ReturnSource();

			var addMethod = allMethods.FirstOrDefault(x => x.Name.ToLower() == "addasync");
			var getMethod = allMethods.FirstOrDefault(x => x.Name.ToLower().StartsWith("getasync"));
			var updateMethod = allMethods.FirstOrDefault(x => x.Name.ToLower() == "updateasync");

			if (!method.HasReturnType())
				return source.ReturnSource();

			source.AppendCodeLine(4, "// Act");

			// If AddAsync exist, call AddAsync to create a test record.
			if (addMethod != null && method.Name.ToLower() == "addasync")
			{
				source.AppendCodeLine(4, $"result = await _contract.AddAsync({addMethod.GenerateParameterListAsCamelCased()});");
			}

			// If GetAsync exist, call GetAsync to get a record.
			else if (getMethod != null && method.Name.ToLower() == "getasync" && addMethod != null)
			{
				source.AppendCodeLine(4, $"result = await _contract.AddAsync({addMethod.GenerateParameterListAsCamelCased()});");
				source.AppendCodeLine(4, $"result = await _contract.GetAsync({getMethod.GenerateParameterListAsProperCased()});");
			}

			// If AddAsync exist, call AddAsync to create a test record.
			else if (updateMethod != null && method.Name.ToLower() == "updateasync" && addMethod != null)
			{
				source.AppendCodeLine(4, $"result = await _contract.AddAsync({addMethod.GenerateParameterListAsCamelCased()});");
				source.AppendCodeLine(4, $"result.{updateMethod.Parameters[0].ParameterType.GetFirstStringTypePropertyName()} += \" Update\";");
				source.AppendCodeLine(4, $"result = await _contract.UpdateAsync(result);");
			}
			else
			{
				source.AppendCodeLine(4, method.HasReturnType()
					? $"result = {method.GenerateStringAwaitStatement()}_contract.{method.Name}({method.GenerateParameterListAsCamelCased()});"
					: $"{method.GenerateStringAwaitStatement()}_contract.{method.Name}({method.GenerateParameterListAsCamelCased()});");

			}

			source.AppendCodeLine(0);
			return source.ReturnSource().TrimEndLines();
		}

		/// <summary>
		/// Builds the Assert body contents of a test.
		/// </summary>
		/// <param name="source">Source formatter of the test.</param>
		/// <returns>Formatted test contents.</returns>
		private static string AssertSectionBuilder(CsMethod method, List<CsMethod> allMethods)
		{
			SourceFormatter source = new SourceFormatter();
			if (method == null) return source.ReturnSource();

			var addMethod = allMethods.FirstOrDefault(x => x.Name.ToLower() == "addasync");
			var updateMethod = allMethods.FirstOrDefault(x => x.Name.ToLower() == "updateasync");
			var getMethod = allMethods.FirstOrDefault(x => x.Name.ToLower() == "getasync");

			if (method.HasReturnType())
			{
				source.AppendCodeLine(0);
				source.AppendCodeLine(4, "// Assert");
				source.AppendCodeLine(4, "Assert.NotNull(result);");
			}
			else
			{
				source.AppendCodeLine(4, "// Act and Assert");

				// If DeleteAsync
				if (method.Name.ToLower() == "deleteasync")
				{
					if (addMethod != null)
					{
						source.AppendCodeLine(4, $"var result = await _contract.AddAsync({addMethod.GenerateParameterListAsCamelCased()});");
						source.AppendCodeLine(4, $"await _contract.DeleteAsync(result);");
						source.AppendCodeLine(0);
					}
				}

				source.AppendCodeLine(4, $"await Assert.ThrowsAsync<UnhandledException>({getMethod.GenerateStringAsyncStatement()} ()=> {getMethod.GenerateStringAwaitStatement()}_contract.{getMethod.Name}({getMethod.GenerateParameterListAsProperCased()}));");
			}

			// If AddAsync, check the result to make sure the id is not 0.
			if (addMethod != null && method.Name.ToLower() == "addasync")
			{
				source.AppendCodeLine(4, $"Assert.NotEqual(0, result.{addMethod.Parameters[0].ParameterType.GetPrimaryKeyName()});");
			}

			// If UpdateAsync, check that the result of the update call during the Act was updated with a " update" text on the first string property.
			if (updateMethod != null && method.Name.ToLower() == "updateasync")
			{
				source.AppendCodeLine(4, $"Assert.Equal({updateMethod.Parameters[0].Name.GenerateCSharpCamelCase()}.{updateMethod.Parameters[0].ParameterType.GetFirstStringTypePropertyName()} + \" Update\", result.{updateMethod.Parameters[0].ParameterType.GetFirstStringTypePropertyName()});");
			}

			return source.ReturnSource().TrimStartEndLines();
		}

		/// <summary>
		/// Builds the body for the finally clause in the try catch.
		/// </summary>
		/// <param name="source">Source formatter of the test.</param>
		/// <returns>Formatted test contents.</returns>
		private static string FinallyClauseSectionBuilder(CsMethod method, List<CsMethod> allMethods)
		{
			SourceFormatter source = new SourceFormatter();
			if (method == null) return source.ReturnSource();

			source.AppendCodeLine(4, $"// Cleanup");
			var deleteMethod = allMethods.FirstOrDefault(x => x.Name.ToLower() == "deleteasync");

			// Add additional assert checks.
			if (method.Name.ToLower() == "updateasync" || method.Name.ToLower() == "getasync" || method.Name.ToLower() == "addasync")
			{
				// Delete the record that was created as a test, if one exist.
				source.AppendCodeLine(4, $"await _contract.DeleteAsync(result!);");
			}

			source.AppendCodeLine(0);
			source.AppendCodeLine(4, $"// Add any cleanup code if necessary");

			return source.ReturnSource().TrimEndLines();
		}

		/// <summary>
		/// Formats the repositories test class name.
		/// </summary>
		/// <param name="source">Source interface to convert to the test class name.</param>
		/// <returns>Formatted test class name or null if it was not found.</returns>
		private static string FormatTestClassName(this CsInterface source)
		{
			if (source == null) return null;

			return $"{source.Name.GenerateCSharpFormattedClassName()}Test";
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
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(2, "/// <summary>");
			sourceFormatter.AppendCodeLine(2, "/// Backing field for the <see cref=\"ServiceProvider\"/> property.");
			sourceFormatter.AppendCodeLine(2, "/// </summary>");
			sourceFormatter.AppendCodeLine(2, "private static readonly IServiceProvider _serviceProvider;");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(2, "/// <summary>");
			sourceFormatter.AppendCodeLine(2, "/// Logging factory used for testing");
			sourceFormatter.AppendCodeLine(2, "/// </summary>");
			sourceFormatter.AppendCodeLine(2, "private static readonly ILoggerFactory _loggerFactory;");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(2, "/// <summary>");
			sourceFormatter.AppendCodeLine(2, "/// Constructor that gets called when the class is accessed for the first time.");
			sourceFormatter.AppendCodeLine(2, "/// </summary>");
			sourceFormatter.AppendCodeLine(2, "static TestLoader()");
			sourceFormatter.AppendCodeLine(2, "{");
			sourceFormatter.AppendCodeLine(3, "//Loading the configuration");
			sourceFormatter.AppendCodeLine(3, "var configuration = new ConfigurationBuilder()");
			sourceFormatter.AppendCodeLine(3, "    .AddJsonFile(\"appsettings.local.json\", true)");
			sourceFormatter.AppendCodeLine(3, "    .AddEnvironmentVariables().Build();");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(3, "//Setting the config property.");
			sourceFormatter.AppendCodeLine(3, "_configuration = configuration;");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(3, "//Creating the service container");
			sourceFormatter.AppendCodeLine(3, "var services = new ServiceCollection();");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(3, "//Adding access to configuration from the service container");
			sourceFormatter.AppendCodeLine(3, "services.AddSingleton<IConfiguration>(Configuration);");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(3, "//Loading the libraries into the service collection.");
			sourceFormatter.AppendCodeLine(3, "LoadLibraries(services, _configuration);");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(3, "_loggerFactory = new NullLoggerFactory();");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(3, "services.AddSingleton(_loggerFactory);");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(3, "services.AddLogging();");
			sourceFormatter.AppendCodeLine(3, "//Building the service provider.");
			sourceFormatter.AppendCodeLine(3, "_serviceProvider = services.BuildServiceProvider();");
			sourceFormatter.AppendCodeLine(0);

			sourceFormatter.AppendCodeLine(2, "}");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(2, "/// <summary>");
			sourceFormatter.AppendCodeLine(2, "/// The loaded configuration to be used with testing.");
			sourceFormatter.AppendCodeLine(2, "/// </summary>");
			sourceFormatter.AppendCodeLine(2, "public static IConfiguration Configuration => _configuration;");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(2, "/// <summary>");
			sourceFormatter.AppendCodeLine(2, "/// Service provider for the loaded dependency configuration.");
			sourceFormatter.AppendCodeLine(2, "/// </summary>");
			sourceFormatter.AppendCodeLine(2, "public static IServiceProvider ServiceProvider => _serviceProvider;");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(2, "/// <summary>");
			sourceFormatter.AppendCodeLine(2, "/// Gets the required service to use with testing.");
			sourceFormatter.AppendCodeLine(2, "/// </summary>");
			sourceFormatter.AppendCodeLine(2, "/// <typeparam name=\"T\">Type of the service to be loaded.</typeparam>");
			sourceFormatter.AppendCodeLine(2, "/// <returns>Instance of the service</returns>");
			sourceFormatter.AppendCodeLine(2, "public static T GetRequiredService<T>() where T : notnull");
			sourceFormatter.AppendCodeLine(2, "{");
			sourceFormatter.AppendCodeLine(3, "return _serviceProvider.GetRequiredService<T>();");

			sourceFormatter.AppendCodeLine(2, "}");
			sourceFormatter.AppendCodeLine(0);
			sourceFormatter.AppendCodeLine(2, "/// <summary>");
			sourceFormatter.AppendCodeLine(2, "/// Loads the libraries into service collection");
			sourceFormatter.AppendCodeLine(2, "/// </summary>");
			sourceFormatter.AppendCodeLine(2, "/// <param name=\"services\">Service collection to load.</param>");
			sourceFormatter.AppendCodeLine(2, "/// <param name=\"configuration\">The configuration to be provided to services.</param>");
			sourceFormatter.AppendCodeLine(2, "public static void LoadLibraries(IServiceCollection services, IConfiguration configuration)");
			sourceFormatter.AppendCodeLine(2, "{");
			sourceFormatter.AppendCodeLine(3, "// Load Default Library Loader.");
			sourceFormatter.AppendCodeLine(3, "var libraryLoader = new LibraryLoader();");
			sourceFormatter.AppendCodeLine(3, "libraryLoader.Load(services, configuration);");
			sourceFormatter.AppendCodeLine(2, "}");


			sourceFormatter.AppendCodeLine(1, "}");
			sourceFormatter.AppendCodeLine(0, "}");

			await testProject.AddDocumentAsync("TestLoader.cs", sourceFormatter.ReturnSource().TrimStartEndLines());
		}

		/// <summary>
		/// Helper method that checks a project to make sure all required project references exist before building a test.
		/// </summary>
		/// <param name="project">Project to check.</param>
		/// <param name="throwError">Optional flag that determines if an exception should be thrown if the project is not configred, default is false.</param>
		/// <returns>True if configured with xUnit Tests, false if not.</returns>
		/// <exception cref="CodeFactoryException">Thrown if required data is missing.</exception>
		public static async Task<bool> TestProjectIsConfiguredXUnitAsync(this VsProject project, bool throwError = false)
		{
			if (project == null) throw new CodeFactoryException("No test project was provided cannot build integration tests.");

			var projectRefs = await project.GetProjectReferencesAsync();

			if (!projectRefs.Any(r => r.Name == "Microsoft.Extensions.Logging.Abstractions"))
			{
				if (throwError) throw new CodeFactoryException("The test project must reference 'Microsoft.Extensions.Logging.Abstractions'");
				return false;
			}
			if (!projectRefs.Any(r => r.Name == "Microsoft.Extensions.Configuration"))
			{
				if (throwError) throw new CodeFactoryException("The test project must reference 'Microsoft.Extensions.Configuration'");
				return false;
			}
			if (!projectRefs.Any(r => r.Name == "Microsoft.Extensions.DependencyInjection"))
			{
				if (throwError) throw new CodeFactoryException("The test project must reference 'Microsoft.Extensions.DependencyInjection'");
				return false;
			}
			if (!projectRefs.Any(r => r.Name == "Microsoft.VisualStudio.TestPlatform.ObjectModel"))
			{
				if (throwError) throw new CodeFactoryException("The test project must reference 'Microsoft.VisualStudio.TestPlatform.ObjectModel'");
				return false;
			}
			if (!projectRefs.Any(r => r.Name.Contains("xunit.")))
			{
				if (throwError) throw new CodeFactoryException("The test project must reference 'xunit'");
				return false;
			}
			return true;
		}
	}
}
