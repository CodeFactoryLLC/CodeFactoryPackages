
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using CodeFactory.NDF;
using Demo.LicenseTrack.Client.Contracts;
using Demo.LicenseTrack.Client.Transport.Rest;
using Demo.LicenseTrack.Transport.Rest.Model;
namespace Demo.LicenseTrack.Client.Transport.Rest
{
	/// <summary>
	/// Web abstraction implementation for abstraction contract 'ICustomerClient'
	/// </summary>
	public  class CustomerClient:RestAbstraction, ICustomerClient
	{
		/// <summary>
		/// Url for the service being accessed by this abstraction.
		/// </summary>
		private readonly ServiceUrl _serviceUrl;
		
		/// <summary>
		/// Creates a instance of the web abstraction.
		/// <param name="logger">Logger that supports this abstraction.</param>
		/// <param name="url">service url</param>
		/// </summary>
		public CustomerClient(ILogger<CustomerClient> logger, RestDemoLicenseTrackTransportRestServiceUrl url):base(logger)
		{
			_serviceUrl = url;
		}
		
		/// <summary>
		/// Deletes the instance of the <see cref="CustomerAppModel"/> model.
		/// </summary>
		public async Task DeleteAsync(Demo.LicenseTrack.App.Model.CustomerAppModel customerAppModel)
		{
			_logger.InformationEnterLog();
			if(customerAppModel == null)
			{
				_logger.ErrorLog($"The parameter {nameof(customerAppModel)} was not provided. Will raise an argument exception");
				_logger.InformationExitLog();
				throw new ValidationException(nameof(customerAppModel));
			}
			
			try
			{
				using (HttpClient httpClient = await GetHttpClient())
				{
					var serviceData = await httpClient.PostAsJsonAsync<Demo.LicenseTrack.App.Model.CustomerAppModel>($"{_serviceUrl.Url}/api/Customer/Delete", customerAppModel);
					
					await RaiseUnhandledExceptionsAsync(serviceData);
					
					var serviceResult = await serviceData.Content.ReadFromJsonAsync<NoDataResult>();
					
					if (serviceResult == null) throw new ManagedException("Internal error occurred no data was returned");
					
					serviceResult.RaiseException();
					
				}
			}
			catch (ManagedException)
			{
				//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.
				throw;
			}
			catch (Exception unhandledException)
			{
				_logger.ErrorLog("An unhandled exception occurred, see the exception for details. Will throw a UnhandledException", unhandledException);
				_logger.InformationExitLog();
				throw new UnhandledException();
			}
			
			_logger.InformationExitLog();
			
		}
		
		
		/// <summary>
		/// Updates a instance of the <see cref="CustomerAppModel"/> model.
		/// </summary>
		public async Task<Demo.LicenseTrack.App.Model.CustomerAppModel> UpdateAsync(Demo.LicenseTrack.App.Model.CustomerAppModel customerAppModel)
		{
			_logger.InformationEnterLog();
			if(customerAppModel == null)
			{
				_logger.ErrorLog($"The parameter {nameof(customerAppModel)} was not provided. Will raise an argument exception");
				_logger.InformationExitLog();
				throw new ValidationException(nameof(customerAppModel));
			}
			
			Demo.LicenseTrack.App.Model.CustomerAppModel result = null; 
			
			try
			{
				using (HttpClient httpClient = await GetHttpClient())
				{
					var serviceData = await httpClient.PostAsJsonAsync<Demo.LicenseTrack.App.Model.CustomerAppModel>($"{_serviceUrl.Url}/api/Customer/Update", customerAppModel);
					
					await RaiseUnhandledExceptionsAsync(serviceData);
					
					var serviceResult = await serviceData.Content.ReadFromJsonAsync<ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>>();
					
					if (serviceResult == null) throw new ManagedException("Internal error occurred no data was returned");
					
					serviceResult.RaiseException();
					
					result = serviceResult.Result;
					
				}
			}
			catch (ManagedException)
			{
				//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.
				throw;
			}
			catch (Exception unhandledException)
			{
				_logger.ErrorLog("An unhandled exception occurred, see the exception for details. Will throw a UnhandledException", unhandledException);
				_logger.InformationExitLog();
				throw new UnhandledException();
			}
			
			_logger.InformationExitLog();
			
			return result;
		}
		
		
		/// <summary>
		/// Adds a new instance of the <see cref="CustomerAppModel"/> model.
		/// </summary>
		public async Task<Demo.LicenseTrack.App.Model.CustomerAppModel> AddAsync(Demo.LicenseTrack.App.Model.CustomerAppModel customerAppModel)
		{
			_logger.InformationEnterLog();
			if(customerAppModel == null)
			{
				_logger.ErrorLog($"The parameter {nameof(customerAppModel)} was not provided. Will raise an argument exception");
				_logger.InformationExitLog();
				throw new ValidationException(nameof(customerAppModel));
			}
			
			Demo.LicenseTrack.App.Model.CustomerAppModel result = null; 
			
			try
			{
				using (HttpClient httpClient = await GetHttpClient())
				{
					var serviceData = await httpClient.PostAsJsonAsync<Demo.LicenseTrack.App.Model.CustomerAppModel>($"{_serviceUrl.Url}/api/Customer/Add", customerAppModel);
					
					await RaiseUnhandledExceptionsAsync(serviceData);
					
					var serviceResult = await serviceData.Content.ReadFromJsonAsync<ServiceResult<Demo.LicenseTrack.App.Model.CustomerAppModel>>();
					
					if (serviceResult == null) throw new ManagedException("Internal error occurred no data was returned");
					
					serviceResult.RaiseException();
					
					result = serviceResult.Result;
					
				}
			}
			catch (ManagedException)
			{
				//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.
				throw;
			}
			catch (Exception unhandledException)
			{
				_logger.ErrorLog("An unhandled exception occurred, see the exception for details. Will throw a UnhandledException", unhandledException);
				_logger.InformationExitLog();
				throw new UnhandledException();
			}
			
			_logger.InformationExitLog();
			
			return result;
		}
		
		
	}
}