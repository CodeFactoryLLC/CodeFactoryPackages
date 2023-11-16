
using CodeFactory.NDF;

namespace Demo.LicenseTrack.Transport.Rest.Model
{
	/// <summary>
	/// Result data from a service call.
	/// </summary>
	/// <typeparam name="T">The target type of data being returned from the service call.</typeparam>
	public class ServiceResult<T>
	{
		/// <summary>
		/// Result data from the service call.
		/// </summary>
		public T Result { get; set; }
		
		/// <summary>
		/// Boolean flag that determines if the service result had and exception occur.
		/// </summary>
		public bool HasExceptions { get; set; }
		
		/// <summary>
		/// If <see cref="HasExceptions"/> is true then the ManagedException will be provided.
		/// </summary>
		public List<ManagedExceptionRest> ManagedExceptions { get; set; }
		
		/// <summary>
		/// Returns a new instance of a service result with a single exception.
		/// </summary>
		/// <param name="exception">Exception to return.</param>
		/// <returns>Service result with error.</returns>
		public static ServiceResult<T> CreateError(ManagedException exception)
		{
			return new ServiceResult<T>
			{
				HasExceptions = true,
				ManagedExceptions = ManagedExceptionRest.CreateManagedExceptionRest(exception)
			};
			
		}
		
		/// <summary>
		/// Returns a new instance of a service result with multiple exceptions.
		/// </summary>
		/// <param name="exceptions">Exceptions to return.</param>
		/// <returns>Service result with errors.</returns>
		public static ServiceResult<T> CreateErrors(List<ManagedException> exceptions)
		{
			return new ServiceResult<T>
			{
				HasExceptions = true,
				ManagedExceptions = exceptions.SelectMany(ManagedExceptionRest.CreateManagedExceptionRest).ToList()
			};
			
		}
		
		/// <summary>
		/// Returns a new instance of a service result with result data.
		/// </summary>
		/// <returns>Service result with error.</returns>
		public static ServiceResult<T> CreateResult(T result)
		{
			return new ServiceResult<T> { HasExceptions = false, Result = result };
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