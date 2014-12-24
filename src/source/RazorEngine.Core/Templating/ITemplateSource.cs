using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// Represents a template source (ie the source code of a template).
    /// </summary>
    public interface ITemplateSource
    {
        /// <summary>
        /// When not null this file is used for debugging the template.
        /// </summary>
        string TemplateFile { get; }

        /// <summary>
        /// The source code of the template.
        /// </summary>
        string Template { get; }

        /// <summary>
        /// Get a reader to read the template.
        /// </summary>
        /// <returns></returns>
        TextReader GetTemplateReader();
    }
}
