using CodeFactory.WinVs.Models.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic.Data.Sql.EF
{
/// <summary>
    /// Extension methods to support the <see cref="CsProperty"/> data model.
    /// </summary>
    public static class PropertyExtensions
    {

        /// <summary>
        /// Formats the set value syntax when updated a EF model.
        /// </summary>
        /// <param name="source">Property to validate from.</param>
        /// <returns>Syntax to set the target property or field.</returns>
        /// <exception cref="CodeFactoryException">Raised when formatting errors occured.</exception>
        public static string FormatSetEfModelFieldValue(this CsProperty source)
        {
            string result = "";

            if(source.PropertyType.Namespace == "System" &  (source.PropertyType.Name =="Nullable"))
            {
                CsType csType = source.PropertyType.GenericTypes.FirstOrDefault();
                if (csType == null)
                {
                    throw new CodeFactoryException("Cannot get target type of the nullable type cannot define the C# syntax to get the properties value for the property '" + source.Name + "'");
                }

                if ((csType.Namespace == "System") & (csType.Name == "Guid"))
                {
                    return $"{source.Name}.ReturnNullableValue()";
                }

                switch (csType.WellKnownType)
                {
                    case CsKnownLanguageType.Boolean:

                    case CsKnownLanguageType.Character:

                    case CsKnownLanguageType.DateTime:

                    case CsKnownLanguageType.Decimal:

                    case CsKnownLanguageType.Double:

                    case CsKnownLanguageType.Signed8BitInteger: 
                        
                    case CsKnownLanguageType.UnSigned8BitInteger:

                    case CsKnownLanguageType.Signed16BitInteger: 

                    case CsKnownLanguageType.Unsigned16BitInteger: 

                    case CsKnownLanguageType.Signed32BitInteger: 

                    case CsKnownLanguageType.Unsigned32BitInteger: 

                    case CsKnownLanguageType.Signed64BitInteger:
                        
                    case CsKnownLanguageType.Unsigned64BitInteger:

                        result = $"{source.Name}.ReturnNullableValue()";
                        
                        break;

                    default:
                        result = $"{source.Name}";  
                        break;

                }
            }
            else
            { 
                result = $"{source.Name}";  
            }

            return result;
        }

        /// <summary>
        /// Formats the set value syntax when updated a POCO model.
        /// </summary>
        /// <param name="source">Property to validate from.</param>
        /// <returns>Syntax to set the target property or field.</returns>
        /// <exception cref="CodeFactoryException">Raised when formatting errors occured.</exception>
        public static string FormatSetPocoModelFieldValue(this CsProperty source)
        {
            string result = "";

            if(source.PropertyType.Namespace == "System" &  (source.PropertyType.Name =="Nullable"))
            {
                CsType csType = source.PropertyType.GenericTypes.FirstOrDefault();
                if (csType == null)
                {
                    throw new CodeFactoryException("Cannot get target type of the nullable type cannot define the C# syntax to get the properties value for the property '" + source.Name + "'");
                }

                if ((csType.Namespace == "System") & (csType.Name == "Guid"))
                {
                    return $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.GuidDefaultValue)"; 
                }

                switch (csType.WellKnownType)
                {
                    case CsKnownLanguageType.Boolean:
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.BooleanDefaultValue)";  
                        break;

                    case CsKnownLanguageType.Character:
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.CharDefaultValue)";  
                        break;

                    case CsKnownLanguageType.DateTime:
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.DateTimeDefaultValue)";  
                        break;

                    case CsKnownLanguageType.Decimal:
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.DecimalDefaultValue)";  
                        break;

                    case CsKnownLanguageType.Double:
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.DoubleDefaultValue)";  
                        break;

                    case CsKnownLanguageType.Signed8BitInteger: 
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.SbyteDefaultValue)";  
                        break;
                        
                    case CsKnownLanguageType.UnSigned8BitInteger:
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.ByteDefaultValue)";  
                        break;

                    case CsKnownLanguageType.Signed16BitInteger: 
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.ShortDefaultValue)";  
                        break;
                    case CsKnownLanguageType.Unsigned16BitInteger: 
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.UshortDefaultValue)";  
                        break;
                    case CsKnownLanguageType.Signed32BitInteger: 
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.IntDefaultValue)";  
                        break;
                    case CsKnownLanguageType.Unsigned32BitInteger: 
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.UintDefaultValue)";  
                        break;
                    case CsKnownLanguageType.Signed64BitInteger:
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.LongDefaultValue);";  
                        break;
                        
                    case CsKnownLanguageType.Unsigned64BitInteger:
                        result = $"{source.Name}.GetValueOrDefault(DefaultNullValueManager.UlongDefaultValue)";  
                        break;
                    
                    case CsKnownLanguageType.String: 
                        result = $"{source.Name}"; 
                        break;

                    default:
                        result = $"{source.Name}.GetValueOrDefault()";  
                        break;

                }
            }
            else
            { 
                result = $"{source.Name}";  
            }

            return result;
        }

    }
}
