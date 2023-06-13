//***************************************************************************
//* Code Factory Packages
//* Copyright (c) 2023 CodeFactory, LLC
//***************************************************************************
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.WinVs.Models.CSharp.Builder;

namespace CodeFactory.Automation.Data.Sql.EF.Logic
{
    /// <summary>
    /// Automation class that manages the transform of EF entities into POCO entities.
    /// </summary>
    public static class EntityTransformAutomation
    {
        /// <summary>
        /// Refreshes the transformation logic between an EF entity and a POCO class.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="pocoModel">POCO model that will be transformed to and from.</param>
        /// <param name="efModel">Entity Framework model that will be transformed to and from.</param>
        /// <param name="entityProject">EF project the entity models are stored.</param>
        /// <param name="entityFolder">Optional, EF project folder where entities are stored, default is null.</param>
        /// <returns>EF model with refreshed transform logic.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data for transformation logic is not provided.</exception>
        public static async Task<CsClass> RefreshEntityFrameworkEntityTransform(this IVsActions source,
            CsClass pocoModel, CsClass efModel, VsProject entityProject, VsProjectFolder entityFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot refresh the entity transform.");

            if (pocoModel == null)
                throw new CodeFactoryException("The target entity was not provided, cannot refresh the entity transform.");

            if (efModel == null)
                throw new CodeFactoryException("The entity framework entity was not provided, cannot refresh the entity transform.");

            if (entityProject == null)
                throw new CodeFactoryException("The entity framework project was not provided, cannot refresh the entity transform.");

            string transformFileName = $"{efModel.Name}.Transform.cs";

            CsSource dataSource = null;

            if (entityFolder != null)
            {
                var folderChildren = await entityFolder.GetChildrenAsync(false, true);

                dataSource = folderChildren.Where(m => m.ModelType == VisualStudioModelType.CSharpSource).Cast<VsCSharpSource>().FirstOrDefault(c => c.Name == transformFileName)?.SourceCode;
            }
            else
            {
                var projectChildren = await entityProject.GetChildrenAsync(false, true);

                dataSource = projectChildren.Where(m => m.ModelType == VisualStudioModelType.CSharpSource).Cast<VsCSharpSource>().FirstOrDefault(c => c.Name == transformFileName)?.SourceCode;
            }

            if (dataSource == null) dataSource = await source.AddRefreshLogic(pocoModel, efModel, entityProject, entityFolder);

            return await source.UpdateRefreshLogic(pocoModel, dataSource);

        }

        /// <summary>
        /// Adds refresh logic to a partial class assigned to the target EF entity model.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="pocoModel">POCO model that will be transformed to and from.</param>
        /// <param name="efModel">Entity Framework model that will be transformed to and from.</param>
        /// <param name="entityProject">EF project the entity models are stored.</param>
        /// <param name="entityFolder">Optional, EF project folder where entities are stored, default is null.</param>
        /// <returns>Added transform logic.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing to add refresh logic. </exception>
        private static async Task<CsSource> AddRefreshLogic(this IVsActions source, CsClass pocoModel, CsClass efModel,
            VsProject entityProject, VsProjectFolder entityFolder = null)
        {
            if (source == null)
                throw new CodeFactoryException("CodeFactory automation was not provided, cannot refresh the entity transform.");

            if (pocoModel == null)
                throw new CodeFactoryException("The target entity was not provided, cannot refresh the entity transform.");

            if (efModel == null)
                throw new CodeFactoryException("The entity framework entity was not provided, cannot refresh the entity transform.");

            if (entityProject == null)
                throw new CodeFactoryException("The entity framework project was not provided, cannot refresh the entity transform.");

            SourceFormatter classFormatter = new SourceFormatter();

            classFormatter.AppendCodeLine(0, $"using Poco = {pocoModel.Namespace};");
            classFormatter.AppendCodeLine(0, $"namespace {efModel.Namespace}");
            classFormatter.AppendCodeLine(0, "{");
            classFormatter.AppendCodeLine(1, $"public partial class {efModel.Name}");
            classFormatter.AppendCodeLine(1, "{");
            classFormatter.AppendCodeLine(1);
            classFormatter.AppendCodeLine(1, "}");
            classFormatter.AppendCodeLine(0, "}");

            var document = entityFolder != null
                ? await entityFolder.AddDocumentAsync($"{efModel.Name}.Transform.cs", classFormatter.ReturnSource())
                : await entityProject.AddDocumentAsync($"{efModel.Name}.Transform.cs", classFormatter.ReturnSource());

            if (document == null)
                throw new CodeFactoryException($"Could not create the model transformation class file for '{efModel.Name}'");

            var dataModelSource = await document.GetCSharpSourceModelAsync();

            return dataModelSource 
                   ?? throw new CodeFactoryException($"Could not load the partial class definition for the data model '{efModel.Name}, cannot refresh the data model transformation logic.'");
        }

