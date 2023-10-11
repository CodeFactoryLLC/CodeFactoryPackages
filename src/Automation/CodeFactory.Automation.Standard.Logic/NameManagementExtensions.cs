using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic
{
// <summary>
    /// Extension methods that support the <see cref="NameManagement"/> data class.
    /// </summary>
    public static class NameManagementExtensions
    {
        /// <summary>
        /// Formates an object name using the the name management provided critera.
        /// </summary>
        /// <param name="source">The name mamangement data object this method is based on.</param>
        /// <param name="name">The name of the object to be formatted.</param>
        /// <param name="defaultPrefix">Optional parameter that sets a default prefix that the name must begin with.</param>
        /// <returns>The formatted name of the object.</returns>
        public static string FormatName(this NameManagement source, string name,string defaultPrefix = null) 
        {

            //bounds checking
            if(source == null) return name;
            if(string.IsNullOrEmpty(name)) return name;

            string formattedName = name.Trim();

            if(defaultPrefix != null) 
            {
                if (formattedName.StartsWith(defaultPrefix))
                { 
                    formattedName = formattedName.Substring(defaultPrefix.Length);    
                }
            }

            if(source.RemovePrefixes.Any())
            { 
                string removePrefix = source.RemovePrefixes.FirstOrDefault( p => formattedName.StartsWith(p) );
                
                if(removePrefix != null) formattedName = formattedName.Substring(removePrefix.Length);
            }

            if(source.RemoveSuffixes.Any()) 
            {
                string removeSuffix = source.RemoveSuffixes.FirstOrDefault( p => formattedName.EndsWith(p) );
                
                if(removeSuffix != null) formattedName = formattedName.Substring(0, formattedName.Length -  (removeSuffix.Length));
            }

            if(!string.IsNullOrEmpty(source.AddPrefix)) formattedName = $"{source.AddPrefix}{formattedName}";

            if(!string.IsNullOrEmpty(source.AddSuffix)) formattedName = $"{formattedName}{source.AddSuffix}";

            return defaultPrefix != null
                ?$"{defaultPrefix}{formattedName}"
                :formattedName;
        }
    }
}
