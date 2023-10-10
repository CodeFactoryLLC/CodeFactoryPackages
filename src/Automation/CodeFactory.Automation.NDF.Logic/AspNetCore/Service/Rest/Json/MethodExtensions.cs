using CodeFactory.WinVs.Models.CSharp;
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
    }
}
