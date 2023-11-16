
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CodeFactory.NDF;
using Demo.LicenseTrack.Logic.Contracts;
using Demo.LicenseTrack.App.Model;
using Demo.LicenseTrack.Data.Contracts;

namespace Demo.LicenseTrack.Logic
{
	/// <summary>
	/// Logic implementation that supports the contract <see cref="ICustomerLogic"/>
	/// </summary>
	public class CustomerLogic:ICustomerLogic
	{
		/// <summary>
		/// Logger used by the logic class.
		/// </summary>
		private readonly ILogger _logger;

		private readonly ICustomerRepository _customerRepository;
		
		/// <summary>
		/// Creates a new instance of the logic class.
		/// </summary>
		/// <param name="logger">Logger used with the repository.</param>
		public CustomerLogic(ILogger<CustomerLogic> logger, ICustomerRepository customerRepository)
		{
			_logger = logger;
			_customerRepository = customerRepository;
		}
		
		/// <summary>
		/// Adds a new instance of the <see cref="CustomerAppModel"/> model.
		/// </summary>
		public async Task<CustomerAppModel> AddAsync(CustomerAppModel customerAppModel)
		{
			_logger.EnterLog(LogLevel.Information);
			
			if (customerAppModel == null)
			{
				_logger.ErrorLog($"The parameter {nameof(customerAppModel)} was not provided. Will raise an argument exception");
				_logger.ExitLog(LogLevel.Error);
				throw new ArgumentNullException(nameof(customerAppModel));
			}
			CustomerAppModel result = null;
			
			
			try
			{
				result = await _customerRepository.AddAsync(customerAppModel);
			}
			
			catch (ManagedException)
			{
				_logger.ExitLog(LogLevel.Information);
				throw;
			}
			
			catch (Exception unhandledException)
			{
				_logger.ErrorLog("The following unhandled exception occurred, see exception details. Throwing a unhandled managed exception.", unhandledException);
				_logger.ExitLog(LogLevel.Error);
				throw new UnhandledException();
			}
			_logger.ExitLog(LogLevel.Information);
			return result;
			
		}
		
		/// <summary>
		/// Updates a instance of the <see cref="CustomerAppModel"/> model.
		/// </summary>
		public async Task<CustomerAppModel> UpdateAsync(CustomerAppModel customerAppModel)
		{
			_logger.EnterLog(LogLevel.Information);
			
			if (customerAppModel == null)
			{
				_logger.ErrorLog($"The parameter {nameof(customerAppModel)} was not provided. Will raise an argument exception");
				_logger.ExitLog(LogLevel.Error);
				throw new ArgumentNullException(nameof(customerAppModel));
			}
			CustomerAppModel result = null;
			
			
			try
			{
				result = await _customerRepository.UpdateAsync(customerAppModel);
			}
			
			catch (ManagedException)
			{
				_logger.ExitLog(LogLevel.Information);
				throw;
			}
			
			catch (Exception unhandledException)
			{
				_logger.ErrorLog("The following unhandled exception occurred, see exception details. Throwing a unhandled managed exception.", unhandledException);
				_logger.ExitLog(LogLevel.Error);
				throw new UnhandledException();
			}
			_logger.ExitLog(LogLevel.Information);
			return result;
			
		}
		
		/// <summary>
		/// Deletes the instance of the <see cref="CustomerAppModel"/> model.
		/// </summary>
		public async Task DeleteAsync(CustomerAppModel customerAppModel)
		{
			_logger.EnterLog(LogLevel.Information);
			
			if (customerAppModel == null)
			{
				_logger.ErrorLog($"The parameter {nameof(customerAppModel)} was not provided. Will raise an argument exception");
				_logger.ExitLog(LogLevel.Error);
				throw new ArgumentNullException(nameof(customerAppModel));
			}
			
			try
			{
				await _customerRepository.DeleteAsync(customerAppModel);
			}
			
			catch (ManagedException)
			{
				_logger.ExitLog(LogLevel.Information);
				throw;
			}
			
			catch (Exception unhandledException)
			{
				_logger.ErrorLog("The following unhandled exception occurred, see exception details. Throwing a unhandled managed exception.", unhandledException);
				_logger.ExitLog(LogLevel.Error);
				throw new UnhandledException();
			}
			_logger.ExitLog(LogLevel.Information);
		}
		
		
		
		
	}
}