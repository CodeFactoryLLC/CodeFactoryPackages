using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.CSharp.Builder;

using System;
using System.Linq;
using System.Text;

namespace CodeFactory.Automation.Standard.Logic.Extensions
{
	/// <summary>
	/// Additional Extension Methods for CsMethod Object Types. 
	/// </summary>
	public static class CsMethodExtensions
	{
		//
		// Summary:
		//     Generates a string parameter list as propercased, to be used while generating a comma delimited list of parameters.
		//
		// Parameters:
		//   source:
		//     CsMethod to generate parameters for.
		//   parameterPrefix:
		//	   Specify a string that will be prefixed to each parameter.
		//
		// Returns:
		//     Comma delimited list of parameter names.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     Instance of the method was not provided.
		public static string GenerateParameterListAsProperCased(this CsMethod source, string resultObjectName = "result")
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			string result = null;

			if (source.HasParameters)
			{
				// Build parameters                        
				for (int i = 0; i < source.Parameters.Count; i++)
				{
					if (i > 0)
						result += ", ";

					if (!source.Parameters[i].ParameterType.IsWellKnownType)
						result += $"{source.Parameters[i].Name.GenerateCSharpProperCase()}";
					else
						result += $"{resultObjectName}!.{source.Parameters[i].Name.GenerateCSharpProperCase()}";
				}
			}
			return result;
		}

		//
		// Summary:
		//     Generates a string parameter list as propercased, to be used while generating a comma delimited list of parameters.
		//
		// Parameters:
		//   source:
		//     CsMethod to generate parameters for.
		//   parameterPrefix:
		//	   Specify a string that will be prefixed to each parameter.
		//
		// Returns:
		//     Comma delimited list of parameter names.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     Instance of the method was not provided.
		public static string GenerateParameterListAsCamelCased(this CsMethod source, string resultObjectName = "result")
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			string result = null;

			if (source.HasParameters)
			{
				// Build parameters                        
				for (int i = 0; i < source.Parameters.Count; i++)
				{
					if (i > 0)
						result += ", ";

					if (!source.Parameters[i].ParameterType.IsWellKnownType)
						result += $"{source.Parameters[i].Name.GenerateCSharpCamelCase()}";
					else
						result += $"{resultObjectName}.{source.Parameters[i].Name.GenerateCSharpProperCase()}";
				}
			}
			return result;
		}

		//
		// Summary:
		//     Indicates whether or not the source method has a return type.
		//
		// Parameters:
		//   source:
		//     CsMethod to generate parameters for.
		//
		// Returns:
		//     True if the method is a task only type or is void.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     Instance of the method was not provided.
		public static bool HasReturnType(this CsMethod source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (source.ReturnType.IsTaskType())
				return !source.ReturnType.IsTaskOnlyType();
			else
				return !source.IsVoid;
		}

		//
		// Summary:
		//     Indicates whether or not the source method has a return type.
		//
		// Parameters:
		//   source:
		//     CsMethod to generate parameters for.
		//
		// Returns:
		//     True if the method is a task only type or is void.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     Instance of the method was not provided.
		public static CsType GetReturnType(this CsMethod source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			return source.IsAsync() ? source.ReturnType.GenericParameters.First().Type : source.ReturnType;
		}

		//
		// Summary:
		//     Indicates whether or not the source method is a Task Type.
		//
		// Parameters:
		//   source:
		//     CsMethod to generate parameters for.
		//
		// Returns:
		//     True if it is a Task type.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     Instance of the method was not provided.
		public static bool IsAsync(this CsMethod source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			return source.ReturnType.IsTaskType();
		}

		//
		// Summary:
		//     Generates a string representation of the async statement.
		//
		// Parameters:
		//   source:
		//     CsMethod to generate parameters for.
		//
		// Returns:
		//     True if has a return type.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     Instance of the method was not provided.
		public static string GenerateStringAsyncStatement(this CsMethod source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			return source.IsAsync() ? "async" : " ";
		}

		//
		// Summary:
		//     Generates a string representation of the await statement.
		//
		// Parameters:
		//   source:
		//     CsMethod to generate parameters for.
		//
		// Returns:
		//     True if has a return type.
		//
		// Exceptions:
		//   T:System.ArgumentNullException:
		//     Instance of the method was not provided.
		public static string GenerateStringAwaitStatement(this CsMethod source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			return source.IsAsync() ? "await " : "";
		}
    }
}
