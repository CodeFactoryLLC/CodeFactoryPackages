using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic
{
    /// <summary>
    /// Automation class that will clone the implementation of one interface and create or update another interface method implementations. 
    /// </summary>
    public static class CloneInterfaceBuilder
    {
        /// <summary>
        /// Clones an existing interface and creates a new interface definition and clones all the methods in the interface.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="sourceInterface">The source interface to be cloned.</param>
        /// <param name="includeAttributes">Flag that determines if the attributes on member should be included in the implementation of the new member, default is false.</param>
        /// <param name="targetProject">The target project the interface will be created in, optional default is null.</param>
        /// <param name="targetFolder">The target project folder the interface will be crearted in, optional default is null.</param>
        /// <param name="nameManagement">Optional information used to format the name of the new interface definition, default is null.</param>
        /// <param name="summaryText">The summary text to put in the XML documentation for the summary of the interface.</param>
        /// <returns>The target interface data model.</returns>
        /// <exception cref="CodeFactoryException">Raised when required data is missing or processing errors occured.</exception>

        public static async  Task<CsInterface> CloneInterfaceAsync(this IVsActions source,CsInterface sourceInterface, 
            bool includeAttributes = false, VsProject targetProject = null, VsProjectFolder targetFolder = null, NameManagement nameManagement = null, string summaryText = null)
        { 
            CsInterface result = null;
            
            if(sourceInterface == null)
                throw new CodeFactoryException("Interface model was not provided, cannot refresh the target interface.");
            
            if(targetProject == null & targetFolder == null)
            throw new CodeFactoryException("No target project or folder was provided cannot locate the target interface to refresh.");

            CsSource targetSource = null;

            string targetInterfaceName = nameManagement != null
                ? nameManagement.FormatName(sourceInterface.Name,"I") : sourceInterface.Name;

            if(targetFolder != null) targetSource = (await targetFolder.FindCSharpSourceByInterfaceNameAsync(targetInterfaceName))?.SourceCode;

            if(targetSource == null & targetProject != null) targetSource =  (await targetProject.FindCSharpSourceByInterfaceNameAsync(targetInterfaceName))?.SourceCode;

            if(targetSource == null) targetSource = await source.CreateInterfaceAsync(targetInterfaceName,targetProject,targetFolder,summaryText);

            if(targetSource == null)
                throw new CodeFactoryException($"The target interface '{targetInterfaceName}' could not be created cannot clone the interface.");

            return await source.UpdateInterfaceAsync(sourceInterface,targetSource);
        }

        /// <summary>
        /// Creates a new public interface definition.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="interfaceName">The name of the new interface implementation.</param>
        /// <param name="targetProject">The target project the interface will be created in, optional default is null.</param>
        /// <param name="targetFolder">The target project folder the interface will be crearted in, optional default is null.</param>
        /// <param name="summaryText">The summary text to put in the XML documentation for the summary of the interface.</param>
        /// <returns>The source definition of the created interface.</returns>
        /// <exception cref="CodeFactoryException">Raised if required information is missing.</exception>
        private static async Task<CsSource> CreateInterfaceAsync(this IVsActions source, string interfaceName, VsProject targetProject = null,VsProjectFolder targetFolder = null,string summaryText = null)
        { 
            if(source == null) throw new CodeFactoryException("CodeFactory automation was not provided, cannot create the interface.");
            
            if(targetProject == null & targetFolder == null)
            throw new CodeFactoryException("No target project or folder was provided, cannot create the interface.");

            var targetNamespace = targetFolder != null
                ? await targetFolder.GetCSharpNamespaceAsync()
                : targetProject.DefaultNamespace;

            SourceFormatter formatter = new SourceFormatter();

            formatter.AppendCodeLine(0,"using System;");
            formatter.AppendCodeLine(0,"using System.Collections.Generic;");
            formatter.AppendCodeLine(0,"using System.Linq;");
            formatter.AppendCodeLine(0,"using System.Text;");
            formatter.AppendCodeLine(0,"using System.Threading.Tasks;");
            formatter.AppendCodeLine(0,$"namespace {targetNamespace}");
            formatter.AppendCodeLine(0,"{");

            formatter.AppendCodeLine(1,$"/// <summary>");
            formatter.AppendCodeLine(1,summaryText != null ? $"/// {summaryText}" : "/// Contract definition for implementation.");
            formatter.AppendCodeLine(1,$"/// </summary>");
            formatter.AppendCodeLine(1,$"public interface {interfaceName}");
            formatter.AppendCodeLine(1,"{");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(1,"}");

            formatter.AppendCodeLine(0,"}");

            var doc = targetFolder != null
                
            ? await targetFolder.AddDocumentAsync($"{interfaceName}.cs",formatter.ReturnSource())
            : await targetProject.AddDocumentAsync($"{interfaceName}.cs",formatter.ReturnSource());

            return await doc.GetCSharpSourceModelAsync();
        }

        /// <summary>
        /// Updates a target interfaces implementation with the source interface method members.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="sourceInterface">The source interface to clone methods from.</param>
        /// <param name="targetSource">The source implementation that contains the target interface to be updated.</param>
        /// <param name="includeAttributes">Flag that determines if the attributes on member should be included in the implementation of the new member, default is false.</param>
        /// <returns>The target interface that has been updated if changes were identified. </returns>
        /// <exception cref="CodeFactoryException">Raised when required information is missing. </exception>
        private static async Task<CsInterface> UpdateInterfaceAsync(this IVsActions source, CsInterface sourceInterface,CsSource targetSource,bool includeAttributes = false)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot update the abstraction contract.");

            if (sourceInterface == null)
                throw new CodeFactoryException("Cannot load the source interface, cannot update the target interface.");

            if (targetSource == null)
                throw new CodeFactoryException("Cannot load current target interface source code, cannot update the target interface.");

            var currentSource = targetSource;

            var contractInterface = currentSource.Interfaces.FirstOrDefault()
                ?? throw new CodeFactoryException("Could not load the target interface from the provided source code, cannot update the target interface.");

            var interfaceMethods = contractInterface.Methods;

            var contractMethods = sourceInterface.Methods;

            var missingMethods = contractMethods.Where(m =>
            {
                var contractHash = m.GetComparisonHashCode();
                return !interfaceMethods.Any(c => c.GetComparisonHashCode() == contractHash);
            }).ToList();


            if (!missingMethods.Any()) return contractInterface;

            var interfaceManager = new SourceInterfaceManager(currentSource, contractInterface, source);

            interfaceManager.LoadNamespaceManager();

            var injectMethodSyntax = new MethodBuilderInterface();

            foreach (var missingMethod in missingMethods)
            {
                await injectMethodSyntax.InjectMethodAsync(missingMethod, interfaceManager, 2,includeAttributes:includeAttributes);
            }

            return interfaceManager.Container;

        }
    }
}
