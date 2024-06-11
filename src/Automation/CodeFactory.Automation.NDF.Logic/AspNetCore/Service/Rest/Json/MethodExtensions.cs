using CodeFactory.Automation.Standard.Logic.Extensions;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.CSharp.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic.AspNetCore.Service.Rest.Json
{
/// <summary>
    /// Method extensions that support name generation for rest services.
    /// </summary>
    public static class MethodExtensions
    {
        /// <summary>
        /// Generates the rest action name based on the method name and the supporting parameters of the method.
        /// </summary>
        /// <param name="source">The source method to extract the name from.</param>
        /// <param name="fullSignatureName">Flag that determines if the parameters of the method should also be used in the name rest call.</param>
        /// <returns>The formatted name of the rest call.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing.</exception>
        public static string GetRestName(this CsMethod source, bool fullSignatureName = false)
        {
            if (source == null)
                throw new CodeFactoryException(
                    "Cannot update the rest service, a method was not provided so cannot determine the name of the rest call.");

            if (string.IsNullOrEmpty(source.Name))
                throw new CodeFactoryException(
                    "Cannot update the rest service, the method name was empty or null, cannot determine the name of the rest call.");


            StringBuilder restNameBuilder = new StringBuilder();

            var methodName = source.Name.Trim();

            restNameBuilder.Append(methodName.EndsWith("Async", StringComparison.InvariantCultureIgnoreCase)
                ? methodName.Substring(0, methodName.Length - 5)
                : methodName);

            if (!(fullSignatureName & source.HasParameters)) return restNameBuilder.ToString();

            restNameBuilder.Append("By");

            bool firstParameter = true;
            foreach (var sourceParameter in source.Parameters)
            {
                if (string.IsNullOrEmpty(sourceParameter.Name)) continue;

                if (firstParameter)
                {
                    restNameBuilder.Append(sourceParameter.Name.ToUpper().First());
                    firstParameter = false;
                }
                else
                {
                    restNameBuilder.Append(sourceParameter.Name.ToLower().First());
                }
            }

            return restNameBuilder.ToString();
        }

        /// <summary>
        /// Determines of the source method will be called as a post call or not. 
        /// </summary>
        /// <param name="source">Target method to check for a post call.</param>
        /// <returns>True if the call will be post based or false if not.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing.</exception>
        public static bool IsPostCall(this CsMethod source)
        {
            if (source == null)
                throw new CodeFactoryException("No method was provided cannot determine if a Post call.");

            return source.HasParameters;
        }


		//
		// Summary:
		//     Generates a string representation of the await statement.
		//
		// Parameters:
		//   source:
		//     CsMethod to generate parameters for.
		//
		// Returns:
		//     True if has a return type.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     Instance of the method was not provided.
		public static SourceFormatter GenerateNewTestObjectsFromParameters(this CsMethod source, SourceFormatter sourceFormatter)
		{
			StringBuilder parameterBuilder = new StringBuilder();

			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			// Initialize parameter variables
			if (source.HasParameters)
			{
				bool firstParameter = true;

				foreach (var testParameter in source.Parameters)
				{
					if (firstParameter)
					{
						parameterBuilder.Append($"{testParameter.Name}");
						firstParameter = false;
					}
					else parameterBuilder.Append($", {testParameter.Name}");

					string defaultValue = testParameter.ParameterType.GenerateCSharpDefaultTestValue(testParameter.Name);
					sourceFormatter.AppendCodeLine(3,
						defaultValue != null
						 ? $"{testParameter.ParameterType.GenerateCSharpTypeName()} {testParameter.Name} = {defaultValue};"
						: $"{testParameter.ParameterType.GenerateCSharpTypeName()} {testParameter.Name};");

					// Create default properties.
					if (!testParameter.ParameterType.IsWellKnownType)
					{
						foreach (var property in testParameter.ParameterType.GetClassModel().Properties)
						{
							// Ignore the first property, as this is the primary key.
							if (property.Name != testParameter.ParameterType.GetClassModel().Properties[0].Name)
								sourceFormatter.AppendCodeLine(3, $"{testParameter.Name}.{property.Name} = {property.PropertyType.GenerateCSharpDefaultTestValue(property.Name)};");
						}
					}

					sourceFormatter.AppendCodeLine(0);
				}
			}

			return sourceFormatter;
		}

		/// <summary>
		/// Generates the full method signature for a REST Controller.
		/// </summary>
		/// <param name="source">Method to generate type name from.</param>
		/// <returns>Fully formatted list of attribute parameters.</returns>
		public static string GenerateRESTControllerMethodSignature(this CsMethod source, SourceClassManager serviceManager, bool isOverload)
		{
			var returnSyntax = (string)null;
			if (source == null || !source.IsLoaded) return returnSyntax;

			string serviceCallName = source.GetRestName(isOverload);
			string serviceMethodName = $"{serviceCallName}Async";
			CsType returnType = source.ReturnType.TaskReturnType();

			bool returnsData = returnType != null;

			var returnTypeSyntax = returnType == null ? "NoDataResult" : $"ServiceResult<{returnType.GenerateCSharpTypeName(serviceManager.NamespaceManager, serviceManager.MappedNamespaces)}>";

			returnSyntax = $"public async Task<ActionResult<{returnTypeSyntax}>> {serviceMethodName}({source.BuildServiceMethodParameters(serviceManager)})";

			return returnSyntax;
		}

		/// <summary>
		/// Generates 
		/// </summary>
		/// <param name="source">Method to generate type name from.</param>
		/// <returns>Fully formatted list of attribute parameters.</returns>
		public static SourceFormatter GenerateHttpClientCaller(this CsMethod source, SourceFormatter contentFormatter, SourceClassManager classManager, string serviceName, string returnTypeSyntax)
		{
			var returnSyntax = (string)null;
			if (source == null || !source.IsLoaded) return contentFormatter;

			CsType returnType = source.ReturnType.TaskReturnType();
			var serviceUrlParameter = $"_serviceUrl";
			var collectionPath = serviceName;
			if (!string.IsNullOrEmpty(collectionPath))
				collectionPath = $"{collectionPath.ToLower()}/{collectionPath.ToLower().Pluralize()}/";

			var url = $"$\"{{{serviceUrlParameter}.Url}}/api/v1/{collectionPath}\"";

			if (source.Name != "GetAsync")
			{
				// build api caller with query parameters.
				if (source.HasParameters && source.Parameters.All(p => p.ParameterType.IsWellKnownType))
				{
					var queryParameters = new StringBuilder();
					bool isFirst = true;

					foreach (var parameter in source.Parameters)
					{
						if (isFirst)
						{
							queryParameters.Append($"?{parameter.Name.ToSnakeCase()}={{{parameter.Name}}}");
							isFirst = false;
						}
						else
						{
							queryParameters.Append($"&{parameter.Name.ToSnakeCase()}={{{parameter.Name}}}");
						}
					}

					url = $"$\"{{{serviceUrlParameter}.Url}}/api/v1/{collectionPath}{queryParameters}\"";
					contentFormatter.AppendCodeLine(5, $"var request = new HttpRequestMessage({source.GenerateHttpRequestMethodType()}, {url});");
					contentFormatter.AppendCodeLine(5, $"request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(\"application/json\"));");
				}
				// Build api caller with serialized body content.
				else
				{
					var requestName = source.GetRestServiceRequestModelName(serviceName);

					contentFormatter.AppendCodeLine(5, $"var request = new HttpRequestMessage({source.GenerateHttpRequestMethodType()}, {url});");
					contentFormatter.AppendCodeLine(5, $"request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(\"application/json\"));");

					if (source.HasParameters)
					{
						contentFormatter.AppendCodeLine(5, $"request.Content = new StringContent(JsonSerializer.Serialize({source.Parameters[0].Name}), Encoding.UTF8);");
						contentFormatter.AppendCodeLine(5, $"request.Content.Headers.ContentType = new MediaTypeHeaderValue(\"application/json\");");
					}
				}
			}
			else
			{
				url = $"$\"{{{serviceUrlParameter}.Url}}/api/v1/{collectionPath}{{{source.Parameters[0].Name}}}\"";
				contentFormatter.AppendCodeLine(5, $"var request = new HttpRequestMessage({source.GenerateHttpRequestMethodType()}, {url});");
				contentFormatter.AppendCodeLine(5, $"request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(\"application/json\"));");
			}

			contentFormatter.AppendCodeLine(5, $"var serviceData = await httpClient.SendAsync(request);");
			contentFormatter.AppendCodeLine(5);

			contentFormatter.AppendCodeLine(5, "await RaiseUnhandledExceptionsAsync(serviceData);");
			contentFormatter.AppendCodeLine(5);
			contentFormatter.AppendCodeLine(5, $"var serviceResult = await serviceData.Content.ReadFromJsonAsync<{returnTypeSyntax}>();");
			contentFormatter.AppendCodeLine(5);
			return contentFormatter;
		}

		/// <summary>
		/// Generates the full method signature for a BFF Controller.
		/// </summary>
		/// <param name="source">Method to generate type name from.</param>
		/// <returns>Fully formatted list of attribute parameters.</returns>
		public static string GenerateBFFControllerMethodSignature(this CsMethod source, SourceClassManager serviceManager, bool isOverload)
		{
			var returnSyntax = (string)null;
			if (source == null || !source.IsLoaded) return returnSyntax;

			string serviceCallName = source.GetRestName(isOverload);
			string serviceMethodName = $"{serviceCallName}Async";
			CsType returnType = source.ReturnType.TaskReturnType();

			if (returnType != null)
				returnSyntax = $"public async Task<{returnType.GenerateCSharpTypeName(serviceManager.NamespaceManager, serviceManager.MappedNamespaces)}> {serviceMethodName}({source.BuildServiceMethodParameters(serviceManager)})";
			else
				returnSyntax = $"public async Task {serviceMethodName}({source.BuildServiceMethodParameters(serviceManager)})";
			return returnSyntax;
		}

		/// <summary>
		/// Builds a service attribute based on method properties.
		/// </summary>
		/// <param name="source">Method to generate type name from.</param>
		/// <returns>Fully formatted list of attributes.</returns>
		public static string GenerateHttpAttribute(this CsMethod source, SourceClassManager serviceManager)
		{
			var returnSyntax = (string)null;
			if (source == null || !source.IsLoaded) return returnSyntax;

			if (source.HasParameters && source.Parameters.Count == 1 && source.Parameters[0].ParameterType.IsWellKnownType)
			{
				if (source.Parameters[0].ParameterType.WellKnownType == CsKnownLanguageType.String)
					returnSyntax = "[" + source.GenerateHttpAttributeType() + "(\"{" + source.Parameters[0].Name.ToSnakeCase() + "}\")]";
				else
					returnSyntax = "[" + source.GenerateHttpAttributeType() + "(\"{" + source.Parameters[0].Name.ToSnakeCase() + ":" + source.Parameters[0].ParameterType.GenerateCSharpTypeName(serviceManager.NamespaceManager, serviceManager.MappedNamespaces) + "}\")]";
			}
			else
			{
				// If there are more than 1 parameter, exclude them from here since they will need to be added as method parameters.
				returnSyntax = $"[{source.GenerateHttpAttributeType()}]";
			}

			return returnSyntax;
		}

		/// <summary>
		/// Builds a string list of attribute parameters.
		/// </summary>
		/// <param name="source">Method to generate type name from.</param>
		/// <returns>Fully formatted list of attribute parameters.</returns>
		private static string BuildServiceMethodParameters(this CsMethod source, SourceClassManager serviceManager)
		{
			var returnSyntax = (string)null;
			if (source == null || !source.IsLoaded) return returnSyntax;

			var parameterAttribute = "";
			if (source.Name.ToLower() == "getasync")
				parameterAttribute = "";
			else if (source.Parameters.All(p => p.ParameterType.IsWellKnownType))
				parameterAttribute = "[FromQuery] ";
			else
				parameterAttribute = "[FromBody] ";

			if (source.HasParameters)
			{
				int count = 0;
				foreach (var parameter in source.Parameters)
				{
					if (count > 0)
						returnSyntax += ", ";

					returnSyntax += ($"{parameterAttribute}{parameter.ParameterType.GenerateCSharpTypeName(serviceManager.NamespaceManager, serviceManager.MappedNamespaces)} {parameter.Name.ToSnakeCase()}");
					count++;
				}
			}

			return returnSyntax;
		}

		/// <summary>
		/// Returns an Http Attribute Type.
		/// </summary>
		/// <param name="source">Method to generate type name from.</param>
		/// <returns>An Http Attribute Type.</returns>
		private static string GenerateHttpAttributeType(this CsMethod source)
		{
			var returnSyntax = (string)null;
			if (source == null || !source.IsLoaded) return returnSyntax;

			if (source.Name == "GetAsync" || source.Name == "QueryAsync")
				returnSyntax = "HttpGet";
			else if (source.Name == "AddAsync")
				returnSyntax = "HttpPost";
			else if (source.Name == "UpdateAsync")
				returnSyntax = "HttpPut";
			else if (source.Name == "DeleteAsync")
				returnSyntax = "HttpDelete";
			else if (source.IsPostCall())
				returnSyntax = "HttpPost";
			else
				returnSyntax = "HttpGet";

			return returnSyntax;
		}

		/// <summary>
		/// Returns an Http Attribute Type.
		/// </summary>
		/// <param name="source">Method to generate type name from.</param>
		/// <returns>An Http Attribute Type.</returns>
		private static string GenerateHttpMethodType(this CsMethod source)
		{
			var returnSyntax = (string)null;
			if (source == null || !source.IsLoaded) return returnSyntax;

			if (source.Name == "GetAsync" || source.Name == "QueryAsync")
				returnSyntax = "GetFromJsonAsync";
			else if (source.Name == "AddAsync")
				returnSyntax = "PostAsJsonAsync";
			else if (source.Name == "UpdateAsync")
				returnSyntax = "PutAsJsonAsync";
			else if (source.Name == "DeleteAsync")
				returnSyntax = "DeleteAsync";
			else if (source.IsPostCall())
				returnSyntax = "PostAsJsonAsync";
			else
				returnSyntax = "GetFromJsonAsync";

			return returnSyntax;
		}

		/// <summary>
		/// Returns an Http Attribute Type.
		/// </summary>
		/// <param name="source">Method to generate type name from.</param>
		/// <returns>An Http Attribute Type.</returns>
		private static string GenerateHttpRequestMethodType(this CsMethod source)
		{
			var returnSyntax = (string)null;
			if (source == null || !source.IsLoaded) return returnSyntax;

			if (source.Name == "GetAsync" || source.Name == "QueryAsync")
				returnSyntax = "HttpMethod.Get";
			else if (source.Name == "AddAsync")
				returnSyntax = "HttpMethod.Post";
			else if (source.Name == "UpdateAsync")
				returnSyntax = "HttpMethod.Put";
			else if (source.Name == "DeleteAsync")
				returnSyntax = "HttpMethod.Delete";
			else if (source.IsPostCall())
				returnSyntax = "HttpMethod.Post";
			else
				returnSyntax = "HttpMethod.Get";

			return returnSyntax;
		}
	}
}
