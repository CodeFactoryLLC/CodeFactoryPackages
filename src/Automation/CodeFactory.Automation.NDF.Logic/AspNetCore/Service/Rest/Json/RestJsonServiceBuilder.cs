using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.Automation.Standard.Logic;

namespace CodeFactory.Automation.NDF.Logic.AspNetCore.Service.Rest.Json
{
/// <summary>
    /// Automation logic for creating and updated web api rest based services.
    /// </summary>
    public static class RestJsonServiceBUilder
    {

        public static async Task<CsClass> RefreshJsonRestService(this IVsActions source, CsInterface logicContract,
            VsProject serviceProject, VsProjectFolder serviceFolder, VsProject modelProject, VsProject abstractionProject,
            VsProject contractProject, VsProjectFolder modelFolder = null, VsProjectFolder abstractionFolder = null,
            VsProjectFolder contractFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot refresh the service.");

            if (logicContract == null)
                throw new CodeFactoryException("Cannot load the logic contract, cannot refresh the service.");

            if (serviceProject == null)
                throw new CodeFactoryException("Cannot load the service project, cannot refresh the service.");

            if (serviceFolder == null)
                throw new CodeFactoryException("Cannot load the service project folder, cannot refresh the service.");

            if (modelProject == null)
                throw new CodeFactoryException("Cannot load the model project, cannot refresh the service.");

            if (abstractionProject == null)
                throw new CodeFactoryException("Cannot load the abstraction project, cannot refresh the service.");

            if (contractProject == null)
                throw new CodeFactoryException("Cannot load the abstraction contract project, cannot refresh the service.");

            var abstractContract =
                await source.RefreshCSharpAbstractionContractAsync(logicContract, contractProject, contractFolder);

            await source.AddSupportRestClassesAsync(modelProject);
            await source.AddJsonServiceExtensionsAsync(serviceProject);

            var serviceName = $"{logicContract.Name.GenerateCSharpFormattedClassName()}Controller";

            CsSource serviceSource = (await serviceFolder.FindCSharpSourceByClassNameAsync(serviceName))?.SourceCode
                                     ?? await source.CreateJsonRestServiceAsync(logicContract, serviceProject, serviceFolder);

            var serviceClass = await source.UpdateJsonRestServiceAsync(logicContract, serviceSource, serviceProject, serviceFolder,
                modelProject, modelFolder);

            await source.RefreshAbstractionClass(serviceClass, abstractContract, serviceProject, abstractionProject, modelProject, abstractionFolder, modelFolder);


            return serviceClass;
        }

        private static async Task<CsSource> CreateJsonRestServiceAsync(this IVsActions source,
            CsInterface logicContract, VsProject serviceProject, VsProjectFolder serviceFolder)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot create the service.");

            if (logicContract == null)
                throw new CodeFactoryException("Cannot load the logic contract, cannot create the service.");

            if (serviceProject == null)
                throw new CodeFactoryException("Cannot load the service project, cannot create the service.");

            if (serviceFolder == null)
                throw new CodeFactoryException("Cannot load the service project folder, cannot create the service.");

            var sourceNamespace = await serviceFolder.GetCSharpNamespaceAsync();
            if (string.IsNullOrEmpty(sourceNamespace)) throw new CodeFactoryException("Could not identify the target namespace for the service, service cannot be created.");

