
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using CodeFactory.NDF;
using Demo.LicenseTrack.Logic.Contracts;
using Demo.LicenseTrack.Transport.Rest.Model;
using Demo.LicenseTrack.Transport.Rest;
namespace Demo.LicenseTrack.Transport.Rest.Controllers
{
	/// <summary>
	/// Rest service implementation of the logic contract <see cref="ICustomerLogic"/>
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public  class CustomerController:ControllerBase
	{
		/// <summary>
		/// Logger used for the class.
		/// </summary>
		private readonly ILogger _logger;
		
		/// <summary>
		/// Logic class supporting the service implementation.
		/// </summary>
		private readonly ICustomerLogic _customerLogic;
		
		/// <summary>
		/// Creates a instance of the controller/>
		/// <param name="logger">Logger that supports this controller.</param>
		/// <param name="customerLogic">Logic contract implemented by this controller.</param>
		/// </summary>
		public CustomerController(ILogger<CustomerController> logger,ICustomerLogic customerLogic)
		{
			_logger = logger;
			_customerLogic = customerLogic;
		}
		/// <summary>
		/// Service implementation for the logic method 'DeleteAsync'
		/// </summary>
		[HttpPost("Delete")]
		public async Task<ActionResult<NoDataResult>> DeleteAsync([FromBody]Demo.LicenseTrack.App.Model.CustomerAppModel customerAppModel)
		{
			_logger.InformationEnterLog();
			
			if (customerAppModel == null)
			{
				_logger.ErrorLog($"The parameter {nameof(customerAppModel)} was not provided. Will raise an argument exception");
				_logger.InformationExitLog();
				return NoDataResult.CreateError(new ValidationException(nameof(customerAppModel)));
			}
			
			try
			{
				await  _customerLogic.DeleteAsync(customerAppModel);
			}
			catch (ManagedException managed)
			{
				_logger.ErrorLog("Raising the handled exception to the caller of the service.");
				_logger.InformationExitLog();
				return NoDataResult.CreateError(managed);
			}
			catch (Exception unhandledException)
			{
				_logger.CriticalLog("An unhandled exception occurred, see the exception for details. Will throw a UnhandledException", unhandledException);
				_logger.InformationExitLog();
				return NoDataResult.CreateError(new UnhandledException());
			}
			
			_logger.InformationExitLog();
			return NoDataResult.CreateSuccess();
		}
		
		/// <summary>
		/// Service implementation for the logic method 'UpdateAsync'
		/// </summary>
		[HttpPost("Update")]
		public async Task<ActionResult<ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>>> UpdateAsync([FromBody]Demo.LicenseTrack.App.Model.CustomerAppModel customerAppModel)
		{
			_logger.InformationEnterLog();
			
			if (customerAppModel == null)
			{
				_logger.ErrorLog($"The parameter {nameof(customerAppModel)} was not provided. Will raise an argument exception");
				_logger.InformationExitLog();
				return ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>.CreateError(new ValidationException(nameof(customerAppModel)));
			}
			
			Demo.LicenseTrack.App.Model.CustomerAppModel result = null;
			try
			{
				result = await  _customerLogic.UpdateAsync(customerAppModel);
			}
			catch (ManagedException managed)
			{
				_logger.ErrorLog("Raising the handled exception to the caller of the service.");
				_logger.InformationExitLog();
				return ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>.CreateError(managed);
			}
			catch (Exception unhandledException)
			{
				_logger.CriticalLog("An unhandled exception occurred, see the exception for details. Will throw a UnhandledException", unhandledException);
				_logger.InformationExitLog();
				return ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>.CreateError(new UnhandledException());
			}
			
			_logger.InformationExitLog();
			return ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>.CreateResult(result);
		}
		
		/// <summary>
		/// Service implementation for the logic method 'AddAsync'
		/// </summary>
		[HttpPost("Add")]
		public async Task<ActionResult<ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>>> AddAsync([FromBody]Demo.LicenseTrack.App.Model.CustomerAppModel customerAppModel)
		{
			_logger.InformationEnterLog();
			
			if (customerAppModel == null)
			{
				_logger.ErrorLog($"The parameter {nameof(customerAppModel)} was not provided. Will raise an argument exception");
				_logger.InformationExitLog();
				return ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>.CreateError(new ValidationException(nameof(customerAppModel)));
			}
			
			Demo.LicenseTrack.App.Model.CustomerAppModel result = null;
			try
			{
				result = await  _customerLogic.AddAsync(customerAppModel);
			}
			catch (ManagedException managed)
			{
				_logger.ErrorLog("Raising the handled exception to the caller of the service.");
				_logger.InformationExitLog();
				return ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>.CreateError(managed);
			}
			catch (Exception unhandledException)
			{
				_logger.CriticalLog("An unhandled exception occurred, see the exception for details. Will throw a UnhandledException", unhandledException);
				_logger.InformationExitLog();
				return ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>.CreateError(new UnhandledException());
			}
			
			_logger.InformationExitLog();
			return ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>.CreateResult(result);
		}
		
		
	}
}