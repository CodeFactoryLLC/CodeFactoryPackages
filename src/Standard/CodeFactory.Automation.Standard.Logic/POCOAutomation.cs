//***************************************************************************
//* Code Factory Packages
//* Copyright (c) 2023 CodeFactory, LLC
//***************************************************************************
using CodeFactory.WinVs.Logging;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.WinVs.Models.CSharp.Builder;

namespace CodeFactory.Automation.Standard.Logic
{
    /// <summary>
    /// Automation library that creates plain old CLR objects (POCOs).
    /// </summary>
    public static class POCOAutomation
    {
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(POCOAutomation));

        /// <summary>
        /// Refreshes the implementation of a plain old CLR object based on the definition of a provided class. This will also create the POCO if it does not exist.
        /// </summary>
        /// <param name="source">CodeFactory automation framework used to trigger the refresh. </param>
        /// <param name="sourceClass">The source class model the POCO will be generated or updated from.</param>
        /// <param name="pocoProject">The target project the POCO will be created in.</param>
        /// <param name="defaultNamespaces">The default namespaces to be added as using statements if they do not exist in the POCO class.</param>
        /// <param name="pocoFolder">Optional parameter that determines the target folder the poco is to be located in if not on the root of the project.</param>
        /// <param name="pocoSummary">Optional parameter that sets the XML summary for the POCO class.</param>
        /// <param name="convertNullableTypes">Optional parameter that determines if properties should converted to non nullable types, default value is true. </param>
        /// <param name="mappedNamespaces">Optional parameter that maps source and target namespaces when creating the POCO, default value is null.</param>
        /// <returns>The loaded class model for the created POCO class.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing or a processing error has occurred.</exception>
        public static async Task<CsClass> RefreshPOCOAsync(this IVsActions source, CsClass sourceClass,
            VsProject pocoProject, List<IUsingStatementNamespace> defaultNamespaces, VsProjectFolder pocoFolder = null,
            string pocoSummary = null, bool convertNullableTypes = true,List<MapNamespace> mappedNamespaces = null)
        {
            //Bounds checking
            if (source == null)
                throw new CodeFactoryException(
                    "The CodeFactory automation was not provided cannot refresh the POCO class.");
            if (sourceClass == null)
                throw new CodeFactoryException("The source class was not provided cannot refresh the POCO class.");
            if (pocoProject == null)
                throw new CodeFactoryException(
                    "The target POCO project was not provided cannot refresh the POCO class.");

            CsSource pocoSource = null;

            if (pocoFolder == null)
            {

                pocoSource = (await pocoProject.FindCSharpSourceByClassNameAsync(sourceClass.Name))?.SourceCode;

                if (pocoSource == null)
                    pocoSource = await source.CreatePOCOAsync(sourceClass, pocoProject, defaultNamespaces, null,
                        pocoSummary, convertNullableTypes);

                if (pocoSource == null)
                    throw new CodeFactoryException($"Could not create the POCO for the entity '{sourceClass.Name}'");
            }
            else
            {
                pocoSource = (await pocoFolder.FindCSharpSourceByClassNameAsync(sourceClass.Name))?.SourceCode;

                if (pocoSource == null)
                    pocoSource = await source.CreatePOCOAsync(sourceClass, pocoProject, defaultNamespaces, pocoFolder,
                        pocoSummary, convertNullableTypes);

                if (pocoSource == null)
                    throw new CodeFactoryException($"Could not create the POCO for the entity '{sourceClass.Name}'");
            }

            return await source.UpdatePOCOAsync(sourceClass, pocoProject, pocoSource, defaultNamespaces, pocoFolder,
                pocoSummary, convertNullableTypes,mappedNamespaces);

        }

