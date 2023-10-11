using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic.Data.Sql.EF
{
/// <summary>
    /// Automation class that manages the transform of EF entities into POCO entities.
    /// </summary>
    public static class EntityTransformBuilder
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

            await source.LoadDefaultNullValueManager(entityProject);

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

            classFormatter.AppendCodeLine(0, $"using {pocoModel.Namespace};");
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

            //if (!efModelSource.HasUsingStatement(pocoModel.Namespace, "Poco"))
            //    currentSource = await currentSource.AddUsingStatementAsync(pocoModel.Namespace, "Poco");

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
            createDataModelFormatter.AppendCodeLine(2, $"public static {currentDataClass.Name} CreateDataModel({pocoModel.Name} pocoModel)");
            createDataModelFormatter.AppendCodeLine(2, "{");
            createDataModelFormatter.AppendCodeLine(3, $"return pocoModel != null ? new {currentDataClass.Name} {{");

            //srg 8-15-2023 using the properties from the poco to determine what gets transformed.
            //var properties = dataModelManager.Container.Properties.OrderBy(p => p.Name);
            var properties = pocoModel.Properties.OrderBy(p => p.Name).ToList();

            int propCount = properties.Count();

            int propIndex = 1;
            bool isLastParameter = !(propIndex < propCount);

            foreach (var pocoProperty in properties)
            {
                var property =  dataModelManager.Container.Properties.FirstOrDefault(p=> p.Name == pocoProperty.Name);

                if (property == null) continue;

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
                            : $"{property.Name} = pocoModel.{property.FormatSetEfModelFieldValue()}{endingStatement}");
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
            createAppModelFormatter.AppendCodeLine(2, $"public {pocoModel.Name} CreatePocoModel()");
            createAppModelFormatter.AppendCodeLine(2, "{");
            createAppModelFormatter.AppendCodeLine(3, $"return new {pocoModel.Name}{{");


            propCount = properties.Count();

            propIndex = 1;
            isLastParameter = !(propIndex < propCount);

            foreach (var pocoProperty in properties)
            {
                var property =  dataModelManager.Container.Properties.FirstOrDefault(p=> p.Name == pocoProperty.Name);

                if (property == null) continue;
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
                            : $"{property.Name} = {property.FormatSetPocoModelFieldValue()}{endingStatement}");

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

        /// <summary>
        /// Checks to make sure the DefaultNullValueManager has been added to the entity framework model project.
        /// </summary>
        /// <param name="source">CodeFactory Automation.</param>
        /// <param name="efModelProject">The visual studio project that hosts the entity framework models. </param>
        private static async Task LoadDefaultNullValueManager(this IVsActions source,VsProject efModelProject)
        { 
            if(source == null) 
                throw new CodeFactoryException("CodeFactory automation was not provided cannot load the default null value manager.");

            if(efModelProject == null)    
                throw new CodeFactoryException("The entity framework project was not provided cannot load the default null value manager.");

            var managerName = "DefaultNullValueManager";

            var defaultNullValueManager = await efModelProject.FindCSharpSourceByClassNameAsync(managerName,false);

            if(defaultNullValueManager != null) return;

            SourceFormatter formatter = new SourceFormatter();

            formatter.AppendCodeLine(0);
            formatter.AppendCodeLine(0,$"namespace {efModelProject.DefaultNamespace}");
            formatter.AppendCodeLine(0,"{");

            formatter.AppendCodeLine(1,"/// <summary>");
            formatter.AppendCodeLine(1,"/// Data manager that handles that transforms nullable data types into default values and back from default values to nulls.");
            formatter.AppendCodeLine(1,"/// </summary>");
            formatter.AppendCodeLine(1,$"public static class {managerName}");
            formatter.AppendCodeLine(1,"{");

            formatter.AppendCodeLine(2,"#region Default Null Values");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static bool BooleanDefaultValue => false;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static char CharDefaultValue => char.MinValue;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static sbyte SbyteDefaultValue => 0;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static byte ByteDefaultValue => 0;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static short ShortDefaultValue => 0;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static ushort UshortDefaultValue => 0;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static int IntDefaultValue => 0;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static uint UintDefaultValue => 0;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static long LongDefaultValue => 0;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static ulong UlongDefaultValue => 0;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static float FloatDefaultValue => 0;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static double DoubleDefaultValue => 0;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static decimal DecimalDefaultValue => 0;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static DateTime DateTimeDefaultValue => DateTime.MinValue;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Assigned default value when the value is null.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"public static Guid GuidDefaultValue => Guid.Empty;");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"#endregion");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"#region Extension methods to transform default values back to null");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static bool? ReturnNullableValue(this bool source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static char? ReturnNullableValue(this char source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == CharDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static sbyte? ReturnNullableValue(this sbyte source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == SbyteDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static byte? ReturnNullableValue(this byte source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == ByteDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static short? ReturnNullableValue(this short source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == ShortDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static ushort? ReturnNullableValue(this ushort source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == UshortDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static int? ReturnNullableValue(this int source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == IntDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);
        
            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static uint? ReturnNullableValue(this uint source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == UintDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static long? ReturnNullableValue(this long source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == LongDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static ulong? ReturnNullableValue(this ulong source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == UlongDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static float? ReturnNullableValue(this float source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == FloatDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static double? ReturnNullableValue(this double source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == DoubleDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static decimal? ReturnNullableValue(this decimal source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == DecimalDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static DateTime? ReturnNullableValue(this DateTime source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == DateTimeDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.");
            formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"/// <param name=\"source\">Source data to evaluate.</param>");
            formatter.AppendCodeLine(2,"/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>");
            formatter.AppendCodeLine(2,"public static Guid? ReturnNullableValue(this Guid source)");
            formatter.AppendCodeLine(2,"{");
            
            formatter.AppendCodeLine(3,"return source == GuidDefaultValue");
            formatter.AppendCodeLine(4,"? null");
            formatter.AppendCodeLine(4,":source;");

            formatter.AppendCodeLine(2,"}");
            formatter.AppendCodeLine(2);
            formatter.AppendCodeLine(2,"#endregion");

            formatter.AppendCodeLine(1,"}");

            formatter.AppendCodeLine(0,"}");

            await efModelProject.AddDocumentAsync($"{managerName}.cs",formatter.ReturnSource());

        }
    }
}
