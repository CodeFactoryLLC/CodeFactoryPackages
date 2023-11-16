
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Demo.LicenseTrack.App.Model;
namespace Demo.LicenseTrack.Client.Contracts
{
	/// <summary>
	/// Abstract implementation that supports 'CustomerClient'/>
	/// </summary>
	public interface ICustomerClient
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