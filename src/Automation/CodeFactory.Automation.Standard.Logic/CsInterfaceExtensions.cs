using CodeFactory.WinVs.Models.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic
{
    /// <summary>
    /// Extension methods that support the <see cref="CsInterface"/>
    /// </summary>
    public static class CsInterfaceExtensions
    {
        /// <summary>
        /// Gets all the interface methods implemented in the interface and all inherited interfaces.
        /// </summary>
        /// <param name="source">Interface to get the methods from.</param>
        /// <returns>Returns a list of methods or an empty list if no methods are found.</returns>
        public static List<CsMethod> GetAllInterfaceMethods(this CsInterface source)
        {
            var result = new List<CsMethod>();

            if (source == null) return result;

            if (source.Methods.Any())
            {
                foreach (var method in source.Methods) 
                {
                    var currentMethodHash = method.GetComparisonHashCode();

                    if (result.Any(m => m.GetComparisonHashCode() == currentMethodHash)) continue;
                    result.Add(method);
                }
            }
            if (source.InheritedInterfaces.Any())
            {
                foreach (var inheritedInterface in source.InheritedInterfaces)
                {
                    var inheritedMethods = inheritedInterface.GetAllInterfaceMethods();
                    if (inheritedMethods.Any())
                    {
                        foreach (var method in inheritedMethods)
                        {
                            var currentMethodHash = method.GetComparisonHashCode();

                            if (result.Any(m => m.GetComparisonHashCode() == currentMethodHash)) continue;
                            result.Add(method);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Generates the C# type name for the target interface.
        /// </summary>
        /// <param name="source">Interface to generate type name from.</param>
        /// <param name="manager">Optional, namespace manager to use for namespace formatting, default is null.</param>
        /// <param name="mappedNamespaces">Optional, mapped namespaces to convert the original namespace to the new target namespace.</param>
        /// <returns>Fully formatted interface type name.</returns>
        public static string GenerateInterfaceTypeName(this CsInterface source, NamespaceManager manager = null,
            List<MapNamespace> mappedNamespaces = null)
        {
            if (source == null || !source.IsLoaded) return (string) null;

            StringBuilder stringBuilder = new StringBuilder();
            NamespaceManager nsManager = manager ?? new NamespaceManager();
            string str = nsManager.AppendingNamespace(source.Namespace);
            stringBuilder.Append(str == null ? source.Name : str + "." + source.Name);
            if (source.IsGeneric)
                stringBuilder.Append(source.GenericParameters.GenerateCSharpGenericParametersSignature(manager, mappedNamespaces));
            return stringBuilder.ToString();
        }
    }
}
