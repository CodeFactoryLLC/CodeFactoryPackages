using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.CSharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic
{
    /// <summary>
    /// Method builder that generates a interface method signature.
    /// </summary>
    public class MethodBuilderInterface : BaseMethodBuilder
    {
        /// <inheritdoc/>
        public override async Task<string> GenerateBuildMethodAsync(CsMethod sourceModel, ISourceManager manager, int indentLevel, string methodName = null, CsSecurity security = CsSecurity.Unknown, bool includeAttributes = false, IEnumerable<string> ignoreAttributeTypes = null, bool includeKeywords = false, bool includeAbstractKeyword = false, bool abstractKeyword = false, bool sealedKeyword = false, bool staticKeyword = false, bool virtualKeyword = false, bool overrideKeyword = false, bool includeAsyncKeyword = true, LogLevel defaultLogLevel = LogLevel.Critical, bool forceAsyncDefinition = false, string syntax = null, IEnumerable<NamedSyntax> multipleSyntax = null, MethodNameFormatting nameFormat = null)
        {
            if (sourceModel == null)
            {
                throw new ArgumentNullException("sourceModel");
            }

            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            bool hasLogging = base.LoggerBlock != null;
            string formattedMethodName = methodName ?? sourceModel.GenerateCSharpMethodName(forceAsyncDefinition, nameFormat?.NamePrefix, nameFormat?.NameSuffix, nameFormat?.AsyncPrefix, nameFormat?.AsyncSuffix);
            int indentLevel2 = indentLevel + 1;
            SourceFormatter methodFormatter = new SourceFormatter();
            if (sourceModel.HasDocumentation)
            {
                string docs = sourceModel.GenerateCSharpXmlDocumentation();
                if (docs != null)
                {
                    methodFormatter.AppendCodeBlock(indentLevel, docs);
                }
            }

            await manager.AddMissingUsingStatementsAsync(sourceModel);
            if (sourceModel.HasAttributes && includeAttributes)
            {
                bool hasIgnoreAttributes = ignoreAttributeTypes != null;
                foreach (CsAttribute sourceModelAttribute in sourceModel.Attributes)
                {
                    CsAttribute loadAttribute = sourceModelAttribute;
                    if (hasIgnoreAttributes && ignoreAttributeTypes.Any((string a) => a == loadAttribute.Type.Name))
                    {
                        loadAttribute = null;
                    }

                    if (loadAttribute != null)
                    {
                        methodFormatter.AppendCodeLine(indentLevel, loadAttribute.GenerateCSharpAttributeSignature(manager.NamespaceManager, manager.MappedNamespaces));
                    }
                }
            }

            CsMethod source = sourceModel;
            NamespaceManager namespaceManager = manager.NamespaceManager;
            bool includeKeywords2 = includeKeywords;
            bool abstractKeyword2 = abstractKeyword;
            bool includeAbstractKeyword2 = includeAbstractKeyword;
            methodFormatter.AppendCodeLine(indentLevel, $"{source.GenerateCSharpMethodSignature(namespaceManager, includeAsyncKeyword, includeSecurity: false, security, includeKeywords2, abstractKeyword2, sealedKeyword, staticKeyword, virtualKeyword, overrideKeyword, includeAbstractKeyword2, manager.MappedNamespaces, forceAsyncDefinition, nameFormat?.AsyncPrefix, nameFormat?.AsyncSuffix, isInterfaceSignature: true, null, nameFormat?.NamePrefix, nameFormat?.NameSuffix)};");
            methodFormatter.AppendCodeLine(indentLevel);
            return methodFormatter.ReturnSource();
        }
    }
}
