using CodeFactory.WinVs.Models.CSharp.Builder;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic.AspNetCore.Blazor
{
    /// <summary>
    /// CodeBlock that builds a catch block for ManagedException and passes the exception through to the next caller in the chain.
    /// </summary>
    public class CatchBlockManagedExceptionBlazorControllerMessage:BaseCatchBlock
    {
        /// <summary>
        /// Log level used for catch block log messages.
        /// </summary>
        private readonly LogLevel _logLevel;

        /// <summary>
        /// Name of the method that will display the error message.
        /// </summary>
        private readonly string _showErrorMessageName;

        /// <summary>
        /// Creates a instances of the <see cref="CatchBlockManagedExceptionNDF"/>
        /// </summary>
        /// <param name="loggerBlock">Optional, logger block to use for logging in the catch block.</param>
        /// <param name="logLevel">Optional, sets the level for log messages default is Error.</param>
        /// <param name="showErrorMethodName">Default name of the method to call from the controller when raising the message from a NDF managed exception, default is 'ShowErrorMessage'</param>
        public CatchBlockManagedExceptionBlazorControllerMessage(ILoggerBlock loggerBlock = null,LogLevel logLevel = LogLevel.Error,string showErrorMethodName = null) : base(loggerBlock)
        {
            _logLevel = logLevel;

            _showErrorMessageName = showErrorMethodName != null 
                ? showErrorMethodName
                : "RaiseShowErrorMessage";
        }

        /// <summary>Builds the catch block</summary>
        /// <param name="syntax">Syntax to be injected into the catch block, optional parameter.</param>
        /// <param name="multipleSyntax">Multiple syntax statements has been provided to be used by the catch block,optional parameter.</param>
        /// <param name="memberName">Optional parameter that determines the target member the catch block is implemented in.</param>
        /// <returns>Returns the generated catch block</returns>
        protected override string BuildCatchBlock(string syntax = null, IEnumerable<NamedSyntax> multipleSyntax = null, string memberName = null)
        {
            SourceFormatter formatter = new SourceFormatter();

            formatter.AppendCodeLine(0,"catch (ManagedException managedException)");
            formatter.AppendCodeLine(0,"{");
            if (LoggerBlock != null)
            {
                formatter.AppendCodeLine(1, LoggerBlock.GenerateExitLogging(_logLevel));
            }
            formatter.AppendCodeLine(1,$"{_showErrorMessageName}(managedException.Message);");
            formatter.AppendCodeLine(0,"}");

            return formatter.ReturnSource();
        }
    }
}
