using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic
{
/// <summary>
    /// Helper automation that will add missing members from the implementation of a class.
    /// </summary>
    public static class AddClassMissingMemberBuilder
    {
        /// <summary>
        ///  Add missing interface members
        /// </summary>
        /// <param name="source">The CodeFactory automation for Visual Studio Windows</param>
        /// <param name="sourceCode">Source code model to be updated with add members in the target class.</param>
        /// <param name="updateClass">Class model to add missing members to.</param>
        /// <param name="addMemberAttributes">Flag that determines if attributes will be assigned to the member from the interface, default value is false.</param>
        /// <param name="loggerBlock">The logger block to be used with the membmers, default value is null.</param>
        /// <param name="defaultLogLevel">The default level of logging to use when logging is supported, default value is information level.</param>
        /// <param name="boundsChecks">The bounds checks logic to use with methods, default is to set to null.</param>
        /// <param name="tryBlock">Optional, the target try block to use for methods when adding missing methods.</param>
        /// <param name="missingInterfaceMembers">Optional paramemter that provides the </param>
        /// <returns>Updates class with the missing members.</returns>
        public static async Task<CsClass> AddClassMissingMembersAsync(this IVsActions source, CsSource sourceCode,
            CsClass updateClass, bool addMemberAttributes = false, ILoggerBlock loggerBlock = null, LogLevel defaultLogLevel = LogLevel.Information,
            IList<IBoundsCheckBlock> boundsChecks = null,  ITryBlock tryBlock = null,IReadOnlyList<CsMember> missingInterfaceMembers = null)
        {
            //Bounds checks to make sure all data needed is provided.
            if (sourceCode == null)
                throw new CodeFactoryException(
                    "Visual Studio automation for CodeFactory was not provided cannot add missing members.");

            if (sourceCode == null)
                throw new CodeFactoryException("No source code was provided, cannot add the missing members.");

            if (updateClass == null)
                throw new CodeFactoryException(
                    "No target class to add missing members was provided, cannot add the missing members.");

            
            //Get the missing members to be added
            var missingMembers = missingInterfaceMembers != null 
                ? missingInterfaceMembers 
                : updateClass.GetMissingInterfaceMembers();

            //If no missing members are found just return the current class.
            if (!missingMembers.Any()) return updateClass;

            //Creating the source code manager for the class.
            var manager = new SourceClassManager(sourceCode, updateClass, source);
            manager.LoadNamespaceManager();

            //Creating the builders to generate code by member type.
            IMethodBuilder methodBuilder = new MethodBuilderStandard(loggerBlock, boundsChecks, tryBlock);
            IPropertyBuilder propertyBuilder = new PropertyBuilderStandard();
            IEventBuilder eventBuilder = new EventBuilderStandard();
            
            //Process all missing properties.
            var missingProperties = missingMembers.Where(m => m.MemberType == CsMemberType.Property).Cast<CsProperty>()
                .ToList();

            foreach (var missingProperty in missingProperties)
            {
                var propertySyntax = await propertyBuilder.BuildPropertyAsync(missingProperty, manager, 2,includeAttributes:addMemberAttributes);

                if(propertySyntax == null) continue;

                await manager.PropertiesAddAfterAsync(propertySyntax);

            }

            //Process all missing methods.
            var missingMethods = missingMembers.Where(m => m.MemberType == CsMemberType.Method).Cast<CsMethod>()
                .ToList();

            foreach (var missingMethod in missingMethods)
            {
                var methodSyntax = await methodBuilder.BuildMethodAsync(missingMethod, manager, 2,includeAttributes:addMemberAttributes, defaultLogLevel:defaultLogLevel);

                if(methodSyntax == null) continue;

                await manager.MethodsAddAfterAsync(methodSyntax);
            }

            //Process all missing events.
            var missingEvents = missingMembers.Where(m => m.MemberType == CsMemberType.Event).Cast<CsEvent>()
                .ToList();

            foreach (var missingEvent in missingEvents)
            {
                var eventSyntax = await eventBuilder.BuildEventAsync(missingEvent, manager, 2,includeAttributes:addMemberAttributes);

                if(eventSyntax == null) continue;

                await manager.EventsAddAfterAsync(eventSyntax);
            }

            return manager.Container;
        }
    }
}
