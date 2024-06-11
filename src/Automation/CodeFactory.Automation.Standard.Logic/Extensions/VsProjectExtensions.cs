using CodeFactory.Document;
using CodeFactory.WinVs.Models.ProjectSystem;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic.Extensions
{
	/// <summary>
	/// Extension methods that support the <see cref="VsProject"/>
	/// </summary>
	public static class VsProjectExtensions
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

		/// <summary>
		/// Returns an int representing the number of lines of code within each code file within the source project folder.
		/// </summary>
		/// <param name="source">The source VsProject object</param>
		/// <returns>Returns a count of all children .cshtml or .cs file's lines of code.</returns>
		public static async Task<int> CountLinesOfCodeAsync(this VsProject source)
		{
			int count = 0;
			IReadOnlyList<VsModel> codeFiles = await source.GetChildrenAsync(true);
			IEnumerable<VsModel> children = codeFiles.Where(p => p.ModelType.Equals(VisualStudioModelType.Document) && (p.Name.Contains(".cshtml")) || p.Name.Contains(".cs"));
			foreach (VsDocument codeFile in children)
			{
				IDocumentContent content = await codeFile.GetDocumentContentAsContentAsync();
				count += content.Count;
			}
			return count;
		}
	}
}
