using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RazorEngine.Templating
{
    /// <summary>
    /// A simple <see cref="ITemplateSource"/> implementation which represents an in-memory string.
    /// </summary>
    [Serializable]
    public class LoadedTemplateSource : ITemplateSource
    {
        private readonly string _template;
        private readonly string _templateFile;

        /// <summary>
        /// Initializes a new <see cref="LoadedTemplateSource"/> instance.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="templateFile"></param>
        public LoadedTemplateSource(string template, string templateFile = null)
        {
            this._template = template;
            this._templateFile = templateFile;
        }

        /// <summary>
        /// The in-memory template sourcecode.
        /// </summary>
        public string Template
        {
            get { return _template; }
        }

        /// <summary>
        /// The template file or null if none exists.
        /// </summary>
        public string TemplateFile
        {
            get { return _templateFile; }
        }

        /// <summary>
        /// Creates a new <see cref="StringReader"/> to read the in-memory stream.
        /// </summary>
        /// <returns></returns>
        public TextReader GetTemplateReader()
        {
            return new StringReader(_template);
        }
    }

    // FileTemplateSource?
}
