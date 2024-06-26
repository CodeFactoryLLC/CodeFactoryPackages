using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.WinVs.Models.ProjectSystem;
using Microsoft.Extensions.Logging;
using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.Automation.Standard.Logic;

namespace CodeFactory.Automation.NDF.Logic.General
{
    /// <summary>
    /// Builder that generates a standard implementation class that has a supporting interface that defines the methods of the implementation class.
    /// </summary>
    public static class ImplementationBuilder
    {
        /// <summary>
        /// Refreshes the implementation of a implementation class from a supporting interface contract.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="implementationName">The name of the implementation class.</param>
        /// <param name="contractName">The name of the implementation interface.</param>
        /// <param name="implementationProject">The hosting project for the implementation class.</param>
        /// <param name="contractProject">The hosting project for the implementation interface.</param>
        /// <param name="useNDF">Optional, Flag that determines if the implementation class supports the NDF library, default value is true.</param>
        /// <param name="supportLogging">Optional, Flag that determines if the implementation class supports logging, default value is true.</param>
        /// <param name="implementationFolder">Optional, Project folder where the implementation class should be found, default value is null.</param>
        /// <param name="contractFolder">Optional, Project folder where the implementation interface should be found, default value is null.</param>
        /// <param name="additionNamespaces">Optional, List of additional using statements to add to a new implementation class when created.</param>
        /// <param name="loggerFieldName">Optional, The name assigned to the logger field, default value is _logger.</param>
        /// <param name="logLevel">Optional, The level of logging that is the default when logging, default value is set to Information.</param>
        /// <returns>Class model of the refreshed implementation class.</returns>
        /// <exception cref="CodeFactoryException">Raised when required information is missing, or when automation errors occur.</exception>
        public static async Task<CsClass> RefreshImplementationAsync(this IVsActions source, string implementationName, string contractName, VsProject implementationProject,
            VsProject contractProject, bool useNDF = true,bool supportLogging = true, VsProjectFolder implementationFolder = null, VsProjectFolder contractFolder = null,
            List<ManualUsingStatementNamespace> additionNamespaces = null, string loggerFieldName = "_logger", LogLevel logLevel = LogLevel.Information)
        { 
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot refresh implementation class.");

            if(string.IsNullOrEmpty(implementationName))
                throw new CodeFactoryException("The implementation name was not provided, cannot create the implementation class.");

            if(string.IsNullOrEmpty(contractName))
                throw new CodeFactoryException("The contract name was not provided, cannot create the implementation class.");

            if (implementationProject == null) throw new CodeFactoryException("The implementation project was not provided, cannot refresh the implementation class.");

            if (contractProject == null)
                throw new CodeFactoryException("The contract project was not provided, cannot refresh the implementation class.");

            CsInterface contractInterface = contractFolder != null ? (await contractFolder.FindCSharpSourceByInterfaceNameAsync(contractName))?.SourceCode?.Interfaces?.FirstOrDefault() 
                : (await contractProject.FindCSharpSourceByInterfaceNameAsync(contractName))?.SourceCode?.Interfaces?.FirstOrDefault();

            if (contractInterface == null)
                throw new CodeFactoryException($"Could not find interface {contractName}, cannot refresh the implementation class");

            CsSource implementationSource = implementationFolder != null
                ? (await implementationFolder.FindCSharpSourceByClassNameAsync(implementationName))?.SourceCode
                : (await implementationProject.FindCSharpSourceByClassNameAsync(implementationName))?.SourceCode;

            bool newImplementationClass = false;


            if(implementationSource == null)
            { 
                newImplementationClass = true;
                
                implementationSource = await source.CreateImplementationClassAsync(implementationName,implementationProject,contractInterface,useNDF,supportLogging,implementationFolder,additionNamespaces,loggerFieldName)
                    ?? throw new CodeFactoryException($"Could not create the implementation class '{implementationName}' cannot refresh the implementation.");
            }

            CsClass implementationClass = await source.UpdateImplementationClassAsync(implementationSource,useNDF,supportLogging,loggerFieldName,logLevel);

            if(newImplementationClass) await source.RegisterTransientClassesAsync(implementationProject,false);

            return implementationClass;
            
        }



