using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeFactory.Automation.Standard.Logic
{
    /// <summary>
    /// Data class that provides management of object names.
    /// </summary>
    public class NameManagement
    {
        #region Backing fields

        /// <summary>
        /// Backing field for the property <see cref="RemovePrefixes"/>
        /// </summary>
        private readonly ImmutableList<string> _removePrefixes;

        /// <summary>
        /// Backing field for the property <see cref="RemoveSuffixes"/>
        /// </summary>
        private readonly ImmutableList<string> _removeSuffixes;

        /// <summary>
        /// Backing field for the property <see cref="AddPrefix"/>
        /// </summary>
        private readonly string _addPrefix;

        /// <summary>
        /// Backing field for the property <see cref="AddSuffix"/>
        /// </summary>
        private readonly string _addSuffix;

        #endregion

        /// <summary>
        /// Constructor that loads the name management data.
        /// </summary>
        /// <param name="removePrefixes">List of prefixes to remove from the beginning of the objects name.</param>
        /// <param name="removeSuffixes">List of suffixes to remove from the end of the objects name.</param>
        /// <param name="addPrefix">The prefix to append to the beginning of the objects name;</param>
        /// <param name="addSuffix">The suffix to append to the end of the objects name.</param>
        protected NameManagement(IEnumerable<string> removePrefixes, IEnumerable<string> removeSuffixes, string addPrefix, string addSuffix)
        { 
            _removePrefixes = removePrefixes != null ? ImmutableList<string>.Empty.AddRange(removePrefixes) : ImmutableList<string>.Empty;
            _removeSuffixes = removeSuffixes != null ? ImmutableList<string>.Empty.AddRange(removeSuffixes) : ImmutableList<string>.Empty;
            _addPrefix = addPrefix;
            _addSuffix = addSuffix;
        }

        /// <summary>
        /// Initializes a new instance of a <see cref="NameManagement"/> data class.
        /// </summary>
        /// <param name="removePrefixes">List of prefixes to remove from the beginning of the objects name.</param>
        /// <param name="removeSuffixes">List of suffixes to remove from the end of the objects name.</param>
        /// <param name="addPrefix">The prefix to append to the beginning of the objects name.</param>
        /// <param name="addSuffix">The suffix to append to the end of the objects name.</param>  
        /// <returns>Initialized name manager.</returns>
        public static NameManagement Init(IEnumerable<string> removePrefixes, IEnumerable<string>  removeSuffixes, string addPrefix, string addSuffix)
        { 
           return new NameManagement(removePrefixes,removeSuffixes, addPrefix, addSuffix);   
        }

                /// <summary>
        /// Initializes a new instance of a <see cref="NameManagement"/> data class.
        /// </summary>
        /// <param name="removePrefixes">Comma seperated list of prefixes to remove from name.</param>
        /// <param name="removeSuffixes">Comma seperated list of suffixes to remove from name.</param>
        /// <param name="addPrefix">The prefix to append to the beginning of the objects name.</param>
        /// <param name="addSuffix">The suffix to append to the end of the objects name.</param>  
        /// <returns>Initialized name manager.</returns>
        public static NameManagement Init(string removePrefixes, string removeSuffixes, string addPrefix, string addSuffix)
        { 
           return new NameManagement(removePrefixes != null? removePrefixes.Split(','):null ,removeSuffixes != null? removeSuffixes.Split(','):null , addPrefix, addSuffix);   
        }


        /// <summary>
        /// List of prefixes to remove from the beginning of the objects name.
        /// </summary>
        public IReadOnlyList<string> RemovePrefixes => _removePrefixes;

        /// <summary>
        /// List of suffixes to remove from the end of the objects name.
        /// </summary>
        public IReadOnlyList<string> RemoveSuffixes => _removeSuffixes;

        /// <summary>
        /// The prefix to append to the beginning of the objects name;
        /// </summary>
        public string AddPrefix => _addPrefix;

        /// <summary>
        /// The suffix to append to the end of the objects name.
        /// </summary>
        public string AddSuffix => _addSuffix; 
    }
}
