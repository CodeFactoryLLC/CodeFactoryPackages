using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.LicenseTrack.App.Model
{
    /// <summary>
    /// Data class that captures information needed for a set size page of data.
    /// </summary>
    public class PageDataRequest
    {
        /// <summary>
        /// The page number to return.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The number of records for a page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Optional property that determines the text to be search for on all supported fields.
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// Optional property that provides a list of fields to be sorted and the sort direction.
        /// </summary>
        public List<SortingDirection> FieldsSorting { get; set; } = new List<SortingDirection>();
    }
}