        /// <summary>
        /// Creates the shell implementation of a plain old CLR object.
        /// </summary>
        /// <param name="source">CodeFactory automation framework used to trigger the refresh. </param>
        /// <param name="sourceClass">The source class model the poco will be generated or updated from.</param>
        /// <param name="pocoProject">The target project the poco will be created in.</param>
        /// <param name="defaultNamespaces">The default namespaces to be added as using statements if they do not exist in the POCO class.</param>
        /// <param name="pocoFolder">Optional parameter that determines the target folder the poco is to be located in if not on the root of the project.</param>
        /// <param name="pocoSummary">Optional parameter that sets the XML summary for the POCO class.</param>
        /// <param name="convertNullableTypes">Optional parameter that determines if properties should converted to non nullable types, default value is true. </param>
        /// <returns>The loaded class model for the created poco class.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing or a processing error has occurred.</exception>
        private static async Task<CsSource> CreatePOCOAsync(this IVsActions source, CsClass sourceClass,
            VsProject pocoProject, List<IUsingStatementNamespace> defaultNamespaces, VsProjectFolder pocoFolder = null,
            string pocoSummary = null, bool convertNullableTypes = true)
        {
            if (source == null)
                throw new CodeFactoryException(
                    "The CodeFactory automation was not provided cannot create the POCO class.");
            if (sourceClass == null)
                throw new CodeFactoryException("The source class was not provided cannot create the POCO class.");
            if (pocoProject == null)
                throw new CodeFactoryException(
                    "The target POCO project was not provided cannot create the POCO class.");

            CsSource result = null;

            try
            {
                SourceFormatter pocoFormatter = new SourceFormatter();

                if (defaultNamespaces != null)
                {
                    if (defaultNamespaces.Any())
                    {
                        foreach (var usingStatement in defaultNamespaces)
                        {
                            pocoFormatter.AppendCodeLine(0,
                                usingStatement.HasAlias
                                    ? $"using {usingStatement.Alias} = {usingStatement.ReferenceNamespace};"
                                    : $"using {usingStatement.ReferenceNamespace};");
                        }
                    }
                }

                pocoFormatter.AppendCodeLine(0,
                    pocoFolder == null
                        ? $"namespace {pocoProject.DefaultNamespace}"
                        : $"namespace {await pocoFolder.GetCSharpNamespaceAsync()}");
                pocoFormatter.AppendCodeLine(0, "{");

                pocoFormatter.AppendCodeLine(1, "/// <summary>");

                if (pocoSummary == null)
                    pocoFormatter.AppendCodeLine(1, "/// Plain old CLR object (POCO) data class implementation.");
                else
                {
                    var summaryLines = pocoSummary.Split(Environment.NewLine.ToCharArray());
                    foreach (var summaryLine in summaryLines) pocoFormatter.AppendCodeLine(1, $"/// {summaryLine}");
                }

                pocoFormatter.AppendCodeLine(1, "/// </summary>");
                pocoFormatter.AppendCodeLine(1, $"public class {sourceClass.Name}");
                pocoFormatter.AppendCodeLine(1, "{");
                pocoFormatter.AppendCodeLine(1);
                pocoFormatter.AppendCodeLine(1, "}");

                pocoFormatter.AppendCodeLine(0, "}");


                VsDocument pocoDoc = pocoFolder == null
                    ? await pocoProject.AddDocumentAsync($"{sourceClass.Name}.cs", pocoFormatter.ReturnSource())
                    : await pocoFolder.AddDocumentAsync($"{sourceClass.Name}.cs", pocoFormatter.ReturnSource());

                if (pocoDoc == null) throw new CodeFactoryException($"Error occurred saving the poco to the project.");

                result = await pocoDoc.GetCSharpSourceModelAsync();
            }
            catch (CodeFactoryException)
            {
                throw;
            }
            catch (Exception unhandledError)
            {
                _logger.Error("The following unhandled error occurred creating a POCO.", unhandledError);
                throw new CodeFactoryException(
                    $"An unhandled error occurred while creating the poco '{sourceClass.Name}'. Check the code factory logs for details of what happened.");
            }

            return result;
        }

