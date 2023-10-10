using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic.FluentValidation
{
        /// <summary>
    /// Automation logic for the creation and update of fluent validation classes.
    /// </summary>
    public static class FluentValidationBuilder
    {
        /// <summary>
        /// Refreshes the definition of a fluent validation class.
        /// </summary>
        /// <param name="source">CodeFactory automation to refresh the validation.</param>
        /// <param name="sourceClass">The source class that the validation is supporting.</param>
        /// <param name="sourceProject">The source project the validation will be added to.</param>
        /// <param name="sourceFolder">The source project folder the validation will be added to. This is optional default is null.</param>
        /// <param name="namePrefix">The prefix to assign to the name of the validation class. This is optional default is null.</param>
        /// <param name="nameSuffix">The suffix to assign to the name of the validation class. This is optional default is null.</param>
        /// <returns>The refreshed validation class model.</returns>
        /// <exception cref="CodeFactoryException">Raised when automation exception occur.</exception>
        public static async Task<CsClass> RefreshValidationAsync(this IVsActions source, CsClass sourceClass,
            VsProject sourceProject, VsProjectFolder sourceFolder = null, NameManagement nameManagement = null)
        {
            //Bounds checking
            if (source == null)
                throw new CodeFactoryException(
                    "The CodeFactory automation was not provided cannot refresh the validation class.");

            if (sourceClass == null)
                throw new CodeFactoryException("The source class was not provided cannot refresh the validation class.");

            if (sourceProject == null)
                throw new CodeFactoryException("The source project was not provided cannot refresh the validation class.");

            string validationClassName = nameManagement != null ? nameManagement.FormatName(sourceClass.Name) : sourceClass.Name;

            validationClassName = validationClassName.Trim();

            if (sourceFolder == null)
            {
                var validationSource = await sourceProject.FindCSharpSourceByClassNameAsync(validationClassName, true);

                if (validationSource != null) return validationSource.SourceCode.Classes.FirstOrDefault(c => c.Name == validationClassName);
            }
            else
            {
                var validationSource = await sourceFolder.FindCSharpSourceByClassNameAsync(validationClassName, true);

                if (validationSource != null) return validationSource.SourceCode.Classes.FirstOrDefault(c => c.Name == validationClassName);
            }

            var validationSourceCode = await CreateValidationClassAsync(source, validationClassName, sourceClass, sourceProject, sourceFolder);

            return validationSourceCode?.Classes?.FirstOrDefault(c => c.Name == validationClassName);

        }

        /// <summary>
        /// Create a new fluent validation class.
        /// </summary>
        /// <param name="source">CodeFactory automation to refresh the validation.</param>
        /// <param name="className">The target class name for the fluent validation class.</param>
        /// <param name="sourceClass">The source class that the validation is supporting.</param>
        /// <param name="sourceProject">The source project the validation will be added to.</param>
        /// <param name="sourceFolder">The source project folder the validation will be added to. This is optional default is null.</param>
        /// <returns>The source code model for the created fluent validation class.</returns>
        /// <exception cref="CodeFactoryException">Raised when automation errors occur.</exception>
        private static async Task<CsSource> CreateValidationClassAsync(this IVsActions source, string className, CsClass sourceClass,
            VsProject sourceProject, VsProjectFolder sourceFolder = null)
        {
            //Bounds checking
            if (source == null)
                throw new CodeFactoryException("The CodeFactory automation was not provided cannot create the validation class.");

            if (string.IsNullOrEmpty(className))
                throw new CodeFactoryException("The validation class name must be provided cannot create the validation class.");

            if (sourceClass == null)
                throw new CodeFactoryException("The source class was not provided cannot create the validation class.");

            if (sourceProject == null)
                throw new CodeFactoryException("The source project was not provided cannot create the validation class.");

            string classNamespace = sourceFolder != null
                ? await sourceFolder.GetCSharpNamespaceAsync()
                : sourceProject.DefaultNamespace;

            if (string.IsNullOrEmpty(classNamespace))
                throw new CodeFactoryException(
                    $"Could not determine the namespace for the validation class that supports '{sourceClass.Name}' cannot create the validation class.");

            NamespaceManager manager = new NamespaceManager(targetNamespace: classNamespace);

            string sourceClassTypeName = sourceClass.GenerateCSharpTypeName(manager: manager);

            if (string.IsNullOrEmpty(sourceClassTypeName))
                throw new CodeFactoryException($"Could not determine the source class type name for the validation class that supports '{sourceClass.Name}' cannot create the validation class.");


            SourceFormatter validationFormatter = new SourceFormatter();

            validationFormatter.AppendCodeLine(0,"using System;");
            validationFormatter.AppendCodeLine(0,"using System.Collections.Generic;");
            validationFormatter.AppendCodeLine(0,"using System.Text;");
            validationFormatter.AppendCodeLine(0,"using FluentValidation;");
            validationFormatter.AppendCodeLine(0);
            validationFormatter.AppendCodeLine(0,$"namespace {classNamespace}");
            validationFormatter.AppendCodeLine(0,"{");

            validationFormatter.AppendCodeLine(1,"/// <summary>");
            validationFormatter.AppendCodeLine(1,$"/// Validation class that supports the model <see cref=\"{sourceClassTypeName}\"/>");
            validationFormatter.AppendCodeLine(1,"/// </summary>");
            validationFormatter.AppendCodeLine(1,$"public class {className}:AbstractValidator<{sourceClassTypeName}>");
            validationFormatter.AppendCodeLine(1,"{");

            validationFormatter.AppendCodeLine(2,"/// <summary>");
            validationFormatter.AppendCodeLine(2,"/// Creates a new instance of the validator.");
            validationFormatter.AppendCodeLine(2,"/// </summary>");
            validationFormatter.AppendCodeLine(2,$"public {className}()");
            validationFormatter.AppendCodeLine(2,"{");

            validationFormatter.AppendCodeLine(3,"DataValidation();");
            validationFormatter.AppendCodeLine(3,"CustomValidation();");

            validationFormatter.AppendCodeLine(2,"}");
            validationFormatter.AppendCodeLine(2);

            validationFormatter.AppendCodeLine(2,"/// <summary>");
		    validationFormatter.AppendCodeLine(2,"/// Implementation of custom validation.");
		    validationFormatter.AppendCodeLine(2,"/// </summary>");
		    validationFormatter.AppendCodeLine(2,"private void CustomValidation()");
		    validationFormatter.AppendCodeLine(2,"{"); 
			validationFormatter.AppendCodeLine(3,$"//TODO: Add custom validation for '{sourceClass.Name}'");
		    validationFormatter.AppendCodeLine(2,"}");
            validationFormatter.AppendCodeLine(2);
		    validationFormatter.AppendCodeLine(2,"/// <summary>");
		    validationFormatter.AppendCodeLine(2,"/// Implementation of data annotation validation.");
		    validationFormatter.AppendCodeLine(2,"/// </summary>");
		    validationFormatter.AppendCodeLine(2,"private void DataValidation()");
		    validationFormatter.AppendCodeLine(2,"{");
            validationFormatter.AppendCodeLine(2);
            validationFormatter.AppendCodeLine(2,"}");
            validationFormatter.AppendCodeLine(2);
            validationFormatter.AppendCodeLine(1,"}");
            validationFormatter.AppendCodeLine(0,"}");

            var validationDocument = sourceFolder != null
                ? await sourceFolder.AddDocumentAsync($"{className}.cs", validationFormatter.ReturnSource())
                : await sourceProject.AddDocumentAsync($"{className}.cs", validationFormatter.ReturnSource());

            return validationDocument == null
                ? throw new CodeFactoryException($"There was an internal error could not create the validation class that supports '{sourceClass.Name}'.")
                : await validationDocument.GetCSharpSourceModelAsync();
        }
    }
}
