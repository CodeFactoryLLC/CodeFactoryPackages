﻿using CodeFactory.WinVs.Models.CSharp.Builder;
using CodeFactory.WinVs.Models.CSharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.NDF.Logic
{
    /// <summary>
    /// Null bounds check that support NDF logging and exceptions.
    /// </summary>
    public class BoundsCheckBlockNullNDFException:BaseBoundsCheckBlock
    {
        /// <summary>Initializes the base class for the bounds check.</summary>
        /// <param name="ignoreWhenDefaultValueIsSet">Flag that determines if the bounds checking should be ignored if a default value is set.</param>
        /// <param name="loggerBlock">Logger block used with bounds check logic.</param>
        public BoundsCheckBlockNullNDFException(bool ignoreWhenDefaultValueIsSet, ILoggerBlock loggerBlock) : base(nameof(BoundsCheckBlockNullNDFException), ignoreWhenDefaultValueIsSet, loggerBlock)
        {
            //Intentionally blank
        }

        /// <summary>
        /// Generates the bounds check syntax if the parameter meets the criteria for a bounds check.
        /// </summary>
        /// <param name="sourceMethod">The target method the parameter belongs to.</param>
        /// <param name="checkParameter">The parameter to build the bounds check for.</param>
        /// <returns>Returns a tuple that contains a boolean that determines if the bounds check syntax was created for the parameter.</returns>
        public override (bool hasBoundsCheck, string boundsCheckSyntax) GenerateBoundsCheck(CsMethod sourceMethod, CsParameter checkParameter)
        {
            //bounds check to make sure we have parameter data.
            if (checkParameter == null) return (false, null);

            if(checkParameter.ParameterType.IsValueType) return (false, null);

            if(checkParameter.HasDefaultValue) return (false, null);

            SourceFormatter formatter = new SourceFormatter();

            formatter.AppendCodeLine(0,$"if ({checkParameter.Name} == null)");
            formatter.AppendCodeLine(0,"{");
            if (LoggerBlock != null)
            {
                var errorMessage = $"$\"The parameter {{nameof({checkParameter.Name})}} was not provided. Will raise an argument exception\"";
                formatter.AppendCodeLine(1,LoggerBlock.GenerateLogging(LogLevel.Error, errorMessage,true));
                formatter.AppendCodeLine(1, LoggerBlock.GenerateExitLogging(LogLevel.Error,sourceMethod.Name));
            }
            formatter.AppendCodeLine(1,$"throw new ArgumentNullException(nameof({checkParameter.Name}));");
            formatter.AppendCodeLine(0,"}");
            
            return (true,formatter.ReturnSource());
        }
    }
}
