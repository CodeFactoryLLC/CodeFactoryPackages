
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.LicenseTrack.Data.Sql.Model
{
	/// <summary>
	/// Contract for loading a connection string.
	/// </summary>
	/// <typeparam name="T">The context the connection belongs to.</typeparam>
	public interface IDBContextConnection<T> where T : class
	{
		/// <summary>
		/// Connection string to use with the DB context.
		/// </summary>
		public string ConnectionString { get;}
	}
}