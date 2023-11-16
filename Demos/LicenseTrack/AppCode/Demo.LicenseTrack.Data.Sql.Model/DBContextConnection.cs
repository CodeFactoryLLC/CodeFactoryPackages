
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.LicenseTrack.Data.Sql.Model
{
	/// <summary>
	/// Stores the connection string to be used with a target EF context.
	/// </summary>
	/// <typeparam name="T">The EF context class the connection string is for.</typeparam>
	public class DBContextConnection<T>:IDBContextConnection<T>  where T:class
	{
		/// <summary>
		/// Backing field for the connection string property.
		/// </summary>
		private readonly string _connectionString;
		
		/// <summary>
		/// Creates an instance that holds the connection string.
		/// </summary>
		public DBContextConnection(string connectionString)
		{
			_connectionString = connectionString;
		}
		
		/// <summary>
		/// Connection string to use with the DB context.
		/// </summary>
		public string ConnectionString => _connectionString;
	}
}