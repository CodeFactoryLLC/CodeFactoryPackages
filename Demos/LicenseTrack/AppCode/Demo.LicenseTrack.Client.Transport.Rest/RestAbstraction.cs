
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using CodeFactory.NDF;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
namespace Demo.LicenseTrack.Client.Transport.Rest
{
	/// <summary>
	/// Base class implementation that supports rest abstractions 
	/// </summary>
	public abstract class RestAbstraction
	{
		/// <summary>
		/// Logger for use by the abstraction.
		/// </summary>
		protected readonly ILogger _logger;
		
		/// <summary>
		/// Initializes the base class logic and functionality.
		/// </summary>
		/// <param name="logger">Logger to be used by the abstraction.</param>
		protected RestAbstraction(ILogger logger)
		{
			_logger = logger;
		}
		
		/// <summary>
		/// Creates a http client used for REST service calls.
		/// </summary>
		/// <returns>The http client to be used with the service request.</returns>
		protected async Task<HttpClient> GetHttpClient()
		{
			_logger.InformationEnterLog();
			HttpClient client = null;
			
			try
			{
				client = new HttpClient(new HttpClientHandler { UseDefaultCredentials = true });
				
				//Extend any additional changes to the client here.
			}
			catch (ManagedException)
			{
				_logger.InformationExitLog();
				throw;
			}
			catch (Exception unhandledError)
			{
				_logger.ErrorLog("A unhandled error occurred, review the exception for details of the error.",unhandledError);
				_logger.InformationExitLog();
				throw new UnhandledException();
			}
			
			_logger.InformationExitLog();
			return client;
		}
		
		/// <summary>
		/// Converts bad requests triggered by model validation errors into <see cref="ManagedException"/> data.
		/// </summary>
		/// <param name="message">The response message to check for validation errors.</param>
		/// <exception cref="ManagedException">Managed exception that is thrown from the bad request.</exception>
		protected  async Task RaiseUnhandledExceptionsAsync(HttpResponseMessage message)
		{
			if (message.StatusCode == HttpStatusCode.BadRequest)
			{
				//Checking to see if problem details were returned.
				if (message.Content.Headers.ContentType?.MediaType != null && message.Content.Headers.ContentType.MediaType.Contains(@"problem+json"))
				{
					//Reading in the problem details
					var problemDetails = await message.Content.ReadFromJsonAsync<ProblemDetails>();
					
					//If error extensions are provided parse them
					if (problemDetails?.Extensions != null)
					{
						//Get JsonNode when there are errors.
						var jsonNode = problemDetails.Extensions.Where(x => x.Key == "errors")
						    .Select(x => JsonNode.Parse(x.Value.ToString())).FirstOrDefault();
						
						//Build up the list of validation exceptions.
						if (jsonNode != null)
						{
							var errorArrays = jsonNode.AsObject().Select(x => x.Value.AsArray()).ToList();
							var exceptions = new List<ManagedException>();
							var errorMessages = (from errors in errorArrays from error in errors where error != null select error.ToString()).ToList();
							exceptions.AddRange(errorMessages.Select(x => new ValidationException(x)).ToList());
							throw new ManagedExceptions(exceptions);
						}
					}
				}
			}
			message.EnsureSuccessStatusCode();
		}
		
	}
}