        /// <summary>
        /// Creates a instance of a implementation class from a supporting interface contract.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="implementationName">The name of the implementation class.</param>
        /// <param name="implementationProject">The hosting project for the implementation class.</param>
        /// <param name="useNDF">Optional, Flag that determines if the implementation class supports the NDF library, default value is true.</param>
        /// <param name="supportLogging">Optional, Flag that determines if the implementation class supports logging, default value is true.</param>
        /// <param name="implementationFolder">Optional, Project folder where the implementation class should be found, default value is null.</param>
        /// <param name="additionNamespaces">Optional, List of additional using statements to add to a new implementation class when created.</param>
        /// <param name="loggerFieldName">Optional, The name assigned to the logger field, default value is _logger.</param>
        /// <returns>Source code model of the created implementation class.</returns>
        /// <exception cref="CodeFactoryException">Raised when required information is missing, or when automation errors occur.</exception>
        private static async Task<CsSource> CreateImplementationClassAsync(this IVsActions source, string implementationName, VsProject implementationProject,
            CsInterface implementationContract, bool useNDF = true, bool supportLogging = true, VsProjectFolder implementationFolder = null,
            List<ManualUsingStatementNamespace> additionNamespaces = null, string loggerFieldName = "_logger")
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot create the implementation class.");

            if(string.IsNullOrEmpty(implementationName))
                throw new CodeFactoryException("The implementation class name was not provided, cannot create the implementation class.");

            if (implementationContract == null)
                throw new CodeFactoryException("The implementation interface was not provided, cannot create the implementation class.");

            if (implementationProject == null)
                throw new CodeFactoryException("The implementation project for the repository was not provided, cannot create the implementation class.");

            string defaultNamespace = implementationFolder != null
                ? await implementationFolder.GetCSharpNamespaceAsync()
                : implementationProject.DefaultNamespace;

            string implementationClassName = implementationName;

            SourceFormatter repoFormatter = new SourceFormatter();

            repoFormatter.AppendCodeLine(0, "using System;");
            repoFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            repoFormatter.AppendCodeLine(0, "using System.Text;");
            repoFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");

            if (supportLogging) 
                repoFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            if (useNDF)
            {
                repoFormatter.AppendCodeLine(0, "using CodeFactory.NDF;");
            }

            if (additionNamespaces != null)
            {
                foreach (var implementationClassNamespace in additionNamespaces)
                {
                    repoFormatter.AppendCodeLine(0, implementationClassNamespace.HasAlias
                        ? $"using {implementationClassNamespace.Alias} = {implementationClassNamespace.ReferenceNamespace};"
                        : $"using {implementationClassNamespace.ReferenceNamespace};");
                }
            }
            repoFormatter.AppendCodeLine(0, $"using {implementationContract.Namespace};");
            repoFormatter.AppendCodeLine(0);
            repoFormatter.AppendCodeLine(0, $"namespace {defaultNamespace}");
            repoFormatter.AppendCodeLine(0, "{");
            repoFormatter.AppendCodeLine(1, "/// <summary>");
            repoFormatter.AppendCodeLine(1, $"/// Implementation class that supports the contract <see cref=\"{implementationContract.Name}\"/>");
            repoFormatter.AppendCodeLine(1, "/// </summary>");
            repoFormatter.AppendCodeLine(1, $"public class {implementationClassName}:{implementationContract.Name}");
            repoFormatter.AppendCodeLine(1, "{");

            if (supportLogging)
            { 
                repoFormatter.AppendCodeLine(2, "/// <summary>");
                repoFormatter.AppendCodeLine(2, "/// Logger used by the implementation class.");
                repoFormatter.AppendCodeLine(2, "/// </summary>");
                repoFormatter.AppendCodeLine(2, $"private readonly ILogger {loggerFieldName};");
                repoFormatter.AppendCodeLine(2);
                repoFormatter.AppendCodeLine(2, "/// <summary>");
                repoFormatter.AppendCodeLine(2, "/// Creates a new instance of the implementation class.");
                repoFormatter.AppendCodeLine(2, "/// </summary>");
                repoFormatter.AppendCodeLine(2, "/// <param name=\"logger\">Logger used with the repository.</param>");
                repoFormatter.AppendCodeLine(2, $"public {implementationClassName}(ILogger<{implementationClassName}> logger)");
                repoFormatter.AppendCodeLine(2, "{");
                repoFormatter.AppendCodeLine(3, $"{loggerFieldName} = logger;");
                repoFormatter.AppendCodeLine(2, "}");
                repoFormatter.AppendCodeLine(2);
            }
            else
            { 
                repoFormatter.AppendCodeLine(2);
                repoFormatter.AppendCodeLine(2, "/// <summary>");
                repoFormatter.AppendCodeLine(2, "/// Creates a new instance of the implementation class.");
                repoFormatter.AppendCodeLine(2, "/// </summary>");
                repoFormatter.AppendCodeLine(2, $"public {implementationClassName}()");
                repoFormatter.AppendCodeLine(2, "{");
                repoFormatter.AppendCodeLine(3);
                repoFormatter.AppendCodeLine(2, "}");
                repoFormatter.AppendCodeLine(2);
            }

