

namespace Demo.LicenseTrack.Data.Sql.Model
{
	/// <summary>
	/// Data manager that handles that transforms nullable data types into default values and back from default values to nulls.
	/// </summary>
	public static class DefaultNullValueManager
	{
		#region Default Null Values
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static bool BooleanDefaultValue => false;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static char CharDefaultValue => char.MinValue;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static sbyte SbyteDefaultValue => 0;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static byte ByteDefaultValue => 0;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static short ShortDefaultValue => 0;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static ushort UshortDefaultValue => 0;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static int IntDefaultValue => 0;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static uint UintDefaultValue => 0;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static long LongDefaultValue => 0;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static ulong UlongDefaultValue => 0;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static float FloatDefaultValue => 0;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static double DoubleDefaultValue => 0;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static decimal DecimalDefaultValue => 0;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static DateTime DateTimeDefaultValue => DateTime.MinValue;
		
		/// <summary>
		/// Assigned default value when the value is null.
		/// </summary>
		public static Guid GuidDefaultValue => Guid.Empty;
		
		#endregion
		
		#region Extension methods to transform default values back to null
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static bool? ReturnNullableValue(this bool source)
		{
			return source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static char? ReturnNullableValue(this char source)
		{
			return source == CharDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static sbyte? ReturnNullableValue(this sbyte source)
		{
			return source == SbyteDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static byte? ReturnNullableValue(this byte source)
		{
			return source == ByteDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static short? ReturnNullableValue(this short source)
		{
			return source == ShortDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static ushort? ReturnNullableValue(this ushort source)
		{
			return source == UshortDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static int? ReturnNullableValue(this int source)
		{
			return source == IntDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static uint? ReturnNullableValue(this uint source)
		{
			return source == UintDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static long? ReturnNullableValue(this long source)
		{
			return source == LongDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static ulong? ReturnNullableValue(this ulong source)
		{
			return source == UlongDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static float? ReturnNullableValue(this float source)
		{
			return source == FloatDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static double? ReturnNullableValue(this double source)
		{
			return source == DoubleDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static decimal? ReturnNullableValue(this decimal source)
		{
			return source == DecimalDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static DateTime? ReturnNullableValue(this DateTime source)
		{
			return source == DateTimeDefaultValue
				? null
				:source;
		}
		
		/// <summary>
		/// Returns a nullable value from the none nullable type. If the default value is provided will return null otherwise will return the supplied value.
		/// </summary>
		/// <param name="source">Source data to evaluate.</param>
		/// <returns>Null if the default null value is set, otherwise the instance of the target value.</returns>
		public static Guid? ReturnNullableValue(this Guid source)
		{
			return source == GuidDefaultValue
				? null
				:source;
		}
		
		#endregion
	}
}