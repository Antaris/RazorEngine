using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{

    public interface ICompiledTemplate
    {
        ITemplateSource Template { get; }


        Type TemplateType { get; }

        Assembly TemplateAssembly { get; }

        Type ModelType { get; }
    }
}
