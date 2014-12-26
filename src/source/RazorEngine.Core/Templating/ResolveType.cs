using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// The type of a resolve action.
    /// </summary>
    public enum ResolveType
    {
        /// <summary>
        /// When we search for a template in as part of TemplateService.
        /// </summary>
        Global,
        /// <summary>
        /// When we search for a template which is included.
        /// </summary>
        Include,
        /// <summary>
        /// When we search for a layout template.
        /// </summary>
        Layout
    }
}
