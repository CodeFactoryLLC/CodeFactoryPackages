using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic.Data.Sql.EF
{
    /// <summary>
    /// Automation logic that will generate fluent validation from data annotations set on a EF model to a target validation class.
    /// </summary>
    public static class EntityModelFluentValidationBuilder
    {
        /// <summary>
        /// Data annotations namespace
        /// </summary>
        private const string DataAnnotationsNamespace = "System.ComponentModel.DataAnnotations";

        /// <summary>
        /// Required attribute name
        /// </summary>
        private const string RequiredAttribute = "RequiredAttribute";

        /// <summary>
        /// String length attribute name
        /// </summary>
        private const string StringLengthAttribute = "StringLengthAttribute";

        /// <summary>
        /// Refreshes the implementation of fluent validation logic for data annoations assigned to a class.
        /// </summary>
        /// <param name="source">CodeFactory automation.</param>
        /// <param name="sourceModel">Source data model that contains data annotations.</param>
        /// <param name="entityModel">The entity model that validation is being performed on.</param>
        /// <param name="validationClass">The fluent validation class where validation is to execute.</param>
        /// <exception cref="CodeFactoryException">Raised when errors occur while processing the refresh.</exception>
        public static async Task RefreshFluentValidationAsync(this IVsActions source, CsClass sourceModel, CsClass entityModel, CsClass validationClass)
        { 
            if(source == null) 
                throw new CodeFactoryException("CodeFactory automation was not provided cannot refresh the fluent validation.");

            if(sourceModel == null)
                throw new CodeFactoryException("The source model was not provided cannot refresh the fluent validation.");

            if(entityModel == null)
                throw new CodeFactoryException("The entity model was not provided cannot refresh the fluent validation.");

            if(validationClass == null)
                throw new CodeFactoryException("The validation class was not provided cannot refresh the fluent validation.");

            if(!entityModel.Properties.Any()) return;


            SourceFormatter formatter = new SourceFormatter();

            formatter.AppendCodeLine(2,"/// <summary>");
            formatter.AppendCodeLine(2,"/// Implementation of data annotation validation.");
		    formatter.AppendCodeLine(2,"/// </summary>");
            formatter.AppendCodeLine(2,"private void DataValidation()");
            formatter.AppendCodeLine(2,"{");

            foreach(var property in entityModel.Properties) 
            {
                var efProperty = sourceModel.Properties.FirstOrDefault(p => p.Name == property.Name);
                
                if(efProperty == null) continue;

                if(!efProperty.HasAttributes) continue;

                bool hasRequired = false;

                bool hasStringLength = false;
                
                var requiredValidation = efProperty.FormatRequiredRule();

                hasRequired = !string.IsNullOrEmpty(requiredValidation);

                var stringLengthValidation = efProperty.FormatStringLengthRule();

                hasStringLength = !string.IsNullOrEmpty(stringLengthValidation);

                if(hasRequired | hasStringLength)
                { 
                    formatter.AppendCodeLine(3,$"//Rules for the {efProperty.Name} property.");
                    if(hasRequired) formatter.AppendCodeLine(3,requiredValidation);
                    if(hasStringLength) formatter.AppendCodeLine(3,stringLengthValidation);
                    formatter.AppendCodeLine(3);
                  
                }
            }

            formatter.AppendCodeLine(3);
            formatter.AppendCodeLine(2,"}");

            var validationMethod = validationClass.Methods.FirstOrDefault(m => m.Name == "DataValidation");

            if(validationMethod !=null) await validationMethod.ReplaceAsync(formatter.ReturnSource());
            else await validationClass.AddToEndAsync(formatter.ReturnSource());

        }

        /// <summary>
        /// Extension method that will format a required rule if the property has the data annoation for Required.
        /// </summary>
        /// <param name="source">Property to evaluate.</param>
        /// <returns>Null if the required rule does not apply, or the fully formatted validation rule.</returns>
        public static string FormatRequiredRule(this CsProperty source)
        {
            if (source == null) return null;

            if(!source.HasAttributes) return null;

            var required = source.Attributes.FirstOrDefault(a => a.Type.Namespace == DataAnnotationsNamespace & a.Type.Name == RequiredAttribute);

            return required == null 
                ? null
                : $"RuleFor(m => m.{source.Name}).NotEmpty().WithMessage(\"{source.Name} is required.\");";
        }

        /// <summary>
        /// Extension method that will format a string length rule if the property has the data annoation for Required.
        /// </summary>
        /// <param name="source">Property to evaluate.</param>
        /// <returns>Null if the string length rule does not apply, or the fully formatted validation rule.</returns>
        public static string FormatStringLengthRule(this CsProperty source) 
        { 
            if (source == null) return null;

            if(!source.HasAttributes) return null;

            var required = source.Attributes.FirstOrDefault(a => a.Type.Namespace == DataAnnotationsNamespace & a.Type.Name == StringLengthAttribute);

            if(required == null) return null;   

            var lengthParm =  required.Parameters.FirstOrDefault();

            if (lengthParm == null) return null;
            
            var stringLength = lengthParm?.Value?.Value;

            if (string.IsNullOrEmpty(stringLength)) return null;

            return $"RuleFor(m=> m.{source.Name}).MaximumLength({stringLength}).When(m=> !string.IsNullOrEmpty(m.{source.Name}));";
            
        }
    }
}
