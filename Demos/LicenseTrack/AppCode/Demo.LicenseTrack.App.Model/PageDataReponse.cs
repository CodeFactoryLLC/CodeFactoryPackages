using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.LicenseTrack.App.Model
{
    /// <summary>
    /// Data class that provides the results from a <see cref="PageDataRequest"/>.
    /// </summary>
    /// <typeparam name="T">The target type of the data model to be returned.</typeparam>
    public class PageDataResponse<T> where T : class
    {
        /// <summary>
        /// The total number of records in the full query results.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// The data results from the paging request.
        /// </summary>
        public List<T> Results { get; set; } = new List<T>();
    }
}
