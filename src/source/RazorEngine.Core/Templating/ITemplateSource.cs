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
        /// <summary>
        /// When not null this file is used for debugging the template.
        /// </summary>
        string TemplateFile { get; }

        string Template { get; }


        TextReader GetTemplateReader();
    }
}
