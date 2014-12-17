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
        private readonly ITemplateSource _source;
        private readonly ITemplateKey _key;
        private readonly Type _templateType;
        private readonly Type _modelType;
        public CompiledTemplate(ITemplateKey key, ITemplateSource source, Type templateType, Type modelType)
        {
            _key = key;
            _source = source;
            _templateType = templateType;
            _modelType = modelType;
        }

        public ITemplateKey Key
        {
            get { return _key; }
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
