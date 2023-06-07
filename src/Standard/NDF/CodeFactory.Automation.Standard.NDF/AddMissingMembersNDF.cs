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
using CodeFactory.Automation.Standard.NDF.Logic;

namespace CodeFactory.Automation.Standard.NDF
{
    /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class AddMissingMembersNDF : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Add Missing Interface Members NDF";
        private static readonly string commandDescription = "Adds interface members that are missing from the implementation of the class using the NDF library.";



#pragma warning disable CS1998

        /// <inheritdoc />
        public AddMissingMembersNDF(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(AddMissingMembersNDF).FullName;

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            return null;
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
                //Getting the first class in the source code file.
                var sourceClass = result.SourceCode?.Classes?.FirstOrDefault();

                //enable if only a class was found.
                isEnabled = sourceClass != null;

                //If enabled if no interface members are missing then do not show.
                if (isEnabled) isEnabled = sourceClass.GetMissingInterfaceMembers().Any();

                if (isEnabled)
                {
                    var project = await result.GetHostingProjectAsync();

                    isEnabled = project != null;

                    if (isEnabled)
                    {
                        var references = await project.GetProjectReferencesAsync();

                        isEnabled = references != null;

                        if(isEnabled) isEnabled = (references.Any(r => r.Name.StartsWith(AddMissingMembers.MicrosoftLoggingNamespace)) & references.Any(r => r.Name == AddMissingMembers.NDFNamespace));
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
                var sourceClass = result?.SourceCode?.Classes?.FirstOrDefault()
                                  ?? throw new CodeFactoryException(
                                      "The class could not be loaded cannot add members.");

                var updatedClass = await  VisualStudioActions.AddMissingMembersStandardNDFAsync(result.SourceCode, sourceClass, true);

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
