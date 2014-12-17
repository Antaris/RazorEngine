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
        readonly ITemplateKey _context;

        public BaseTemplateKey(string name, ResolveType resolveType, ITemplateKey context)
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
        public ITemplateKey Context
        {
            get { return _context; }
        }

        public abstract string GetUniqueKeyString();
    }

}
