using CodeFactory.WinVs.Logging;
using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.ProjectSystem;
using CodeFactory.WinVs;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.Automation.Standard.Logic;

namespace CodeFactory.Automation.NDF.Logic
{
    /// <summary>
    /// Automation class that will manage creation of dependency injection transient code in libraries that support NDF.
    /// </summary>
    public static class DependencyInjectionBuilder
    {
        //Logger used for code factory logging
        // ReSharper disable once InconsistentNaming
        private static readonly ILogger _logger = LogManager.GetLogger(typeof(DependencyInjectionBuilder));

                /// <summary>
        /// Namespace for dependency injection.
        /// </summary>
        public const string DependencyInjectionAbstractions = "Microsoft.Extensions.DependencyInjection.Abstractions";

        /// <summary>
        /// Namespace for configuration.
        /// </summary>
        public const string ConfigurationAbstractions = "Microsoft.Extensions.Configuration.Abstractions";

        /// <summary>
        /// The fully qualified name of the a aspnet core controller.
        /// </summary>
        public const string ControllerBaseName = "Microsoft.AspNetCore.Mvc.ControllerBase";

        /// <summary>
        /// Checks to make sure the command should display dependency injection or not.
        /// </summary>
        /// <param name="source">Project to check for dependency injection.</param>
        /// <returns>True if it can register or false if not.</returns>
        public static async Task<bool> CanRegisterTransientClassesAsync(this VsProject source)
        { 
            if(source == null) return false;

            bool result = false;


            result = await source.HasMicrosoftExtensionDependencyInjectionLibrariesAsync();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!result) return result;
            
            var loaders = await source.GetProjectLoadersAsync();

            if(loaders != null) result = loaders.Any();

            return result;
        }