            var serviceName = logicContract.Name.GenerateCSharpFormattedClassName();

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using System;");
            sourceFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            sourceFormatter.AppendCodeLine(0, "using System.Linq;");
            sourceFormatter.AppendCodeLine(0, "using System.Text;");
            sourceFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            sourceFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            sourceFormatter.AppendCodeLine(0, "using Microsoft.AspNetCore.Mvc;");
            sourceFormatter.AppendCodeLine(0, "using CodeFactory.NDF;");
            sourceFormatter.AppendCodeLine(0, $"using {logicContract.Namespace};");
            sourceFormatter.AppendCodeLine(0, $"namespace {sourceNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Rest service implementation of the logic contract <see cref=\"{logicContract.Name}\"/>");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, "[Route(\"api/[controller]\")]");
            sourceFormatter.AppendCodeLine(1, "[ApiController]");
            sourceFormatter.AppendCodeLine(1, $"public  class {serviceName}Controller:ControllerBase");
            sourceFormatter.AppendCodeLine(1, "{");
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Logger used for the class.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "private readonly ILogger _logger;");
            sourceFormatter.AppendCodeLine(2);

            var logicVariable = $"_{logicContract.Name.GenerateCSharpFormattedClassName().GenerateCSharpCamelCase()}";

            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Logic class supporting the service implementation.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, $"private readonly {logicContract.Name} {logicVariable};");
            sourceFormatter.AppendCodeLine(2);


            var logicParameter = $"{logicContract.Name.GenerateCSharpFormattedClassName().GenerateCSharpCamelCase()}";

            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, $"/// Creates a instance of the controller/>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"logger\">Logger that supports this controller.</param>");
            sourceFormatter.AppendCodeLine(2, $"/// <param name=\"{logicParameter}\">Logic contract implemented by this controller.</param>");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, $"public {serviceName}Controller(ILogger<{serviceName}Controller> logger,{logicContract.Name} {logicParameter})");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "_logger = logger;");
            sourceFormatter.AppendCodeLine(3, $"{logicVariable} = {logicParameter};");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");

            var sourceDocument = (await serviceFolder.AddDocumentAsync($"{serviceName}Controller.cs", sourceFormatter.ReturnSource()))
                ?? throw new CodeFactoryException($"Was not able to load the created service implementation for '{serviceName}Controller'");

            var serviceSource = (await sourceDocument.GetCSharpSourceModelAsync())
                ?? throw new CodeFactoryException($"Was not able to load the created service source code implementation for '{serviceName}Controller'");

            return serviceSource;
        }

        private static async Task<CsClass> UpdateJsonRestServiceAsync(this IVsActions source,
            CsInterface logicContract, CsSource serviceSource, VsProject serviceProject, VsProjectFolder serviceFolder,
            VsProject modelProject, VsProjectFolder modelFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot create the service.");

            if (serviceSource == null)
                throw new CodeFactoryException("Cannot load the service source code, cannot update the service.");

            var currentSource = serviceSource;

            var serviceClass = currentSource.Classes.FirstOrDefault();

            if (serviceClass == null)
                throw new CodeFactoryException(
                    "Cannot load the service class from the source code, cannot update the service.");

            if (logicContract == null)
                throw new CodeFactoryException("Cannot load the logic contract, cannot update the service.");

            if (serviceProject == null)
                throw new CodeFactoryException("Cannot load the service project, cannot update the service.");

            if (serviceFolder == null)
                throw new CodeFactoryException("Cannot load the service project folder, cannot update the service.");

            if (modelProject == null)
                throw new CodeFactoryException("Cannot load the model project, cannot update the service.");

            var logicVariable = $"_{logicContract.Name.GenerateCSharpFormattedClassName().GenerateCSharpCamelCase()}";

            var serviceName = logicContract.Name.GenerateCSharpFormattedClassName();

            SourceFormatter logicFormatter = new SourceFormatter();

            var logicMethods = logicContract.GetAllInterfaceMethods();

            var serviceMethods = serviceClass.Methods;

            var serviceManager = new SourceClassManager(currentSource, serviceClass, source);

            serviceManager.LoadNamespaceManager();

            if (modelFolder != null) await serviceManager.UsingStatementAddAsync(await modelFolder.GetCSharpNamespaceAsync());

            await serviceManager.UsingStatementAddAsync(modelProject.DefaultNamespace);
            await serviceManager.UsingStatementAddAsync(serviceProject.DefaultNamespace);

            foreach (var logicMethod in logicMethods)
            {

                if (logicMethod == null) continue;

                bool isOverload = logicMethods.Count(m => m.Name == logicMethod.Name) > 1;

                string serviceCallName = logicMethod.GetRestName(isOverload);

                string serviceMethodName = $"{serviceCallName}Async";

                if (serviceMethods.Any(s => s.Name == serviceMethodName)) continue;

                bool isPost = logicMethod.IsPostCall();

                string returnTypeSyntax = null;

                bool usesRequestModel = logicMethod.Parameters.Count > 1;

                CsClass requestModel = usesRequestModel ? await source.GetRestRequestAsync(logicMethod, serviceName, modelProject, modelFolder) : null;

                string parameterName = usesRequestModel ? "request" : logicMethod.HasParameters ? logicMethod.Parameters[0].Name : null;
                string parameterType = usesRequestModel ? requestModel.Name : logicMethod.HasParameters ? logicMethod.Parameters[0].ParameterType.GenerateCSharpTypeName(serviceManager.NamespaceManager, serviceManager.MappedNamespaces) : null;

                bool isNullBoundsCheck = false;
                bool isStringBoundsCheck = false;
                bool isLogicAsyncMethod = logicMethod.ReturnType.IsTaskType();
                CsType logicReturnType = logicMethod.ReturnType.TaskReturnType();

                bool returnsData = logicReturnType != null;

                returnTypeSyntax = logicReturnType == null ? "NoDataResult" : $"ServiceResult<{logicReturnType.GenerateCSharpTypeName(serviceManager.NamespaceManager,serviceManager.MappedNamespaces)}>";


                logicFormatter.AppendCodeLine(2, "/// <summary>");
                logicFormatter.AppendCodeLine(2, $"/// Service implementation for the logic method '{logicMethod.Name}'");
                logicFormatter.AppendCodeLine(2, "/// </summary>");
                if (!isPost)
                {
                    logicFormatter.AppendCodeLine(2, $"[HttpGet(\"{serviceCallName}\")]");
                    logicFormatter.AppendCodeLine(2, $"public async Task<ActionResult<{returnTypeSyntax}>> {serviceMethodName}()");
                }
                else
                {
                    logicFormatter.AppendCodeLine(2, $"[HttpPost(\"{serviceCallName}\")]");
                    logicFormatter.AppendCodeLine(2, $"public async Task<ActionResult<{returnTypeSyntax}>> {serviceMethodName}([FromBody]{parameterType} {parameterName})");
                }

                logicFormatter.AppendCodeLine(2, "{");

                logicFormatter.AppendCodeLine(3, "_logger.InformationEnterLog();");
                logicFormatter.AppendCodeLine(3);

                if (logicMethod.HasParameters)
                {
                    if (usesRequestModel) isNullBoundsCheck = true;
                    else
                    {
                        if (logicMethod.Parameters[0].ParameterType.WellKnownType == CsKnownLanguageType.String) isStringBoundsCheck = true;
                        else if (!logicMethod.Parameters[0].ParameterType.IsValueType) isNullBoundsCheck = true;
                    }

                    if (isNullBoundsCheck)
                    {
                        logicFormatter.AppendCodeLine(3, $"if ({parameterName} == null)");
                        logicFormatter.AppendCodeLine(3, "{");
                        logicFormatter.AppendCodeLine(4, $"_logger.ErrorLog($\"The parameter {{nameof({parameterName})}} was not provided. Will raise an argument exception\");");
                        logicFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                        logicFormatter.AppendCodeLine(4, $"return {returnTypeSyntax}.CreateError(new ValidationException(nameof({parameterName})));");
                        logicFormatter.AppendCodeLine(3, "}");
                        logicFormatter.AppendCodeLine(3);
                    }

                    if (isStringBoundsCheck)
                    {
                        logicFormatter.AppendCodeLine(3, $"if(string.IsNullOrEmpty({parameterName}))");
                        logicFormatter.AppendCodeLine(3, "{");
                        logicFormatter.AppendCodeLine(4, $"_logger.ErrorLog($\"The parameter {{nameof({parameterName})}} was not provided. Will raise an argument exception\");");
                        logicFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                        logicFormatter.AppendCodeLine(4, $"return {returnTypeSyntax}.CreateError(new ValidationException(nameof({parameterName})));");
                        logicFormatter.AppendCodeLine(3, "}");
                        logicFormatter.AppendCodeLine(3);
                    }
                }

                if (logicReturnType != null)
                {
                    logicFormatter.AppendCodeLine(3, logicReturnType.IsValueType
                            ? $"{logicReturnType.GenerateCSharpTypeName(serviceManager.NamespaceManager, serviceManager.MappedNamespaces)} result;"
                            : $"{logicReturnType.GenerateCSharpTypeName(serviceManager.NamespaceManager, serviceManager.MappedNamespaces)} result = null;");
                }

                logicFormatter.AppendCodeLine(3, "try");
                logicFormatter.AppendCodeLine(3, "{");


                string returnValue = returnsData ? "result = " : "";
                string awaitStatement = isLogicAsyncMethod ? "await " : "";

                string formattedParameters = "";

                if (logicMethod.HasParameters)
                {
                    if (logicMethod.Parameters.Count == 1)
                    {
                        formattedParameters = logicMethod.Parameters[0].ParameterType.WellKnownType == CsKnownLanguageType.String ? $"{parameterName}.GetPostValue()" : parameterName;
                    }
                    else
                    {
                        bool isFirstParameter = true;
                        StringBuilder logicStringBuilder = new StringBuilder();
                        foreach (var logicMethodParameter in logicMethod.Parameters)
                        {

                            if (isFirstParameter)
                            {
                                logicStringBuilder.Append(logicMethodParameter.ParameterType.WellKnownType == CsKnownLanguageType.String ? $"request.{logicMethodParameter.Name.GenerateCSharpProperCase()}.GetPostValue()" : $"request.{logicMethodParameter.Name.GenerateCSharpProperCase()}");
                                isFirstParameter = false;
                            }
                            else
                            {
                                logicStringBuilder.Append(logicMethodParameter.ParameterType.WellKnownType == CsKnownLanguageType.String ? $", request.{logicMethodParameter.Name.GenerateCSharpProperCase()}.GetPostValue()" : $", request.{logicMethodParameter.Name.GenerateCSharpProperCase()}");
                            } 
                        }

                        formattedParameters = logicStringBuilder.ToString();
                    }
                }

                logicFormatter.AppendCodeLine(4, $"{returnValue}{awaitStatement} {logicVariable}.{logicMethod.Name}({formattedParameters});");


                logicFormatter.AppendCodeLine(3, "}");

                logicFormatter.AppendCodeLine(3, "catch (ManagedException managed)");
                logicFormatter.AppendCodeLine(3, "{");
                logicFormatter.AppendCodeLine(4, "_logger.ErrorLog(\"Raising the handled exception to the caller of the service.\");");
                logicFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                logicFormatter.AppendCodeLine(4, $"return {returnTypeSyntax}.CreateError(managed);");
                logicFormatter.AppendCodeLine(3, "}");

                logicFormatter.AppendCodeLine(3, "catch (Exception unhandledException)");
                logicFormatter.AppendCodeLine(3, "{");
                logicFormatter.AppendCodeLine(4, "_logger.CriticalLog(\"An unhandled exception occurred, see the exception for details. Will throw a UnhandledException\", unhandledException);");
                logicFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                logicFormatter.AppendCodeLine(4, $"return {returnTypeSyntax}.CreateError(new UnhandledException());");
                logicFormatter.AppendCodeLine(3, "}");
                logicFormatter.AppendCodeLine(3);

                logicFormatter.AppendCodeLine(3, "_logger.InformationExitLog();");
                logicFormatter.AppendCodeLine(3, returnsData ? $"return {returnTypeSyntax}.CreateResult(result);" : $"return {returnTypeSyntax}.CreateSuccess();");

                logicFormatter.AppendCodeLine(2, "}");
                logicFormatter.AppendCodeLine(2);

                await serviceManager.ConstructorsAddAfterAsync(logicFormatter.ReturnSource());

                logicFormatter.ResetFormatter();
            }

            return serviceManager.Container;
        }

        /// <summary>
        /// Adds the json service extensions if they are not found in the service project.
        /// </summary>
        /// <param name="source">CodeFactory automation used to write the extensions.</param>
        /// <param name="targetProject">The target project to check.</param>
        /// <exception cref="CodeFactoryException">Returned if required data is missing.</exception>
        public static async Task AddJsonServiceExtensionsAsync(this IVsActions source, VsProject targetProject)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot add json service extensions.");

            if (targetProject == null)
                throw new CodeFactoryException("The target project was not provided cannot add json service extensions.");

            if ((await targetProject.FindCSharpSourceByClassNameAsync("JsonServiceExtensions")) != null) return;

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using System;");
            sourceFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            sourceFormatter.AppendCodeLine(0, "using System.Linq;");
            sourceFormatter.AppendCodeLine(0, "using System.Text;");
            sourceFormatter.AppendCodeLine(0);
            sourceFormatter.AppendCodeLine(0, $"namespace {targetProject.DefaultNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Extensions for string type management with json data.");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, $"public static class JsonServiceExtensions");
            sourceFormatter.AppendCodeLine(1, "{");
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Place holder value used when passing strings in rest.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "private static string RestPostPlaceHolderValueForString = \"~~~Empty~~~\";");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Extension method that sets the post value for a string. If the string is null or empty will send a formatted string to represent empty or null.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"source\">Source string to set.</param>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>The formatted string value.</returns>");
            sourceFormatter.AppendCodeLine(2, "public static string SetPostValue(this string source)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "return string.IsNullOrEmpty(source) ? RestPostPlaceHolderValueForString : source;");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Extension method that gets the received value from a post. Will check for the empty value to convert the result to null or will pass the returned response.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"source\">Source string to get.</param>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>The formatted string value or null.</returns>");
            sourceFormatter.AppendCodeLine(2, "public static string GetPostValue(this string source)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "return source != RestPostPlaceHolderValueForString ? source : null;");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Extension method that determines if the string has a value or is empty.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"source\">Source string to check for a value.</param>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>True has a value or false if not.</returns>");
            sourceFormatter.AppendCodeLine(2, "public static bool HasValue(this string source)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "return !string.IsNullOrEmpty(source);");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");

            await targetProject.AddDocumentAsync("JsonServiceExtensions.cs", sourceFormatter.ReturnSource());
        }
    }
}
