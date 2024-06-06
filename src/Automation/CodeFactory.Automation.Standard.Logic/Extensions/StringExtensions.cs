using CodeFactory.WinVs.Models.ProjectSystem;
using System;
using System.Text;

namespace CodeFactory.Automation.Standard.Logic.Extensions
{
	/// <summary>
	/// Additional Extension Methods for String Types. 
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Returns a boolean to indicate whether the string object has return lines at the beginning or end that need trimmed.
		/// </summary>  
		/// <returns>The target return type is a boolean</returns>
		public static bool HasTrailingLeadingLines(this IVsDocument source) => (source.Name.StartsWith("\r\n") || source.Name.EndsWith("\r\n"));

		/// <summary>
		/// Returns a string where the end is trimmed of all return lines.
		/// </summary>  
		/// <returns>The target return type is a string</returns>
		public static string TrimEndLines(this string content)
		{
			try
			{
				if (content.EndsWith("\r") || content.EndsWith("\n") || content.EndsWith("\t"))
				{
					do
					{
						content = content.Remove(content.Length, 1);
					}
					while (content.EndsWith("\r") || content.EndsWith("\n") || content.EndsWith("\t"));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error attempting to trim the end: " + ex.Message);
			}
			return content;
		}

		/// <summary>
		/// Returns a string where the beginning is trimmed of all return lines.
		/// </summary>  
		/// <returns>The target return type is a string</returns>
		public static string TrimStartLines(this string content)
		{
			try
			{
				// First strip out any garbage.
				if (content.Contains("'''"))
					content = content.Remove(0, content.IndexOf(("\\")));

				// Strip off any extra empty lines or tabs.
				if (content.StartsWith("\r") || content.StartsWith("\n"))
				{
					do
					{
						content = content.Remove(0, 1);
					}
					while (content.StartsWith("\r") || content.StartsWith("\n"));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error attempting to trim the end: " + ex.Message);
			}
			return content;
		}

		/// <summary>
		/// Returns a string where the beginning and end are trimmed of all return lines.
		/// </summary>  
		/// <returns>The target return type is a string</returns>
		public static string TrimStartEndLines(this string content)
		{
			try
			{
				content = content.TrimStartLines();
				content = content.TrimEndLines();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error attempting to trim the end: " + ex.Message);
			}
			return content;
		}

		/// <summary>
		/// Returns a pluralized string.
		/// </summary>  
		/// <returns>The target return type is a plural string</returns>
		public static string Pluralize(this string content)
		{
			try
			{
				if (string.IsNullOrEmpty(content))
				{
					return content;
				}

				// Add your custom logic for pluralization here
				// This is a basic example and might not cover all cases
				if (content.EndsWith("y", StringComparison.OrdinalIgnoreCase))
				{
					return content.Remove(content.Length - 1) + "ies";
				}
				else if (content.EndsWith("s", StringComparison.OrdinalIgnoreCase) || content.EndsWith("x", StringComparison.OrdinalIgnoreCase) || content.EndsWith("z", StringComparison.OrdinalIgnoreCase) || content.EndsWith("ch", StringComparison.OrdinalIgnoreCase) || content.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
				{
					return content + "es";
				}
				else
				{
					return content + "s";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error attempting to pluralize input: " + ex.Message);
			}
			return content;
		}

		/// <summary>
		/// Returns a prettified string with spaces.
		/// </summary>  
		/// <returns>The target return type is a prettified string</returns>
		public static string Prettify(this string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return input;
			}

			StringBuilder result = new StringBuilder();

			foreach (char c in input)
			{
				if (char.IsUpper(c))
				{
					result.Append(' ');
				}

				result.Append(c);
			}

			return result.ToString().Trim();
		}

		// <summary>
		// Gets a ProperCased string and converts it to a kebab-cased string.
		//
		// Parameters:
		// - string: properCased.
		//
		// Returns:
		// - A kebab-cased string.
		// </summary>
		public static string ToKebabCase(this string properCasedString)
		{
			if (string.IsNullOrEmpty(properCasedString))
			{
				return string.Empty;
			}

			StringBuilder kebabCaseBuilder = new StringBuilder();
			bool isFirstChar = true;

			foreach (char c in properCasedString)
			{
				if (char.IsUpper(c))
				{
					if (!isFirstChar)
					{
						kebabCaseBuilder.Append('-');
					}

					kebabCaseBuilder.Append(char.ToLower(c));
				}
				else
				{
					kebabCaseBuilder.Append(c);
				}

				isFirstChar = false;
			}

			return kebabCaseBuilder.ToString();
		}

		// <summary>
		// Gets a ProperCased string and converts it to a snake_cased string.
		//
		// Parameters:
		// - string: properCased.
		//
		// Returns:
		// - A snake_cased string.
		// </summary>
		public static string ToSnakeCase(this string properCasedString)
		{
			if (string.IsNullOrEmpty(properCasedString))
			{
				return string.Empty;
			}

			StringBuilder snakeCaseBuilder = new StringBuilder();
			bool isFirstChar = true;

			foreach (char c in properCasedString)
			{
				if (char.IsUpper(c))
				{
					if (!isFirstChar)
					{
						snakeCaseBuilder.Append('_');
					}

					snakeCaseBuilder.Append(char.ToLower(c));
				}
				else
				{
					snakeCaseBuilder.Append(c);
				}

				isFirstChar = false;
			}

			return snakeCaseBuilder.ToString();
		}
	}
}
