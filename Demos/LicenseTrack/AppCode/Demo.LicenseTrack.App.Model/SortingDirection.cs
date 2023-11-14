using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.LicenseTrack.App.Model
{
    /// <summary>
    /// Data class to provide sort direction for fields on grid.
    /// </summary>
    public class SortingDirection
    {
        /// <summary>
        /// Field being sorted
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Is the field in descending direction
        /// </summary>
        public bool IsDesc { get; set; }
    }
}
