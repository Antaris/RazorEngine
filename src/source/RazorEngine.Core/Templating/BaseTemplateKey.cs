using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorEngine.Templating
{
    public abstract class BaseTemplateKey : ITemplateKey
    {
        readonly string _name;
        readonly ResolveType _resolveType;
        readonly ICompiledTemplate _context;

        public BaseTemplateKey(string name, ResolveType resolveType, ICompiledTemplate context)
        {
            _name = name;
            _resolveType = resolveType;
            _context = context;
        }
        public string Name
        {
            get { return _name; }
        }

        public ResolveType TemplateType
        {
            get { return _resolveType; }
        }
        public ICompiledTemplate Context
        {
            get { return _context; }
        }

        public abstract string GetUniqueKeyString();
    }

}
