
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo.LicenseTrack.App.Model;
namespace Demo.LicenseTrack.Logic.Contracts
{
	/// <summary>
	/// Logic contract implementation.
	/// </summary>
	public interface ICustomerLogic
	{

		
		/// <summary>
		/// Adds a new instance of the <see cref="CustomerAppModel"/> model.
		/// </summary>
		Task<CustomerAppModel> AddAsync(CustomerAppModel customerAppModel);
		
		/// <summary>
		/// Updates a instance of the <see cref="CustomerAppModel"/> model.
		/// </summary>
		Task<CustomerAppModel> UpdateAsync(CustomerAppModel customerAppModel);
		
		/// <summary>
		/// Deletes the instance of the <see cref="CustomerAppModel"/> model.
		/// </summary>
		Task DeleteAsync(CustomerAppModel customerAppModel);
		
		
				
	}
}