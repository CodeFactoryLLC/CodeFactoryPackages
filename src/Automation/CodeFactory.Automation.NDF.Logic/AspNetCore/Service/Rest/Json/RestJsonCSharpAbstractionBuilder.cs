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

namespace CodeFactory.Automation.NDF.Logic.AspNetCore.Service.Rest.Json
{
/// <summary>
    /// Automation class that generates C# abstraction contracts.
    /// </summary>
    public static class RestJsonCSharpAbstractionBuilder
    {
        /// <summary>
        /// Refreshes the interface definition of an service abstraction client.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="contractName">The name of the abstraction contract to refresh.</param>
        /// <param name="sourceContract">The source contract used to refresh the abstraction contract.</param>
        /// <param name="contractProject">The project the abstraction is created in.</param>
        /// <param name="contractFolder">Optional, the target project folder the abstraction contract should be located in. Default value is null.</param>
        /// <returns>The inteface model that represents the abstraction contract.</returns>
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
                ? (await contractFolder.FindCSharpSourceByInterfaceNameAsync(sourceContract.Name))?.SourceCode
                : (await contractProject.FindCSharpSourceByInterfaceNameAsync(sourceContract.Name))?.SourceCode)
                ?? await source.CreateCSharpAbstractionContractAsync(contractName, sourceContract, contractProject, contractFolder);

