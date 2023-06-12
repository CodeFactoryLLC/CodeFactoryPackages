//***************************************************************************
//* Code Factory Packages
//* Copyright (c) 2023 CodeFactory, LLC
//***************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Data.Sql.EF.Logic
{
    /// <summary>
    /// Shared data used for lookup values
    /// </summary>
    public static class SharedData
    {
        /// <summary>
        /// Library name and root namespace for NDF library
        /// </summary>
        public const string NDFLibraryName = "CodeFactory.NDF";

        /// <summary>
        /// Library name and root namespace for logging extensions from Microsoft.
        /// </summary>
        public const string MicrosoftLogging = "Microsoft.Extensions.Logging";

        /// <summary>
        /// Library name for abstractions for logging extensions from Microsoft.
        /// </summary>
        public const string MicrosoftLoggingAbstractions = "Microsoft.Extensions.Logging";
    }
}
