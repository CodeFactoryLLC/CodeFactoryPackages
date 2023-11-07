using CodeFactory.WinVs.Logging;
using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.Automation.Standard.Logic;

namespace CodeFactory.Automation.NDF.Logic.General
{
    /// <summary>
    /// Automation library that creates a model class that supports standard property implementation.
    /// </summary>
    public static class ModelBuilder
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(ModelBuilder));

        /// <summary>
        /// Refreshes the implementation of a plain old CLR object based on the definition of a provided class. This will also create the model if it does not exist.
        /// </summary>
        /// <param name="source">CodeFactory automation framework used to trigger the refresh. </param>
        /// <param name="sourceClass">The source class model the model will be generated or updated from.</param>
        /// <param name="modelProject">The target project the model will be created in.</param>
        /// <param name="defaultNamespaces">The default namespaces to be added as using statements if they do not exist in the model class.</param>
        /// <param name="nameManagement">Optional parameter that provides the details for how to format the name of the model class.</param>
        /// <param name="modelFolder">Optional parameter that determines the target folder the model is to be located in if not on the root of the project.</param>
        /// <param name="modelSummary">Optional parameter that sets the XML summary for the model class.</param>
        /// <param name="convertNullableTypes">Optional parameter that determines if properties should converted to non nullable types, default value is true. </param>
        /// <param name="mappedNamespaces">Optional parameter that maps source and target namespaces when creating the model, default value is null.</param>
        /// <param name="useSourceProperty">Optional parameter that calls a delegate to confirm the property should be included with the model class.</param>
        /// <returns>The loaded class model for the created model class.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing or a processing error has occurred.</exception>
        public static async Task<CsClass> RefreshModelAsync(this IVsActions source, CsClass sourceClass,
            VsProject modelProject, List<IUsingStatementNamespace> defaultNamespaces, NameManagement nameManagement = null, VsProjectFolder modelFolder = null,
            string modelSummary = null, bool convertNullableTypes = true, List<MapNamespace> mappedNamespaces = null, UseSourcePropertyDelegate useSourceProperty = null)
        {
            //Bounds checking
            if (source == null)
                throw new CodeFactoryException(
                    "The CodeFactory automation was not provided cannot refresh the model class.");
            if (sourceClass == null)
                throw new CodeFactoryException("The source class was not provided cannot refresh the model class.");
            if (modelProject == null)
                throw new CodeFactoryException(
                    "The target model project was not provided cannot refresh the model class.");

            CsSource modelSource = null;

            string modelName = null;

            modelName = nameManagement != null ? nameManagement.FormatName(sourceClass.Name) : sourceClass.Name;

            if(string.IsNullOrEmpty(modelName))
                throw new CodeFactoryException("Could not format the target model class name, cannot create the model class.");

            bool wasCreated = false;

            if (modelFolder == null)
            {
                modelSource = (await modelProject.FindCSharpSourceByClassNameAsync(modelName))?.SourceCode;
            }
            else
            {
                modelSource = (await modelFolder.FindCSharpSourceByClassNameAsync(modelName))?.SourceCode;
            }

            if (modelSource == null)
            { 
                modelSource = await source.CreateModelAsync(sourceClass, modelProject,modelName, defaultNamespaces, modelFolder,
                        modelSummary, convertNullableTypes)
                    ?? throw new CodeFactoryException($"Could not create the model '{modelName}' cannot refresh the model.");
            }


            
            var modelClass =  await source.UpdateModelAsync(sourceClass, modelProject, modelSource, defaultNamespaces,nameManagement, modelFolder,
                modelSummary, convertNullableTypes, mappedNamespaces);


            //if(wasCreated) await source.RegisterTransientClassesAsync(modelProject,false);

            return modelClass;

        }

        /// <summary>
        /// Creates the shell implementation of a plain old CLR object.
        /// </summary>
        /// <param name="source">CodeFactory automation framework used to trigger the refresh. </param>
        /// <param name="sourceClass">The source class model the model will be generated or updated from.</param>
        /// <param name="modelProject">The target project the model will be created in.</param>
        /// <param name="defaultNamespaces">The default namespaces to be added as using statements if they do not exist in the model class.</param>
        /// <param name="modelFolder">Optional parameter that determines the target folder the model is to be located in if not on the root of the project.</param>
        /// <param name="modelSummary">Optional parameter that sets the XML summary for the model class.</param>
        /// <param name="convertNullableTypes">Optional parameter that determines if properties should converted to non nullable types, default value is true. </param>
        /// <returns>The loaded class model for the created model class.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing or a processing error has occurred.</exception>
        private static async Task<CsSource> CreateModelAsync(this IVsActions source, CsClass sourceClass,
            VsProject modelProject, string targetClassName, List<IUsingStatementNamespace> defaultNamespaces, VsProjectFolder modelFolder = null,
            string modelSummary = null, bool convertNullableTypes = true)
        {
            if (source == null)
                throw new CodeFactoryException(
                    "The CodeFactory automation was not provided cannot create the model class.");
            if (sourceClass == null)
                throw new CodeFactoryException("The source class was not provided cannot create the model class.");
            if (modelProject == null)
                throw new CodeFactoryException(
                    "The target model project was not provided cannot create the model class.");

            CsSource result = null;

            try
            {
                SourceFormatter modelFormatter = new SourceFormatter();

                if (defaultNamespaces != null)
                {
                    if (defaultNamespaces.Any())
                    {
                        foreach (var usingStatement in defaultNamespaces)
                        {
                            modelFormatter.AppendCodeLine(0,
                                usingStatement.HasAlias
                                    ? $"using {usingStatement.Alias} = {usingStatement.ReferenceNamespace};"
                                    : $"using {usingStatement.ReferenceNamespace};");
                        }
                    }
                }

                modelFormatter.AppendCodeLine(0,
                    modelFolder == null
                        ? $"namespace {modelProject.DefaultNamespace}"
                        : $"namespace {await modelFolder.GetCSharpNamespaceAsync()}");
                modelFormatter.AppendCodeLine(0, "{");

                modelFormatter.AppendCodeLine(1, "/// <summary>");

                if (modelSummary == null)
                    modelFormatter.AppendCodeLine(1, "/// Plain old CLR object (model) data class implementation.");
                else
                {
                    var summaryLines = modelSummary.Split(Environment.NewLine.ToCharArray());
                    foreach (var summaryLine in summaryLines) modelFormatter.AppendCodeLine(1, $"/// {summaryLine}");
                }

                modelFormatter.AppendCodeLine(1, "/// </summary>");
                modelFormatter.AppendCodeLine(1, $"public class {targetClassName}");
                modelFormatter.AppendCodeLine(1, "{");
                modelFormatter.AppendCodeLine(1);
                modelFormatter.AppendCodeLine(1, "}");

                modelFormatter.AppendCodeLine(0, "}");


                VsDocument modelDoc = modelFolder == null
                    ? await modelProject.AddDocumentAsync($"{targetClassName}.cs", modelFormatter.ReturnSource())
                    : await modelFolder.AddDocumentAsync($"{targetClassName}.cs", modelFormatter.ReturnSource());

                if (modelDoc == null) throw new CodeFactoryException($"Error occurred saving the model to the project.");

                result = await modelDoc.GetCSharpSourceModelAsync();
            }
            catch (CodeFactoryException)
            {
                throw;
            }
            catch (Exception unhandledError)
            {
                _logger.Error("The following unhandled error occurred creating a model.", unhandledError);
                throw new CodeFactoryException(
                    $"An unhandled error occurred while creating the model '{targetClassName}'. Check the code factory logs for details of what happened.");
            }

            return result;
        }

        /// <summary>
        /// Updates the implementation of a plain old CLR object from the definition of a source class.
        /// </summary>
        /// <param name="source">CodeFactory automation framework used to trigger the refresh. </param>
        /// <param name="sourceClass">The source class model the model will be generated or updated from.</param>
        /// <param name="modelProject">The target project the model will be created in.</param>
        /// <param name="modelSource">The source model that holds the existing model implementation.</param>
        /// <param name="defaultNamespaces">The default namespaces to be added as using statements if they do not exist in the model class.</param>
        /// <param name="nameManagement">Optional parameter that provides the details for how to format the name of the model class.</param>
        /// <param name="modelFolder">Optional parameter that determines the target folder the model is to be located in if not on the root of the project.</param>
        /// <param name="modelSummary">Optional parameter that sets the XML summary for the model class.</param>
        /// <param name="convertNullableTypes">Optional parameter that determines if properties should converted to non nullable types, default value is true. </param>
        /// <param name="mappedNamespaces">Optional parameter that maps source and target namespaces when creating the model, default value is null.</param>
        /// <param name="useSourceProperty">Optional parameter that calls a delegate to confirm the property should be included with the model class.</param>
        /// <returns>The loaded class model for the created model class.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing or a processing error has occurred.</exception>
        private static async Task<CsClass> UpdateModelAsync(this IVsActions source, CsClass sourceClass,
            VsProject modelProject, CsSource modelSource, List<IUsingStatementNamespace> defaultNamespaces,NameManagement nameManagement = null,
            VsProjectFolder modelFolder = null, string modelSummary = null, bool convertNullableTypes = true, List<MapNamespace> mappedNamespaces = null,UseSourcePropertyDelegate useSourceProperty = null)
        {

            if (source == null)
                throw new CodeFactoryException(
                    "The CodeFactory automation was not provided cannot update the model class.");
            if (sourceClass == null)
                throw new CodeFactoryException("The source class was not provided cannot update the model class.");
            if (modelSource == null)
                throw new CodeFactoryException("The target model source was not provided cannot update the model class.");

            var sourcePoco = modelSource;
            var modelClass = sourcePoco.Classes.FirstOrDefault();

            if (modelClass == null)
                throw new CodeFactoryException(
                    $"The call information for the model could not be loaded, cannot update the model class from source class '{sourceClass.Name}'.");

            var sourceProperties = useSourceProperty != null 
                ? sourceClass.Properties.Where( p=> useSourceProperty(p)).ToList()
                : sourceClass.Properties;

            if (!sourceProperties.Any()) return modelClass;

            var modelProperties = modelClass.Properties
                .Where(p => p.HasGet & p.HasSet & p.Security == CsSecurity.Public & !p.IsStatic).ToList();

            var addList = sourceProperties.Where(s => modelProperties.All(p => p.Name != s.Name)).ToList();
            var checkList = sourceProperties.Where(s => modelProperties.Any(p => p.Name == s.Name)).ToList();
            var removeList = new List<CsProperty>();

            bool hasChanges = false;

            var modelMappedNamespaces = mappedNamespaces ?? new List<MapNamespace>();
            modelMappedNamespaces.Add(new MapNamespace { Destination = modelClass.Namespace, Source = sourceClass.Namespace });

            foreach (var sourceProperty in checkList)
            {
                var modelProperty = modelProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);

                if (modelProperty == null)
                {
                    addList.Add(sourceProperty);
                    continue;
                }

                if (sourceProperty.PropertyType.GenerateCSharpTypeName(mappedNamespaces: modelMappedNamespaces)
                    != modelProperty.PropertyType.GenerateCSharpTypeName(mappedNamespaces: modelMappedNamespaces))
                {
                    addList.Add(sourceProperty);
                    removeList.Add(modelProperty);
                }
            }

            hasChanges = removeList.Any();
            if (!hasChanges) hasChanges = addList.Any();

            if (!hasChanges) return modelClass;

            if (defaultNamespaces != null)
            {
                foreach (var defaultNamespace in defaultNamespaces)
                {
                    modelSource = await modelSource.AddUsingStatementAsync(defaultNamespace.ReferenceNamespace,
                        defaultNamespace.Alias);
                }
            }

            var propBuilder = new PropertyBuilderTransformNullableTypes(convertNullableTypes);

            var modelUpdateManager = new SourceClassManager(modelSource, modelClass, source, mappedNamespaces: modelMappedNamespaces);

            modelUpdateManager.LoadNamespaceManager();

            foreach (var removeProperty in removeList)
            {
                if (removeProperty == null) continue;

                var propertyToRemove = modelUpdateManager.Container.GetModel<CsProperty>(removeProperty.LookupPath);

                if (propertyToRemove != null) await modelUpdateManager.MemberCommentOut(propertyToRemove);
            }

            foreach (var propertyToAdd in addList)
            {
                if (propertyToAdd == null) continue;

                //Checking to make sure other source class objects used by the model also have model's created.
                if (propertyToAdd.PropertyType.TypeInTargetNamespace(sourceClass.Namespace))
                {
                    if (propertyToAdd.PropertyType.Name != sourceClass.Name)
                    {
                        var targetPocoModel =
                            await modelProject.FindCSharpSourceByClassNameAsync(propertyToAdd.PropertyType.Name);

                        if (targetPocoModel == null)
                        {
                            var sourceModelClass = propertyToAdd.PropertyType.GetClassModel() ?? throw new CodeFactoryException(
                                    $"Could not load the data model '{propertyToAdd.PropertyType.Name}' could not refresh the model model, cannot update the model class '{modelClass.Name}'.");

                            if (sourceModelClass.IsLoaded == false) throw new CodeFactoryException($"Could not load the data model '{propertyToAdd.PropertyType.Name}' could not refresh the model model, cannot update the model class '{modelClass.Name}'.");

                            await source.RefreshModelAsync(sourceModelClass, modelProject, defaultNamespaces,nameManagement,
                                modelFolder,modelSummary, convertNullableTypes,useSourceProperty:useSourceProperty);
                        }
                    }
                }

                if (propertyToAdd.PropertyType.IsGeneric)
                {
                    //Checking to make sure other source class objects used by the model also have model's created.
                    foreach (var propGenericType in propertyToAdd.PropertyType.GenericTypes)
                    {
                        //Checking to make sure other source class objects used by the model also have model's created.
                        if (propGenericType.TypeInTargetNamespace(sourceClass.Namespace))
                        {
                            if (propGenericType.Name != sourceClass.Name)
                            {
                                var targetPocoModel =
                                    await modelProject.FindCSharpSourceByClassNameAsync(propGenericType.Name);

                                if (targetPocoModel == null)
                                {
                                    var sourceModelClass = propGenericType.GetClassModel() ?? throw new CodeFactoryException(
                                            $"Could not load the data model '{propertyToAdd.PropertyType.Name}' could not refresh the model model, cannot update the model class '{modelClass.Name}'.");

                                    if (sourceModelClass.IsLoaded == false) throw new CodeFactoryException($"Could not load the data model '{propertyToAdd.PropertyType.Name}' could not refresh the model model, cannot update the model class '{modelClass.Name}'.");

                                    await source.RefreshModelAsync(sourceModelClass, modelProject, defaultNamespaces,nameManagement, 
                                        modelFolder,modelSummary,convertNullableTypes,useSourceProperty:useSourceProperty);
                                }
                            }
                        }
                    }
                }

                var propertySyntax = await propBuilder.BuildPropertyAsync(propertyToAdd, modelUpdateManager, 2);

                if (propertySyntax != null) await modelUpdateManager.PropertiesAddAfterAsync(propertySyntax);

            }

            return modelUpdateManager.Container;
        }

        /// <summary>
        /// Logic to check if a property should be included in a model class implementation.
        /// </summary>
        /// <param name="source">The property model to be evaluated.</param>
        /// <returns>True if the property should be used, false if not.</returns>
        public delegate bool UseSourcePropertyDelegate(CsProperty source);
    }
}
