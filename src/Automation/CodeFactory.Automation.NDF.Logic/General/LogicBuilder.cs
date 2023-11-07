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
    /// Builder that generates a standard logic class that has a supporting interface that defines the methods of the logic class.
    /// </summary>
    public static class LogicBuilder
    {
        /// <summary>
        /// Refreshes the implementation of a logic class from a supporting interface contract.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="logicName">The name of the logic class.</param>
        /// <param name="contractName">The name of the logic interface.</param>
        /// <param name="logicProject">The hosting project for the logic class.</param>
        /// <param name="contractProject">The hosting project for the logic interface.</param>
        /// <param name="useNDF">Optional, Flag that determines if the logic class supports the NDF library, default value is true.</param>
        /// <param name="supportLogging">Optional, Flag that determines if the logic class supports logging, default value is true.</param>
        /// <param name="logicFolder">Optional, Project folder where the logic class should be found, default value is null.</param>
        /// <param name="contractFolder">Optional, Project folder where the logic interface should be found, default value is null.</param>
        /// <param name="additionNamespaces">Optional, List of additional using statements to add to a new logic class when created.</param>
        /// <param name="loggerFieldName">Optional, The name assigned to the logger field, default value is _logger.</param>
        /// <param name="logLevel">Optional, The level of logging that is the default when logging, default value is set to Information.</param>
        /// <returns>Class model of the refreshed logic class.</returns>
        /// <exception cref="CodeFactoryException">Raised when required information is missing, or when automation errors occur.</exception>
        public static async Task<CsClass> RefreshLogicAsync(this IVsActions source, string logicName, string contractName, VsProject logicProject,
            VsProject contractProject, bool useNDF = true,bool supportLogging = true, VsProjectFolder logicFolder = null, VsProjectFolder contractFolder = null,
            List<ManualUsingStatementNamespace> additionNamespaces = null, string loggerFieldName = "_logger", LogLevel logLevel = LogLevel.Information)
        { 
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot refresh logic class.");

            if(string.IsNullOrEmpty(logicName))
                throw new CodeFactoryException("The logic name was not provided, cannot create the logic class.");

            if(string.IsNullOrEmpty(contractName))
                throw new CodeFactoryException("The contract name was not provided, cannot create the logic class.");

            if (logicProject == null) throw new CodeFactoryException("The logic project was not provided, cannot refresh the logic class.");

            if (contractProject == null)
                throw new CodeFactoryException("The contract project was not provided, cannot refresh the logic class.");

            CsInterface contractInterface = contractFolder != null ? (await contractFolder.FindCSharpSourceByInterfaceNameAsync(contractName))?.SourceCode?.Interfaces?.FirstOrDefault() 
                : (await contractProject.FindCSharpSourceByInterfaceNameAsync(contractName))?.SourceCode?.Interfaces?.FirstOrDefault();

            if (contractInterface == null)
                throw new CodeFactoryException($"Could not find interface {contractName}, cannot refresh the logic class");

            CsSource logicSource = logicFolder != null
                ? (await logicFolder.FindCSharpSourceByClassNameAsync(logicName))?.SourceCode
                : (await logicProject.FindCSharpSourceByClassNameAsync(logicName))?.SourceCode;

            bool newLogicClass = false;


            if(logicSource == null)
            { 
                newLogicClass = true;
                
                logicSource = await source.CreateLogicClassAsync(logicName,logicProject,contractInterface,useNDF,supportLogging,logicFolder,additionNamespaces,loggerFieldName)
                    ?? throw new CodeFactoryException($"Could not create the logic class '{logicName}' cannot refresh the logic.");
            }

            CsClass logicClass = await source.UpdateLogicClassAsync(logicSource,useNDF,supportLogging,loggerFieldName,logLevel);

            if(newLogicClass) await source.RegisterTransientClassesAsync(logicProject,false);

            return logicClass;
            
        }



        /// <summary>
        /// Creates a instance of a logic class from a supporting interface contract.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="logicName">The name of the logic class.</param>
        /// <param name="logicProject">The hosting project for the logic class.</param>
        /// <param name="useNDF">Optional, Flag that determines if the logic class supports the NDF library, default value is true.</param>
        /// <param name="supportLogging">Optional, Flag that determines if the logic class supports logging, default value is true.</param>
        /// <param name="logicFolder">Optional, Project folder where the logic class should be found, default value is null.</param>
        /// <param name="additionNamespaces">Optional, List of additional using statements to add to a new logic class when created.</param>
        /// <param name="loggerFieldName">Optional, The name assigned to the logger field, default value is _logger.</param>
        /// <returns>Source code model of the created logic class.</returns>
        /// <exception cref="CodeFactoryException">Raised when required information is missing, or when automation errors occur.</exception>
        private static async Task<CsSource> CreateLogicClassAsync(this IVsActions source, string logicName, VsProject logicProject,
            CsInterface logicContract, bool useNDF = true, bool supportLogging = true, VsProjectFolder logicFolder = null,
            List<ManualUsingStatementNamespace> additionNamespaces = null, string loggerFieldName = "_logger")
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot create the logic class.");

            if(string.IsNullOrEmpty(logicName))
                throw new CodeFactoryException("The logic class name was not provided, cannot create the logic class.");

            if (logicContract == null)
                throw new CodeFactoryException("The logic interface was not provided, cannot create the logic class.");

            if (logicProject == null)
                throw new CodeFactoryException("The logic project for the repository was not provided, cannot create the logic class.");

            string defaultNamespace = logicFolder != null
                ? await logicFolder.GetCSharpNamespaceAsync()
                : logicProject.DefaultNamespace;

            string logicClassName = logicName;

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
                foreach (var logicClassNamespace in additionNamespaces)
                {
                    repoFormatter.AppendCodeLine(0, logicClassNamespace.HasAlias
                        ? $"using {logicClassNamespace.Alias} = {logicClassNamespace.ReferenceNamespace};"
                        : $"using {logicClassNamespace.ReferenceNamespace};");
                }
            }
            repoFormatter.AppendCodeLine(0, $"using {logicContract.Namespace};");
            repoFormatter.AppendCodeLine(0);
            repoFormatter.AppendCodeLine(0, $"namespace {defaultNamespace}");
            repoFormatter.AppendCodeLine(0, "{");
            repoFormatter.AppendCodeLine(1, "/// <summary>");
            repoFormatter.AppendCodeLine(1, $"/// Logic implementation that supports the contract <see cref=\"{logicContract.Name}\"/>");
            repoFormatter.AppendCodeLine(1, "/// </summary>");
            repoFormatter.AppendCodeLine(1, $"public class {logicClassName}:{logicContract.Name}");
            repoFormatter.AppendCodeLine(1, "{");

            if (supportLogging)
            { 
                repoFormatter.AppendCodeLine(2, "/// <summary>");
                repoFormatter.AppendCodeLine(2, "/// Logger used by the logic class.");
                repoFormatter.AppendCodeLine(2, "/// </summary>");
                repoFormatter.AppendCodeLine(2, $"private readonly ILogger {loggerFieldName};");
                repoFormatter.AppendCodeLine(2);
                repoFormatter.AppendCodeLine(2, "/// <summary>");
                repoFormatter.AppendCodeLine(2, "/// Creates a new instance of the logic class.");
                repoFormatter.AppendCodeLine(2, "/// </summary>");
                repoFormatter.AppendCodeLine(2, "/// <param name=\"logger\">Logger used with the repository.</param>");
                repoFormatter.AppendCodeLine(2, $"public {logicClassName}(ILogger<{logicClassName}> logger)");
                repoFormatter.AppendCodeLine(2, "{");
                repoFormatter.AppendCodeLine(3, $"{loggerFieldName} = logger;");
                repoFormatter.AppendCodeLine(2, "}");
                repoFormatter.AppendCodeLine(2);
            }
            else
            { 
                repoFormatter.AppendCodeLine(2);
                repoFormatter.AppendCodeLine(2, "/// <summary>");
                repoFormatter.AppendCodeLine(2, "/// Creates a new instance of the logic class.");
                repoFormatter.AppendCodeLine(2, "/// </summary>");
                repoFormatter.AppendCodeLine(2, $"public {logicClassName}()");
                repoFormatter.AppendCodeLine(2, "{");
                repoFormatter.AppendCodeLine(3);
                repoFormatter.AppendCodeLine(2, "}");
                repoFormatter.AppendCodeLine(2);
            }

            repoFormatter.AppendCodeLine(1, "}");
            repoFormatter.AppendCodeLine(0, "}");

            var doc = logicFolder != null ? await logicFolder.AddDocumentAsync($"{logicClassName}.cs", repoFormatter.ReturnSource())
                : await logicProject.AddDocumentAsync($"{logicClassName}.cs", repoFormatter.ReturnSource());

            return doc == null
                ? throw new CodeFactoryException($"Failed to create the logic class '{logicClassName}' cannot complete refresh of the class.")
                : await doc.GetCSharpSourceModelAsync();
        }

        /// <summary>
        /// Updates an instance of a logic class from a supporting interface contract.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="useNDF">Optional, Flag that determines if the logic class supports the NDF library, default value is true.</param>
        /// <param name="supportLogging">Optional, Flag that determines if the logic class supports logging, default value is true.</param>
        /// <param name="loggerFieldName">Optional, The name assigned to the logger field, default value is _logger.</param>
        /// <returns>Class model of the refreshed logic class.</returns>
        /// <exception cref="CodeFactoryException">Raised when required information is missing, or when automation errors occur.</exception>
        private static async Task<CsClass> UpdateLogicClassAsync(this IVsActions source, CsSource logicSource, bool useNDF = true, bool supportLogging = true,
            string loggerFieldName = "_logger", LogLevel logLevel = LogLevel.Information)
        { 
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot update the logic class.");

            if (logicSource == null) throw new CodeFactoryException("The source for the logic class could not be loaded, cannot update the logic class.");

            var logicClass = logicSource.Classes.FirstOrDefault();

            CsClass updatedlogicClass = logicClass;

            if(logicClass == null) throw new CodeFactoryException("The logic class could not be loaded, cannot update the logic class.");

            var missingMembers = logicClass.GetMissingInterfaceMembers();

            if( !missingMembers.Any() ) return updatedlogicClass;

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

            updatedlogicClass = await source.AddClassMissingMembersAsync(logicSource,logicClass,false,loggerBlock,logLevel,boundsChecks,tryBlock,missingMembers);

            return updatedlogicClass;
        }
    }
}
