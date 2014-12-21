using RazorEngine.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    internal class DynamicWrapperService : IRazorEngineService
    {
        private readonly IRazorEngineService _origin;
        private readonly bool _mustSerialize;
        private readonly bool _allowMissingPropertiesOnDynamic;
        public DynamicWrapperService(IRazorEngineService origin, bool mustSerialize, bool allowMissingPropertiesOnDynamic)
        {
            _origin = origin;
            _mustSerialize = mustSerialize;
            _allowMissingPropertiesOnDynamic = allowMissingPropertiesOnDynamic;
        }
        public ITemplateKey GetKey(string name, ResolveType resolveType = ResolveType.Global, ITemplateKey context = null)
        {
            return _origin.GetKey(name, resolveType, context);
        }

        public void AddTemplate(ITemplateKey key, ITemplateSource templateSource)
        {
            _origin.AddTemplate(key, templateSource);
        }

        internal static bool IsAnonymousTypeRecursive(Type t)
        {
            return t != null && (CompilerServicesUtility.IsAnonymousType(t) ||
                // part of generic
                t.GetGenericArguments().Any(arg => IsAnonymousTypeRecursive(arg)) ||
                // Array is special
                (t.IsArray && IsAnonymousTypeRecursive(t.GetElementType())));
        }

        internal static void CheckModelType(Type modelType)
        {
            if (modelType == null)
            {
                return;
            }
            if (IsAnonymousTypeRecursive(modelType))
            {
                throw new ArgumentException(@"We cannot support anonymous model types as those are internal! 
However you can just use 'dynamic' (modelType == null) and we try to make it work for you (at the cost of performance).");
            }
        }

        internal static object GetDynamicModel(Type modelType, object original, bool allowMissing)
        {
            object result = original;
            if (modelType == null && original != null)
            {
                // We try to make some things work:
                if (IsAnonymousTypeRecursive(original.GetType()))
                {
                    // TODO: we should handle Configuration.AllowMissingPropertiesOnDynamic
                    result = RazorDynamicObject.Create(original, allowMissing);
                }
                else if (allowMissing)
                {
                    result = RazorDynamicObject.Create(original, allowMissing);
                }
            }
            return result;
        }
        
        public bool IsTemplateCached(ITemplateKey key, Type modelType)
        {
            CheckModelType(modelType);
            return _origin.IsTemplateCached(key, modelType);
        }

        public void CompileAndCache(ITemplateKey key, Type modelType = null)
        {
            CheckModelType(modelType);
            _origin.CompileAndCache(key, modelType);
        }

        public void RunCompileOnDemand(ITemplateKey key, System.IO.TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            CheckModelType(modelType);
            _origin.RunCompileOnDemand(key, writer, modelType, GetDynamicModel(modelType, model, _allowMissingPropertiesOnDynamic), viewBag);
        }

        public void RunCachedTemplate(ITemplateKey key, System.IO.TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            CheckModelType(modelType);
            _origin.RunCachedTemplate(key, writer, modelType, GetDynamicModel(modelType, model, _allowMissingPropertiesOnDynamic), viewBag);
        }

        public void Dispose()
        {
            _origin.Dispose();
        }
    }
}
