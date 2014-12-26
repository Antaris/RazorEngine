using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// With a template key a template can be resolved and loaded.
    /// Implementations of this interface are provided along with the ITemplateManager implementation.
    /// See <see cref="BaseTemplateKey"/> for a base implementation.
    /// </summary>
    public interface ITemplateKey
    {
        /// <summary>
        /// The name of the template (ie. when used in a @Include)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The layer where the template is to be resolved.
        /// </summary>
        ResolveType TemplateType { get; }

        /// <summary>
        /// The context where the template is to be resolved (ie the parent template).
        /// </summary>
        ITemplateKey Context { get; }

        /// <summary>
        /// Gets a unique string which can be used as key by the caching layer.
        /// </summary>
        /// <returns></returns>
        string GetUniqueKeyString();
    }
}
