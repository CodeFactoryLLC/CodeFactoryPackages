using CodeFactory.Document;
using CodeFactory.WinVs.Models.ProjectSystem;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic.Extensions
{
	/// <summary>
	/// Extension methods that support the <see cref="VsSolution"/>
	/// </summary>
	public static class VsSolutionExtensions
    {
		/// <summary>
		/// Returns an int representing the number of lines of code within each code file within the source vs solution.
		/// </summary>
		/// <param name="source">The source VsSolution object</param>
		/// <returns>Returns a count of all children .cshtml or .cs file's lines of code.</returns>
		public static async Task<int> CountLinesOfCodeAsync(this VsSolution source)
		{
			int count = 0;
			var projects = await source.GetProjectsAsync(true);

			foreach (var project in projects)
			{
				IReadOnlyList<VsModel> codeFiles = await project.GetChildrenAsync(true);
				IEnumerable<VsModel> children = codeFiles.Where(p => p.ModelType.Equals(VisualStudioModelType.Document) && (p.IsSourceCode() || p.IsMarkupCode()));
				foreach (VsDocument codeFile in children)
				{
					IDocumentContent content = await codeFile.GetDocumentContentAsContentAsync();
					count += content.Count;
				}
			}
			return count;
		}
	}
}
