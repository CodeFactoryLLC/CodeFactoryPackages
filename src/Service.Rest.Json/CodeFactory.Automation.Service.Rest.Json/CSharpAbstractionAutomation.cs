using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.WinVs.Models.CSharp.Builder;

namespace CodeFactory.Automation.Service.Rest.Json
{
    /// <summary>
    /// Automation class that generates C# abstraction contracts.
    /// </summary>
    public static class CSharpAbstractionAutomation
    {
        public static async Task<CsInterface> RefreshCSharpAbstractionContractAsync(this IVsActions source,
            CsInterface sourceContract, VsProject contractProject, VsProjectFolder contractFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot refresh the abstraction contract.");

            if (sourceContract == null)
                throw new CodeFactoryException("Cannot load the source contract, cannot refresh the abstraction contract.");

            if (contractProject == null)
                throw new CodeFactoryException("Cannot load the abstraction contract project, cannot refresh the abstraction contract.");

            CsSource contractSource = (contractFolder != null
                ? (await contractFolder.FindCSharpSourceByInterfaceNameAsync(sourceContract.Name))?.SourceCode
                : (await contractProject.FindCSharpSourceByInterfaceNameAsync(sourceContract.Name))?.SourceCode)
                ?? await source.CreateCSharpAbstractionContractAsync(sourceContract, contractProject, contractFolder);

            return await source.UpdateCSharpAbstractionContractAsync(sourceContract, contractSource);
        }

        private static async Task<CsSource> CreateCSharpAbstractionContractAsync(this IVsActions source,
            CsInterface sourceContract,
            VsProject contractProject, VsProjectFolder contractFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot create the abstraction contract.");

            if (sourceContract == null)
                throw new CodeFactoryException("Cannot load the source contract, cannot create the abstraction contract.");

            if (contractProject == null)
                throw new CodeFactoryException("Cannot load the abstraction contract project, cannot create the abstraction contract.");

            string defaultNamespace = contractFolder != null
                ? await contractFolder.GetCSharpNamespaceAsync()
                : contractProject.DefaultNamespace;

            SourceFormatter contractFormatter = new SourceFormatter();
            contractFormatter.AppendCodeLine(0, "using System;");
            contractFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            contractFormatter.AppendCodeLine(0, "using System.Text;");
            contractFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            contractFormatter.AppendCodeLine(0, $"namespace {defaultNamespace}");
            contractFormatter.AppendCodeLine(0, "{");
            contractFormatter.AppendCodeLine(1, "/// <summary>");
            contractFormatter.AppendCodeLine(1, $"/// Abstract implementation that supports '{sourceContract.Name.GenerateCSharpFormattedClassName()}'/>");
            contractFormatter.AppendCodeLine(1, "/// </summary>");
            contractFormatter.AppendCodeLine(1, $"public interface {sourceContract.Name}");
            contractFormatter.AppendCodeLine(1, "{");


            contractFormatter.AppendCodeLine(1, "}");
            contractFormatter.AppendCodeLine(0, "}");

            var doc = contractFolder != null ? await contractFolder.AddDocumentAsync($"{sourceContract.Name}.cs", contractFormatter.ReturnSource())
                : await contractProject.AddDocumentAsync($"{sourceContract.Name}.cs", contractFormatter.ReturnSource());

            return doc == null
                ? throw new CodeFactoryException($"Failed to create the abstraction contract '{sourceContract.Name}'.")
                : await doc.GetCSharpSourceModelAsync();

        }

        private static async Task<CsInterface> UpdateCSharpAbstractionContractAsync(this IVsActions source,
            CsInterface sourceContract, CsSource contractSource)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot update the abstraction contract.");

            if (sourceContract == null)
                throw new CodeFactoryException("Cannot load the source contract, cannot update the abstraction contract.");

            if (contractSource == null)
                throw new CodeFactoryException("Cannot load current abstraction source code, cannot update the abstraction contract.");


            var currentSource = contractSource;

            var contractInterface = currentSource.Interfaces.FirstOrDefault()
                ?? throw new CodeFactoryException("Could not load the abstraction contract from the provided source code, cannot update the abstraction contract.");

            var abstractMethods = contractInterface.Methods;

            var contractMethods = sourceContract.Methods;

            var missingMethods = contractMethods.Where(m =>
            {
                var contractHash = m.GetComparisonHashCode();
                return !abstractMethods.Any(c => c.GetComparisonHashCode() == contractHash);
            }).ToList();

            if (!missingMethods.Any()) return contractInterface;

            var abstractManager = new SourceInterfaceManager(currentSource, contractInterface, source);

            abstractManager.LoadNamespaceManager();

            ILoggerBlock logFormatter = null;

            var injectMethodSyntax = new method

            foreach (var missingMethod in missingMethods)
            {
                await injectMethodSyntax.InjectSyntaxAsync(abstractManager, missingMethod, 2);
            }
            return abstractManager.Container;

        }
    }
}
