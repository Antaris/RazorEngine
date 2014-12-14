using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    class CompiledTemplate : ICompiledTemplate
    {
        readonly ITemplateSource _source;
        readonly Type _templateType;
        readonly Type _modelType;
        public CompiledTemplate(ITemplateSource source, Type templateType, Type modelType)
        {
            _source = source;
            _templateType = templateType;
            _modelType = modelType;
        }
        public ITemplateSource Template
        {
            get { return _source; }
        }

        public Type TemplateType
        {
            get { return _templateType; }
        }

        public Assembly TemplateAssembly
        {
            get { return _templateType.Assembly; }
        }

        public Type ModelType
        {
            get { return _modelType; }
        }
    }

}
