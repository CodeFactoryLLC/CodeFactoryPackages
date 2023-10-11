using CodeFactory.WinVs.Models.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.Automation.Standard.Logic;
namespace CodeFactory.Automation.NDF.Logic
{
    /// <summary>
    /// Extension methods that support the <see cref="VsProject"/>
    /// </summary>
    public static class ProjectExtensions
    {

        /// <summary>
        /// Library name and root namespace for NDF library
        /// </summary>
        public const string NDFLibraryName = "CodeFactory.NDF";

        /// <summary>
        /// Determines if CodeFactory NDF library is loaded in the target project.
        /// </summary>
        /// <param name="source">The project to check the library in.</param>
        /// <returns>True if found or false if not.</returns>
        public static async Task<bool> SupportsNDF(this VsProject source)
        {
            return await source.SupportsLibraryAsync(NDFLibraryName);
        }
    }
}
