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
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CodeFactory.Architecture.Blazor.Server.CSharpFile
{
   /// <summary>
    /// Code factory command for automation of a C# document when selected from a project in solution explorer.
    /// </summary>
    public class AddMissingComponentMembers : CSharpSourceCommandBase
    {
        private static readonly string commandTitle = "Add Missing Component Members";
        private static readonly string commandDescription = "Adds missing contract interface members to the component implementation.";

#pragma warning disable CS1998

        /// <inheritdoc />
        public AddMissingComponentMembers(CodeFactory.WinVs.Logging.ILogger logger, IVsActions vsActions) : base(logger, vsActions, commandTitle, commandDescription)
        {
            //Intentionally blank
        }

        #region External Configuration

        /// <summary>
        /// The fully qualified name of the command to be used with configuration.
        /// </summary>
        public static string Type = typeof(AddMissingComponentMembers).FullName;


        /// <summary>
        /// Execution project for the command.
        /// </summary>
        public static string ExecutionProject = "ExecutionProject";

        /// <summary>
        /// Execution folder for the command.
        /// </summary>
        public static string ExecutionFolder = "ExecutionFolder";

        /// <summary>
        /// Class name prefix
        /// </summary>
        public static string ClassNamePrefix = "ClassNamePrefix";
        

        /// <summary>
        /// Class name suffix.
        /// </summary>
        public static string ClassNameSuffix = "ClassNameSuffix";

        /// <summary>
        /// Loads the external configuration definition for this command.
        /// </summary>
        /// <returns>Will return the command configuration or null if this command does not support external configurations.</returns>
        public override ConfigCommand LoadExternalConfigDefinition()
        {
            var config = new ConfigCommand
            { 
                CommandType = Type, 
                Name = nameof(AddMissingComponentMembers), 
                Category = "Components",
                Guidance = "Command is used when updating missing members from a component implementation." 
            }
            .UpdateExecutionProject
            (
                new ConfigProject
                { 
                    Name = ExecutionProject,
                    Guidance = "The project where the component class file resides in."
                }
                .AddFolder
                (
                    new ConfigFolder
                    { 
                      Name = ExecutionFolder,
                      Required = false,
                      Guidance = "The target folder the component class will be found in."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = ClassNamePrefix,
                        Guidance = "Optional, checks to makes sure the class name starts with the provided prefix before considering the target component type."
                    }
                )
                .AddParameter
                (
                    new ConfigParameter
                    { 
                        Name = ClassNameSuffix,
                        Guidance = "Optional, checks to makes sure the class ends with the provided suffix before considering it a target component type."
                    }
                )
               
            );

            return config;
        }

        /// <summary>
        /// Registers the default configuration with the configuration manager in CodeFactory.
        /// </summary>
        public static void RegisterDefaultConfiguration()
        {
            var command = new AddMissingComponentMembers(null, null);
            var config = command.LoadExternalConfigDefinition();
            config?.RegisterCommandWithDefaultConfiguration();
        }
        #endregion

        #region Overrides of VsCommandBase<IVsCSharpDocument>

        /// <summary>
        /// Validation component that will determine if this command should be enabled for execution.
        /// </summary>
        /// <param name="result">The target model data that will be used to determine if this command should be enabled.</param>
        /// <returns>Boolean flag that will tell code factory to enable this command or disable it.</returns>
        public override async Task<bool> EnableCommandAsync(VsCSharpSource result)
        {
            //Result that determines if the command is enabled and visible in the context menu for execution.
            bool isEnabled = false;

            try
            {
               var componentClass = result?.SourceCode?.Classes.FirstOrDefault();

               isEnabled = componentClass != null;

                ConfigCommand command = null;

                if( isEnabled ) 
                {
                    command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                    isEnabled = command != null;
                }

                if(isEnabled ) 
                {
                    var componentPrefix = command.ExecutionProject.ParameterValue(ClassNamePrefix);
                    var componentSuffix = command.ExecutionProject.ParameterValue(ClassNameSuffix);
                    isEnabled = IsComponentClass(componentClass,componentPrefix,componentSuffix);
                }

                if(isEnabled ) isEnabled = GetMissingContainerInterfaceMembersFromComponent(componentClass).Any();
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
                var componentSource = result?.SourceCode;

                if ( componentSource == null ) return;

                var componentClass = componentSource.Classes.FirstOrDefault();

                if(componentClass == null) return;

                var missingMembers = GetMissingContainerInterfaceMembersFromComponent(componentClass);

                if( !missingMembers.Any() ) return;

                componentSource = await componentSource.AddUsingStatementAsync("Microsoft.Extensions.Logging");
                componentSource = await componentSource.AddUsingStatementAsync("CodeFactory.NDF");

                componentClass = componentSource.Classes.FirstOrDefault();


                var command = await ConfigManager.LoadCommandByFolderAsync(Type, ExecutionFolder, result)
                              ?? await ConfigManager.LoadCommandByProjectAsync(Type, result);

                if(command == null)return;


                var loggerBlock = new LoggerBlockNDFLogger("_logger");

                var catchBlocks = new List<ICatchBlock>
                {
                    new CatchBlockManagedExceptionNDFException(loggerBlock,LogLevel.Error),
                    new CatchBlockExceptionNDFException(loggerBlock)
                };

                var boundChecks = new List<IBoundsCheckBlock>
                {

                    new BoundsCheckBlockStringNDFException(true,loggerBlock),
                    new BoundsCheckBlockNull(true,loggerBlock)
                };

                var tryBlock = new TryBlockStandard(loggerBlock,catchBlocks);

                var updatedComponentClass = await VisualStudioActions.AddClassMissingMembersAsync(result.SourceCode,componentClass,false,loggerBlock,Microsoft.Extensions.Logging.LogLevel.Information,boundChecks,tryBlock,missingMembers);
                
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

        private bool IsComponentClass(CsClass componentClass,string componentPrefix, string componentSuffix)
        { 
            
            bool iscomponentClass = false;

            if (componentClass != null) iscomponentClass = true;

            if(iscomponentClass & componentPrefix != null) iscomponentClass = componentClass.Name.StartsWith(componentPrefix);

            if(iscomponentClass & componentSuffix != null) iscomponentClass = componentClass.Name.EndsWith(componentSuffix);
            
            return iscomponentClass;
        }

        #endregion


        private IReadOnlyList<CsMember> GetMissingContainerInterfaceMembersFromComponent(CsContainer source, List<MapNamespace> mappedNamespaces = null)
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
