
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demo.LicenseTrack.Client.Transport.Rest
{
	/// <summary>
	/// Extensions for string type management with json data.
	/// </summary>
	public static class JsonServiceExtensions
	{
		/// <summary>
		/// Place holder value used when passing strings in rest.
		/// </summary>
		private static string RestPostPlaceHolderValueForString = "~~~Empty~~~";
		
		/// <summary>
		/// Extension method that sets the post value for a string. If the string is null or empty will send a formatted string to represent empty or null.
		/// </summary>
		/// <param name="source">Source string to set.</param>
		/// <returns>The formatted string value.</returns>
		public static string SetPostValue(this string source)
		{
			return string.IsNullOrEmpty(source) ? RestPostPlaceHolderValueForString : source;
		}
		
		/// <summary>
		/// Extension method that gets the received value from a post. Will check for the empty value to convert the result to null or will pass the returned response.
		/// </summary>
		/// <param name="source">Source string to get.</param>
		/// <returns>The formatted string value or null.</returns>
		public static string GetPostValue(this string source)
		{
			return source != RestPostPlaceHolderValueForString ? source : null;
		}
		
		/// <summary>
		/// Extension method that determines if the string has a value or is empty.
		/// </summary>
		/// <param name="source">Source string to check for a value.</param>
		/// <returns>True has a value or false if not.</returns>
		public static bool HasValue(this string source)
		{
			return !string.IsNullOrEmpty(source);
		}
		
	}
}