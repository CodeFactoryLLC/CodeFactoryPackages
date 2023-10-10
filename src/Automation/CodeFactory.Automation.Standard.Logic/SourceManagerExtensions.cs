using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic
{
    /// <summary>
    /// Extensions methods that support the <see cref="ISourceManager"/> interface.
    /// </summary>
    public static class SourceManagerExtensions
    {
        /// <summary>
        /// Checks all types definitions and makes sure they are included in the namespace manager for the target update source.
        /// </summary>
        /// <param name="sourceProperty">The target model to check using statements on.</param>
        /// <param name="includeAttributes">Flag that determines if attributes namespaces should be added to the missing using statements.</param>
        public static async Task AddMissingUsingStatementsAsync(this ISourceManager source, CsProperty sourceProperty,bool includeAttributes)
        { 

            if(sourceProperty == null)
            { 
                throw new CodeFactoryException("A property model was not provided cannot add missing using statements to target container.");    
            }

            if(source.NamespaceManager == null) source.LoadNamespaceManager();
            
            if(sourceProperty.HasAttributes & includeAttributes) 
            {
                foreach (var methodAttributes in sourceProperty.Attributes )
                {
                    await source.AddMissingUsingStatementsAsync(methodAttributes);
                }
            }

            await source.AddMissingUsingStatementsAsync(sourceProperty.PropertyType);
        }
    }
}