        /// <summary>
        /// Updates the existing refresh logic for the EF entity model. 
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="pocoModel">POCO model that will be transformed to and from.</param>
        /// <param name="efModelSource">The source for the model to be updated.</param>
        /// <returns></returns>
        /// <exception cref="CodeFactoryException"></exception>
        private static async Task<CsClass> UpdateRefreshLogic(this IVsActions source, CsClass pocoModel,
            CsSource efModelSource)
        {

            if (pocoModel == null)
                throw new CodeFactoryException("The poco model  was not provided, cannot refresh the data model transformation logic.");

            if (efModelSource == null)
                throw new CodeFactoryException("The data model source was not provided, cannot refresh the data model transformation logic.");

            CsSource currentSource = efModelSource;

            if (!efModelSource.HasUsingStatement(pocoModel.Namespace, "Poco"))
                currentSource = await currentSource.AddUsingStatementAsync(pocoModel.Namespace, "Poco");

            var currentDataClass = currentSource.Classes.FirstOrDefault();

            if (currentDataClass == null)
                throw new CodeFactoryException($"The data model class could not be loaded, cannot refresh the data model transformation logic.");

            var  dataModelManager = new SourceClassManager(currentSource, currentDataClass, source, null);

            dataModelManager.LoadNamespaceManager();


            CsMethod createAppModelMethod = dataModelManager.Container.Methods.FirstOrDefault(m => m.Name == "CreatePocoModel");

            bool hasCreateAppModel = createAppModelMethod != null;

            CsMethod createDataModel =
                dataModelManager.Container.Methods.FirstOrDefault(m => m.IsStatic & m.Name == "CreateDataModel");

            bool hasCreateDataModel = createDataModel != null;


            SourceFormatter createDataModelFormatter = new SourceFormatter();

            createDataModelFormatter.AppendCodeLine(2, "///<Summary>");
            createDataModelFormatter.AppendCodeLine(2, "///Creates a instance of the data model from the source poco model.");
            createDataModelFormatter.AppendCodeLine(2, "/// </summary>");
            createDataModelFormatter.AppendCodeLine(2, "/// <param name=\"pocoModel\">The POCO model to used to load the data model.</param>");
            createDataModelFormatter.AppendCodeLine(2, "/// <returns>New instance of the data model.</returns>");
            createDataModelFormatter.AppendCodeLine(2, $"public static {currentDataClass.Name} CreateDataModel(Poco.{pocoModel.Name} pocoModel)");
            createDataModelFormatter.AppendCodeLine(2, "{");
            createDataModelFormatter.AppendCodeLine(3, $"return pocoModel != null ? new {currentDataClass.Name} {{");

            var properties = dataModelManager.Container.Properties.OrderBy(p => p.Name);


            int propCount = properties.Count();

            int propIndex = 1;
            bool isLastParameter = !(propIndex < propCount);

            foreach (var property in properties)
            {
                string endingStatement = isLastParameter ? "" : ",";
                if (!property.HasSet) continue;

                if (property.PropertyType.Namespace == "System.Collections.Generic" & (property.PropertyType.Name == "ICollection" || property.PropertyType.Name == "List"))
                {
                    var targetDataModelName = property.PropertyType.GenericParameters.FirstOrDefault()?.Type.Name;

                    createDataModelFormatter.AppendCodeLine(4,
                        property.PropertyType.TypeInNamespace(currentDataClass.Namespace)
                            ? $"{property.Name} = pocoModel.{property.Name}.Select(m => {targetDataModelName}.CreateDataModel(m)).ToList() ?? null{endingStatement}"
                            : $"{property.Name} = pocoModel.{property.Name}{endingStatement}");
                }
                else
                {
                    createDataModelFormatter.AppendCodeLine(4,
                        property.PropertyType.TypeInNamespace(currentDataClass.Namespace)
                            ? $"{property.Name} = {property.PropertyType.Name}.CreateDataModel(pocoModel.{property.Name}){endingStatement}"
                            : $"{property.Name} = pocoModel.{property.Name}{endingStatement}");
                }

                propIndex++;
                isLastParameter = !(propIndex < propCount);
            }

            createDataModelFormatter.AppendCodeLine(3, "}: null;");
            //createDataModelFormatter.AppendCodeLine(3, $"return appModel != null ? Mapper.LoadModel.Map<AppModel.{appModel.Name},{appModel.Name}>(appModel) : null;");
            createDataModelFormatter.AppendCodeLine(2, "}");


            if (hasCreateDataModel) await dataModelManager.MemberReplaceAsync(createDataModel, createDataModelFormatter.ReturnSource());
            else await dataModelManager.MethodsAddAfterAsync(createDataModelFormatter.ReturnSource());



            SourceFormatter createAppModelFormatter = new SourceFormatter();

            createAppModelFormatter.AppendCodeLine(2);
            createAppModelFormatter.AppendCodeLine(2, "///<Summary>");
            createAppModelFormatter.AppendCodeLine(2, "///Creates a instance of the POCO model from the source data model.");
            createAppModelFormatter.AppendCodeLine(2, "/// </summary>");
            createAppModelFormatter.AppendCodeLine(2, "/// <returns>New instance of the app model.</returns>");
            createAppModelFormatter.AppendCodeLine(2, $"public Poco.{pocoModel.Name} CreatePocoModel()");
            createAppModelFormatter.AppendCodeLine(2, "{");
            createAppModelFormatter.AppendCodeLine(3, $"return new Poco.{pocoModel.Name}{{");


            propCount = properties.Count();

            propIndex = 1;
            isLastParameter = !(propIndex < propCount);

            foreach (var property in properties)
            {
                string endingStatement = isLastParameter ? "" : ",";

                if (property.PropertyType.Namespace == "System.Collections.Generic" & (property.PropertyType.Name == "ICollection" || property.PropertyType.Name == "List"))
                {
                    var targetDataModelName = property.PropertyType.GenericParameters.FirstOrDefault()?.Type.Name;

                    createAppModelFormatter.AppendCodeLine(4,
                        property.PropertyType.TypeInNamespace(currentDataClass.Namespace)
                            ? $"{property.Name} = {property.Name}.Select(m => m.CreatePocoModel()).ToList() ?? null{endingStatement}"
                            : $"{property.Name} = {property.Name}{endingStatement}");
                }
                else
                {
                    createAppModelFormatter.AppendCodeLine(4,
                        property.PropertyType.TypeInNamespace(currentDataClass.Namespace)
                            ? $"{property.Name} = {property.Name}?.CreatePocoModel(){endingStatement}"
                            : $"{property.Name} = {property.GenerateCSharpDefaultValue()}{endingStatement}");
                }

                propIndex++;
                isLastParameter = !(propIndex < propCount);
            }

            createAppModelFormatter.AppendCodeLine(3, "};");

            //createAppModelFormatter.AppendCodeLine(3, $"return Mapper.LoadModel.Map<{appModel.Name}, AppModel.{appModel.Name}>(this);");
            createAppModelFormatter.AppendCodeLine(2, "}");


            if (createAppModelMethod != null) createAppModelMethod = dataModelManager.Container.GetModel<CsMethod>(createAppModelMethod.LookupPath);
            if (hasCreateAppModel) await dataModelManager.MemberReplaceAsync(createAppModelMethod, createAppModelFormatter.ReturnSource());
            else await dataModelManager.MethodsAddAfterAsync(createAppModelFormatter.ReturnSource());

            return dataModelManager.Container;
        }
    }
}
