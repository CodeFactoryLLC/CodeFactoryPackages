using CodeFactory.WinVs.Models.CSharp.Builder;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic
{
    /// <summary>
    /// Logger block implementation that supports the logger extension methods in the NDF library.
    /// </summary>
    public class LoggerBlockNDFLogger:BaseLoggerBlock
    {
        /// <summary>Constructor for the base class implementation.</summary>
        /// <param name="fieldName">The name of the logger field.</param>
        public LoggerBlockNDFLogger(string fieldName) : base(fieldName,"TraceLog","DebugLog","InformationLog","WarningLog","ErrorLog","CriticalLog")
        {
            //Intentionally blank
        }

        /// <summary>Create formatted logging to be used with automation.</summary>
        /// <param name="level">The logging level for the logger Name.</param>
        /// <param name="message">the target message for logging.</param>
        /// <param name="isFormattedMessage">optional parameter that determines if the string uses a $ formatted string for the message with double quotes in the formatted output.</param>
        /// <param name="exceptionName">Optional parameter to pass the exception field name to be included with the logging.</param>
        /// <returns>The formatted logging Name to be Generated. If no message is provided will return null.</returns>
        public override string GenerateLogging(LogLevel level, string message, bool isFormattedMessage = false, string exceptionName = null)
        {
            if (string.IsNullOrEmpty(message)) return null;
            
            string loggingSyntax = null;
            if(!isFormattedMessage) loggingSyntax =  string.IsNullOrEmpty(exceptionName)
                ? $"{LoggerFieldName}.{LogMethodName(level)}(\"{message}\");"
                : $"{LoggerFieldName}.{LogMethodName(level)}(\"{message}\", {exceptionName});";
            else loggingSyntax =  string.IsNullOrEmpty(exceptionName)
                ? $"{LoggerFieldName}.{LogMethodName(level)}({message});"
                : $"{LoggerFieldName}.{LogMethodName(level)}({message}, {exceptionName});";

            return loggingSyntax;
        }

        /// <summary>
        /// Generates a logging message entering the target member name.
        /// </summary>
        /// <param name="level">The level to log the message at.</param>
        /// <param name="memberName">Optional parameter that provides the member name.</param>
        /// <returns>The formatted logging string.</returns>
        public override string GenerateEnterLogging(LogLevel level, string memberName = null)
        {
            return $"{LoggerFieldName}.EnterLog(LogLevel.{level});";
        }

        /// <summary>
        /// Generates a logging message exiting the target member name.
        /// </summary>
        /// <param name="level">The level to log the message at.</param>
        /// <param name="memberName">Optional parameter that provides the member name.</param>
        /// <returns>The formatted logging string.</returns>
        public override string GenerateExitLogging(LogLevel level, string memberName = null)
        {
            return $"{LoggerFieldName}.ExitLog(LogLevel.{level});";
        }
    }
}
