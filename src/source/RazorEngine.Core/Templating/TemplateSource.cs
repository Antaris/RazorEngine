using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorEngine.Templating
{
    public class TemplateSource : ITemplateSource
    {
        private readonly string _template;
        private readonly string _templateName;

        public TemplateSource(string template, string templateName)
        {
            this._template = template;
            this._templateName = templateName;
        }

        public string TemplateName
        {
            get { return _templateName; }
        }

        public string Template
        {
            get { return _template; }
        }

        public System.IO.Stream TemplateStream
        {
            get { return null; }
        }
    }
}
