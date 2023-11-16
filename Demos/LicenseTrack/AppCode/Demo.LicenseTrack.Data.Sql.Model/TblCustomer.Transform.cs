
using Demo.LicenseTrack.App.Model;
namespace Demo.LicenseTrack.Data.Sql.Model
{
	public partial class TblCustomer
	{

		///<Summary>
		///Creates a instance of the data model from the source poco model.
		/// </summary>
		/// <param name="pocoModel">The POCO model to used to load the data model.</param>
		/// <returns>New instance of the data model.</returns>
		public static TblCustomer CreateDataModel(CustomerAppModel pocoModel)
		{
			return pocoModel != null ? new TblCustomer {
				Address = pocoModel.Address,
				Address2 = pocoModel.Address2,
				City = pocoModel.City,
				Email = pocoModel.Email,
				FirstName = pocoModel.FirstName,
				Id = pocoModel.Id,
				LastName = pocoModel.LastName,
				MiddleName = pocoModel.MiddleName,
				PostalCode = pocoModel.PostalCode,
				State = pocoModel.State
			}: null;
		}
		
		///<Summary>
		///Creates a instance of the POCO model from the source data model.
		/// </summary>
		/// <returns>New instance of the app model.</returns>
		public CustomerAppModel CreatePocoModel()
		{
			return new CustomerAppModel{
				Address = Address,
				Address2 = Address2,
				City = City,
				Email = Email,
				FirstName = FirstName,
				Id = Id,
				LastName = LastName,
				MiddleName = MiddleName,
				PostalCode = PostalCode,
				State = State
			};
		}	
	}
}