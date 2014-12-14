using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    public interface ITemplateSource
    {
        string TemplateName { get; }
        string Template { get; }
        Stream TemplateStream { get; }
    }
}
