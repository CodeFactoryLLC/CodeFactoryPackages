
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.NDF;
namespace Demo.LicenseTrack.Transport.Rest.Model
{
	/// <summary>
	/// Rest implementation of a exception that supports the base exception type of <see cref="ManagedException"/>
	/// </summary>
	public class ManagedExceptionRest
	{
		/// <summary>
		/// Type of exception that occurred.
		/// </summary>
		public string ExceptionType { get; set; }
		
		/// <summary>
		/// Message that occurred in the exception.
		/// </summary>
		public string Message { get; set; }
		
		/// <summary>
		/// Which data field was impacted by the exception.
		/// </summary>
		public string DataField { get; set; }
		
		/// <summary>
		/// A list of aggregate exceptions
		/// </summary>
		public List<ManagedExceptionRest> Exceptions { get; set; }
		
		/// <summary>
		/// Creates a new instance of a <see cref="ManagedExceptionRest"/> from exceptions that are based on <see cref="ManagedException"/>
		/// </summary>
		/// <param name="exception">Exception to convert to a JSON message.</param>
		/// <returns>Formatted JSON message or null if not found.</returns>
		public static List<ManagedExceptionRest> CreateManagedExceptionRest(ManagedException exception)
		{
			if (exception == null) return null;
			
			ManagedExceptionRest result = null;
			
			result = exception switch
			{
				AuthenticationException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(AuthenticationException) },
				AuthorizationException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(AuthorizationException) },
				SecurityException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(SecurityException) },
				CommunicationTimeoutException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(CommunicationTimeoutException) },
				CommunicationException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(CommunicationException) },
				ConfigurationException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(ConfigurationException) },
				DataValidationException exceptionData => new ManagedExceptionRest { DataField = exceptionData.PropertyName, Message = exceptionData.Message, ExceptionType = nameof(DataValidationException) },
				DuplicateException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(DuplicateException) },
				ValidationException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(ValidationException), DataField = exceptionData.DataField },
				DataException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(DataException) },
				LogicException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(LogicException) },
				ManagedExceptions exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, Exceptions = exceptionData.Exceptions.SelectMany(CreateManagedExceptionRest).ToList() },
				UnhandledException exceptionData => new ManagedExceptionRest { Message = exceptionData.Message, ExceptionType = nameof(UnhandledException) },
				_ => new ManagedExceptionRest { Message = exception.Message, ExceptionType = nameof(ManagedException) },
			};
			
			return result.Exceptions?.Count > 0 ? result.Exceptions : new List<ManagedExceptionRest> { result };
			
		}
		
		/// <summary>
		/// Creates a <see cref="ManagedException"/> from the data.
		/// </summary>
		/// <returns>The target managed exception type.</returns>
		public ManagedException CreateManagedException()
		{
			ManagedException result = null;
			result = ExceptionType switch
			{
				nameof(AuthenticationException) => new AuthenticationException(Message),
				nameof(AuthorizationException) => new AuthorizationException(Message),
				nameof(SecurityException) => new SecurityException(Message),
				nameof(CommunicationTimeoutException) => new CommunicationTimeoutException(Message),
				nameof(CommunicationException) => new CommunicationException(Message),
				nameof(ConfigurationException) => new ConfigurationException(Message),
				nameof(DataValidationException) => new DataValidationException(DataField, Message),
				nameof(DuplicateException) => new DuplicateException(Message),
				nameof(ValidationException) => new ValidationException(Message, DataField),
				nameof(DataException) => new DataException(Message),
				nameof(LogicException) => new LogicException(Message),
				nameof(UnhandledException) => new UnhandledException(Message),
				_ => new ManagedException(Message)
			};
			return result;
		}
	}
}