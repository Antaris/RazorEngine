using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorEngine.Templating
{
    public class TemplateSource : ITemplateSource
    {
        private readonly string _template;

        public TemplateSource(string template)
        {
            this._template = template;
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
