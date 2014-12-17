using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RazorEngine.Templating
{
    public class LoadedTemplateSource : ITemplateSource
    {
        private readonly string _template;
        private readonly string _templateFile;

        public LoadedTemplateSource(string template, string templateFile = null)
        {
            this._template = template;
            this._templateFile = templateFile;
        }

        public string Template
        {
            get { return _template; }
        }
        public string TemplateFile
        {
            get { return _templateFile; }
        }

        public TextReader GetTemplateReader()
        {
            return new StringReader(_template);
        }
    }

    // FileTemplateSource?
}
