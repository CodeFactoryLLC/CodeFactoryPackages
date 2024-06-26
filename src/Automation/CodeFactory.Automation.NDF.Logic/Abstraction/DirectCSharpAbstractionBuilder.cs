using CodeFactory.Automation.Standard.Logic;
using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CodeFactory.Automation.NDF.Logic.Abstraction
{
    /// <summary>
    /// Automation class that generates C# abstraction contracts.
    /// </summary>
    public static class DirectCSharpAbstractionBuilder
    {
        /// <summary>
        /// Refreshes the interface definition of a service abstraction client.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="contractName">The name of the abstraction contract to refresh.</param>
        /// <param name="sourceContract">The source contract used to refresh the abstraction contract.</param>
        /// <param name="contractProject">The project the abstraction is created in.</param>
        /// <param name="contractFolder">Optional, the target project folder the abstraction contract should be located in. Default value is null.</param>
        /// <returns>The interface model that represents the abstraction contract.</returns>
        /// <exception cref="CodeFactoryException">Raised if required information is missing or automation errors occurred.</exception>
        public static async Task<CsInterface> RefreshCSharpAbstractionContractAsync(this IVsActions source, string contractName,
            CsInterface sourceContract, VsProject contractProject, VsProjectFolder contractFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot refresh the abstraction contract.");

            if (string.IsNullOrEmpty(contractName))
                throw new CodeFactoryException("No contract name was provided, cannot refresh the abstraction contract.");

            if (sourceContract == null)
                throw new CodeFactoryException("Cannot load the source contract, cannot refresh the abstraction contract.");

            if (contractProject == null)
                throw new CodeFactoryException("Cannot load the abstraction contract project, cannot refresh the abstraction contract.");

            CsSource contractSource = (contractFolder != null
                ? (await contractFolder.FindCSharpSourceByInterfaceNameAsync(contractName))?.SourceCode
                : (await contractProject.FindCSharpSourceByInterfaceNameAsync(contractName))?.SourceCode)
                ?? await source.CreateCSharpAbstractionContractAsync(contractName, sourceContract, contractProject, contractFolder);

            return await source.UpdateCSharpAbstractionContractAsync(sourceContract, contractSource);
        }

        /// <summary>
        /// Creates the interface definition of a service abstraction client.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="contractName">The name of the abstraction contract to refresh.</param>
        /// <param name="sourceContract">The source contract used to refresh the abstraction contract.</param>
        /// <param name="contractProject">The project the abstraction is created in.</param>
        /// <param name="contractFolder">Optional, the target project folder the abstraction contract should be located in. Default value is null.</param>
        /// <returns>The source code that hosts the interface model that represents the abstraction contract.</returns>
        /// <exception cref="CodeFactoryException">Raised if required information is missing or automation errors occurred.</exception>
        private static async Task<CsSource> CreateCSharpAbstractionContractAsync(this IVsActions source, string contractName,
            CsInterface sourceContract,
            VsProject contractProject, VsProjectFolder contractFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot create the abstraction contract.");

            if (string.IsNullOrEmpty(contractName))
                throw new CodeFactoryException("No contract name was provided, cannot refresh the abstraction contract.");

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
            contractFormatter.AppendCodeLine(1, $"/// Abstract implementation that supports '{contractName.GenerateCSharpFormattedClassName()}'/>");
            contractFormatter.AppendCodeLine(1, "/// </summary>");
            contractFormatter.AppendCodeLine(1, $"public interface {contractName}");
            contractFormatter.AppendCodeLine(1, "{");


            contractFormatter.AppendCodeLine(1, "}");
            contractFormatter.AppendCodeLine(0, "}");

            var doc = contractFolder != null ? await contractFolder.AddDocumentAsync($"{contractName}.cs", contractFormatter.ReturnSource())
                : await contractProject.AddDocumentAsync($"{contractName}.cs", contractFormatter.ReturnSource());

            return doc == null
                ? throw new CodeFactoryException($"Failed to create the abstraction contract '{contractName}'.")
                : await doc.GetCSharpSourceModelAsync();

        }

        /// <summary>
        /// Updates the interface definition of a service abstraction client.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="sourceContract">The source contract used to refresh the abstraction contract.</param>
        /// <param name="contractSource">The abstraction contract source code file to refresh.</param>
        /// <returns>The interface model that represents the abstraction contract.</returns>
        /// <exception cref="CodeFactoryException">Raised if required information is missing or automation errors occurred.</exception>
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

            var injectMethodSyntax = new MethodBuilderInterface();

            foreach (var missingMethod in missingMethods)
            {
                await injectMethodSyntax.InjectMethodAsync(missingMethod, abstractManager, 2);
            }
            return abstractManager.Container;

        }

        /// <summary>
        /// Refreshes the instance of the abstraction client class.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="clientName">The class name of the client to be refreshed.</param>
        /// <param name="abstractionContract">The abstraction interface the client consumes.</param>
        /// <param name="logicContract">The source service class the client is calling.</param>
        /// <param name="abstractionProject">The abstraction project that hosts the client.</param>
        /// <param name="abstractionFolder">The project folder where the abstraction client is located.</param>
        /// <returns>The client class model that was refreshed.</returns>
        /// <exception cref="CodeFactoryException">Raised if configuration information is missing or automation errors occurred.</exception>
        public static async Task<CsClass> RefreshAbstractionClass(this IVsActions source, string clientName, CsInterface abstractionContract,
            CsInterface logicContract, VsProject abstractionProject, VsProjectFolder abstractionFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot refresh the abstraction.");

            if (string.IsNullOrEmpty(clientName))
                throw new CodeFactoryException("The client name was not provided, cannot refresh the abstraction.");

            if (abstractionContract == null)
                throw new CodeFactoryException("Cannot load the abstraction contract, cannot refresh the abstraction.");

            if (logicContract == null)
                throw new CodeFactoryException("Cannot load the service class, cannot refresh the abstraction.");

            if (abstractionProject == null)
                throw new CodeFactoryException("Cannot load the abstraction project, cannot refresh the abstraction.");

            var abstractionSource = (abstractionFolder != null
                ? (await abstractionFolder.FindCSharpSourceByClassNameAsync(clientName))?.SourceCode
                : (await abstractionProject.FindCSharpSourceByClassNameAsync(clientName))?.SourceCode);

            var abstractionCreated = false;
            if (abstractionSource == null)
            {
                abstractionCreated = true;
                abstractionSource = await source.CreateAbstractionClassAsync(clientName, abstractionContract, logicContract, abstractionProject, abstractionFolder)
                    ?? throw new CodeFactoryException($"Could not create the client abstraction '{clientName}', cannot refresh the abstraction client.");
            }

            var clientClass = await source.UpdateAbstractionClassAsync(abstractionSource, abstractionContract, logicContract, abstractionProject, abstractionFolder);

            if (abstractionCreated) await source.RegisterTransientClassesAsync(abstractionProject, false);

            return clientClass;
        }

        /// <summary>
        /// Creates the instance of the abstraction client class.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="clientName">The class name of the client to be refreshed.</param>
        /// <param name="abstractionContract">The abstraction interface the client consumes.</param>
        /// <param name="logicContract">The project that hosts the service being consumed.</param>
        /// <param name="abstractionProject">The abstraction project that hosts the client.</param>
        /// <param name="abstractionFolder">The project folder where the abstraction client is located.</param>
        /// <returns>The source code that contains the client class model that was created.</returns>
        /// <exception cref="CodeFactoryException">Raised if configuration information is missing or automation errors occurred.</exception>
        private static async Task<CsSource> CreateAbstractionClassAsync(this IVsActions source, string clientName, CsInterface abstractionContract,
            CsInterface logicContract, VsProject abstractionProject, VsProjectFolder abstractionFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot create the abstraction.");

            if (string.IsNullOrEmpty(clientName))
                throw new CodeFactoryException("The client name was not provided, cannot create the abstraction.");

            if (abstractionContract == null)
                throw new CodeFactoryException("Cannot load the abstraction contract, cannot create the abstraction.");

            if (abstractionProject == null)
                throw new CodeFactoryException("Cannot load the abstraction project, cannot create the abstraction.");

            if (logicContract == null)
                throw new CodeFactoryException("Cannot load the service project, cannot create the abstraction.");

            var sourceNamespace = abstractionFolder != null
                ? await abstractionFolder.GetCSharpNamespaceAsync()
                : abstractionProject.DefaultNamespace;

            if (string.IsNullOrEmpty(sourceNamespace)) throw new CodeFactoryException("Could not identify the target namespace for the abstraction, abstraction cannot be created.");

            var abstractionName = clientName;

            var logicTypeName = logicContract.GenerateInterfaceTypeName();

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using System;");
            sourceFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            sourceFormatter.AppendCodeLine(0, "using System.Linq;");
            sourceFormatter.AppendCodeLine(0, "using System.Text;");
            sourceFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            sourceFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            sourceFormatter.AppendCodeLine(0, "using CodeFactory.NDF;");
            if (sourceNamespace != abstractionContract.Namespace) sourceFormatter.AppendCodeLine(0, $"using {abstractionContract.Namespace};");
            if (sourceNamespace != abstractionProject.DefaultNamespace) sourceFormatter.AppendCodeLine(0, $"using {abstractionProject.DefaultNamespace};");
            sourceFormatter.AppendCodeLine(0, $"using {logicContract.Namespace};");
            sourceFormatter.AppendCodeLine(0);
            sourceFormatter.AppendCodeLine(0, $"namespace {sourceNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Direct abstraction implementation for abstraction contract '{abstractionContract.Name}'");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, $"public class {abstractionName} : {abstractionContract.Name}");
            sourceFormatter.AppendCodeLine(1, "{");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Logger used by this abstraction class.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "private readonly ILogger _logger;");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Logic class directly called by this abstraction");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, $"private readonly {logicTypeName} _logic;");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, $"public {abstractionName}(ILogger<{abstractionName}> logger, {logicTypeName} logic)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "_logger = logger;");
            sourceFormatter.AppendCodeLine(3, "_logic = logic;");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");


            var sourceDocument = (abstractionFolder != null
                                     ? await abstractionFolder.AddDocumentAsync($"{abstractionName}.cs", sourceFormatter.ReturnSource())
                                     : await abstractionProject.AddDocumentAsync($"{abstractionName}.cs", sourceFormatter.ReturnSource()))
                                 ?? throw new CodeFactoryException("Was not able to create the abstraction class document, cannot create the abstraction.");

            return (await sourceDocument.GetCSharpSourceModelAsync())
                ?? throw new CodeFactoryException($"Was not able to load the abstraction source code implementation for '{abstractionName}'");

        }

        /// <summary>
        /// Updates the instance of the abstraction client class.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="logicContract">The source logic class the client is calling.</param>
        /// <param name="abstractionSource">The abstraction class being modified.</param>
        /// <param name="abstractionContract">The abstraction interface the client consumes.</param>
        /// <param name="abstractionProject">The abstraction project that hosts the client.</param>
        /// <param name="abstractionFolder">The project folder where the abstraction client is located.</param>
        /// <param name="useNDF">Optional, flag that determines if the NDF libraries are used, default true.</param>
        /// <param name="supportLogging">Optional, flag that determines if logging is supported, default true.</param>
        /// <param name="loggerFieldName">Optional, the name of the logger field if logging is supported, default value is '_logger'</param>
        /// <param name="logLevel">Optional, the target logging level for logging messages, default value is Information.</param>
        /// <returns>The client class model that was refreshed.</returns>
        /// <exception cref="CodeFactoryException">Raised if configuration information is missing or automation errors occurred.</exception>
        private static async Task<CsClass> UpdateAbstractionClassAsync(this IVsActions source, CsSource abstractionSource, CsInterface abstractionContract,
            CsInterface logicContract, VsProject abstractionProject, VsProjectFolder abstractionFolder = null, bool useNDF = true, bool supportLogging = true,
            string loggerFieldName = "_logger", LogLevel logLevel = LogLevel.Information)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot update the abstraction.");

            if (abstractionSource == null)
                throw new CodeFactoryException("Cannot load the abstraction source code, cannot update the abstraction.");

            if (abstractionContract == null)
                throw new CodeFactoryException("Cannot load the abstraction contract, cannot update the abstraction.");

            if (logicContract == null)
                throw new CodeFactoryException("Cannot load the logic class, cannot update the abstraction.");

            if (abstractionProject == null)
                throw new CodeFactoryException("Cannot load the abstraction project, cannot update the abstraction.");

            var currentSource = abstractionSource;

            var abstractionClass = currentSource.Classes.FirstOrDefault()
                                   ?? throw new CodeFactoryException("Cannot load the abstraction class from source code, cannot update the abstraction");

            var abstractMethods = abstractionClass.Methods;

            var contractMethods = abstractionContract.GetAllInterfaceMethods();

            var missingMethods = contractMethods.Where(c =>
            {
                var contractSignature = c.GetComparisonHashCode();

                return !abstractMethods.Any(a => contractSignature == a.GetComparisonHashCode());
            }).ToList();

            if (!missingMethods.Any()) return abstractionClass;

            var missingLogicMethods = missingMethods.Where(m =>
            {
                var methodSignature = m.GetComparisonHashCode();

                return logicContract.Methods.Any(l => methodSignature == l.GetComparisonHashCode());
            }).ToList();

            if (!missingLogicMethods.Any()) return abstractionClass;



            var abstractManager = new SourceClassManager(currentSource, abstractionClass, source);
            abstractManager.LoadNamespaceManager();

            await abstractManager.UsingStatementAddAsync(logicContract.Namespace);




            ILoggerBlock loggerBlock = null;

            if (supportLogging) loggerBlock = useNDF ? new LoggerBlockNDFLogger(loggerFieldName) as ILoggerBlock : new LoggerBlockMicrosoft(loggerFieldName) as ILoggerBlock;

            var catchBlocks = new List<ICatchBlock>();

            var boundsChecks = new List<IBoundsCheckBlock>();

            if (useNDF)
            {
                catchBlocks.Add(new CatchBlockManagedExceptionNDFException(loggerBlock));
                catchBlocks.Add(new CatchBlockExceptionNDFException(loggerBlock));

                boundsChecks.Add(new BoundsCheckBlockStringNDFException(true, loggerBlock));
                boundsChecks.Add(new BoundsCheckBlockNullNDFException(true, loggerBlock));
            }
            else
            {
                catchBlocks.Add(new CatchBlockStandard(loggerBlock));

                boundsChecks.Add(new BoundsCheckBlockString(true, loggerBlock));
                boundsChecks.Add(new BoundsCheckBlockNull(true, loggerBlock));
            }

            var tryBlock = new TryBlockStandard(loggerBlock, catchBlocks);

            //Creating the builders to generate code by member type.
            IMethodBuilder methodBuilder = new MethodBuilderStandard(loggerBlock, boundsChecks, tryBlock);


            foreach (var contractMethod in missingLogicMethods)
            {
                var logicReturnType = contractMethod.ReturnType.TaskReturnType();
                var returnsData = logicReturnType != null;



                var contentFormatter = new SourceFormatter();


                var paramList = contractMethod.Parameters.Select(p => p.Name).ToList();
                var paramString = string.Join(", ", paramList);

                if (returnsData)
                {
                    contentFormatter.AppendCodeLine(0, $"result = await _logic.{contractMethod.Name}({paramString});");
                }
                else
                {
                    contentFormatter.AppendCodeLine(0, $"await _logic.{contractMethod.Name}({paramString});");
                }

                var trySyntax = contentFormatter.ReturnSource();
                var methodSyntax = await methodBuilder.BuildMethodAsync(contractMethod, abstractManager, 2, includeAttributes: true, defaultLogLevel: logLevel, syntax: trySyntax);

                if (methodSyntax == null) continue;

                await abstractManager.ConstructorsAddAfterAsync(methodSyntax);

                contentFormatter.ResetFormatter();
            }

            return abstractManager.Container;
        }

    }
}