            repoFormatter.AppendCodeLine(1, "}");
            repoFormatter.AppendCodeLine(0, "}");

            var doc = implementationFolder != null ? await implementationFolder.AddDocumentAsync($"{implementationClassName}.cs", repoFormatter.ReturnSource())
                : await implementationProject.AddDocumentAsync($"{implementationClassName}.cs", repoFormatter.ReturnSource());

            return doc == null
                ? throw new CodeFactoryException($"Failed to create the implementation class '{implementationClassName}' cannot complete refresh of the class.")
                : await doc.GetCSharpSourceModelAsync();
        }

        /// <summary>
        /// Updates an instance of a implementation class from a supporting interface contract.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="useNDF">Optional, Flag that determines if the implementation class supports the NDF library, default value is true.</param>
        /// <param name="supportLogging">Optional, Flag that determines if the implementation class supports logging, default value is true.</param>
        /// <param name="loggerFieldName">Optional, The name assigned to the logger field, default value is _logger.</param>
        /// <returns>Class model of the refreshed implementation class.</returns>
        /// <exception cref="CodeFactoryException">Raised when required information is missing, or when automation errors occur.</exception>
        private static async Task<CsClass> UpdateImplementationClassAsync(this IVsActions source, CsSource implementationSource, bool useNDF = true, bool supportLogging = true,
            string loggerFieldName = "_logger", LogLevel logLevel = LogLevel.Information)
        { 
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot update the implementation class.");

            if (implementationSource == null) throw new CodeFactoryException("The source for the implementation class could not be loaded, cannot update the implementation class.");

            var implementationClass = implementationSource.Classes.FirstOrDefault();

            CsClass updatedImplementationClass = implementationClass;

            if(implementationClass == null) throw new CodeFactoryException("The implementation class could not be loaded, cannot update the implementation class.");

            var missingMembers = implementationClass.GetMissingInterfaceMembers();

            if( !missingMembers.Any() ) return updatedImplementationClass;

            ILoggerBlock loggerBlock = null;

            if(supportLogging) loggerBlock = useNDF ? new LoggerBlockNDFLogger(loggerFieldName) as ILoggerBlock: new LoggerBlockMicrosoft(loggerFieldName) as ILoggerBlock;
              
            var catchBlocks = new List<ICatchBlock>();

            var boundsChecks = new List<IBoundsCheckBlock>();

            if(useNDF)
            { 
                catchBlocks.Add(new CatchBlockManagedExceptionNDFException(loggerBlock));
                catchBlocks.Add(new CatchBlockExceptionNDFException(loggerBlock));

                boundsChecks.Add(new BoundsCheckBlockStringNDFException(true,loggerBlock));
                boundsChecks.Add(new BoundsCheckBlockNullNDFException(true,loggerBlock));
            }
            else
            { 
                catchBlocks.Add(new CatchBlockStandard(loggerBlock));

                boundsChecks.Add(new BoundsCheckBlockString(true,loggerBlock));
                boundsChecks.Add(new BoundsCheckBlockNull (true,loggerBlock));
            }

            var tryBlock = new TryBlockStandard(loggerBlock,catchBlocks);

            updatedImplementationClass = await source.AddClassMissingMembersAsync(implementationSource,implementationClass,false,loggerBlock,logLevel,boundsChecks,tryBlock,missingMembers);

            return updatedImplementationClass;
        }
    }
}
