using CodeFactory.WinVs.Models.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic
{
    /// <summary>
    /// Extension methods that suppor the <see cref="CsClass"/> model.
    /// </summary>
    public static class CsClassExtensions
    {
        /// <summary>
        /// Generates the C# type name for the target class.
        /// </summary>
        /// <param name="source">Class to generate type name from.</param>
        /// <param name="manager">Optional, namespace manager to use for namespace formatting, default is null.</param>
        /// <param name="mappedNamespaces">Optional, mapped namespaces to convert the original namespace to the new target namespace.</param>
        /// <returns>Fully formatted class type name.</returns>
        public static string GenerateClassTypeName(this CsClass source, NamespaceManager manager = null,
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
