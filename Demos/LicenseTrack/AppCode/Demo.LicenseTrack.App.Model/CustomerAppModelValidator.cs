
using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace Demo.LicenseTrack.App.Model
{
	/// <summary>
	/// Validation class that supports the model <see cref="CustomerAppModel"/>
	/// </summary>
	public class CustomerAppModelValidator:AbstractValidator<CustomerAppModel>
	{
		/// <summary>
		/// Creates a new instance of the validator.
		/// </summary>
		public CustomerAppModelValidator()
		{
			DataValidation();
			CustomValidation();
		}
		
		/// <summary>
		/// Implementation of custom validation.
		/// </summary>
		private void CustomValidation()
		{
			//TODO: Add custom validation for 'CustomerAppModel'
		}


        /// <summary>
        /// Implementation of data annotation validation.
        /// </summary>
        private void DataValidation()
        {
            //Rules for the FirstName property.
            RuleFor(m => m.FirstName).MaximumLength(100).When(m => !string.IsNullOrEmpty(m.FirstName));

            //Rules for the LastName property.
            RuleFor(m => m.LastName).MaximumLength(100).When(m => !string.IsNullOrEmpty(m.LastName));

            //Rules for the MiddleName property.
            RuleFor(m => m.MiddleName).MaximumLength(100).When(m => !string.IsNullOrEmpty(m.MiddleName));

            //Rules for the Email property.
            RuleFor(m => m.Email).MaximumLength(100).When(m => !string.IsNullOrEmpty(m.Email));

            //Rules for the Address property.
            RuleFor(m => m.Address).MaximumLength(512).When(m => !string.IsNullOrEmpty(m.Address));

            //Rules for the Address2 property.
            RuleFor(m => m.Address2).MaximumLength(512).When(m => !string.IsNullOrEmpty(m.Address2));

            //Rules for the City property.
            RuleFor(m => m.City).MaximumLength(255).When(m => !string.IsNullOrEmpty(m.City));

            //Rules for the State property.
            RuleFor(m => m.State).MaximumLength(10).When(m => !string.IsNullOrEmpty(m.State));

            //Rules for the PostalCode property.
            RuleFor(m => m.PostalCode).MaximumLength(20).When(m => !string.IsNullOrEmpty(m.PostalCode));


        }

    }
}