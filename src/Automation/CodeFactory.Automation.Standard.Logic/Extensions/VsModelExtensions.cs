using CodeFactory.WinVs.Models.ProjectSystem;

namespace CodeFactory.Automation.Standard.Logic.Extensions
{
	/// <summary>
	/// Additional Extension Methods for VSDocument object types. 
	/// </summary>
	public static class VsModelExtensions
	{
		/// <summary>
		/// Returns a boolean to indicate whether or the respective VsModel is a Markup file.
		/// </summary>  
		/// <returns>The target return type is a boolean</returns>
		public static bool IsMarkupCode(this IVsModel source) => (source.Name.Contains(".html") || source.Name.Contains(".cshtml") || source.Name.Contains(".razor") || source.Name.Contains(".xaml"));

		/// <summary>
		/// Returns a boolean to indicate whether or the respective VsModel is Source Code.
		/// </summary>  
		/// <returns>The target return type is a boolean</returns>
		public static bool IsSourceCode(this IVsModel source) => (source.Name.Contains(".cs"));
	}
}