        /// <summary>
        /// Updates the implementation of a plain old CLR object from the definition of a source class.
        /// </summary>
        /// <param name="source">CodeFactory automation framework used to trigger the refresh. </param>
        /// <param name="sourceClass">The source class model the poco will be generated or updated from.</param>
        /// <param name="pocoProject">The target project the poco will be created in.</param>
        /// <param name="pocoSource">The source model that holds the existing poco implementation.</param>
        /// <param name="defaultNamespaces">The default namespaces to be added as using statements if they do not exist in the POCO class.</param>
        /// <param name="pocoFolder">Optional parameter that determines the target folder the poco is to be located in if not on the root of the project.</param>
        /// <param name="pocoSummary">Optional parameter that sets the XML summary for the POCO class.</param>
        /// <param name="convertNullableTypes">Optional parameter that determines if properties should converted to non nullable types, default value is true. </param>
        /// <param name="mappedNamespaces">Optional parameter that maps source and target namespaces when creating the POCO, default value is null.</param>
        /// <returns>The loaded class model for the created poco class.</returns>
        /// <exception cref="CodeFactoryException">Raised if required data is missing or a processing error has occurred.</exception>
        private static async Task<CsClass> UpdatePOCOAsync(this IVsActions source, CsClass sourceClass,
            VsProject pocoProject, CsSource pocoSource, List<IUsingStatementNamespace> defaultNamespaces,
            VsProjectFolder pocoFolder = null, string pocoSummary = null, bool convertNullableTypes = true, List<MapNamespace> mappedNamespaces = null)
        {

            if (source == null)
                throw new CodeFactoryException(
                    "The CodeFactory automation was not provided cannot update the POCO class.");
            if (sourceClass == null)
                throw new CodeFactoryException("The source class was not provided cannot update the POCO class.");
            if (pocoSource == null)
                throw new CodeFactoryException("The target POCO source was not provided cannot update the POCO class.");

            var sourcePoco = pocoSource;
            var pocoClass = sourcePoco.Classes.FirstOrDefault();

            if (pocoClass == null)
                throw new CodeFactoryException(
                    $"The call information for the poco could not be loaded, cannot update the POCO class from source class '{sourceClass.Name}'.");

            var sourceProperties = sourceClass.Properties
                .Where(p => p.HasGet & p.HasSet & p.Security == CsSecurity.Public & !p.IsStatic).ToList();

            if (!sourceProperties.Any()) return pocoClass;

            var pocoProperties = pocoClass.Properties
                .Where(p => p.HasGet & p.HasSet & p.Security == CsSecurity.Public & !p.IsStatic).ToList();

            var addList = sourceProperties.Where(s => pocoProperties.All(p => p.Name != s.Name)).ToList();
            var checkList = sourceProperties.Where(s => pocoProperties.Any(p => p.Name == s.Name)).ToList();
            var removeList = new List<CsProperty>();

            bool hasChanges = false;

            var pocoMappedNamespaces = mappedNamespaces ?? new List<MapNamespace>();
            pocoMappedNamespaces.Add(new MapNamespace { Destination = pocoClass.Namespace, Source = sourceClass.Namespace } );

            foreach (var sourceProperty in checkList)
            {
                var pocoProperty = pocoProperties.FirstOrDefault(p => p.Name == sourceProperty.Name);

                if (pocoProperty == null)
                {
                    addList.Add(sourceProperty);
                    continue;
                }

                if (sourceProperty.PropertyType.GenerateCSharpTypeName(mappedNamespaces: pocoMappedNamespaces)
                    != pocoProperty.PropertyType.GenerateCSharpTypeName(mappedNamespaces: pocoMappedNamespaces))
                {
                    addList.Add(sourceProperty);
                    removeList.Add(pocoProperty);
                }
            }

            hasChanges = removeList.Any();
            if (!hasChanges) hasChanges = addList.Any();

            if (!hasChanges) return pocoClass;

            if (defaultNamespaces != null)
            {
                foreach (var defaultNamespace in defaultNamespaces)
                {
                    pocoSource = await pocoSource.AddUsingStatementAsync(defaultNamespace.ReferenceNamespace,
                        defaultNamespace.Alias);
                }
            }

            var propBuilder = new BuilderPropertyTransformNullableTypes(convertNullableTypes);

            var pocoUpdateManager = new SourceClassManager(pocoSource, pocoClass, source, mappedNamespaces: pocoMappedNamespaces);

            pocoUpdateManager.LoadNamespaceManager();

            foreach (var removeProperty in removeList)
            {
                if (removeProperty == null) continue;

                var propertyToRemove = pocoUpdateManager.Container.GetModel<CsProperty>(removeProperty.LookupPath);

                if (propertyToRemove != null) await pocoUpdateManager.MemberCommentOut(propertyToRemove);
            }

            foreach (var propertyToAdd in addList)
            {
                if (propertyToAdd == null) continue;

                //Checking to make sure other source class objects used by the poco also have poco's created.
                if (propertyToAdd.PropertyType.TypeInTargetNamespace(sourceClass.Namespace))
                {
                    if (propertyToAdd.PropertyType.Name != sourceClass.Name)
                    {
                        var targetPocoModel =
                            await pocoProject.FindCSharpSourceByClassNameAsync(propertyToAdd.PropertyType.Name);

                        if (targetPocoModel == null)
                        {
                            var sourceModelClass = propertyToAdd.PropertyType.GetClassModel();

                            if (sourceModelClass == null)
                                throw new CodeFactoryException(
                                    $"Could not the data model '{propertyToAdd.PropertyType.Name}' could not refresh the poco model, cannot update the POCO class '{pocoClass.Name}'.");

                            await source.RefreshPOCOAsync(sourceModelClass, pocoProject, defaultNamespaces, pocoFolder,
                                pocoSummary, convertNullableTypes);
                        }
                    }
                }

                if (propertyToAdd.PropertyType.IsGeneric)
                {
                    //Checking to make sure other source class objects used by the poco also have poco's created.
                    foreach (var propGenericType in propertyToAdd.PropertyType.GenericTypes)
                    {
                        if (propGenericType.Name != sourceClass.Name)
                        {
                            var targetPocoModel =
                                await pocoProject.FindCSharpSourceByClassNameAsync(propGenericType.Name);

                            if (targetPocoModel == null)
                            {
                                var sourceModelClass = propGenericType.GetClassModel();

                                if (sourceModelClass == null)
                                    throw new CodeFactoryException(
                                        $"Could not the data model '{propGenericType.Name}' could not refresh the poco model, cannot update the POCO class '{pocoClass.Name}'.");

                                await source.RefreshPOCOAsync(sourceModelClass, pocoProject, defaultNamespaces,
                                    pocoFolder, pocoSummary, convertNullableTypes);
                            }
                        }
                    }
                }

                var propertySyntax = await propBuilder.BuildPropertyAsync(propertyToAdd, pocoUpdateManager, 2);

                if (propertySyntax != null) await pocoUpdateManager.PropertiesAddAfterAsync(propertySyntax);

            }

            return pocoUpdateManager.Container;
        }
    }
}
