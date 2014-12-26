using RazorEngine.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// A simple readonly implementation of <see cref="ICompiledTemplate"/>.
    /// </summary>
    internal class CompiledTemplate : ICompiledTemplate
    {
        private readonly CompilationData _tempFiles;
        private readonly ITemplateSource _source;
        private readonly ITemplateKey _key;
        private readonly Type _templateType;
        private readonly Type _modelType;

        public CompiledTemplate(CompilationData tempFiles, ITemplateKey key, ITemplateSource source, Type templateType, Type modelType)
        {
            _tempFiles = tempFiles;
            _key = key;
            _source = source;
            _templateType = templateType;
            _modelType = modelType;
        }

        public CompilationData CompilationData
        {
            get { return _tempFiles; }
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
