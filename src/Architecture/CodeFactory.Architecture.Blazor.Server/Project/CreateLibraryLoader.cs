using CodeFactory.Architecture.Blazor.Server.CSharpFile;
using CodeFactory.Automation.NDF.Logic;
using CodeFactory.Automation.NDF.Logic.DependencyInjection;
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

namespace CodeFactory.Architecture.Blazor.Server.Project
{
    /// <summary>
    /// Code factory command for automation of a project when selected from solution explorer.
    /// </summary>
    public class CreateLibraryLoader : ProjectCommandBase
    {
        private static readonly string commandTitle = "Create Library Loader";
        private static readonly string commandDescription = "Create a instance of the LibraryLoader class if it is missing from the project.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public CreateLibraryLoader(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }
#pragma warning disable CS1998

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(CreateLibraryLoader).FullName;

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            return null;
        }

        /// <summary>
        /// Registers the default configuration with the configuration manager in CodeFactory.
        /// </summary>
        public static void RegisterDefaultConfiguration()
        {
            var command = new CreateLibraryLoader(null, null);
            var config = command.LoadExternalConfigDefinition();
            config?.RegisterCommandWithDefaultConfiguration();
        }
        #endregion

        #region Overrides of VsCommandBase<VsProject>

        /// <summary>
        /// Validation logic that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsProject result)
        {
            //Result that determines if the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {

                var references = await result.GetProjectReferencesAsync();

                //Checking for dependency injection libraries and NDF.
                isEnabled = references.Any(r => r.Name == "Microsoft.Extensions.DependencyInjection.Abstractions");
                if(isEnabled) isEnabled = references.Any(r => r.Name == "Microsoft.Extensions.Configuration.Abstractions");
                if (isEnabled) isEnabled = references.All(r => r.Name == "CodeFactory.NDF");

                if (isEnabled)
                { 
                    //Checking all c# files at the root of the project to see if library loader has already been implemented.
                    var projectFiles = (await result.GetChildrenAsync(false, true)).Where(m => m.ModelType == VisualStudioModelType.CSharpSource).Cast<VsCSharpSource>().ToList();
                    isEnabled = projectFiles.Any(f => (f.SourceCode?.Classes?.Any(c => c.Name == "LibraryLoader")).GetValueOrDefault(false));
                }
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while checking if the solution explorer project command {commandTitle} is enabled. ",
                    unhandledError);
                isEnabled = false;
            }

            return isEnabled;
        }

        /// <summary>
        /// Code factory framework calls this method when the command has been executed. 
        /// </summary>
        /// <param name="result">The code factory model that has generated and provided to the command to process.</param>
        public override async Task ExecuteCommandAsync(VsProject result)
        {
            try
            {
                var libraryLoader = VisualStudioActions.CreateLibraryLoaderClassAsync(result);
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while executing the solution explorer project command {commandTitle}. ",
                    unhandledError);

            }
        }

        #endregion
    }
}
