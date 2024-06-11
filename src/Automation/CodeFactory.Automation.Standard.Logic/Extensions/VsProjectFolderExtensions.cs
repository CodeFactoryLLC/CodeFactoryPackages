using CodeFactory.WinVs.Models.ProjectSystem;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic.Extensions
{
	/// <summary>
	/// Additional Extension Methods for VsProjectFolder object types. 
	/// </summary>
	public static class VsProjectFolderExtensions
	{
        //
        // Summary:
        //     Locates a target CodeFactory.WinVs.Models.ProjectSystem.VsDocument model
        //     in a code file hosted in the project folder.
        //
        // Parameters:
        //   source:
        //     The project folder to start to search.
        //
        //   className:
        //     The name of the class that is managed in the source control file.
        //
        //   searchSubFolders:
        //     Optional parameter that determines if sub folders should also be searched. By
        //     default this is set to true.
        //
        // Returns:
        //     The source code model the target class was found in as a VsDocument.
        public static async Task<VsDocument> FindTypscriptSourceByClassNameAsync(this VsProjectFolder source, string className, bool searchSubFolders = true)
        {
            if (source == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(className))
            {
                return null;
            }

            IEnumerable<VsDocument> sourceCode = (await source.GetChildrenAsync(searchSubFolders, loadSourceCode: true)).Where((VsModel m) => m.Name.Contains(".ts")).Cast<VsDocument>();
            return sourceCode.FirstOrDefault((VsDocument s) => s.GetDocumentContentAsStringAsync().Result.Contains("class " + className));
        }

		//
		// Summary:
		//     Locates a target CodeFactory.WinVs.Models.ProjectSystem.VsProjectFolder subfolder model
		//     in a code file hosted in the project folder.
		//
		// Parameters:
		//   source:
		//     The project folder to start to search.
		//
		//   folderName:
		//     The name of the subfolder that exist under the root, source folder..
		//
		//   searchSubFolders:
		//     Optional parameter that determines if sub folders should also be searched. By
		//     default this is set to true.
		//
		// Returns:
		//     The model the target subfolder was found in as a VsProjectFolder.
		public static async Task<VsProjectFolder> FindSubfolderByName(this VsProjectFolder source, string folderName, bool searchSubFolders = false)
		{
			if (source == null)
			{
				return null;
			}

			if (string.IsNullOrEmpty(folderName))
			{
				return null;
			}
			var folders = await source.GetChildrenAsync(searchSubFolders, loadSourceCode: false); 
			VsProjectFolder subfolder = folders.FirstOrDefault(m => m.Name.Contains(folderName)) as VsProjectFolder;
			return subfolder;
		}
	}
}
