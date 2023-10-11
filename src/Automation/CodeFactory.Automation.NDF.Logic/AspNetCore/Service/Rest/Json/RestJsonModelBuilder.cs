using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic.AspNetCore.Service.Rest.Json
{
    /// <summary>
    /// Automation logic for generation of supporting libraries for rest implementation.
    /// </summary>
    public static class RestJsonModelBuilder
    {
        /// <summary>
        /// Gets the request type based on the target method provided.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="sourceMethod">The method to get the request for.</param>
        /// <param name="serviceName">The name of the service the request will be supporting.</param>
        /// <param name="modelProject">The project the rest request is located in.</param>
        /// <param name="modelFolder">The project folder the rest request is located in, this is optional.</param>
        /// <returns>The class definition of the request.</returns>
        /// <exception cref="CodeFactoryException">If required data is missing, or information cannot be loaded.</exception>
        public static async Task<CsClass> GetRestRequestAsync(this IVsActions source, CsMethod sourceMethod, string serviceName, VsProject modelProject, VsProjectFolder modelFolder = null)
        {
            if (source == null) throw new CodeFactoryException("No CodeFactory automation was provided cannot find the request type.");
            if (sourceMethod == null) throw new CodeFactoryException("Cannot get the request type since no method was provided.");
            if (sourceMethod.Parameters.Count < 2)
                throw new CodeFactoryException(
                    "The provided method has one or less parameters does not qualify to generate a rest request type.");
            if (string.IsNullOrEmpty(serviceName))
                throw new CodeFactoryException("The service name was not provided cannot find the request type.");

            var requestName = sourceMethod.GetRestServiceRequestModelName(serviceName);

            var modelSource = modelFolder != null
                ? await modelFolder.FindCSharpSourceByClassNameAsync(requestName)
                : await modelProject.FindCSharpSourceByClassNameAsync(requestName);

            CsClass result = null;

            if (modelSource != null)
            {
                result = modelSource.SourceCode.Classes.FirstOrDefault();
            }
            else
            {
                string targetNamespace = modelFolder != null
                    ? await modelFolder.GetCSharpNamespaceAsync()
                    : modelProject.DefaultNamespace;
                SourceFormatter requestFormatter = new SourceFormatter();

                requestFormatter.AppendCodeLine(0, $"namespace {targetNamespace}");
                requestFormatter.AppendCodeLine(0, "{");
                requestFormatter.AppendCodeLine(0);
                requestFormatter.AppendCodeLine(1, $"public class {requestName}");
                requestFormatter.AppendCodeLine(1, "{");

                foreach (var sourceMethodParameter in sourceMethod.Parameters)
                {
                    requestFormatter.AppendCodeLine(2, $"public {sourceMethodParameter.ParameterType.GenerateCSharpTypeName()} {sourceMethodParameter.Name.GenerateCSharpProperCase()} {{ get; set;}}");
                    requestFormatter.AppendCodeLine(2);
                }
                 
                requestFormatter.AppendCodeLine(1, "}");
                requestFormatter.AppendCodeLine(1);
                requestFormatter.AppendCodeLine(0, "}");

                var document = modelFolder != null
                    ? await modelFolder.AddDocumentAsync($"{requestName}.cs", requestFormatter.ReturnSource())
                    : await modelProject.AddDocumentAsync($"{requestName}.cs", requestFormatter.ReturnSource());

                if (document == null)
                    throw new CodeFactoryException($"Failed to create the rest request class '{requestName}', cannot continue service operation.");

                var loadedSource = await document.GetCSharpSourceModelAsync();

                result = loadedSource.Classes.FirstOrDefault();
            }

            if (result == null)
                throw new CodeFactoryException($"Could not load the request model data for '{requestName}'.");

            return result;
        }

        /// <summary>
        /// Get the target name of a service request class. 
        /// </summary>
        /// <param name="source">The source method used to generate the request.</param>
        /// <param name="serviceName">The target name of the service being implemented.</param>
        /// <returns>The name of the request model or null if no request model is needed for this method.</returns>
        public static string GetRestServiceRequestModelName(this CsMethod source, string serviceName)
        {
            if (source == null) throw new CodeFactoryException("No method was provided cannot get the name of the service request model.");
            if (serviceName == null) throw new CodeFactoryException("No service name was provided cannot get the name of the service request model.");

            if (!source.Parameters.Any()) return null;

            StringBuilder requestModelName = new StringBuilder(serviceName);

            bool firstParameter = true;
            foreach (var parameter in source.Parameters)
            {
                if (firstParameter)
                {
                    requestModelName.Append(parameter.Name.ToUpper().First());
                    firstParameter = false;
                }
                else
                {
                    requestModelName.Append(parameter.Name.ToLower().First());
                }
            }
            requestModelName.Append("Request");
            return requestModelName.ToString();
        }

        /// <summary>
        /// Adds the ManagedExceptionRest class to the root of the model project if it does not exist.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="modelProject">The target model project the rest managed exception will be added if missing.</param>
        public static async Task AddSupportRestClassesAsync(this IVsActions source, VsProject modelProject)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot add the rest support classes.");

            if (modelProject == null)
                throw new CodeFactoryException("The model project was not provided, cannot add the rest support classes.");

            if ((await modelProject.FindCSharpSourceByClassNameAsync("ManagedExceptionRest", true)) == null) await source.CreateManagedExceptionRestAsync(modelProject);

            if ((await modelProject.FindCSharpSourceByClassNameAsync("NoDataResult")) == null) await source.CreateNoDataResultAsync(modelProject);

            if ((await modelProject.FindCSharpSourceByClassNameAsync("ServiceResult")) == null) await source.CreateServiceResultAsync(modelProject);

        }

        private static async Task CreateManagedExceptionRestAsync(this IVsActions source, VsProject modelProject)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot add ManagedExceptionRest.");

            if (modelProject == null)
                throw new CodeFactoryException("The model project was not provided, cannot add ManagedExceptionRest.");

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using System;");
            sourceFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            sourceFormatter.AppendCodeLine(0, "using System.Linq;");
            sourceFormatter.AppendCodeLine(0, "using System.Text;");
            sourceFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            sourceFormatter.AppendCodeLine(0, "using CodeFactory.NDF;");
            sourceFormatter.AppendCodeLine(0, $"namespace {modelProject.DefaultNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Rest implementation of a exception that supports the base exception type of <see cref=\"ManagedException\"/>");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, $"public class ManagedExceptionRest");
            sourceFormatter.AppendCodeLine(1, "{");
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Type of exception that occurred.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public string ExceptionType { get; set; }");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Message that occurred in the exception.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public string Message { get; set; }");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Which data field was impacted by the exception.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public string DataField { get; set; }");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// A list of aggregate exceptions");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public List<ManagedExceptionRest> Exceptions { get; set; }");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Creates a new instance of a <see cref=\"ManagedExceptionRest\"/> from exceptions that are based on <see cref=\"ManagedException\"/>");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"exception\">Exception to convert to a JSON message.</param>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>Formatted JSON message or null if not found.</returns>");
            sourceFormatter.AppendCodeLine(2, "public static List<ManagedExceptionRest> CreateManagedExceptionRest(ManagedException exception)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "if (exception == null) return null;");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "ManagedExceptionRest result = null;");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "result = exception switch");
            sourceFormatter.AppendCodeLine(3, "{");
            sourceFormatter.AppendCodeLine(4, "AuthenticationException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(AuthenticationException) },");
            sourceFormatter.AppendCodeLine(4, "AuthorizationException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(AuthorizationException) },");
            sourceFormatter.AppendCodeLine(4, "SecurityException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(SecurityException) },");
            sourceFormatter.AppendCodeLine(4, "CommunicationTimeoutException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(CommunicationTimeoutException) },");
            sourceFormatter.AppendCodeLine(4, "CommunicationException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(CommunicationException) },");
            sourceFormatter.AppendCodeLine(4, "ConfigurationException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(ConfigurationException) },");
            sourceFormatter.AppendCodeLine(4, "DataValidationException exceptionData => new ManagedExceptionRest { DataField = exceptionData.PropertyName, Message = exceptionData.Message, ExceptionType = nameof(DataValidationException) },");
            sourceFormatter.AppendCodeLine(4, "DuplicateException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(DuplicateException) },");
            sourceFormatter.AppendCodeLine(4, "ValidationException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(ValidationException), DataField = exceptionData.DataField },");
            sourceFormatter.AppendCodeLine(4, "DataException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(DataException) },");
            sourceFormatter.AppendCodeLine(4, "LogicException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(LogicException) },");
            sourceFormatter.AppendCodeLine(4, "ManagedExceptions exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, Exceptions = exceptionData.Exceptions.SelectMany(CreateManagedExceptionRest).ToList() },");
            sourceFormatter.AppendCodeLine(4, "UnhandledException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(UnhandledException) },");
            sourceFormatter.AppendCodeLine(4, "_ => new ManagedExceptionRest { Message = exception.Message, ExceptionType = nameof(ManagedException) },");
            sourceFormatter.AppendCodeLine(3, "};");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "return result.Exceptions?.Count > 0 ? result.Exceptions : new List<ManagedExceptionRest> { result };");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Creates a <see cref=\"ManagedException\"/> from the data.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>The target managed exception type.</returns>");
            sourceFormatter.AppendCodeLine(2, "public ManagedException CreateManagedException()");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "ManagedException result = null;");
            sourceFormatter.AppendCodeLine(3, "result = ExceptionType switch");
            sourceFormatter.AppendCodeLine(3, "{");
            sourceFormatter.AppendCodeLine(4, "nameof(AuthenticationException) => new AuthenticationException(Message),");
            sourceFormatter.AppendCodeLine(4, "nameof(AuthorizationException) => new AuthorizationException(Message),");
            sourceFormatter.AppendCodeLine(4, "nameof(SecurityException) => new SecurityException(Message),");
            sourceFormatter.AppendCodeLine(4, "nameof(CommunicationTimeoutException) => new CommunicationTimeoutException(Message),");
            sourceFormatter.AppendCodeLine(4, "nameof(CommunicationException) => new CommunicationException(Message),");
            sourceFormatter.AppendCodeLine(4, "nameof(ConfigurationException) => new ConfigurationException(Message),");
            sourceFormatter.AppendCodeLine(4, "nameof(DataValidationException) => new DataValidationException(DataField, Message),");
            sourceFormatter.AppendCodeLine(4, "nameof(DuplicateException) => new DuplicateException(Message),");
            sourceFormatter.AppendCodeLine(4, "nameof(ValidationException) => new ValidationException(Message, DataField),");
            sourceFormatter.AppendCodeLine(4, "nameof(DataException) => new DataException(Message),");
            sourceFormatter.AppendCodeLine(4, "nameof(LogicException) => new LogicException(Message),");
            sourceFormatter.AppendCodeLine(4, "nameof(UnhandledException) => new UnhandledException(Message),");
            sourceFormatter.AppendCodeLine(4, "_ => new ManagedException(Message)");
            sourceFormatter.AppendCodeLine(3, "};");
            sourceFormatter.AppendCodeLine(3, "return result;");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");

            await modelProject.AddDocumentAsync("ManagedExceptionRest.cs", sourceFormatter.ReturnSource());
        }

        private static async Task CreateNoDataResultAsync(this IVsActions source, VsProject modelProject)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot add NoDataResult.");

            if (modelProject == null)
                throw new CodeFactoryException("The model project was not provided, cannot add NoDataResult.");

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using CodeFactory.NDF;");
            sourceFormatter.AppendCodeLine(0);
            sourceFormatter.AppendCodeLine(0, $"namespace {modelProject.DefaultNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Result from a service call with no data being returned.");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, $"public class NoDataResult");
            sourceFormatter.AppendCodeLine(1, "{");
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Boolean flag that determines if the service result had and exception occur.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public bool HasExceptions { get; set; }");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// If <see cref=\"HasExceptions\"/> is true then the ManagedException will be provided.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public List<ManagedExceptionRest> ManagedExceptions { get; set; }");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Returns a new instance of <see cref=\"NoDataResult\"/> with a single exception.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"exception\">Exception to return.</param>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>Service result with error.</returns>");
            sourceFormatter.AppendCodeLine(2, "public static NoDataResult CreateError(ManagedException exception)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "return new NoDataResult");
            sourceFormatter.AppendCodeLine(3, "{");
            sourceFormatter.AppendCodeLine(4, "HasExceptions = true,");
            sourceFormatter.AppendCodeLine(4, "ManagedExceptions = ManagedExceptionRest.CreateManagedExceptionRest(exception)");
            sourceFormatter.AppendCodeLine(3, "};");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Returns a new instance of <see cref=\"NoDataResult\"/> with multiple exceptions.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"exceptions\">Exceptions to return.</param>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>Result with errors.</returns>");
            sourceFormatter.AppendCodeLine(2, "public static NoDataResult CreateErrors(List<ManagedException> exceptions)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "return new NoDataResult");
            sourceFormatter.AppendCodeLine(3, "{");
            sourceFormatter.AppendCodeLine(4, "HasExceptions = true,");
            sourceFormatter.AppendCodeLine(4, "ManagedExceptions = exceptions.SelectMany(ManagedExceptionRest.CreateManagedExceptionRest).ToList()");
            sourceFormatter.AppendCodeLine(3, "};");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Returns a new instance of <see cref=\"NoDataResult\"/> as a successful operation.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public static NoDataResult CreateSuccess()");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "return new NoDataResult {HasExceptions = false};");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Raises any serialized exceptions if they exists.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public void RaiseException()");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "if (!HasExceptions) return;");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "var managedException = ManagedExceptions?.Select(x => x.CreateManagedException())?.ToList();");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "if (managedException?.Count == 1) throw managedException.First();");
            sourceFormatter.AppendCodeLine(3, "if (managedException?.Count > 1) throw new ManagedExceptions(managedException);");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");

            await modelProject.AddDocumentAsync("NoDataResult.cs", sourceFormatter.ReturnSource());
        }

        private static async Task CreateServiceResultAsync(this IVsActions source, VsProject modelProject)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot add ServiceResult.");

            if (modelProject == null)
                throw new CodeFactoryException("The model project was not provided, cannot add ServiceResult.");

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using CodeFactory.NDF;");
            sourceFormatter.AppendCodeLine(0);
            sourceFormatter.AppendCodeLine(0, $"namespace {modelProject.DefaultNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Result data from a service call.");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, "/// <typeparam name=\"T\">The target type of data being returned from the service call.</typeparam>");
            sourceFormatter.AppendCodeLine(1, $"public class ServiceResult<T>");
            sourceFormatter.AppendCodeLine(1, "{");
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Result data from the service call.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public T Result { get; set; }");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Boolean flag that determines if the service result had and exception occur.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public bool HasExceptions { get; set; }");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// If <see cref=\"HasExceptions\"/> is true then the ManagedException will be provided.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public List<ManagedExceptionRest> ManagedExceptions { get; set; }");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Returns a new instance of a service result with a single exception.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"exception\">Exception to return.</param>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>Service result with error.</returns>");
            sourceFormatter.AppendCodeLine(2, "public static ServiceResult<T> CreateError(ManagedException exception)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "return new ServiceResult<T>");
            sourceFormatter.AppendCodeLine(3, "{");
            sourceFormatter.AppendCodeLine(4, "HasExceptions = true,");
            sourceFormatter.AppendCodeLine(4, "ManagedExceptions = ManagedExceptionRest.CreateManagedExceptionRest(exception)");
            sourceFormatter.AppendCodeLine(3, "};");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Returns a new instance of a service result with multiple exceptions.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"exceptions\">Exceptions to return.</param>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>Service result with errors.</returns>");
            sourceFormatter.AppendCodeLine(2, "public static ServiceResult<T> CreateErrors(List<ManagedException> exceptions)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "return new ServiceResult<T>");
            sourceFormatter.AppendCodeLine(3, "{");
            sourceFormatter.AppendCodeLine(4, "HasExceptions = true,");
            sourceFormatter.AppendCodeLine(4, "ManagedExceptions = exceptions.SelectMany(ManagedExceptionRest.CreateManagedExceptionRest).ToList()");
            sourceFormatter.AppendCodeLine(3, "};");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Returns a new instance of a service result with result data.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>Service result with error.</returns>");
            sourceFormatter.AppendCodeLine(2, "public static ServiceResult<T> CreateResult(T result)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "return new ServiceResult<T> { HasExceptions = false, Result = result };");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Raises any serialized exceptions if they exists.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public void RaiseException()");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "if (!HasExceptions) return;");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "var managedException = ManagedExceptions?.Select(x => x.CreateManagedException())?.ToList();");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "if (managedException?.Count == 1) throw managedException.First();");
            sourceFormatter.AppendCodeLine(3, "if (managedException?.Count > 1) throw new ManagedExceptions(managedException);");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");

            await modelProject.AddDocumentAsync("ServiceResult.cs", sourceFormatter.ReturnSource());
        }
    }
}
