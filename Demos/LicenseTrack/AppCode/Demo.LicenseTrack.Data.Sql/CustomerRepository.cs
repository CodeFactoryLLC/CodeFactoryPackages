
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using CodeFactory.NDF;
using CodeFactory.NDF.SQL;
using Demo.LicenseTrack.Data.Sql.Model;
using Demo.LicenseTrack.App.Model;
using Demo.LicenseTrack.Data.Contracts;

namespace Demo.LicenseTrack.Data.Sql
{
	/// <summary>
	/// Repository implementation that supports the model <see cref="TblCustomer"/>
	/// </summary>
	public class CustomerRepository:ICustomerRepository
	{
		/// <summary>
		/// Connection string of the repository
		/// </summary>
		private readonly string _connectionString;
		
		/// <summary>
		/// Logger used by the repository.
		/// </summary>
		private readonly ILogger _logger;
		
		/// <summary>
		/// Creates a new instance of the repository.
		/// </summary>
		/// <param name="logger">Logger used with the repository.</param>
		/// <param name="connection">The connection information for the repository.</param>
		public CustomerRepository(ILogger<CustomerRepository> logger, IDBContextConnection<LicenseTrackContext> connection)
		{
			_logger = logger;
			_connectionString = connection.ConnectionString;
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
				
				using (var context = new LicenseTrackContext(_connectionString))
				{
					var model = TblCustomer.CreateDataModel(customerAppModel);
					await context.AddAsync(model);
					await context.SaveChangesAsync();
					result = model.CreatePocoModel();
				}
			}
			
			catch (ManagedException)
			{
				_logger.ExitLog(LogLevel.Information);
				throw;
			}
			
			catch (DbUpdateException updateDataException)
			{
				var sqlError = updateDataException.InnerException as SqlException;
				
				if (sqlError == null)
				{
					_logger.ErrorLog("The following database error occurred.", updateDataException);
					_logger.ExitLog(LogLevel.Error);
					throw new DataException();
				}
				_logger.ErrorLog("The following SQL exception occurred.", sqlError);
				_logger.ExitLog(LogLevel.Error);
				sqlError.ThrowManagedException();
			}
			
			catch (SqlException sqlDataException)
			{
				_logger.ErrorLog("The following SQL exception occurred.", sqlDataException);
				_logger.ExitLog(LogLevel.Error);
				sqlDataException.ThrowManagedException();
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
				
				using (var context = new LicenseTrackContext(_connectionString))
				{
					var model = TblCustomer.CreateDataModel(customerAppModel);
					context.Update(model);
					await context.SaveChangesAsync();
					result = model.CreatePocoModel();
				}
			}
			
			catch (ManagedException)
			{
				_logger.ExitLog(LogLevel.Information);
				throw;
			}
			
			catch (DbUpdateException updateDataException)
			{
				var sqlError = updateDataException.InnerException as SqlException;
				
				if (sqlError == null)
				{
					_logger.ErrorLog("The following database error occurred.", updateDataException);
					_logger.ExitLog(LogLevel.Error);
					throw new DataException();
				}
				_logger.ErrorLog("The following SQL exception occurred.", sqlError);
				_logger.ExitLog(LogLevel.Error);
				sqlError.ThrowManagedException();
			}
			
			catch (SqlException sqlDataException)
			{
				_logger.ErrorLog("The following SQL exception occurred.", sqlDataException);
				_logger.ExitLog(LogLevel.Error);
				sqlDataException.ThrowManagedException();
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
				
				using (var context = new LicenseTrackContext(_connectionString))
				{
					var model = TblCustomer.CreateDataModel(customerAppModel);
					context.Remove(model);
					await context.SaveChangesAsync();
				}
			}
			
			catch (ManagedException)
			{
				_logger.ExitLog(LogLevel.Information);
				throw;
			}
			
			catch (DbUpdateException updateDataException)
			{
				var sqlError = updateDataException.InnerException as SqlException;
				
				if (sqlError == null)
				{
					_logger.ErrorLog("The following database error occurred.", updateDataException);
					_logger.ExitLog(LogLevel.Error);
					throw new DataException();
				}
				_logger.ErrorLog("The following SQL exception occurred.", sqlError);
				_logger.ExitLog(LogLevel.Error);
				sqlError.ThrowManagedException();
			}
			
			catch (SqlException sqlDataException)
			{
				_logger.ErrorLog("The following SQL exception occurred.", sqlDataException);
				_logger.ExitLog(LogLevel.Error);
				sqlDataException.ThrowManagedException();
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