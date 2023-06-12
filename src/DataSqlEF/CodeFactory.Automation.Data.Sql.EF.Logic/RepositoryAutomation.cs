//***************************************************************************
//* Code Factory Packages
//* Copyright (c) 2023 CodeFactory, LLC
//***************************************************************************
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.Automation.Standard.NDF.Logic;


namespace CodeFactory.Automation.Data.Sql.EF.Logic
{
   /// <summary>
    /// Automation logic to create or update a data repository that supports a target entity hosted in Entity Framework.
    /// </summary>
    public static class RepositoryAutomation
    {
        /// <summary>
        /// Creates or updates a repository that uses entity framework for management of data.
        /// </summary>
        /// <param name="source">CodeFactory automation for Visual Studio for Windows.</param>
        /// <param name="efEntity">Entity framework entity.</param>
        /// <param name="poco">POCO model that supports the repository.</param>
        /// <param name="namePrefix">Optional, prefix to assign to the name of the repository, default is null.</param>
        /// <param name="nameSuffix">Optional, suffix to assign to the name of the repository, default is null.</param>
        /// <param name="contractProject">Project the contract will be added to.</param>
        /// <param name="contractFolder">Optional, project folder contracts will be stored in, default is null.</param>
        /// <param name="additionalContractNamespaces">Optional, additional namespaces to add to the contract definition, default is null.</param>
        /// <param name="repoProject">Repository project.</param>
        /// <param name="contextClass">EF context class for accessing entity framework.</param>
        /// <param name="useNDF">Optional, flag that determines if the NDF libraries are used, default true.</param>
        /// <param name="supportLogging">Optional, flag that determines if logging is supported, default true.</param>
        /// <param name="repoFolder">Optional, project folder the repositories are stored in, default is null.</param>
        /// <param name="additionRepositoryNamespaces">Optional, list of additional namespaces to update the repository with.</param>
        /// <returns>Created or updated repository.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data to create or update the repository is missing.</exception>
        public static async Task<CsClass> RefreshEFRepositoryAsync(this IVsActions source, CsClass efEntity, VsProject repoProject, 
            VsProject contractProject, CsClass poco, CsClass contextClass, bool useNDF = true,bool supportLogging = true, VsProjectFolder repoFolder = null, VsProjectFolder contractFolder = null,
            string namePrefix = null, string nameSuffix = null,List<ManualUsingStatementNamespace> additionRepositoryNamespaces = null,
            List<ManualUsingStatementNamespace> additionalContractNamespaces = null)
        {

            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot refresh the EF repository.");

            if (efEntity == null)
                throw new CodeFactoryException("The entity framework model was not provided, cannot refresh the EF repository.");

            if (repoProject == null) throw new CodeFactoryException("The repository project was not provided, cannot refresh the EF repository.");

            if (contractProject == null)
                throw new CodeFactoryException("The contract project for the repository was not provided, cannot refresh the EF repository.");

            if (poco == null)
                throw new CodeFactoryException("The POCO model was not provided, cannot refresh the EF repository.");

            if (contextClass == null)
                throw new CodeFactoryException("The entity framework data context class was not provided, cannot refresh the EF repository.");

            var contractName = $"I{namePrefix}{efEntity.Name}{nameSuffix}";

            CsInterface contractInterface = contractFolder != null ? (await contractFolder.FindCSharpSourceByInterfaceNameAsync(contractName))?.SourceCode?.Interfaces?.FirstOrDefault() 
                : (await contractProject.FindCSharpSourceByInterfaceNameAsync(contractName))?.SourceCode?.Interfaces?.FirstOrDefault();

            if (contractInterface == null)
                contractInterface = (await source.CreateRepositoryContractAsync(efEntity, contractProject, poco,
                    contractFolder, namePrefix, nameSuffix, additionalContractNamespaces)) ?? throw new CodeFactoryException("Could not create a repos");

            var repoName = $"{namePrefix}{efEntity.Name}{nameSuffix}";

            CsSource repoSource = (repoFolder != null
                ? (await repoFolder.FindCSharpSourceByClassNameAsync(repoName))?.SourceCode
                : (await repoProject.FindCSharpSourceByClassNameAsync(repoName))?.SourceCode) 
                ?? (await source.CreateEFRepositoryAsync(efEntity, repoProject, contractInterface, poco,
                    contextClass, useNDF, supportLogging, repoFolder, namePrefix, nameSuffix,
                    additionRepositoryNamespaces)
                ?? throw new CodeFactoryException($"Could not create the repository '{repoName}'."));

            return await source.UpdateEFRepositoryAsync(efEntity, repoProject, repoSource, contractInterface, poco,
                contextClass, useNDF, supportLogging, repoFolder, additionRepositoryNamespaces);
        }

