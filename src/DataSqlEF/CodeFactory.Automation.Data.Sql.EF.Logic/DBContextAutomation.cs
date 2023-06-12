//***************************************************************************
//* Code Factory Packages
//* Copyright (c) 2023 CodeFactory, LLC
//***************************************************************************
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Data.Sql.EF.Logic
{
    /// <summary>
    /// Automation that refreshes the DbContext to make sure the injection of a connection string is supported.
    /// </summary>
    public static class DbContextAutomation
    {
        /// <summary>
        /// Refreshes the implementation of the DbContext.
        /// </summary>
        /// <param name="source">CodeFactory automation</param>
        /// <param name="contextName">Name of the context class.</param>
        /// <param name="modelProject">The entity framework project hosting models.</param>
        /// <param name="modelFolder">Optional parameter that holds the target profile folders the models live in.</param>
        /// <returns>Refreshed instance of the DbContext</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing.</exception>
        public static async Task<CsClass> RefreshDbContextAsync(this IVsActions source, string contextName,
            VsProject modelProject, VsProjectFolder modelFolder)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot refresh the DbContext.");

            if (string.IsNullOrEmpty(contextName))
                throw new CodeFactoryException(
                    "The entity framework context name was not provided, cannot refresh the DbContext.");

            if (modelProject == null)
                throw new CodeFactoryException(
                    "The entity framework project was not provided, cannot refresh the DbContext.");


            var contextClass = (await modelProject.FindCSharpSourceByClassNameAsync(contextName))
                                 ?.SourceCode?.Classes?.FirstOrDefault()
                                 ?? throw new CodeFactoryException($"The entity framework context class '{contextName}' could not be loaded, cannot refresh the EF repository,");

            CsSource contextSource = null;

            if (modelFolder != null)
            {
                var folderChildren = await modelFolder.GetChildrenAsync(false, true);
                contextSource = folderChildren.Where(m => m.ModelType == VisualStudioModelType.CSharpSource).Cast<VsCSharpSource>().FirstOrDefault(s => s.Name == $"{contextName}.Load.cs")?.SourceCode;
            }
            else
            {
                var projectChildren = await modelProject.GetChildrenAsync(false, true);
                contextSource = projectChildren.Where(m => m.ModelType == VisualStudioModelType.CSharpSource).Cast<VsCSharpSource>().FirstOrDefault(s => s.Name == $"{contextName}.Load.cs")?.SourceCode;
            }

            if(contextSource == null) contextSource = await 
                CreateDbContextLoad(source,contextClass,modelProject,modelFolder);

            var connectionStringInterface = (modelFolder != null
                ? (await modelFolder.FindCSharpSourceByInterfaceNameAsync("IDBContextConnection"))?.SourceCode?.Interfaces?.FirstOrDefault()
                : (await modelProject.FindCSharpSourceByInterfaceNameAsync("IDBContextConnection"))?.SourceCode?.Interfaces?.FirstOrDefault()) 
                ?? await source.CreateConnectionStringInterfaceAsync(modelProject, modelFolder);

            var connectionStringClass = (modelFolder != null
                                                ? (await modelFolder.FindCSharpSourceByClassNameAsync("DBContextConnection"))?.SourceCode?.Classes?.FirstOrDefault()
                                                : (await modelProject.FindCSharpSourceByClassNameAsync("DBContextConnection"))?.SourceCode?.Classes?.FirstOrDefault())
                                            ?? await source.CreateConnectionStringClassAsync(modelProject, modelFolder);

            return contextSource?.Classes?.FirstOrDefault();
        }

        /// <summary>
        /// Adds the DbContext partial class file load which contains the constructor for passing a connection string.
        /// </summary>
        /// <param name="source">CodeFactory Automation</param>
        /// <param name="contextClass">The context class to be updated.</param>
        /// <param name="modelProject">The entity framework project hosting models.</param>
        /// <param name="modelFolder">Optional parameter that holds the target profile folders the models live in.</param>
        /// <returns>Updated class for the DbContext</returns>
        /// <exception cref="CodeFactoryException">Required information is missing.</exception>
        private static async Task<CsSource> CreateDbContextLoad(this IVsActions source,CsClass contextClass,
            VsProject modelProject, VsProjectFolder modelFolder = null)
        {
            SourceFormatter loadFormatter = new SourceFormatter();

            loadFormatter.AppendCodeLine(0, "using System;");
            loadFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            loadFormatter.AppendCodeLine(0, "using System.Linq;");
            loadFormatter.AppendCodeLine(0, "using System.Text;");
            loadFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            loadFormatter.AppendCodeLine(0, "using Microsoft.EntityFrameworkCore;");
            loadFormatter.AppendCodeLine(0, $"namespace {contextClass.Namespace}");
            loadFormatter.AppendCodeLine(0, "{");
            loadFormatter.AppendCodeLine(1);
            loadFormatter.AppendCodeLine(1, $"public partial class {contextClass.Name}");
            loadFormatter.AppendCodeLine(1, "{");
            loadFormatter.AppendCodeLine(1);

            if (!contextClass.Fields.Any(f => f.Name == "_connectionString"))
            {
                loadFormatter.AppendCodeLine(2, "private readonly string _connectionString;");
                loadFormatter.AppendCodeLine(2);
            }

            if (!contextClass.Constructors.Any(m =>
                    m.Parameters.Count() == 1 & m.Parameters.Any(p => p.Name == "connectionString")))
            {
                loadFormatter.AppendCodeLine(2, "/// <summary>");
                loadFormatter.AppendCodeLine(2, $"/// Creates a new instance of the context that injects the target connection string to use.");
                loadFormatter.AppendCodeLine(2, "/// </summary>");
                loadFormatter.AppendCodeLine(2, "/// <param name=\"connectionString\">connection string to use with the context.</param>");
                loadFormatter.AppendCodeLine(2, $"public {contextClass.Name}(string connectionString)");
                loadFormatter.AppendCodeLine(2, "{");
                loadFormatter.AppendCodeLine(3, "_connectionString = connectionString;");
                loadFormatter.AppendCodeLine(2, "}");
                loadFormatter.AppendCodeLine(2);
            }


            loadFormatter.AppendCodeLine(2, "protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
            loadFormatter.AppendCodeLine(2, "{");
            loadFormatter.AppendCodeLine(3, "if (!string.IsNullOrEmpty(_connectionString)) optionsBuilder.UseSqlServer(_connectionString);");
            loadFormatter.AppendCodeLine(2, "}");

            loadFormatter.AppendCodeLine(1, "}");
            loadFormatter.AppendCodeLine(0, "}");

            var doc = modelFolder != null ? await modelFolder.AddDocumentAsync($"{contextClass.Name}.Load.cs", loadFormatter.ReturnSource())
                : await modelProject.AddDocumentAsync($"{contextClass.Name}.Load.cs", loadFormatter.ReturnSource());

            return doc == null
                ? throw new CodeFactoryException(
                    $"Failed to create the load logic for the DbContext '{contextClass.Name}' cannot upgrade the repository.")
                : await doc.GetCSharpSourceModelAsync();
        }

        /// <summary>
        /// Creates the IDBContextConnection interface class is created.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="modelProject">The entity framework project hosting models.</param>
        /// <param name="modelFolder">Optional parameter that holds the target profile folders the models live in.</param>
        /// <returns>Created interface</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing.</exception>
        private static async Task<CsInterface> CreateConnectionStringInterfaceAsync(this IVsActions source,
    VsProject modelProject, VsProjectFolder modelFolder = null)
        {
            SourceFormatter connectionFormatter = new SourceFormatter();

            string targetNamespace = modelFolder != null
                ? await modelFolder.GetCSharpNamespaceAsync()
                : modelProject.DefaultNamespace;

            connectionFormatter.AppendCodeLine(0, "using System;");
            connectionFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            connectionFormatter.AppendCodeLine(0, "using System.Linq;");
            connectionFormatter.AppendCodeLine(0, "using System.Text;");
            connectionFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            connectionFormatter.AppendCodeLine(0);
            connectionFormatter.AppendCodeLine(0, $"namespace {targetNamespace}");
            connectionFormatter.AppendCodeLine(0, "{");
            connectionFormatter.AppendCodeLine(1, "/// <summary>");
            connectionFormatter.AppendCodeLine(1, "/// Contract for loading a connection string.");
            connectionFormatter.AppendCodeLine(1, "/// </summary>");
            connectionFormatter.AppendCodeLine(1, "/// <typeparam name=\"T\">The context the connection belongs to.</typeparam>");
            connectionFormatter.AppendCodeLine(1, $"public interface IDBContextConnection<T> where T : class");
            connectionFormatter.AppendCodeLine(1, "{");
            connectionFormatter.AppendCodeLine(2, "/// <summary>");
            connectionFormatter.AppendCodeLine(2, "/// Connection string to use with the DB context.");
            connectionFormatter.AppendCodeLine(2, "/// </summary>");
            connectionFormatter.AppendCodeLine(2, "public string ConnectionString { get;}");
            connectionFormatter.AppendCodeLine(1, "}");
            connectionFormatter.AppendCodeLine(0, "}");

            var doc = modelFolder != null ? await modelFolder.AddDocumentAsync("IDBContextConnection.cs", connectionFormatter.ReturnSource())
                : await modelProject.AddDocumentAsync("IDBContextConnection.cs", connectionFormatter.ReturnSource());

            return doc == null
                ? throw new CodeFactoryException($"Failed to create the IDBContextConnection interface.")
                : (await doc.GetCSharpSourceModelAsync())?.Interfaces.FirstOrDefault();
        }

        /// <summary>
        /// Creates the DBContextConnection class is created.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="modelProject">The entity framework project hosting models.</param>
        /// <param name="modelFolder">Optional parameter that holds the target profile folders the models live in.</param>
        /// <returns>Created interface</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing.</exception>
        private static async Task<CsClass> CreateConnectionStringClassAsync(this IVsActions source,
        VsProject modelProject, VsProjectFolder modelFolder)
        {
            SourceFormatter connectionFormatter = new SourceFormatter();

            string targetNamespace = modelFolder != null
                ? await modelFolder.GetCSharpNamespaceAsync()
                : modelProject.DefaultNamespace;

            connectionFormatter.AppendCodeLine(0, "using System;");
            connectionFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            connectionFormatter.AppendCodeLine(0, "using System.Linq;");
            connectionFormatter.AppendCodeLine(0, "using System.Text;");
            connectionFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            connectionFormatter.AppendCodeLine(0);
            connectionFormatter.AppendCodeLine(0, $"namespace {targetNamespace}");
            connectionFormatter.AppendCodeLine(0, "{");
            connectionFormatter.AppendCodeLine(1, "/// <summary>");
            connectionFormatter.AppendCodeLine(1, "/// Stores the connection string to be used with a target EF context.");
            connectionFormatter.AppendCodeLine(1, "/// </summary>");
            connectionFormatter.AppendCodeLine(1, "/// <typeparam name=\"T\">The EF context class the connection string is for.</typeparam>");
            connectionFormatter.AppendCodeLine(1, $"public class DBContextConnection<T>:IDBContextConnection<T>  where T:class");
            connectionFormatter.AppendCodeLine(1, "{");
            connectionFormatter.AppendCodeLine(2, "/// <summary>");
            connectionFormatter.AppendCodeLine(2, "/// Backing field for the connection string property.");
            connectionFormatter.AppendCodeLine(2, "/// </summary>");
            connectionFormatter.AppendCodeLine(2, "private readonly string _connectionString;");
            connectionFormatter.AppendCodeLine(2);
            connectionFormatter.AppendCodeLine(2, "/// <summary>");
            connectionFormatter.AppendCodeLine(2, "/// Creates an instance that holds the connection string.");
            connectionFormatter.AppendCodeLine(2, "/// </summary>");
            connectionFormatter.AppendCodeLine(2, "public DBContextConnection(string connectionString)");
            connectionFormatter.AppendCodeLine(2, "{");
            connectionFormatter.AppendCodeLine(3, "_connectionString = connectionString;");
            connectionFormatter.AppendCodeLine(2, "}");
            connectionFormatter.AppendCodeLine(2);
            connectionFormatter.AppendCodeLine(2, "/// <summary>");
            connectionFormatter.AppendCodeLine(2, "/// Connection string to use with the DB context.");
            connectionFormatter.AppendCodeLine(2, "/// </summary>");
            connectionFormatter.AppendCodeLine(2, "public string ConnectionString => _connectionString;");
            connectionFormatter.AppendCodeLine(1, "}");
            connectionFormatter.AppendCodeLine(0, "}");

            var doc = modelFolder != null ? await modelFolder.AddDocumentAsync("DBContextConnection.cs", connectionFormatter.ReturnSource())
                : await modelProject.AddDocumentAsync("DBContextConnection.cs", connectionFormatter.ReturnSource());

            return doc == null
                ? throw new CodeFactoryException($"Failed to create the DBContextConnection class.")
                : (await doc.GetCSharpSourceModelAsync())?.Classes.FirstOrDefault();
        }
    }

}
