using CodeFactory.WinVs.Models.CSharp;
using System;
using System.Linq;

namespace CodeFactory.Automation.Standard.Logic.Extensions
{
	/// <summary>
	/// Additional Extension Methods for CsType Object Types. 
	/// </summary>
	public static class CsTypeExtensions
	{
		//
		// Summary:
		//     Gets an initial default value syntax for the target type. This will generally
		//     be used on the right side of a = sign.
		//
		// Parameters:
		//   source:
		//     Type to generate syntax for.
		//   stringTypeName:
		//     The Name of the object type being passed in.  If the type is a string, it will append the object type Name to the end of the default value: i.e. "Test Name";
		//
		//
		// Returns:
		//     Formatted C# syntax for the default value, or null if the default test value syntax
		//     cannot be identified.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     Instance of the type was not provided.
		public static string GenerateCSharpDefaultTestValue(this CsType source, string stringTypeName = "")
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			string result = null;

			if (source.IsWellKnownType)
			{
				switch (source.WellKnownType)
				{
					case CsKnownLanguageType.Boolean: return "false";
					case CsKnownLanguageType.Character: return "char.MaxValue";
					case CsKnownLanguageType.Signed8BitInteger: return "1";
					case CsKnownLanguageType.UnSigned8BitInteger: return "1";
					case CsKnownLanguageType.Signed16BitInteger: return "1";
					case CsKnownLanguageType.Unsigned16BitInteger: return "1";
					case CsKnownLanguageType.Signed32BitInteger: return "1";
					case CsKnownLanguageType.Unsigned32BitInteger: return "1";
					case CsKnownLanguageType.Signed64BitInteger: return "1";
					case CsKnownLanguageType.Unsigned64BitInteger: return "1";
					case CsKnownLanguageType.Decimal: return "10";
					case CsKnownLanguageType.Single: return "10";
					case CsKnownLanguageType.Double: return "10";
					case CsKnownLanguageType.DateTime: return "DateTime.Now";
					case CsKnownLanguageType.Object: return "null";
					case CsKnownLanguageType.Pointer: return "IntPtr.Zero";
					case CsKnownLanguageType.PlatformPointer: return "UIntPtr.Zero";
					case CsKnownLanguageType.String:
						{
							var testValue = ($"Test{stringTypeName}").Prettify();
							return $"\"{testValue}\"";
						}
					default: return null;
				};
			}
			else if ((source.Namespace == "System") & (source.Name == "Nullable"))
			{
				if (source.GenericTypes.Count > 0)
				{
					switch (source.GenericTypes[0].Name.ToLower())
					{
						case "datetime": return "DateTime.Now";
						case "boolean": return "false";
						case "int": return "0";
						default: return null;
					};
				}
				return "null";
			}
			else if ((source.Namespace == "System") & (source.Name == "Guid"))
			{
				return "Guid.NewGuid()";
			}
			else if (!source.IsValueType)
			{
				return $"new {source.GenerateCSharpTypeName()}()";
			}
			else
			{
				return $"null";
			}

			return result;
		}

		//
		// Summary:
		//     Gets an first instance of a property name with a string type
		//
		// Parameters:
		//   source:
		//     Type to generate syntax for.
		//
		// Returns:
		//     Property Name that resolves as a string type.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     Instance of the type was not provided.
		public static string GetFirstStringTypePropertyName(this CsType source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			return source.GetClassModel().Properties.FirstOrDefault(x => x.PropertyType.WellKnownType == CsKnownLanguageType.String).Name;
		}

		//
		// Summary:
		//     Gets an first property name in the list of properties.
		//
		// Parameters:
		//   source:
		//     Type to generate syntax for.
		//
		// Returns:
		//     Property Name that resolves as a string type.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     Instance of the type was not provided.
		public static string GetPrimaryKeyName(this CsType source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			return source.GetClassModel().Properties[0].Name;
		}
	}
}