        /// <summary>
        /// Create the repositories interface contract.
        /// </summary>
        /// <param name="source">CodeFactory automation for Visual Studio for Windows.</param>
        /// <param name="efEntity">Entity framework entity.</param>
        /// <param name="poco">POCO model that supports the repository.</param>
        /// <param name="namePrefix">Optional, prefix to assign to the name of the repository, default is null.</param>
        /// <param name="nameSuffix">Optional, suffix to assign to the name of the repository, default is null.</param>
        /// <param name="contractProject">Project the contract will be added to.</param>
        /// <param name="contractFolder">Optional, project folder contracts will be stored in, default is null.</param>
        /// <param name="additionalContractNamespaces">Optional, additional namespaces to add to the contract definition, default is null.</param>
        /// <returns>Contract interface definition</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing to create the interface.</exception>
        private static async Task<CsInterface> CreateRepositoryContractAsync(this IVsActions source, CsClass efEntity, VsProject contractProject, 
        CsClass poco, VsProjectFolder contractFolder = null, string namePrefix = null, string nameSuffix = null, 
        List<ManualUsingStatementNamespace> additionalContractNamespaces = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot create the repository contract.");

            if (efEntity == null)
                throw new CodeFactoryException("The entity framework model was not provided, cannot create the repository contract.");

            if (contractProject == null)
                throw new CodeFactoryException("The contract project for the repository was not provided, cannot create the repository contract.");

            if (poco == null)
                throw new CodeFactoryException("The POCO model was not provided, cannot create the repository contract.");


            string defaultNamespace = contractFolder != null
                ? await contractFolder.GetCSharpNamespaceAsync()
                : contractProject.DefaultNamespace;

            string contractName = $"I{namePrefix}{efEntity.Name}{nameSuffix}";

            SourceFormatter contractFormatter = new SourceFormatter();

            contractFormatter.AppendCodeLine(0, "using System;");
            contractFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            contractFormatter.AppendCodeLine(0, "using System.Text;");
            contractFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");

            if (additionalContractNamespaces != null)
            {
                foreach (var additionalContractNamespace in additionalContractNamespaces)
                {
                    contractFormatter.AppendCodeLine(0, additionalContractNamespace.HasAlias
                        ? $"using {additionalContractNamespace.Alias} = {additionalContractNamespace.ReferenceNamespace};"
                        : $"using {additionalContractNamespace.ReferenceNamespace};");
                }
            }

            contractFormatter.AppendCodeLine(0, $"using {poco?.Namespace};");
            contractFormatter.AppendCodeLine(0, $"namespace {defaultNamespace}");
            contractFormatter.AppendCodeLine(0, "{");
            contractFormatter.AppendCodeLine(1, "/// <summary>");
            contractFormatter.AppendCodeLine(1, $"/// Contract for implementing a repository that supports the model <see cref=\"{efEntity.Name}\"/>");
            contractFormatter.AppendCodeLine(1, "/// </summary>");
            contractFormatter.AppendCodeLine(1, $"public interface {contractName}");
            contractFormatter.AppendCodeLine(1, "{");
            contractFormatter.AppendCodeLine(1);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, $"/// Adds a new instance of the <see cref=\"{poco?.Name}\"/> model.");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2, $"Task<{poco?.Name}> AddAsync({poco?.Name} {poco?.Name.GenerateCSharpCamelCase()});");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, $"/// Updates a instance of the <see cref=\"{poco?.Name}\"/> model.");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2, $"Task<{poco?.Name}> UpdateAsync({poco?.Name} {poco?.Name.GenerateCSharpCamelCase()});");
            contractFormatter.AppendCodeLine(2);
            contractFormatter.AppendCodeLine(2, "/// <summary>");
            contractFormatter.AppendCodeLine(2, $"/// Deletes the instance of the <see cref=\"{poco?.Name}\"/> model.");
            contractFormatter.AppendCodeLine(2, "/// </summary>");
            contractFormatter.AppendCodeLine(2, $"Task DeleteAsync({poco?.Name} {poco?.Name.GenerateCSharpCamelCase()});");
            contractFormatter.AppendCodeLine(2);

            contractFormatter.AppendCodeLine(1, "}");
            contractFormatter.AppendCodeLine(0, "}");

            var doc = contractFolder != null ? await contractFolder.AddDocumentAsync($"{contractName}.cs", contractFormatter.ReturnSource())
                : await contractProject.AddDocumentAsync($"{contractName}.cs", contractFormatter.ReturnSource());

            return doc == null
                ? throw new CodeFactoryException($"Failed to create the repository contract '{contractName}' cannot upgrade the repository.")
                : ((await doc.GetCSharpSourceModelAsync())?.Interfaces.FirstOrDefault());
        }

        /// <summary>
        /// Creates a new repository that support entity framework. 
        /// </summary>
        /// <param name="source">CodeFactory automation for Visual Studio for Windows.</param>
        /// <param name="efEntity">Entity framework entity.</param>
        /// <param name="repoProject">Repository project.</param>
        /// <param name="repoContract">Repository contract.</param>
        /// <param name="poco">POCO model that supports the repository.</param>
        /// <param name="contextClass">EF context class for accessing entity framework.</param>
        /// <param name="useNDF">Optional, flag that determines if the NDF libraries are used, default true.</param>
        /// <param name="supportLogging">Optional, flag that determines if logging is supported, default true.</param>
        /// <param name="repoFolder">Optional, project folder the repositories are stored in, default is null.</param>
        /// <param name="additionRepositoryNamespaces">Optional, list of additional namespaces to update the repository with.</param>
        /// <param name="namePrefix">Optional, prefix to assign to the name of the repository, default is null.</param>
        /// <param name="nameSuffix">Optional, suffix to assign to the name of the repository, default is null.</param>
        /// <returns>Source for the created repository.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing to create the repository.</exception>
        private static async Task<CsSource> CreateEFRepositoryAsync(this IVsActions source, CsClass efEntity, VsProject repoProject,
            CsInterface repoContract, CsClass poco, CsClass contextClass, bool useNDF = true, bool supportLogging = true, VsProjectFolder repoFolder = null,
            string namePrefix = null, string nameSuffix = null, List<ManualUsingStatementNamespace> additionRepositoryNamespaces = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot create the repository.");

            if (efEntity == null)
                throw new CodeFactoryException("The entity framework model was not provided, cannot create the repository.");

            if (repoContract == null)
                throw new CodeFactoryException("The repository interface was not provided, cannot create the repository.");

            if (repoProject == null)
                throw new CodeFactoryException("The repository project for the repository was not provided, cannot create the repository.");

            if (poco == null)
                throw new CodeFactoryException("The POCO model was not provided, cannot create the repository.");

            if (contextClass == null)
                throw new CodeFactoryException("No db contact class was provided, cannot create the repository.");

            string defaultNamespace = repoFolder != null
                ? await repoFolder.GetCSharpNamespaceAsync()
                : repoProject.DefaultNamespace;

            string repoName = $"{namePrefix}{efEntity.Name}{nameSuffix}";

            SourceFormatter repoFormatter = new SourceFormatter();

            repoFormatter.AppendCodeLine(0, "using System;");
            repoFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            repoFormatter.AppendCodeLine(0, "using System.Text;");
            repoFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            repoFormatter.AppendCodeLine(0, "using Microsoft.EntityFrameworkCore;");
            repoFormatter.AppendCodeLine(0, "using Microsoft.Data.SqlClient;");
            
            if (supportLogging) 
                repoFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            if (useNDF)
            {
                repoFormatter.AppendCodeLine(0, "using CodeFactory.NDF;");
                repoFormatter.AppendCodeLine(0, "using CodeFactory.NDF.SQL;");
            }

            if (additionRepositoryNamespaces != null)
            {
                foreach (var repoNamespace in additionRepositoryNamespaces)
                {
                    repoFormatter.AppendCodeLine(0, repoNamespace.HasAlias
                        ? $"using {repoNamespace.Alias} = {repoNamespace.ReferenceNamespace};"
                        : $"using {repoNamespace.ReferenceNamespace};");
                }
            }

            repoFormatter.AppendCodeLine(0, $"using dataModel = {efEntity.Namespace};");
            repoFormatter.AppendCodeLine(0, $"using poco = {poco.Namespace};");
            repoFormatter.AppendCodeLine(0, $"using {repoContract.Namespace};");
            repoFormatter.AppendCodeLine(0);
            repoFormatter.AppendCodeLine(0, $"namespace {defaultNamespace}");
            repoFormatter.AppendCodeLine(0, "{");
            repoFormatter.AppendCodeLine(1, "/// <summary>");
            repoFormatter.AppendCodeLine(1, $"/// Repository implementation that supports the model <see cref=\"{efEntity.Name}\"/>");
            repoFormatter.AppendCodeLine(1, "/// </summary>");
            repoFormatter.AppendCodeLine(1, $"public class {repoName}:{repoContract.Name}");
            repoFormatter.AppendCodeLine(1, "{");
            repoFormatter.AppendCodeLine(2, "/// <summary>");
            repoFormatter.AppendCodeLine(2, "/// Connection string of the repository");
            repoFormatter.AppendCodeLine(2, "/// </summary>");
            repoFormatter.AppendCodeLine(2, "private readonly string _connectionString;");
            repoFormatter.AppendCodeLine(2);
            repoFormatter.AppendCodeLine(2, "/// <summary>");
            repoFormatter.AppendCodeLine(2, "/// Logger used by the repository.");
            repoFormatter.AppendCodeLine(2, "/// </summary>");
            repoFormatter.AppendCodeLine(2, "private readonly ILogger _logger;");
            repoFormatter.AppendCodeLine(2);
            repoFormatter.AppendCodeLine(2, "/// <summary>");
            repoFormatter.AppendCodeLine(2, "/// Creates a new instance of the repository.");
            repoFormatter.AppendCodeLine(2, "/// </summary>");
            repoFormatter.AppendCodeLine(2, "/// <param name=\"logger\">Logger used with the repository.</param>");
            repoFormatter.AppendCodeLine(2, "/// <param name=\"connection\">The connection information for the repository.</param>");
            repoFormatter.AppendCodeLine(2, $"public {repoName}(ILogger<{repoName}> logger, dataModel.IDBContextConnection<dataModel.{contextClass.Name}> connection)");
            repoFormatter.AppendCodeLine(2, "{");
            repoFormatter.AppendCodeLine(3, "_logger = logger;");
            repoFormatter.AppendCodeLine(3, "_connectionString = connection.ConnectionString;");
            repoFormatter.AppendCodeLine(2, "}");
            repoFormatter.AppendCodeLine(2);
            repoFormatter.AppendCodeLine(1, "}");
            repoFormatter.AppendCodeLine(0, "}");

            var doc = repoFolder != null ? await repoFolder.AddDocumentAsync($"{repoName}.cs", repoFormatter.ReturnSource())
                : await repoProject.AddDocumentAsync($"{repoName}.cs", repoFormatter.ReturnSource());

            return doc == null
                ? throw new CodeFactoryException($"Failed to create the repository '{repoName}' cannot upgrade the repository.")
                : await doc.GetCSharpSourceModelAsync();
        }

        /// <summary>
        /// Updates a repository with contract methods that are missing.
        /// </summary>
        /// <param name="source">CodeFactory automation for Visual Studio for Windows.</param>
        /// <param name="efEntity">Entity framework entity.</param>
        /// <param name="repoProject">Repository project.</param>
        /// <param name="repoSource">Repository source</param>
        /// <param name="repoContract">Repository contract.</param>
        /// <param name="poco">POCO model that supports the repository.</param>
        /// <param name="contextClass">EF context class for accessing entity framework.</param>
        /// <param name="useNDF">Optional, flag that determines if the NDF libraries are used, default true.</param>
        /// <param name="supportLogging">Optional, flag that determines if logging is supported, default true.</param>
        /// <param name="repoFolder">Optional, project folder the repositories are stored in, default is null.</param>
        /// <param name="additionRepositoryNamespaces">Optional, list of additional namespaces to update the repository with.</param>
        /// <returns></returns>
        /// <exception cref="CodeFactoryException"></exception>
        private static async Task<CsClass> UpdateEFRepositoryAsync(this IVsActions source, CsClass efEntity, VsProject repoProject,
        CsSource repoSource,CsInterface repoContract, CsClass poco, CsClass contextClass, bool useNDF = true, bool supportLogging = true, VsProjectFolder repoFolder = null,
        List<ManualUsingStatementNamespace> additionRepositoryNamespaces = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot update the repository.");

            if (efEntity == null)
                throw new CodeFactoryException("The entity framework model was not provided, cannot update the repository.");

            if (repoContract == null)
                throw new CodeFactoryException("The repository interface was not provided, cannot update the repository.");

            if (repoProject == null)
                throw new CodeFactoryException("The repository project for the repository was not provided, cannot update the repository.");

            if (poco == null)
                throw new CodeFactoryException("The POCO model was not provided, cannot update the repository.");

            if (contextClass == null)
                throw new CodeFactoryException("No db contact class was provided, cannot update the repository.");

            var repoClass = repoSource?.Classes?.FirstOrDefault();

            if (repoClass == null)
                throw new CodeFactoryException("Could not load the class model for the existing repository, cannot update the repository.");

            var currentSource = repoSource;

            var repoMethods = repoClass.Methods;

            var contractMethods = repoContract.Methods;

            var missingMethods = contractMethods.Where(m =>
                {
                    var contractHash = m.GetComparisonHashCode();
                    return repoMethods.All(c => c.GetComparisonHashCode() != contractHash);
                }).ToList();

            if (!missingMethods.Any()) return repoClass;

            var repoManager = new SourceClassManager(currentSource, repoClass, source);

            repoManager.LoadNamespaceManager();

            ILoggerBlock logFormatter = null;

            if (supportLogging)
            {
                if (useNDF) logFormatter = new LoggerBlockNDF("_logger");
                else logFormatter = new LoggerBlockMicrosoft("_logger");
            }

            var boundCheckBlocks = new List<IBoundsCheckBlock>();
            var catchBlocks = new List<ICatchBlock>();

            if (useNDF)
            {
                boundCheckBlocks.Add(new BoundsCheckBlockStringNDF(true,logFormatter));
                boundCheckBlocks.Add(new BoundsCheckBlockNullNDF(true,logFormatter));

                catchBlocks.Add(new CatchBlockManagedExceptionNDF(logFormatter));
                catchBlocks.Add(new CatchBlockDBUpdateExceptionNDF(logFormatter));
                catchBlocks.Add(new CatchBlockSqlExceptionNDF(logFormatter));
                catchBlocks.Add(new CatchBlockExceptionNDF(logFormatter));
            }
            else
            {
                boundCheckBlocks.Add(new BoundsCheckBlockString(true,logFormatter));
                boundCheckBlocks.Add(new BoundsCheckBlockNull(true,logFormatter));

                catchBlocks.Add(new CatchBlockExceptionNDF(logFormatter));
            }
            
            var methodBuilder = new  MethodBuilderStandard(logFormatter, boundCheckBlocks,new TryBlockStandard(logFormatter,catchBlocks));

            SourceFormatter injectFormatter = new SourceFormatter();

            foreach (var missingMethod in missingMethods)
            {
                injectFormatter.AppendCodeLine(0, $"using (var context = new dataModel.{contextClass.Name}(_connectionString))");
                injectFormatter.AppendCodeLine(0, "{");

                switch (missingMethod.Name)
                {
                    case "AddAsync":
                        injectFormatter.AppendCodeLine(1, $"var model = dataModel.{efEntity.Name}.CreateDataModel({efEntity.Name.GenerateCSharpCamelCase()});");
                        injectFormatter.AppendCodeLine(1, "await context.AddAsync(model);");
                        injectFormatter.AppendCodeLine(1, "await context.SaveChangesAsync();");
                        injectFormatter.AppendCodeLine(1, "result = model.CreatePocoModel();");
                        break;

                    case "UpdateAsync":
                        injectFormatter.AppendCodeLine(1, $"var model = dataModel.{efEntity.Name}.CreateDataModel({efEntity.Name.GenerateCSharpCamelCase()});");
                        injectFormatter.AppendCodeLine(1, "context.Update(model);");
                        injectFormatter.AppendCodeLine(1, "await context.SaveChangesAsync();");
                        injectFormatter.AppendCodeLine(1, "result = model.CreatePocoModel();");
                        break;

                    case "DeleteAsync":
                        injectFormatter.AppendCodeLine(1, $"var model = dataModel.{efEntity.Name}.CreateDataModel({efEntity.Name.GenerateCSharpCamelCase()});");
                        injectFormatter.AppendCodeLine(1, "context.Remove(model);");
                        injectFormatter.AppendCodeLine(1, "await context.SaveChangesAsync();");
                        break;

                    default:
                        injectFormatter.AppendCodeLine(0);
                        break;
                }

                injectFormatter.AppendCodeLine(0, "}");

                string syntax = injectFormatter.ReturnSource();
                await methodBuilder.InjectMethodAsync(missingMethod, repoManager, 2, syntax: syntax);

                injectFormatter.ResetFormatter();
            }

            return repoManager.Container;
        }
    }
}
