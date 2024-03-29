﻿using CodeFactory.WinVs.Models.CSharp.Builder;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic
{
    /// <summary>
    /// CodeBlock that builds a catch block for a standard Exception and returns a NDF managed exception.
    /// </summary>
    public class CatchBlockExceptionNDFException:BaseCatchBlock
    {
        /// <summary>
        /// Creates a instances of the <see cref="CatchBlockExceptionNDF"/>
        /// </summary>
        /// <param name="loggerBlock">Optional, logger block to use for logging in the catch block.</param>
        public CatchBlockExceptionNDFException(ILoggerBlock loggerBlock = null) : base(loggerBlock)
        {
            //intentionally bank
        }

        /// <summary>Builds the catch block</summary>
        /// <param name="syntax">Syntax to be injected into the catch block, optional parameter.</param>
        /// <param name="multipleSyntax">Multiple syntax statements has been provided to be used by the catch block,optional parameter.</param>
        /// <param name="memberName">Optional parameter that determines the target member the catch block is implemented in.</param>
        /// <returns>Returns the generated catch block</returns>
        protected override string BuildCatchBlock(string syntax = null, IEnumerable<NamedSyntax> multipleSyntax = null, string memberName = null)
        {
            SourceFormatter formatter = new SourceFormatter();

            formatter.AppendCodeLine(0,"catch (Exception unhandledException)");
            formatter.AppendCodeLine(0,"{");
            if (LoggerBlock != null)
            {
                formatter.AppendCodeLine(1, LoggerBlock.GenerateLogging(LogLevel.Error, "The following unhandled exception occurred, see exception details. Throwing a unhandled managed exception.",false,"unhandledException") );
                formatter.AppendCodeLine(1,  LoggerBlock.GenerateExitLogging(LogLevel.Error, memberName));
            }
            formatter.AppendCodeLine(1,"throw new UnhandledException();");
            formatter.AppendCodeLine(0,"}");

            return formatter.ReturnSource();
        }
    }
}
