using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// This implementation adds ModifiedTime property to <see cref="FullPathTemplateKey" />
    /// </summary>
    [Serializable]
    public class FullPathWithModifiedTimeTemplateKey : FullPathTemplateKey
    {
        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fullPath"></param>
        /// <param name="modifiedTime"></param>
        /// <param name="resolveType"></param>
        /// <param name="context"></param>
        public FullPathWithModifiedTimeTemplateKey(string name, string fullPath, DateTime modifiedTime, ResolveType resolveType, ITemplateKey context)
            : base(name, fullPath, resolveType, context)
        {
            ModifiedTime = modifiedTime;
        }

        /// <summary>
        /// This value is used to check if cache is valid
        /// </summary>
        public DateTime ModifiedTime { get; set; }
    }
}
