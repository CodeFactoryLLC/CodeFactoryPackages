﻿using CodeFactory.WinVs.Models.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic
{
    /// <summary>
    /// Extension methods that support the <see cref="VsProject"/>
    /// </summary>
    public static class ProjectExtensions
    {
        /// <summary>
        /// Library name and root namespace for logging extensions from Microsoft.
        /// </summary>
        public const string MicrosoftLogging = "Microsoft.Extensions.Logging";

        /// <summary>
        /// Library name for abstractions for logging extensions from Microsoft.
        /// </summary>
        public const string MicrosoftLoggingAbstractions = "Microsoft.Extensions.Logging";

        /// <summary>
        /// Determines if a target library is loaded in the target project.
        /// </summary>
        /// <param name="source">The project to check the library in.</param>
        /// <param name="libraryName">The name of the library to check for.</param>
        /// <returns>True if found or false if not.</returns>
        public static async Task<bool> SupportsLibraryAsync(this VsProject source, string libraryName)
        {
            if (source == null) return false;
            if (string.IsNullOrEmpty(libraryName)) return false;


            var refs = await source.GetProjectReferencesAsync();

            return refs.Any(r => r.Name == libraryName);
        }

        /// <summary>
        /// Determines if logging is loaded in the target project.
        /// </summary>
        /// <param name="source">The project to check the library in.</param>
        /// <returns>True if found or false if not.</returns>
        public static async Task<bool> SupportsLogging(this VsProject source)
        {
            var refs = await source.GetProjectReferencesAsync();

            bool result = refs.Any(r => r.Name == MicrosoftLogging);

            if (!result) result = refs.Any(r => r.Name == MicrosoftLoggingAbstractions);

            return result;

        }
    }
}
