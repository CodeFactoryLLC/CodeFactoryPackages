using CodeFactory.WinVs.Models.ProjectSystem;
using Drives.Automation.Standards.Logic.Enums;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Drives.Automation.Standards.Logic.Extensions
{
	/// <summary>
	/// Additional Extension Methods for VSDocument object types. 
	/// </summary>
	public static class VsDocumentExtensions
	{
		/// <summary>
		/// Returns a boolean to indicate whether or the respective VsDocument is a Markup file.
		/// </summary>  
		/// <returns>The target return type is a boolean</returns>
		public static bool IsMarkupCode(this IVsDocument source) => (source.Name.Contains(".html") || source.Name.Contains(".cshtml") || source.Name.Contains(".razor") || source.Name.Contains(".xaml"));

		public static string GetFileType(this IVsDocument document)
		{
			var fileType = "file";
			try
			{
				//if(document.GetType() == typeof(VsDocument))
				if (document == null)
					return "file";

				var extension = Path.GetExtension(document.Path);
				switch (extension)
				{
					case (".aspx"):
						fileType = "page";
						break;
					case (".aspx.cs"):
						fileType = "codebehind";
						break;
					case (".asmx"):
						fileType = "service";
						break;
					case (".cs"):
						fileType = "class";
						break;
					case (".cshtml"):
						fileType = "view";
						break;
					case (".css"):
					case (".scss"):
						fileType = "stylesheet";
						break;
					case (".html"):
						fileType = "html";
						break;
					case (".js"):
						fileType = "javascript";
						break;
					case (".json"):
						fileType = "json";
						break;
					case (".razor"):
						fileType = "component";
						break;
					case (".razor.cs"):
						fileType = "codebehind";
						break;
					case (".ts"):
						fileType = "codefile";
						break;
					case (".xaml"):
						fileType = "view";
						break;
					case (".xaml.cs"):
						fileType = "codebehind";
						break;
					case (".xml"):
						fileType = "file";
						break;
					default:
						fileType = "file";
						break;

				}
				return fileType;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error Retrieving config: " + ex.Message);
				return "file";
			}
		}

		public static async Task<string> GetUIFramework(this IVsDocument document)
		{
			UIFrameworkEnum uiFramework = UIFrameworkEnum.Unknown;
			try
			{
				var extension = Path.GetExtension(document.Path);

				// Check to see if this is a UI file
				if (extension == ".xaml" || extension == ".razor" || extension == ".cshtml" || extension == ".html")
				{
					VsProject hostingProject = await (document as VsDocument).GetHostingProjectAsync();
					var references = await hostingProject.GetProjectReferencesAsync();
					string documentContents = await (document as VsDocument).GetDocumentContentAsStringAsync();

					if (documentContents.ToLower().Contains("</div>") || documentContents.ToLower().Contains("</table>") || documentContents.ToLower().Contains("</p>") || documentContents.ToLower().Contains("</section>"))
						uiFramework = UIFrameworkEnum.Html;

					if (references.Any(x => x.Name.ToLower().Contains("materialize")))
						uiFramework = UIFrameworkEnum.Materialize;

					if (references.Any(x => x.Name.ToLower().Contains("angular")) && references.Any(x => x.Name.ToLower().Contains("material")))
						uiFramework = UIFrameworkEnum.AngularMaterial;

					if (references.Any(x => x.Name.ToLower().Contains("mudblazor") || documentContents.Contains("<Mud")))
						uiFramework = UIFrameworkEnum.MudBlazor;

					if (extension == ".xaml" && references.Any(x => x.Name.ToLower().Contains("xamarin") && documentContents.ToLower().Contains("x:class")))
						uiFramework = UIFrameworkEnum.Xamarin;

					return uiFramework == UIFrameworkEnum.Unknown ? "" : $"using the {uiFramework.ToString()} tags and syntax,";
				}
				else
				{
					return "";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error Retrieving config: " + ex.Message);
				return "";
			}
		}

		public static async Task<string> GetProgrammingLanguage(this IVsDocument document)
		{
			string language = "";
			try
			{
				var extension = Path.GetExtension(document.Path);
				VsProject hostingProject = await (document as VsDocument).GetHostingProjectAsync();
				var references = await hostingProject.GetProjectReferencesAsync();

				switch (extension)
				{
					case (".aspx"):
						language = "aspx";
						break;
					case (".aspx.cs"):
						language = "c#";
						break;
					case (".asmx"):
						language = "asmx";
						break;
					case (".cs"):
						language = "c#";
						break;
					case (".cshtml"):
						language = "razor";
						break;
					case (".css"):
						language = "css";
						break;
					case (".html"):
						language = "html";
						break;
					case (".js"):
						language = "javascript";
						break;
					case (".json"):
						language = "json";
						break;
					case (".razor"):
						if (references.Any(x => x.Name.ToLower().Contains("blazor") || hostingProject.Path.ToLower().Contains("blazor")))
							language = "blazor";
						else
							language = "razor";
						break;
					case (".razor.cs"):
						language = "c#";
						break;
					case (".scss"):
						language = "sass css";
						break;
					case (".ts"):
						language = "typescript";
						break;
					case (".xaml"):
						language = "xaml";
						break;
					case (".xaml.cs"):
						language = "xaml";
						break;
					case (".xml"):
						language = "xml";
						break;
				}
				return language;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error Retrieving config: " + ex.Message);
				return "";
			}
		}
	}
}
