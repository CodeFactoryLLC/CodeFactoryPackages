using CodeFactory.Automation.NDF.Logic;
using CodeFactory.Automation.NDF.Logic.AspNetCore.Blazor;
using CodeFactory.Automation.Standard.Logic;
using CodeFactory.WinVs;
using CodeFactory.WinVs.Commands;
using CodeFactory.WinVs.Commands.SolutionExplorer;
using CodeFactory.WinVs.Logging;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CodeFactory.Architecture.Blazor.Server
{
   /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class AddMissingControllerMembers : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Add Missing Controller Members";
        private static readonly string commandDescription = "Adds missing contract interface members to the Controller implementation.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public AddMissingControllerMembers(ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(AddMissingControllerMembers).FullName;


        /// <summary>
        /// Exection project for the command.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// Execution folder for the command.
        /// </summary>
        public static string ExecutionFolder = "ExecutionFolder";

        /// <summary>
        /// Repositories name prefix
        /// </summary>
        public static string ControllerPrefix = "ControllerPrefix";
        

        /// <summary>
        /// Repositories name suffix.
        /// </summary>
        public static string ControllerSuffix = "ControllerSuffix";

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            var config = new ConfigCommand{ CommandType = Type, Name = commandTitle, Category = "Controller",Guidance = "Command is used when updating missing members from a controller implementation." }
            .UpdateExecutionProject
            (
                new ConfigProject
                { 
                    Name = ExecutionProject,
                    Guidance = "The project where the controller class file resides in."
                }
                .AddFolder
                (
                    new ConfigFolder
                    { 
                      Name = ExecutionFolder,
                      Required = false,
                      Guidance = "The target folder the controller class will be found in."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = ControllerPrefix,
                        Guidance = "Optional, checks to makes sure the class starts with the provided prefix before considering it a controller."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = ControllerSuffix,
                        Guidance = "Optional, checks to makes sure the class starts with the provided suffix before considering it a controller."
                    }
                )
               
            );

            return config;
        }
        #endregion

        #region Overrides of VsCommandBase<IVsCSharpDocument>

        /// <summary>
        /// Validation controller that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsCSharpSource result)
        {
            //Result that determines if the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
               var controllerClass = result?.SourceCode?.Classes.FirstOrDefault();

               isEnabled = controllerClass != null;

                ConfigCommand command = null;

                if( isEnabled ) 
                {
                    command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                    isEnabled = command != null;
                }

                if(isEnabled ) 
                {
                    var controllerPrefix = command.ExecutionProject.ParameterValue(ControllerPrefix);
                    var controllerSuffix = command.ExecutionProject.ParameterValue(ControllerSuffix);
                    isEnabled = IsControllerClass(controllerClass,controllerPrefix,controllerSuffix);
                }

                if(isEnabled ) isEnabled = GetMissingContainerInterfaceMembersFromController(controllerClass).Any();
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while checking if the solution explorer C# document command {commandTitle} is enabled. ",
                    unhandledError);
                isEnabled = false;
            }

            return isEnabled;
        }

        /// <summary>
        /// Code factory framework calls this method when the command has been executed. 
        /// </summary>
        /// <param name="result">The code factory model that has generated and provided to the command to process.</param>
        public override async Task ExecuteCommandAsync(VsCSharpSource result)
        {
            try
            {
                var controllerSource = result?.SourceCode;

                if ( controllerSource == null ) return;

                var controllerClass = controllerSource.Classes.FirstOrDefault();

                if(controllerClass == null) return;

                var missingMembers = GetMissingContainerInterfaceMembersFromController(controllerClass);

                if( !missingMembers.Any() ) return;



                controllerSource = await controllerSource.AddUsingStatementAsync("Microsoft.Extensions.Logging");
                controllerSource = await controllerSource.AddUsingStatementAsync("CodeFactory.NDF");

                controllerClass = controllerSource.Classes.FirstOrDefault();


                var command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                if(command == null)return;


                var loggerBlock = new LoggerBlockNDFLogger("_logger");

                var catchBlocks = new List<ICatchBlock>
                { 
                    new CatchBlockManagedExceptionBlazorControllerMessage(loggerBlock),
                    new CatchBlockExceptionBlazorControllerMessage(loggerBlock)
                };

                var boundChecks = new List<IBoundsCheckBlock>
                { 
                  
                    new BoundsCheckBlockNullBlazorControllerMessage(true,loggerBlock),
                    new BoundsCheckBlockStringBlazorControllerMessage(true,loggerBlock)
                };

                var tryBlock = new TryBlockStandard(loggerBlock,catchBlocks);

                var updatedControllerClass = await VisualStudioActions.AddClassMissingMembersAsync(result.SourceCode,controllerClass,false,loggerBlock,Microsoft.Extensions.Logging.LogLevel.Information,boundChecks,tryBlock,missingMembers);
                
            }
            catch (CodeFactoryException cfException)
            { 
                MessageBox.Show(cfException.Message,"CodeFactory Error",MessageBoxButton.OK,MessageBoxImage.Error);    
            }
            catch (Exception unhandledError)
            {
                _logger.Error($"The following unhandled error occurred while executing the solution explorer C# document command {commandTitle}. ",
                    unhandledError);

            }

        }

        private bool IsControllerClass(CsClass controllerClass,string controllerPrefix, string controllerSuffix)
        { 
            
            bool iscontrollerClass = false;

            if (controllerClass != null) iscontrollerClass = true;

            if(iscontrollerClass & controllerPrefix != null) iscontrollerClass = controllerClass.Name.StartsWith(controllerPrefix);

            if(iscontrollerClass & controllerSuffix != null) iscontrollerClass = controllerClass.Name.EndsWith(controllerSuffix);
            
            return iscontrollerClass;
        }

        #endregion


        private IReadOnlyList<CsMember> GetMissingContainerInterfaceMembersFromController(CsContainer source, List<MapNamespace> mappedNamespaces = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.ContainerType == CsContainerType.Interface)
            {
                return ImmutableList<CsMember>.Empty;
            }

            if (source.InheritedInterfaces == null)
            {
                return ImmutableList<CsMember>.Empty;
            }

            IReadOnlyList<KeyValuePair<int, CsMember>> sourceMembers = source.GetComparisonMembers(MemberComparisonType.Security);
            Dictionary<int, CsMember> dictionary = new Dictionary<int, CsMember>();
            foreach (CsInterface inheritedInterface in source.InheritedInterfaces)
            {
                switch (inheritedInterface.Name)
                { 
                    case "IHandleEvent":
                        
                        continue;
                        break;

                    case "IHandleAfterRender":
                        continue;
                        break;

                    case "IComponent":
                        continue;
                        break;

                    default:

                        break;
                }

                IReadOnlyList<KeyValuePair<int, CsMember>> comparisonMembers = inheritedInterface.GetComparisonMembers(MemberComparisonType.Security);
                if (!comparisonMembers.Any())
                {
                    continue;
                }

                foreach (KeyValuePair<int, CsMember> item in comparisonMembers)
                {
                    if (!dictionary.ContainsKey(item.Key))
                    {
                        dictionary.Add(item.Key, item.Value);
                    }
                }
            }

            if (!dictionary.Any())
            {
                return ImmutableList<CsMember>.Empty;
            }

            return dictionary.Where((KeyValuePair<int, CsMember> interfaceMember) => !sourceMembers.Any(delegate (KeyValuePair<int, CsMember> m)
            {
                int key = m.Key;
                KeyValuePair<int, CsMember> keyValuePair2 = interfaceMember;
                return key == keyValuePair2.Key;
            })).Select(delegate (KeyValuePair<int, CsMember> interfaceMember)
            {
                KeyValuePair<int, CsMember> keyValuePair = interfaceMember;
                return keyValuePair.Value;
            }).ToImmutableList();
        }
    }
}
