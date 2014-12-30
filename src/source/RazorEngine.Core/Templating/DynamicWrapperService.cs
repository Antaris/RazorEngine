using RazorEngine.Compilation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// A wrapper around an <see cref="IRazorEngineService"/> instance to provide support for anonymous classes.
    /// </summary>
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

        internal IRazorEngineService Origin
        {
            get
            {
                return _origin;
            }
        }

        public ITemplateKey GetKey(string name, ResolveType resolveType = ResolveType.Global, ITemplateKey context = null)
        {
            return _origin.GetKey(name, resolveType, context);
        }

        public void AddTemplate(ITemplateKey key, ITemplateSource templateSource)
        {
            _origin.AddTemplate(key, templateSource);
        }

        /// <summary>
        /// Checks if the given model-type has a reference to an anonymous type and throws.
        /// </summary>
        /// <param name="modelType">the type to check</param>
        internal static void CheckModelType(Type modelType)
        {
            if (modelType == null)
            {
                return;
            }
            if (CompilerServicesUtility.IsAnonymousTypeRecursive(modelType))
            {
                throw new ArgumentException(@"We cannot support anonymous model types as those are internal! 
However you can just use 'dynamic' (modelType == null) and we try to make it work for you (at the cost of performance).");
            }
        }

        /// <summary>
        /// Checks if we need to wrap the given model in
        /// an <see cref="RazorDynamicObject"/> instance and wraps it.
        /// </summary>
        /// <param name="modelType">the model-type</param>
        /// <param name="original">the original model</param>
        /// <param name="allowMissing">true when we should allow missing properties on dynamic models.</param>
        /// <returns>the original model or an wrapper object.</returns>
        internal static object GetDynamicModel(Type modelType, object original, bool allowMissing)
        {
            object result = original;
            if (modelType == null && original != null)
            {
                // We try to make some things work:
                if (CompilerServicesUtility.IsAnonymousTypeRecursive(original.GetType()))
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

        public void Compile(ITemplateKey key, Type modelType = null)
        {
            CheckModelType(modelType);
            _origin.Compile(key, modelType);
        }

        public void RunCompile(ITemplateKey key, System.IO.TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            CheckModelType(modelType);
            _origin.RunCompile(key, writer, modelType, GetDynamicModel(modelType, model, _allowMissingPropertiesOnDynamic), viewBag);
        }

        public void Run(ITemplateKey key, System.IO.TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            CheckModelType(modelType);
            _origin.Run(key, writer, modelType, GetDynamicModel(modelType, model, _allowMissingPropertiesOnDynamic), viewBag);
        }

        public void Dispose()
        {
            _origin.Dispose();
        }
    }
}
