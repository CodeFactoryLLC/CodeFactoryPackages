using CodeFactory.Automation.Standard.Logic.Extensions;
using CodeFactory.WinVs;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic.Node.Angular
{
	/// <summary>
	/// Automation logic for creating and updated a local service within an angular project.
	/// </summary>
	public static class LocalModelBuilder
    {
        public static async Task<VsDocument> RefreshAngularModel(this IVsActions source, CsClass controllerClass,
            VsProject angularWebProject, string targetFileName, string targetClassName, VsProjectFolder modulesFolder = null, VsProjectFolder modelsFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot refresh the angular service.");

            if (controllerClass == null)
                throw new CodeFactoryException("Cannot load the controller class, cannot refresh the angular service.");

            if (angularWebProject == null)
                throw new CodeFactoryException("Cannot load the angular web project, cannot refresh the angular service.");

            VsDocument modelsDocument = (await modelsFolder.FindTypscriptSourceByClassNameAsync(targetClassName))
                                     ?? await source.CreateAngularModelAsync(controllerClass, angularWebProject, targetFileName, targetClassName, modulesFolder, modelsFolder);
            return modelsDocument;
        }

        private static async Task<VsDocument> CreateAngularModelAsync(this IVsActions source, CsClass modelClass,
            VsProject angularWebProject, string targetFileName, string targetClassName, VsProjectFolder modulesFolder = null, VsProjectFolder modelsFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided cannot refresh the angular service.");

            if (modelClass == null)
                throw new CodeFactoryException("Cannot load the model class, cannot refresh the angular service.");

            if (angularWebProject == null)
                throw new CodeFactoryException("Cannot load the angular web project, cannot refresh the angular service.");

            // Convert the existing CSharp model class to Typescript
            var typescriptModel = TypescriptConverter.ConvertCsModeltoTs(modelClass, targetClassName);

            // Add the new Typescript document
            var sourceDocument = (await modelsFolder.AddDocumentAsync($"{targetFileName}.ts", typescriptModel.TrimStartEndLines()))
                ?? throw new CodeFactoryException($"Was not able to load the created angular service implementation for '{targetFileName}.ts'");

            return sourceDocument;
        }
	}
}