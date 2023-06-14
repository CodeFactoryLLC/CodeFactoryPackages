//***************************************************************************
//* Code Factory Packages
//* Copyright (c) 2023 CodeFactory, LLC
//***************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeFactory.WinVs;
using CodeFactory.WinVs.Models.CSharp;
using CodeFactory.WinVs.Models.CSharp.Builder;
using Microsoft.Extensions.Logging;

namespace CodeFactory.Automation.Standard.NDF.Logic
{
    /// <summary>
    /// Logic for adding missing members. 
    /// </summary>
    public static class AddMissingMembers
    {
        /// <summary>
        /// Name of the Microsoft logging library.
        /// </summary>
        public const string MicrosoftLoggingNamespace = "Microsoft.Extensions.Logging";

        /// <summary>
        /// Name of the NDF namespace.
        /// </summary>
        public const string NDFNamespace = "CodeFactory.NDF";

        /// <summary>
        ///  Add missing interface members
        /// </summary>
        /// <param name="source">The CodeFactory automation for Visual Studio Windows</param>
        /// <param name="sourceCode">Source code model to be updated with add members in the target class.</param>
        /// <param name="updateClass">Class model to add missing members to.</param>
        /// <param name="supportsLogging">Flag that determines if logging is enabled.</param>
        /// <param name="loggerFieldName">Optional, the name of the field to use for logging.</param>
        /// <param name="logLevel">Optional, the target log level to use for logging messages. Default level is Information.</param>
        /// <param name="tryBlock">Optional, the target try block to use for methods when adding missing methods. If not provided will provide standard NDF catch blocks.</param>
        /// <returns>Updated class with missing methods.</returns>
        public static async Task<CsClass> AddMissingMembersStandardNDFAsync(this IVsActions source, CsSource sourceCode,
            CsClass updateClass, bool supportsLogging, string loggerFieldName = "_logger", LogLevel logLevel = LogLevel.Information, ITryBlock tryBlock = null)
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
            var missingMembers = updateClass.GetMissingInterfaceMembers();

            //If no missing members are found just return the current class.
            if (!missingMembers.Any()) return updateClass;

            //Creating the source code manager for the class.
            var manager = new SourceClassManager(sourceCode, updateClass, source);
            manager.LoadNamespaceManager();


            //Creating the blocks to be used for code generation
            ILoggerBlock loggerBlock = supportsLogging ? new LoggerBlockNDF(loggerFieldName) : null;

            var boundsChecks = new IBoundsCheckBlock[]
            {
                new BoundsCheckBlockStringNDF(true, loggerBlock),
                new BoundsCheckBlockNullNDF(true, loggerBlock)
            };

            var catchBlocks = new ICatchBlock[]
            {
                new CatchBlockManagedExceptionNDF(loggerBlock),
                new CatchBlockExceptionNDF(loggerBlock)
            };

            ITryBlock methodTryBlock = tryBlock == null ? new TryBlockStandard(loggerBlock, catchBlocks): tryBlock;

            if(supportsLogging) await manager.UsingStatementAddAsync(MicrosoftLoggingNamespace);
            await manager.UsingStatementAddAsync(NDFNamespace);

            //Creating the builders to generate code by member type.
            IMethodBuilder methodBuilder = new MethodBuilderStandard(loggerBlock, boundsChecks, methodTryBlock);
            IPropertyBuilder propertyBuilder = new PropertyBuilderStandard();
            IEventBuilder eventBuilder = new EventBuilderStandard();
            
            //Process all missing properties.
            var missingProperties = missingMembers.Where(m => m.MemberType == CsMemberType.Property).Cast<CsProperty>()
                .ToList();

            foreach (var missingProperty in missingProperties)
            {
                var propertySyntax = await propertyBuilder.BuildPropertyAsync(missingProperty, manager, 2);

                if(propertySyntax == null) continue;

                await manager.PropertiesAddAfterAsync(propertySyntax);

            }

            //Process all missing methods.
            var missingMethods = missingMembers.Where(m => m.MemberType == CsMemberType.Method).Cast<CsMethod>()
                .ToList();

            foreach (var missingMethod in missingMethods)
            {
                var methodSyntax = await methodBuilder.BuildMethodAsync(missingMethod, manager, 2,defaultLogLevel:logLevel);

                if(methodSyntax == null) continue;

                await manager.MethodsAddAfterAsync(methodSyntax);
            }

            //Process all missing events.
            var missingEvents = missingMembers.Where(m => m.MemberType == CsMemberType.Event).Cast<CsEvent>()
                .ToList();

            foreach (var missingEvent in missingEvents)
            {
                var eventSyntax = await eventBuilder.BuildEventAsync(missingEvent, manager, 2);

                if(eventSyntax == null) continue;

                await manager.EventsAddAfterAsync(eventSyntax);
            }

            return manager.Container;
        }
    }
}