            return await source.UpdateCSharpAbstractionContractAsync(sourceContract, contractSource);
        }

        /// <summary>
        /// Creates the interface definition of an service abstraction client.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="contractName">The name of the abstraction contract to refresh.</param>
        /// <param name="sourceContract">The source contract used to refresh the abstraction contract.</param>
        /// <param name="contractProject">The project the abstraction is created in.</param>
        /// <param name="contractFolder">Optional, the target project folder the abstraction contract should be located in. Default value is null.</param>
        /// <returns>The source code that hosts the inteface model that represents the abstraction contract.</returns>
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
        /// Updates the interface definition of an service abstraction client.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="contractName">The name of the abstraction contract to refresh.</param>
        /// <param name="sourceContract">The source contract used to refresh the abstraction contract.</param>
        /// <returns>The inteface model that represents the abstraction contract.</returns>
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
        /// <param name="serviceClass">The source service class the client is calling.</param>
        /// <param name="abstractionContract">The abstraction interface the client consumes.</param>
        /// <param name="serviceProject">The project that hosts the service being consumed.</param>
        /// <param name="abstractionProject">The abstraction project that hosts the client.</param>
        /// <param name="modelProject">The service model project that contains the service models.</param>
        /// <param name="abstractionFolder">The project folder where the abstaction client is located.</param>
        /// <param name="modelFolder">The project folder where the model data can be found for the client.</param>
        /// <returns>The client class model that was refreshed.</returns>
        /// <exception cref="CodeFactoryException">Raised if configuration information is missing or automation errors occurred.</exception>
        public static async Task<CsClass> RefreshAbstractionClass(this IVsActions source, string clientName, CsClass serviceClass,
            CsInterface abstractionContract, VsProject serviceProject, VsProject abstractionProject, VsProject modelProject, VsProjectFolder abstractionFolder = null, VsProjectFolder modelFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot refresh the abstraction.");

            if (string.IsNullOrEmpty(clientName))
                throw new CodeFactoryException("The client name was not provided, cannot refresh the abstraction.");

            if (abstractionContract == null)
                throw new CodeFactoryException("Cannot load the abstraction contract, cannot refresh the abstraction.");


            if (serviceClass == null)
                throw new CodeFactoryException("Cannot load the service class, cannot refresh the abstraction.");

            if (serviceProject == null)
                throw new CodeFactoryException("Cannot load the service project, cannot refresh the abstraction.");

            if (abstractionProject == null)
                throw new CodeFactoryException("Cannot load the abstraction project, cannot refresh the abstraction.");

            if (modelProject == null)
                throw new CodeFactoryException("Cannot load the model project, cannot refresh the abstraction.");

            await source.AddRestAbstractionBaseClassAsync(abstractionProject);
            await source.AddServiceUrlBaseClassAsync(abstractionProject);
            await source.AddServiceUrlClassAsync(serviceProject, abstractionProject, abstractionFolder);
            await source.AddJsonServiceExtensionsAsync(abstractionProject);

            var abstractionClassName = abstractionContract.Name.GenerateCSharpFormattedClassName();

            var abstractionSource = (abstractionFolder != null
                ? (await abstractionFolder.FindCSharpSourceByClassNameAsync(abstractionClassName))?.SourceCode
                : (await abstractionProject.FindCSharpSourceByClassNameAsync(abstractionClassName))?.SourceCode);

            var abstractionCreated = false;
            if(abstractionSource == null)
            { 
                abstractionCreated = true;
                abstractionSource = await source.CreateAbstractionClassAsync(clientName, abstractionContract, serviceProject, abstractionProject, abstractionFolder)
                    ?? throw new CodeFactoryException($"Could not create the client abstraction '{clientName}', cannot refresh the abstraction client.");
            }
           
            var clientClass = await source.UpdateAbstractionClassAsync(abstractionSource, serviceClass, abstractionContract, serviceProject, abstractionProject, modelProject, abstractionFolder, modelFolder);

            if(abstractionCreated) await source.RegisterTransientClassesAsync(abstractionProject,false);

            return clientClass;
        }

        /// <summary>
        /// Creates the instance of the abstraction client class.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="clientName">The class name of the client to be refreshed.</param>
        /// <param name="serviceClass">The source service class the client is calling.</param>
        /// <param name="abstractionContract">The abstraction interface the client consumes.</param>
        /// <param name="serviceProject">The project that hosts the service being consumed.</param>
        /// <param name="abstractionProject">The abstraction project that hosts the client.</param>
        /// <param name="abstractionFolder">The project folder where the abstaction client is located.</param>
        /// <returns>The source code that containes the client class model that was created.</returns>
        /// <exception cref="CodeFactoryException">Raised if configuration information is missing or automation errors occurred.</exception>
        private static async Task<CsSource> CreateAbstractionClassAsync(this IVsActions source, string clientName,
            CsInterface abstractionContract, VsProject serviceProject, VsProject abstractionProject, VsProjectFolder abstractionFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot create the abstraction.");

            if (string.IsNullOrEmpty(clientName))
                throw new CodeFactoryException("The client name was not provided, cannot create the abstraction.");

            if (abstractionContract == null)
                throw new CodeFactoryException("Cannot load the abstraction contract, cannot create the abstraction.");

            if (abstractionProject == null)
                throw new CodeFactoryException("Cannot load the abstraction project, cannot create the abstraction.");

            if (serviceProject == null)
                throw new CodeFactoryException("Cannot load the service project, cannot create the abstraction.");

            var sourceNamespace = abstractionFolder != null
                ? await abstractionFolder.GetCSharpNamespaceAsync()
                : abstractionProject.DefaultNamespace;

            if (string.IsNullOrEmpty(sourceNamespace)) throw new CodeFactoryException("Could not identify the target namespace for the abstraction, abstraction cannot be created.");

            var abstractionName = clientName;

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using System;");
            sourceFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            sourceFormatter.AppendCodeLine(0, "using System.Linq;");
            sourceFormatter.AppendCodeLine(0, "using System.Text;");
            sourceFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            sourceFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            sourceFormatter.AppendCodeLine(0, "using System.Net.Http;");
            sourceFormatter.AppendCodeLine(0, "using System.Net.Http.Json;");
            sourceFormatter.AppendCodeLine(0, "using CodeFactory.NDF;");

            if (sourceNamespace != abstractionContract.Namespace) sourceFormatter.AppendCodeLine(0, $"using {abstractionContract.Namespace};");
            if (sourceNamespace != abstractionProject.DefaultNamespace) sourceFormatter.AppendCodeLine(0, $"using {abstractionProject.DefaultNamespace};");

            sourceFormatter.AppendCodeLine(0, $"namespace {sourceNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Web abstraction implementation for abstraction contract '{abstractionContract.Name}'");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, $"public  class {abstractionName}:RestAbstraction, {abstractionContract.Name}");
            sourceFormatter.AppendCodeLine(1, "{");
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Url for the service being accessed by this abstraction.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "private readonly ServiceUrl _serviceUrl;");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Creates a instance of the web abstraction.");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"logger\">Logger that supports this abstraction.</param>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"url\">service url</param>");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, $"public {abstractionName}(ILogger<{abstractionName}> logger, Rest{serviceProject.Name.Replace(".", "")}ServiceUrl url):base(logger)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "_serviceUrl = url;");
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
        /// <param name="serviceClass">The source service class the client is calling.</param>
        /// <param name="abstractionContract">The abstraction interface the client consumes.</param>
        /// <param name="serviceProject">The project that hosts the service being consumed.</param>
        /// <param name="abstractionProject">The abstraction project that hosts the client.</param>
        /// <param name="modelProject">The service model project that contains the service models.</param>
        /// <param name="abstractionFolder">The project folder where the abstaction client is located.</param>
        /// <param name="modelFolder">The project folder where the model data can be found for the client.</param>
        /// <returns>The client class model that was refreshed.</returns>
        /// <exception cref="CodeFactoryException">Raised if configuration information is missing or automation errors occurred.</exception>
        private static async Task<CsClass> UpdateAbstractionClassAsync(this IVsActions source, CsSource abstractionSource, CsClass serviceClass,
            CsInterface abstractionContract, VsProject serviceProject, VsProject abstractionProject, VsProject modelProject, VsProjectFolder abstractionFolder = null, VsProjectFolder modelFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot update the abstraction.");

            if (abstractionContract == null)
                throw new CodeFactoryException("Cannot load the abstraction contract, cannot update the abstraction.");

            if (serviceClass == null)
                throw new CodeFactoryException("Cannot load the service class, cannot update the abstraction.");

            if (serviceProject == null)
                throw new CodeFactoryException("Cannot load the service project, cannot update the abstraction.");

            if (abstractionProject == null)
                throw new CodeFactoryException("Cannot load the abstraction project, cannot update the abstraction.");

            if (abstractionSource == null)
                throw new CodeFactoryException("Cannot load the service source code, cannot update the abstraction.");

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

            var missingServiceMethods = missingMethods.Where(m =>
            {
                var isOverload = contractMethods.Count(s => s.Name == m.Name) > 1;
                var serviceMethodName = $"{m.GetRestName(isOverload)}Async";

                var serviceMethod = serviceClass.Methods.FirstOrDefault(svc => svc.Name == serviceMethodName);

                return serviceMethod != null;

            }).ToList();

            if (!missingServiceMethods.Any()) return abstractionClass;

            string serviceName = serviceClass.Name.Replace("Controller", "");

            var serviceUrlParameter = $"_serviceUrl";

            var abstractManager = new SourceClassManager(currentSource, abstractionClass, source);
            abstractManager.LoadNamespaceManager();

            await abstractManager.UsingStatementAddAsync(abstractionProject.DefaultNamespace);

            if (modelFolder != null) await abstractManager.UsingStatementAddAsync(await modelFolder.GetCSharpNamespaceAsync());
            else await abstractManager.UsingStatementAddAsync(modelProject.DefaultNamespace);

            foreach (var contractMethod in missingServiceMethods)
            {

                bool isOverLoad = contractMethods.Count(m => m.Name == contractMethod.Name) > 1;

                bool isPost = contractMethod.IsPostCall();

                string actionName = contractMethod.GetRestName(isOverLoad);

                CsType logicReturnType = contractMethod.ReturnType.TaskReturnType();

                bool returnsData = logicReturnType != null;

                var contentFormatter = new SourceFormatter();



                string returnTypeSyntax = logicReturnType == null
                    ? "NoDataResult"
                    : $"ServiceResult<{logicReturnType.GenerateCSharpTypeName(abstractManager.NamespaceManager,abstractManager.MappedNamespaces)}>";

                if (contractMethod.HasDocumentation)
                {
                    var doc = contractMethod.GenerateCSharpXmlDocumentation();

                    contentFormatter.AppendCodeBlock(2, doc);
                }

                if (contractMethod.HasAttributes)
                {
                    foreach (var attributeData in contractMethod.Attributes)
                    {
                        contentFormatter.AppendCodeLine(2, attributeData.GenerateCSharpAttributeSignature(abstractManager.NamespaceManager,abstractManager.MappedNamespaces));
                    }
                }

                contentFormatter.AppendCodeLine(2,
                    $"{contractMethod.GenerateCSharpMethodSignature(abstractManager.NamespaceManager)}");
                contentFormatter.AppendCodeLine(2, "{");

                contentFormatter.AppendCodeLine(3, "_logger.InformationEnterLog();");


                if (contractMethod.HasParameters)
                {

                    foreach (ICsParameter paramData in contractMethod.Parameters)
                    {
                        //If the parameter has a default value then continue will not bounds check parameters with a default value.
                        if (paramData.HasDefaultValue) continue;

                        //If the parameter is a string type add the following bounds check
                        if (paramData.ParameterType.WellKnownType == CsKnownLanguageType.String &
                            !paramData.HasDefaultValue)
                        {
                            //Adding an if check 
                            contentFormatter.AppendCodeLine(3, $"if(string.IsNullOrEmpty({paramData.Name}))");
                            contentFormatter.AppendCodeLine(3, "{");

                            contentFormatter.AppendCodeLine(4,
                                $"_logger.ErrorLog($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            contentFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                            contentFormatter.AppendCodeLine(4,
                                $"throw new ValidationException(nameof({paramData.Name}));");

                            contentFormatter.AppendCodeLine(3, "}");
                            contentFormatter.AppendCodeLine(3);
                        }

                        // Check to is if the parameter is not a value type or a well know type if not then go ahead and perform a null bounds check.
                        if (!paramData.ParameterType.IsValueType & !paramData.ParameterType.IsWellKnownType &
                            !paramData.HasDefaultValue)
                        {
                            //Adding an if check 
                            contentFormatter.AppendCodeLine(3, $"if({paramData.Name} == null)");
                            contentFormatter.AppendCodeLine(3, "{");

                            contentFormatter.AppendCodeLine(4,
                                $"_logger.ErrorLog($\"The parameter {{nameof({paramData.Name})}} was not provided. Will raise an argument exception\");");
                            contentFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                            contentFormatter.AppendCodeLine(4,
                                $"throw new ValidationException(nameof({paramData.Name}));");

                            contentFormatter.AppendCodeLine(3, "}");
                            contentFormatter.AppendCodeLine(3);
                        }
                    }
                }

                if (returnsData)
                {
                    contentFormatter.AppendCodeLine(3,
                        logicReturnType.IsClass
                            ? $"{logicReturnType.GenerateCSharpTypeName(abstractManager.NamespaceManager,abstractManager.MappedNamespaces)} result = null; "
                            : $"{logicReturnType.GenerateCSharpTypeName(abstractManager.NamespaceManager, abstractManager.MappedNamespaces)} result;");
                    contentFormatter.AppendCodeLine(3);
                }

                contentFormatter.AppendCodeLine(3, "try");
                contentFormatter.AppendCodeLine(3, "{");

                contentFormatter.AppendCodeLine(4,
                    $"using (HttpClient httpClient = await GetHttpClient())");
                contentFormatter.AppendCodeLine(4, "{");

                if (isPost)
                {
                    if (contractMethod.Parameters.Count == 1)
                    {
                        var parameter = contractMethod.Parameters[0].ParameterType.WellKnownType == CsKnownLanguageType.String
                                ? $"{contractMethod.Parameters[0].Name}.SetPostValue()"
                                : contractMethod.Parameters[0].Name;

                        contentFormatter.AppendCodeLine(5, $"var serviceData = await httpClient.PostAsJsonAsync<{contractMethod.Parameters[0].ParameterType.GenerateCSharpTypeName(abstractManager.NamespaceManager,abstractManager.MappedNamespaces)}>($\"{{{serviceUrlParameter}.Url}}/api/{serviceName}/{actionName}\", {parameter});");
                        contentFormatter.AppendCodeLine(5);
                    }
                    else
                    {
                        var requestBuilder = new StringBuilder();
                        bool isFirst = true;

                        foreach (var parameter in contractMethod.Parameters)
                        {
                            var formattedParameter = parameter.ParameterType.WellKnownType == CsKnownLanguageType.String
                                ? $"{parameter.Name}.SetPostValue()"
                                : parameter.Name;

                            if (isFirst)
                            {

                                requestBuilder.Append($"{parameter.Name.GenerateCSharpProperCase()} = {formattedParameter}");
                                isFirst = false;
                            }
                            else
                            {
                                requestBuilder.Append($", {parameter.Name.GenerateCSharpProperCase()} = {formattedParameter}");
                            }
                        }

                        var requestName = contractMethod.GetRestServiceRequestModelName(serviceName);
                        contentFormatter.AppendCodeLine(5, $"var serviceData = await httpClient.PostAsJsonAsync<{requestName}>($\"{{{serviceUrlParameter}.Url}}/api/{serviceName}/{actionName}\", new {requestName} {{ {requestBuilder} }});");
                        contentFormatter.AppendCodeLine(5);
                    }

                    contentFormatter.AppendCodeLine(5, "await RaiseUnhandledExceptionsAsync(serviceData);");
                    contentFormatter.AppendCodeLine(5);
                    contentFormatter.AppendCodeLine(5, $"var serviceResult = await serviceData.Content.ReadFromJsonAsync<{returnTypeSyntax}>();");
                    contentFormatter.AppendCodeLine(5);

                }
                else
                {
                    contentFormatter.AppendCodeLine(5, $"var serviceResult = await httpClient.GetFromJsonAsync<{returnTypeSyntax}>($\"{{{serviceUrlParameter}.Url}}/api/{serviceName}/{actionName}\");");
                    contentFormatter.AppendCodeLine(5);
                }

                contentFormatter.AppendCodeLine(5, "if (serviceResult == null) throw new ManagedException(\"Internal error occurred no data was returned\");");
                contentFormatter.AppendCodeLine(5);
                contentFormatter.AppendCodeLine(5, "serviceResult.RaiseException();");
                contentFormatter.AppendCodeLine(5);

                if (returnsData)
                {
                    contentFormatter.AppendCodeLine(5, "result = serviceResult.Result;");
                    contentFormatter.AppendCodeLine(5);
                }

                contentFormatter.AppendCodeLine(4, "}");

                contentFormatter.AppendCodeLine(3, "}");

                contentFormatter.AppendCodeLine(3, "catch (ManagedException)");
                contentFormatter.AppendCodeLine(3, "{");
                contentFormatter.AppendCodeLine(4, "//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.");
                contentFormatter.AppendCodeLine(4, "throw;");
                contentFormatter.AppendCodeLine(3, "}");

                contentFormatter.AppendCodeLine(3, "catch (Exception unhandledException)");
                contentFormatter.AppendCodeLine(3, "{");
                contentFormatter.AppendCodeLine(4, "_logger.ErrorLog(\"An unhandled exception occurred, see the exception for details. Will throw a UnhandledException\", unhandledException);");
                contentFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
                contentFormatter.AppendCodeLine(4, "throw new UnhandledException();");
                contentFormatter.AppendCodeLine(3, "}");
                contentFormatter.AppendCodeLine(3);
                contentFormatter.AppendCodeLine(3, "_logger.InformationExitLog();");
                contentFormatter.AppendCodeLine(3);

                if (returnsData) contentFormatter.AppendCodeLine(3, "return result;");

                contentFormatter.AppendCodeLine(2, "}");
                contentFormatter.AppendCodeLine(2);

                await abstractManager.ConstructorsAddAfterAsync(contentFormatter.ReturnSource());

                contentFormatter.ResetFormatter();
            }

            return null;
            //return abstractManager.Container;
        }

        private static async Task AddRestAbstractionBaseClassAsync(this IVsActions source, VsProject abstractionProject)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot add the RestAbstraction base class.");

            if (abstractionProject == null)
                throw new CodeFactoryException("Cannot load the abstraction project, cannot add the RestAbstraction base class.");

            if ((await abstractionProject.FindCSharpSourceByClassNameAsync("RestAbstraction")) != null) return;

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using System;");
            sourceFormatter.AppendCodeLine(0, "using System.Threading.Tasks;");
            sourceFormatter.AppendCodeLine(0, "using Microsoft.Extensions.Logging;");
            sourceFormatter.AppendCodeLine(0, "using System.Net.Http;");
            sourceFormatter.AppendCodeLine(0, "using CodeFactory.NDF;");
            sourceFormatter.AppendCodeLine(0, "using System.Net.Http.Json;");
            sourceFormatter.AppendCodeLine(0, "using System.Net;");
            sourceFormatter.AppendCodeLine(0, "using System.Text.Json.Nodes;");
            sourceFormatter.AppendCodeLine(0, "using Microsoft.AspNetCore.Mvc;");
            sourceFormatter.AppendCodeLine(0, $"namespace {abstractionProject.DefaultNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Base class implementation that supports rest abstractions ");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, $"public abstract class RestAbstraction");
            sourceFormatter.AppendCodeLine(1, "{");
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Logger for use by the abstraction.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "protected readonly ILogger _logger;");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Initializes the base class logic and functionality.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"logger\">Logger to be used by the abstraction.</param>");
            sourceFormatter.AppendCodeLine(2, "protected RestAbstraction(ILogger logger)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "_logger = logger;");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Creates a http client used for REST service calls.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <returns>The http client to be used with the service request.</returns>");
            sourceFormatter.AppendCodeLine(2, "protected async Task<HttpClient> GetHttpClient()");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "_logger.InformationEnterLog();");
            sourceFormatter.AppendCodeLine(3, "HttpClient client = null;");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "try");
            sourceFormatter.AppendCodeLine(3, "{");
            sourceFormatter.AppendCodeLine(4, "client = new HttpClient(new HttpClientHandler { UseDefaultCredentials = true });");
            sourceFormatter.AppendCodeLine(4);
            sourceFormatter.AppendCodeLine(4, "//Extend any additional changes to the client here.");
            sourceFormatter.AppendCodeLine(3, "}");
            sourceFormatter.AppendCodeLine(3, "catch (ManagedException)");
            sourceFormatter.AppendCodeLine(3, "{");
            sourceFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            sourceFormatter.AppendCodeLine(4, "throw;");
            sourceFormatter.AppendCodeLine(3, "}");
            sourceFormatter.AppendCodeLine(3, "catch (Exception unhandledError)");
            sourceFormatter.AppendCodeLine(3, "{");
            sourceFormatter.AppendCodeLine(4, "_logger.ErrorLog(\"A unhandled error occurred, review the exception for details of the error.\",unhandledError);");
            sourceFormatter.AppendCodeLine(4, "_logger.InformationExitLog();");
            sourceFormatter.AppendCodeLine(4, "throw new UnhandledException();");
            sourceFormatter.AppendCodeLine(3, "}");
            sourceFormatter.AppendCodeLine(3);
            sourceFormatter.AppendCodeLine(3, "_logger.InformationExitLog();");
            sourceFormatter.AppendCodeLine(3, "return client;");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Converts bad requests triggered by model validation errors into <see cref=\"ManagedException\"/> data.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"message\">The response message to check for validation errors.</param>");
            sourceFormatter.AppendCodeLine(2, "/// <exception cref=\"ManagedException\">Managed exception that is thrown from the bad request.</exception>");
            sourceFormatter.AppendCodeLine(2, "protected  async Task RaiseUnhandledExceptionsAsync(HttpResponseMessage message)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "if (message.StatusCode == HttpStatusCode.BadRequest)");
            sourceFormatter.AppendCodeLine(3, "{");

            sourceFormatter.AppendCodeLine(4, "//Checking to see if problem details were returned.");
            sourceFormatter.AppendCodeLine(4, "if (message.Content.Headers.ContentType?.MediaType != null && message.Content.Headers.ContentType.MediaType.Contains(@\"problem+json\"))");
            sourceFormatter.AppendCodeLine(4, "{");

            sourceFormatter.AppendCodeLine(5, "//Reading in the problem details");
            sourceFormatter.AppendCodeLine(5, "var problemDetails = await message.Content.ReadFromJsonAsync<ProblemDetails>();");
            sourceFormatter.AppendCodeLine(5);
            sourceFormatter.AppendCodeLine(5, "//If error extensions are provided parse them");
            sourceFormatter.AppendCodeLine(5, "if (problemDetails?.Extensions != null)");
            sourceFormatter.AppendCodeLine(5, "{");

            sourceFormatter.AppendCodeLine(6, "//Get JsonNode when there are errors.");
            sourceFormatter.AppendCodeLine(6, "var jsonNode = problemDetails.Extensions.Where(x => x.Key == \"errors\")");
            sourceFormatter.AppendCodeLine(6, "    .Select(x => JsonNode.Parse(x.Value.ToString())).FirstOrDefault();");
            sourceFormatter.AppendCodeLine(6);
            sourceFormatter.AppendCodeLine(6, "//Build up the list of validation exceptions.");
            sourceFormatter.AppendCodeLine(6, "if (jsonNode != null)");
            sourceFormatter.AppendCodeLine(6, "{");

            sourceFormatter.AppendCodeLine(7, "var errorArrays = jsonNode.AsObject().Select(x => x.Value.AsArray()).ToList();");
            sourceFormatter.AppendCodeLine(7, "var exceptions = new List<ManagedException>();");
            sourceFormatter.AppendCodeLine(7, "var errorMessages = (from errors in errorArrays from error in errors where error != null select error.ToString()).ToList();");
            sourceFormatter.AppendCodeLine(7, "exceptions.AddRange(errorMessages.Select(x => new ValidationException(x)).ToList());");
            sourceFormatter.AppendCodeLine(7, "throw new ManagedExceptions(exceptions);");

            sourceFormatter.AppendCodeLine(6, "}");

            sourceFormatter.AppendCodeLine(5, "}");

            sourceFormatter.AppendCodeLine(4, "}");

            sourceFormatter.AppendCodeLine(3, "}");
            sourceFormatter.AppendCodeLine(3, "message.EnsureSuccessStatusCode();");

            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");

            await abstractionProject.AddDocumentAsync("RestAbstraction.cs", sourceFormatter.ReturnSource());
        }

        private static async Task AddServiceUrlBaseClassAsync(this IVsActions source, VsProject abstractionProject)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot add the ServiceUrl base class.");

            if (abstractionProject == null)
                throw new CodeFactoryException("Cannot load the abstraction project, cannot add the ServiceUrl base class.");

            if ((await abstractionProject.FindCSharpSourceByClassNameAsync("ServiceUrl")) != null) return;

            var sourceFormatter = new SourceFormatter();

            sourceFormatter.AppendCodeLine(0, "using System;");
            sourceFormatter.AppendCodeLine(0, "using System.Collections.Generic;");
            sourceFormatter.AppendCodeLine(0, "using System.Text;");
            sourceFormatter.AppendCodeLine(0, $"namespace {abstractionProject.DefaultNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, "/// Base implementation for a web service url definition.");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, "public abstract class ServiceUrl");
            sourceFormatter.AppendCodeLine(1, "{");
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Backing field for the url path.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "private readonly string _url;");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Creates a instance of the service url.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"url\">URL of the service.</param>");
            sourceFormatter.AppendCodeLine(2, "protected ServiceUrl(string url)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "_url = url;");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(2);
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// The url where a service is hosted.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "public string Url => _url;");
            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");

            await abstractionProject.AddDocumentAsync("ServiceUrl.cs", sourceFormatter.ReturnSource());
        }

        private static async Task AddServiceUrlClassAsync(this IVsActions source, VsProject serviceProject,
            VsProject abstractionProject, VsProjectFolder abstractionFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot create the service url.");

            if (abstractionProject == null)
                throw new CodeFactoryException("Cannot load the abstraction project, cannot create the service url.");

            if (serviceProject == null)
                throw new CodeFactoryException("Cannot load the service project, cannot create the service url.");

            string className = $"Rest{serviceProject.Name.Replace(".", "")}ServiceUrl";

            if (abstractionFolder != null)
            {
                if ((await abstractionFolder.FindCSharpSourceByClassNameAsync(className)) != null) return;
            }
            else
            {
                if ((await abstractionProject.FindCSharpSourceByClassNameAsync(className)) != null) return;
            }

            var defaultNamespace = abstractionFolder != null
                ? await abstractionFolder.GetCSharpNamespaceAsync()
                : abstractionProject.DefaultNamespace;

            var sourceFormatter = new SourceFormatter();

            if (defaultNamespace != abstractionProject.DefaultNamespace)
                sourceFormatter.AppendCodeLine(0, $"using {abstractionProject.DefaultNamespace};");

            sourceFormatter.AppendCodeLine(0, $"namespace {defaultNamespace}");
            sourceFormatter.AppendCodeLine(0, "{");
            sourceFormatter.AppendCodeLine(1, "/// <summary>");
            sourceFormatter.AppendCodeLine(1, $"/// Rest service url for the {serviceProject.Name} service.");
            sourceFormatter.AppendCodeLine(1, "/// </summary>");
            sourceFormatter.AppendCodeLine(1, $"public class {className} : ServiceUrl");
            sourceFormatter.AppendCodeLine(1, "{");
            sourceFormatter.AppendCodeLine(2, "/// <summary>");
            sourceFormatter.AppendCodeLine(2, "/// Creates an instance of the service url.");
            sourceFormatter.AppendCodeLine(2, "/// </summary>");
            sourceFormatter.AppendCodeLine(2, "/// <param name=\"url\">URL for accessing the service.</param>");
            sourceFormatter.AppendCodeLine(2, $"public {className}(string url) : base(url)");
            sourceFormatter.AppendCodeLine(2, "{");
            sourceFormatter.AppendCodeLine(3, "//Intentionally blank.");
            sourceFormatter.AppendCodeLine(2, "}");
            sourceFormatter.AppendCodeLine(1, "}");
            sourceFormatter.AppendCodeLine(0, "}");

            if (abstractionFolder != null) await abstractionFolder.AddDocumentAsync($"{className}.cs", sourceFormatter.ReturnSource());
            else await abstractionProject.AddDocumentAsync($"{className}.cs", sourceFormatter.ReturnSource());
        }
    }
}
