using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    public interface ITemplateKey
    {
        string Name { get; }

        ResolveType TemplateType { get; }

        ICompiledTemplate Context { get; }

        string GetUniqueKeyString();
    }
}
