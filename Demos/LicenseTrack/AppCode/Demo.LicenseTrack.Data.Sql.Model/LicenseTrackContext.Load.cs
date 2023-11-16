
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Demo.LicenseTrack.Data.Sql.Model
{
	
	public partial class LicenseTrackContext
	{
	
		private readonly string _connectionString;
		
		/// <summary>
		/// Creates a new instance of the context that injects the target connection string to use.
		/// </summary>
		/// <param name="connectionString">connection string to use with the context.</param>
		public LicenseTrackContext(string connectionString)
		{
			_connectionString = connectionString;
		}
		
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!string.IsNullOrEmpty(_connectionString)) optionsBuilder.UseSqlServer(_connectionString);
		}
	}
}