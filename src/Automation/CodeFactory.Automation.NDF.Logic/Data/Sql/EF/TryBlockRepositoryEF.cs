using CodeFactory.WinVs.Models.CSharp.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic.Data.Sql.EF
{

    /// <summary>
    /// Try basic code block standard implementation. Supports injection of syntax insidet he try block, it also will generate the catch and finally blocks if they are provided.
    /// </summary>
    public class TryBlockRepositoryEF : BaseTryBlock
    {
        private readonly string _efContextName; 
        
        /// <summary>
        ///  Creates a instance of the try block that supports using contenxt in the try block definition.
        /// </summary>
        /// <param name="loggerBlock">Optional parameter that provides the logger block.</param>
        /// <param name="catchBlocks">Optional parameter catch blocks that support the try block.</param>
        /// <param name="finallyBlock">Optional parameter finally block that supports the try block.</param>
        public TryBlockRepositoryEF(string efContextName, ILoggerBlock loggerBlock = null, IEnumerable<ICatchBlock> catchBlocks = null, IFinallyBlock finallyBlock = null)
            : base(loggerBlock, catchBlocks, finallyBlock)
        {
            _efContextName = efContextName;
        }

        /// <summary>
        /// Builds the syntax for the try block
        /// </summary>
        /// <param name="syntax">Syntax to be injected into the try block, optional parameter.</param>
        /// <param name="multipleSyntax"> Multiple syntax statements has been provided to be used by the try block,optional parameter.</param>
        /// <param name="memberName">Optional parameter that determines the target member the try block is implemented in.</param>
        /// <returns>Returns the generated try block</returns>
        protected override string BuildTryBlock(string syntax = null, IEnumerable<NamedSyntax> multipleSyntax = null, string memberName = null)
        {
            SourceFormatter sourceFormatter = new SourceFormatter();
            sourceFormatter.AppendCodeLine(0, "try");
            sourceFormatter.AppendCodeLine(0, "{");
            if (string.IsNullOrEmpty(syntax))
            {

                if(!string.IsNullOrEmpty(_efContextName)) 
                {
                    sourceFormatter.AppendCodeLine(1,$"using (var context = new {_efContextName}(_connectionString))");
                    sourceFormatter.AppendCodeLine(1,"{");
                    sourceFormatter.AppendCodeLine(2,"//TODO: Implement ef logic.");
				    sourceFormatter.AppendCodeLine(1,"}");
                    
                }
                else sourceFormatter.AppendCodeLine(1, string.IsNullOrEmpty(memberName) ? "//TODO: Implement try block" : ("//TODO: Implement try block for '" + memberName + "'"));
            }
            else
            {
                sourceFormatter.AppendCodeBlock(1, syntax);
            }

            sourceFormatter.AppendCodeLine(0, "}");
            bool flag = multipleSyntax != null;
            if (flag)
            {
                flag = multipleSyntax.Any();
            }

            if (base.CatchBlocks.Any())
            {
                foreach (ICatchBlock catchBlock in base.CatchBlocks)
                {
                    string text = (flag ? catchBlock.GenerateCatchBlock(multipleSyntax, memberName) : catchBlock.GenerateCatchBlock(memberName));
                    if (!string.IsNullOrEmpty(text))
                    {
                        sourceFormatter.AppendCodeBlock(0, text);
                    }
                }
            }

            if (base.FinallyBlock != null)
            {
                string text2 = (flag ? base.FinallyBlock.GenerateFinallyBlock(multipleSyntax, memberName) : base.FinallyBlock.GenerateFinallyBlock(memberName));
                if (!string.IsNullOrEmpty(text2))
                {
                    sourceFormatter.AppendCodeBlock(0, text2);
                }
            }

            return sourceFormatter.ReturnSource();
        }
    }
}
