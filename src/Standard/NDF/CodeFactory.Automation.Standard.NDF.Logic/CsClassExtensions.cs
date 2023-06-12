//***************************************************************************
//* Code Factory Packages
//* Copyright (c) 2023 CodeFactory, LLC
//***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.WinVs.Models.CSharp;

namespace CodeFactory.Automation.Standard.NDF.Logic
{
    public static class CsClassExtensions
    {
        /// <summary>
        /// Generates the C# type name for the target class.
        /// </summary>
        /// <param name="source">Class to generate type name from.</param>
        /// <param name="manager">Optional, namespace manager to use for namespace formatting, default is null.</param>
        /// <param name="mappedNamespaces"></param>
        /// <returns></returns>
        public static string GenerateClassTypeNameNDF(this CsClass source, NamespaceManager manager = null,
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
