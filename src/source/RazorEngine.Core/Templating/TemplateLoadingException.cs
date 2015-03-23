using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// Happens when we could compile the template,
    /// but are unable to load the resulting assembly!
    /// </summary>
    public class TemplateLoadingException : Exception
    {
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateLoadingException"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        internal TemplateLoadingException(string message)
            : base(message) { }
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateLoadingException"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The root cause.</param>
        internal TemplateLoadingException(string message, Exception inner)
            : base(message, inner) { }
    }
}
