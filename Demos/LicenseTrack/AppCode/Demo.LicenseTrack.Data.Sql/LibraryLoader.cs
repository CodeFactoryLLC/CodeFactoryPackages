using CodeFactory.NDF;
using Demo.LicenseTrack.Data.Sql.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Demo.LicenseTrack.Data.Sql
{

    /// <summary>
    /// Management class that tracks all the service registrations for the library to be loaded with the hosting applications dependency injection container.
    /// </summary>
    public class LibraryLoader : DependencyInjectionLoader
    {
        /// <summary>
        /// Loads child libraries that are subscribed to by this library.
        /// </summary>
        /// <param name="serviceCollection">The dependency injection provider to register services with.</param>
        /// <param name="configuration">The source configuration to provide for dependency injection.</param>
        protected override void LoadLibraries(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            //Intentionally blank
        }


        /// <summary>
        /// Loads dependency injections that are setup and configured manually.
        /// </summary>
        /// <param name="serviceCollection">The dependency injection provider to register services with.</param>
        /// <param name="configuration">The source configuration to provide for dependency injection.</param>
        /// <exception cref="NotImplementedException"></exception>
        protected override void LoadManualRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            //Intentionally blank
        }







        /// <summary>
        /// Automated registration of classes using transient registration.
        /// </summary>
        /// <param name="serviceCollection">The service collection to register services.</param>
        /// <param name="configuration">The configuration data used with register of services.</param>
        protected override void LoadRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            //This method was auto generated, do not modify by hand!
            serviceCollection.AddTransient<Demo.LicenseTrack.Data.Contracts.ICustomerRepository>(sp => new CustomerRepository(sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CustomerRepository>>(), sp.GetRequiredService<IDBContextConnection<LicenseTrackContext>>()));
        }


    }
}
