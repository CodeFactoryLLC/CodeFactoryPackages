
using CodeFactory.NDF;

namespace Demo.LicenseTrack.Transport.Rest.Model
{
	/// <summary>
	/// Result from a service call with no data being returned.
	/// </summary>
	public class NoDataResult
	{
		/// <summary>
		/// Boolean flag that determines if the service result had and exception occur.
		/// </summary>
		public bool HasExceptions { get; set; }
		
		/// <summary>
		/// If <see cref="HasExceptions"/> is true then the ManagedException will be provided.
		/// </summary>
		public List<ManagedExceptionRest> ManagedExceptions { get; set; }
		
		/// <summary>
		/// Returns a new instance of <see cref="NoDataResult"/> with a single exception.
		/// </summary>
		/// <param name="exception">Exception to return.</param>
		/// <returns>Service result with error.</returns>
		public static NoDataResult CreateError(ManagedException exception)
		{
			return new NoDataResult
			{
				HasExceptions = true,
				ManagedExceptions = ManagedExceptionRest.CreateManagedExceptionRest(exception)
			};
			
		}
		
		/// <summary>
		/// Returns a new instance of <see cref="NoDataResult"/> with multiple exceptions.
		/// </summary>
		/// <param name="exceptions">Exceptions to return.</param>
		/// <returns>Result with errors.</returns>
		public static NoDataResult CreateErrors(List<ManagedException> exceptions)
		{
			return new NoDataResult
			{
				HasExceptions = true,
				ManagedExceptions = exceptions.SelectMany(ManagedExceptionRest.CreateManagedExceptionRest).ToList()
			};
			
		}
		
		/// <summary>
		/// Returns a new instance of <see cref="NoDataResult"/> as a successful operation.
		/// </summary>
		public static NoDataResult CreateSuccess()
		{
			return new NoDataResult {HasExceptions = false};
		}
		
		/// <summary>
		/// Raises any serialized exceptions if they exists.
		/// </summary>
		public void RaiseException()
		{
			if (!HasExceptions) return;
			
			var managedException = ManagedExceptions?.Select(x => x.CreateManagedException())?.ToList();
			
			if (managedException?.Count == 1) throw managedException.First();
			if (managedException?.Count > 1) throw new ManagedExceptions(managedException);
		}
		
	}
}