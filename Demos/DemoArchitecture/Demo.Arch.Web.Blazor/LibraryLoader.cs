using CodeFactory.NDF;

namespace Demo.Arch.Web.Blazor
{
    /// <summary>
    /// Provides dependency injection management for this library.
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
            var abstractLoader = new Abstraction.Transport.Rest.LibraryLoader();

            abstractLoader.Load(serviceCollection, configuration);
        }

        /// <summary>
        /// Loads dependency injections that are setup and configured manually.
        /// </summary>
        /// <param name="serviceCollection">The dependency injection provider to register services with.</param>
        /// <param name="configuration">The source configuration to provide for dependency injection.</param>
        protected override void LoadManualRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            //Manual register singlton and other services that cannot be loaded through automation.
        }

        /// <summary>
        /// Loads dependency injections that are setup and configured manually.
        /// </summary>
        /// <param name="serviceCollection">The dependency injection provider to register services with.</param>
        /// <param name="configuration">The source configuration to provide for dependency injection.</param>
        protected override void LoadRegistration(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            //Auto generation for transient classes through automation.
        }
    }
}
