using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.Automation.Standard.Logic;

namespace CodeFactory.Automation.NDF.Logic.DependencyInjection
{
    /// <summary>
    /// Automation class that generates C# code that supports dependency injection.
    /// </summary>
    public static  class DependencyInjectionBuilder
    {
        /// <summary>
        /// Creates a new instance of a library loader class.
        /// </summary>
        /// <param name="source">CodeFactory automation library.</param>
        /// <param name="project">The target project the library loader will be created in. </param>
        /// <param name="className">The name of class that implement the dependency injection loader functionality.</param>
        /// <exception cref="CodeFactoryException"></exception>
        public static async Task<CsSource> CreateLibraryLoaderClassAsync(this IVsActions source,VsProject project,string className = "LibraryLoader")
        {
            if (source == null) throw new CodeFactoryException("Could not access the CodeFactory automation for visual studio cannot refresh the tests.");

            if (project == null) throw new CodeFactoryException($"No project was provided cannot create the library loader class named  '{className}'");

            if (string.IsNullOrEmpty(className)) throw new CodeFactoryException($"The class name was not provided cannot create the library loader class.");

            SourceFormatter loaderFormatter = new SourceFormatter();

            loaderFormatter.AppendCodeLine(0, "using System;");
            loaderFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            loaderFormatter.AppendCodeLine(0, "using System.Linq;");
            loaderFormatter.AppendCodeLine(0, "using System.Linq.Expressions;");
            loaderFormatter.AppendCodeLine(0, "using System.Text;");
            loaderFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            loaderFormatter.AppendCodeLine(0, "using CodeFactory.NDF;");
            loaderFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Configuration;");
            loaderFormatter.AppendCodeLine(0, "using Microsoft.Extensions.DependencyInjection;");
            loaderFormatter.AppendCodeLine(0);

            loaderFormatter.AppendCodeLine(0, $"namespace {project.DefaultNamespace}");
            loaderFormatter.AppendCodeLine(0, "{");

            loaderFormatter.AppendCodeLine(1, "/// <summary>");
            loaderFormatter.AppendCodeLine(1, $"/// Dependency injection loader for the library '{project.DefaultNamespace}'");
            loaderFormatter.AppendCodeLine(1, "/// </summary>");
            loaderFormatter.AppendCodeLine(1, $"public class {className}: DependencyInjectionLoader");
            loaderFormatter.AppendCodeLine(1, "{");

            loaderFormatter.AppendCodeLine(2, "/// <summary>");
            loaderFormatter.AppendCodeLine(2, $"/// Loads child libraries that are subscribed to by this library.");
            loaderFormatter.AppendCodeLine(2, "/// </summary>");
            loaderFormatter.AppendCodeLine(2, "/// <param name=\"serviceCollection\">The dependency injection provider to register services with.</param>");
            loaderFormatter.AppendCodeLine(2, "/// <param name=\"configuration\">The source configuration to provide for dependency injection.</param>");
            loaderFormatter.AppendCodeLine(2, $"protected override void LoadLibraries(IServiceCollection serviceCollection, IConfiguration configuration)");
            loaderFormatter.AppendCodeLine(2, "{");
            loaderFormatter.AppendCodeLine(3, "//TODO: Create instances of down stream libraries  that need to be loaded by calling their LibraryLoader classes.");
            loaderFormatter.AppendCodeLine(2, "}");
            loaderFormatter.AppendCodeLine(2);

            loaderFormatter.AppendCodeLine(2, "/// <summary>");
            loaderFormatter.AppendCodeLine(2, $"/// Loads dependency injections that are setup and configured manually.");
            loaderFormatter.AppendCodeLine(2, "/// </summary>");
            loaderFormatter.AppendCodeLine(2, "/// <param name=\"serviceCollection\">The dependency injection provider to register services with.</param>");
            loaderFormatter.AppendCodeLine(2, "/// <param name=\"configuration\">The source configuration to provide for dependency injection.</param>");
            loaderFormatter.AppendCodeLine(2, $"protected override void LoadManualRegistration(IServiceCollection serviceCollection, IConfiguration configuration)");
            loaderFormatter.AppendCodeLine(2, "{");
            loaderFormatter.AppendCodeLine(3, "//TODO: Add manual registration where needed.");
            loaderFormatter.AppendCodeLine(2, "}");
            loaderFormatter.AppendCodeLine(2);

            loaderFormatter.AppendCodeLine(2, "/// <summary>");
            loaderFormatter.AppendCodeLine(2, $"/// Loads child libraries that are subscribed to by this library.");
            loaderFormatter.AppendCodeLine(2, "/// </summary>");
            loaderFormatter.AppendCodeLine(2, "/// <param name=\"serviceCollection\">The dependency injection provider to register services with.</param>");
            loaderFormatter.AppendCodeLine(2, "/// <param name=\"configuration\">The source configuration to provide for dependency injection.</param>");
            loaderFormatter.AppendCodeLine(2, $"protected override void LoadRegistration(IServiceCollection serviceCollection, IConfiguration configuration)");
            loaderFormatter.AppendCodeLine(2, "{");
            loaderFormatter.AppendCodeLine(3, "//DO NOT TOUCH..... Loaded through the software factory.");
            loaderFormatter.AppendCodeLine(2, "}");
            loaderFormatter.AppendCodeLine(2);

            loaderFormatter.AppendCodeLine(1, "}");

            loaderFormatter.AppendCodeLine(0, "}");

            var doc = await project.AddDocumentAsync($"{className}.cs", loaderFormatter.ReturnSource());

            await CommandNotifications.SendCommandNotificationAsync(CommandNotificationStatus.Success, "Dependency Injection", $"Created the '{className}' for the project {project.Name}. ");

            var loaderSourceCode = await doc.GetCSharpSourceModelAsync();

            return loaderSourceCode;
        }
    }
}