        /// <summary>
        /// Extension method that registers all transient classes with dependency injection.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetProject"></param>
        /// <returns></returns>
        /// <exception cref="CodeFactoryException"></exception>
        public static async Task RegisterTransientClassesAsync(this IVsActions source, VsProject targetProject)
        {
            if(targetProject == null) return;
            if (!targetProject.IsLoaded) return;

            try
            {
                if (!await targetProject.HasMicrosoftExtensionDependencyInjectionLibrariesAsync()) return;

                var loaderSourceClasses = await targetProject.GetProjectLoadersAsync();

                if (!loaderSourceClasses.Any()) throw new CodeFactoryException($"There are no library loader classes found in project '{targetProject.Name}' cannot update dependency injection registration.");

                var transientClasses = (await targetProject.LoadInstanceProjectClassesForTransientRegistrationAsync()).ToList();

                foreach (var loaderSource in loaderSourceClasses)
                {
                    var loaderClasses = loaderSource.Classes.Where(c => c.BaseClass.Name == "DependencyInjectionLoader" & c.BaseClass.Namespace == "CodeFactory.NDF" & !c.Namespace.EndsWith("Old"));


                    foreach (var loaderClass in loaderClasses)
                    {
                        var manager = new SourceClassManager(loaderSource, loaderClass, source);
                        manager.LoadNamespaceManager();

                        var loadRegistrationMethod = BuildInjectionMethod(transientClasses,CsSecurity.Protected,true, false, "LoadRegistration", "serviceCollection","configuration", manager.NamespaceManager);

                        if (string.IsNullOrEmpty(loadRegistrationMethod)) continue;

                        var registrationMethod = new SourceFormatter();

                        registrationMethod.AppendCodeBlock(2, loadRegistrationMethod);

                        //If the registration method is not being replaced but added new adding an additional indent level. 
                        string newRegistrationMethod = registrationMethod.ReturnSource();

                        var currentRegistrationMethod =
                            loaderClass.Methods.FirstOrDefault(m => m.Name == "LoadRegistration");

                        if (currentRegistrationMethod != null)

                            await manager.MemberReplaceAsync(currentRegistrationMethod,loadRegistrationMethod);
                        else await manager.MethodsAddAfterAsync(newRegistrationMethod);
                    }
                }

            }
            catch (CodeFactoryException)
            {
                throw;
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while registering transient classes for project '{targetProject.Name}'. ",
                    unhandledError);

                throw new CodeFactoryException($"An unhandled error occurred could not complete registration for project '{targetProject.Name}'");
            }
        }

        /// <summary>
        /// Loads all the classes that exist in the project from each code file found within the project. That qualify for transient dependency injection.
        /// </summary>
        /// <param name="project">The source project to get the classes from</param>
        /// <returns>The class models for all classes that qualify for transient dependency injection. If no classes are found an empty enumeration will be returned.</returns>
        public static async Task<IEnumerable<CsClass>> LoadInstanceProjectClassesForTransientRegistrationAsync(this VsProject project)
        {
            var result = new List<CsClass>();
            if (project == null) return result;
            if (!project.HasChildren) return result;

            try
            {
                var projectChildren = await project.GetChildrenAsync(true, true);

                var csSourceCodeDocuments = projectChildren
                    .Where(m => m.ModelType == VisualStudioModelType.CSharpSource)
                    .Cast<VsCSharpSource>();

                foreach (var csSourceCodeDocument in csSourceCodeDocuments)
                {
                    var sourceCode = csSourceCodeDocument.SourceCode;
                    if (sourceCode == null) continue;
                    if (!sourceCode.Classes.Any()) continue;
                    var classes = sourceCode.Classes.Where(IsTransientClass).Where(c =>
                        result.All(r => $"{c.Namespace}.{c.Name}" != $"{r.Namespace}.{r.Name}"));

                    if (classes.Any()) result.AddRange(classes);
                }

            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while loading the classes to be added to dependency injection.",
                    unhandledError);
            }

            return result;
        }
        /// <summary>
        /// Helper method that confirms a target project supports the microsoft extensions for dependency injection and Configuration.
        /// </summary>
        /// <param name="sourceProject">Target project to check.</param>
        /// <returns>True if found or false of not.</returns>
        public static async Task<bool> HasMicrosoftExtensionDependencyInjectionLibrariesAsync(this VsProject sourceProject)
        {
            if (sourceProject == null) return false;
            if (!sourceProject.IsLoaded) return false;
            var references = await sourceProject.GetProjectReferencesAsync();

            //Checking for dependency injection libraries.
            bool returnResult = references.Any(r => r.Name == DependencyInjectionAbstractions);
            if (!returnResult) return false;

            //Checking for the configuration libraries.
            returnResult = references.Any(r => r.Name == ConfigurationAbstractions);
            return returnResult;
        }

        /// <summary>
        /// Helper method that confirms the class model does not implement a controller base class.
        /// </summary>
        /// <param name="classModel">The class model to confirm does not implement a base class.</param>
        /// <returns></returns>
        public static bool IsController(this CsClass classModel)
        {
            var baseClass = classModel?.BaseClass;
            if (baseClass == null) return false;

            var baseClassName = $"{baseClass.Namespace}.{baseClass.Name}";

            if (baseClassName == ControllerBaseName) return true;

            bool isBaseClass = false;
            if (baseClass.BaseClass != null) isBaseClass = IsController(baseClass);
            return isBaseClass;
        }

        /// <summary>
        /// Checks class data to determine if it qualifies for transient dependency injection.
        /// - Checks to make sure the class only has 1 interface defined.
        /// - Checks to see the class only has 1 constructor defined.
        /// - Checks to see if the class is a asp.net controller if it it remove it
        /// - Checks to see the class name is a startup class if so will be removed.
        /// - Confirms the constructor has no well known types if so will be removed.
        /// </summary>
        /// <param name="classData">The class data to check.</param>
        /// <returns>Boolean state if it qualifies.</returns>
        public static bool IsTransientClass(CsClass classData)
        {
            if (classData == null) return false;
            if (classData.IsStatic) return false;
            if (!classData.Constructors.Any()) return false;

            //Adding transient check for Validators
            if((classData.BaseClass.Name == "AbstractValidator") & classData.Constructors.FirstOrDefault(m =>!m.HasParameters) != null) return true;



            if (!classData.InheritedInterfaces.Any()) return false;
            
            if (classData.IsController()) return false;

            if (classData.Constructors.Count > 1) return false;

            var constructor = classData.Constructors.FirstOrDefault(m => m.HasParameters);

            if (constructor == null) return false;

            if (classData.Name == "Startup") return false;

            return !constructor.Parameters.Any(p => p.ParameterType.IsWellKnownType);
        }

        /// <summary>
        /// Gets all source code files from the root level of the project that implement CodeFactory.NDF.DependencyInjectionLoader base class.
        /// </summary>
        /// <param name="sourceProject">The project to search.</param>
        /// <returns>Source code files where dependency injection loader library is implemented.</returns>
        public static async Task<IReadOnlyList<CsSource>> GetProjectLoadersAsync(this VsProject sourceProject)
        {
            ImmutableList<CsSource> results = ImmutableList<CsSource>.Empty;

            //Bounds checking
            if (sourceProject == null) return results;
            if (!sourceProject.IsLoaded) return results;
            if (!sourceProject.HasChildren) return results;

            //Getting the root level project files and folders from the project.
            var projectChildren = await sourceProject.GetChildrenAsync(false, true);

            //Filtering out everything that is not C# source code file and grabbing all source code files that have a class that implement CodeFactory.NDF.DependencyInjectionLoader base class.
            var loaders = projectChildren.Where(c => c.ModelType == VisualStudioModelType.CSharpSource)
                .Cast<VsCSharpSource>()
                .Where(s => s.SourceCode.Classes.Any(c => c.BaseClass.Name == "DependencyInjectionLoader" & c.BaseClass.Namespace == "CodeFactory.NDF"))
                .Select(c => c.SourceCode);

            if (loaders.Any()) results = results.AddRange(loaders);

            return results;
        }


        /// <summary>
        /// Builds the services registration method. This will contain the transient registrations for each class in the target project.
        /// This will return a signature of [Public/Private] [static] void [methodName](IServiceCollection [collectionParameterName])
        /// With a body that contains the full transient registrations.
        /// </summary>
        /// <param name="classes">The classes to be added.</param>
        /// <param name="isOverride">Flag that determines if the override keyword is added to the method signature.</param>
        /// <param name="isStatic">Flag to determine if the method should be defined as a static or instance method.</param>
        /// <param name="methodName">The target name of the method to be created.</param>
        /// <param name="serviceCollectionParameterName">The name of the service collection parameter where transient registrations will take place.</param>
        /// <param name="configurationParameterName">the name of the configuration parameter to use with registrations.</param>
        /// <param name="manager">The namespace manager that will be used to shorten type name registration with dependency injection. This will need to be loaded from the target class.</param>
        /// <param name="targetSecurity">Determines the target security keyword to add to the injection method.</param>
        /// <returns>The formatted method.</returns>
        public static string BuildInjectionMethod(IEnumerable<CsClass> classes, CsSecurity targetSecurity,bool isOverride,  bool isStatic, string methodName, string serviceCollectionParameterName, string configurationParameterName, NamespaceManager manager = null)
        {

            CodeFactory.SourceFormatter registrationFormatter = new CodeFactory.SourceFormatter();

            string overrideKeyword = isOverride ? " override" : "";
            string methodSignature = isStatic
                ? $"{targetSecurity.GenerateCSharpKeyword()} static void {methodName}(IServiceCollection {serviceCollectionParameterName}, IConfiguration {configurationParameterName})"
                : $"{targetSecurity.GenerateCSharpKeyword()}{overrideKeyword} void {methodName}(IServiceCollection {serviceCollectionParameterName}, IConfiguration {configurationParameterName})";

            registrationFormatter.AppendCodeLine(0, "/// <summary>");
            registrationFormatter.AppendCodeLine(0, "/// Automated registration of classes using transient registration.");
            registrationFormatter.AppendCodeLine(0, "/// </summary>");
            registrationFormatter.AppendCodeLine(0, $"/// <param name=\"{serviceCollectionParameterName}\">The service collection to register services.</param>");
            registrationFormatter.AppendCodeLine(0, $"/// <param name=\"{configurationParameterName}\">The configuration data used with register of services.</param>");
            registrationFormatter.AppendCodeLine(0, methodSignature);
            registrationFormatter.AppendCodeLine(0, "{");
            registrationFormatter.AppendCodeLine(1, "//This method was auto generated, do not modify by hand!");
            foreach (var csClass in classes)
            {
                var registration = FormatTransientRegistration(csClass, serviceCollectionParameterName, manager);
                if (registration != null) registrationFormatter.AppendCodeLine(1, registration);
            }
            registrationFormatter.AppendCodeLine(0, "}");

            return registrationFormatter.ReturnSource();
        }

        /// <summary>
        /// Defines the transient registration statement that will register the class.
        /// </summary>
        /// <param name="classData">The class model to get the registration from.</param>
        /// <param name="serviceCollectionParameterName">The name of the service collection parameter that the transient is being made to.</param>
        /// <param name="manager">Optional parameter that contains the namespace manager that contains the known using statements and target namespace for the class that will host this registration data.</param>
        /// <returns>The formatted transient registration call or null if the class does not meet the criteria.</returns>
        private static string FormatTransientRegistration(CsClass classData, string serviceCollectionParameterName, NamespaceManager manager = null)
        {
            //Cannot find the class data will return null
            if (classData == null) return null;

            string registrationType = null;
            string classType = null;

            ICsMethod constructorData = classData.Constructors.FirstOrDefault();

            //Confirming we have a constructor 
            if (constructorData == null) return null;

            //Getting the fully qualified type name for the formatters library for the class.
            classType = classData.GenerateClassTypeName(manager);

            //if we are not able to format the class name correctly return null.
            if (classType == null) return null;

            //Assuming the first interface inherited will be used for dependency injection if any are provided.
            if (classData.InheritedInterfaces.Any())
            {
                CsInterface interfaceData = classData.InheritedInterfaces.FirstOrDefault();

                if (interfaceData != null) registrationType = interfaceData.GenerateInterfaceTypeName(manager);
            }

            string diStatement = null;

            if (!constructorData.HasParameters) diStatement = registrationType != null
                ? $"{serviceCollectionParameterName}.AddTransient<{registrationType},{classType}>();" :
                  $"{serviceCollectionParameterName}.AddTransient<{classType}>();";

            else
            {
                bool isFirstParameter = constructorData.Parameters.Count != 1;

                StringBuilder constructorSyntax = new StringBuilder();

                constructorSyntax.Append($"( sp => new {classData.Name}(");

                foreach (CsParameter parameter in constructorData.Parameters)
                {
                    if (isFirstParameter)
                    {
                        constructorSyntax.Append($"sp.GetRequiredService<{parameter.ParameterType.GenerateCSharpTypeName(manager)}>()");
                        isFirstParameter = false;
                    }
                    else
                    {
                        constructorSyntax.Append($", sp.GetRequiredService<{parameter.ParameterType.GenerateCSharpTypeName(manager)}>()");
                    }
                }
                
                constructorSyntax.Append("));");

                diStatement = registrationType != null
                    ? $"{serviceCollectionParameterName}.AddTransient<{registrationType}>{constructorSyntax.ToString()}" :
                    $"{serviceCollectionParameterName}.AddTransient<{classType}>{constructorSyntax.ToString()}";
            }
            return diStatement;
        }
    }
}
