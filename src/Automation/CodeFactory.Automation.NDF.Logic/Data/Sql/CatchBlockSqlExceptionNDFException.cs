using CodeFactory.WinVs.Models.CSharp.Builder;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic.Data.Sql
{
    /// <summary>
    /// CodeBlock that builds a catch block for SqlExceptions and returns a NDF managed exception.
    /// </summary>
    public class CatchBlockSqlExceptionNDFException:BaseCatchBlock
    {
        /// <summary>
        /// Creates a instances of the <see cref="CatchBlockSqlExceptionNDF"/>
        /// </summary>
        /// <param name="loggerBlock">Optional, logger block to use for logging in the catch block.</param>
        public CatchBlockSqlExceptionNDFException(ILoggerBlock loggerBlock = null) : base(loggerBlock)
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

            formatter.AppendCodeLine(0,"catch (SqlException sqlDataException)");
            formatter.AppendCodeLine(0,"{");
            if (LoggerBlock != null)
            {
                formatter.AppendCodeLine(1, LoggerBlock.GenerateLogging(LogLevel.Error, "The following SQL exception occurred.",false,"sqlDataException"));
                formatter.AppendCodeLine(1, LoggerBlock.GenerateExitLogging(LogLevel.Error, memberName));
            }
            formatter.AppendCodeLine(1,"sqlDataException.ThrowManagedException();");
            formatter.AppendCodeLine(0,"}");

            return formatter.ReturnSource();
        }
    }
}
