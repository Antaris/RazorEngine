using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    public class DefaultCachingProvider : ICachingProvider
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, ICompiledTemplate>> _cache =
            new ConcurrentDictionary<string, ConcurrentDictionary<Type, ICompiledTemplate>>();

        private readonly TypeLoader _loader;
        private readonly ConcurrentBag<Assembly> _assemblies = new ConcurrentBag<Assembly>();

        public DefaultCachingProvider()
        {
            _loader = new TypeLoader(AppDomain.CurrentDomain, _assemblies);
        }

        public TypeLoader TypeLoader
        {
            get
            {
                return _loader;
            }
        }
        private class NoModel { }

        public static Type GetModelTypeKey(Type modelType)
        {
            if (modelType == null || 
                typeof(System.Dynamic.IDynamicMetaObjectProvider).IsAssignableFrom(modelType))
            {
                return typeof(System.Dynamic.DynamicObject);
            }
            return modelType;
        }

        private void CacheTemplateHelper(ICompiledTemplate template, ITemplateKey templateKey, Type modelTypeKey)
        {
            var uniqueKey = templateKey.GetUniqueKeyString();
            _cache.AddOrUpdate(uniqueKey, key =>
            {
                // new item added
                _assemblies.Add(template.TemplateAssembly);
                var dict = new ConcurrentDictionary<Type, ICompiledTemplate>();
                dict.AddOrUpdate(modelTypeKey, template, (t, old) => template);
                return dict;
            }, (key, dict) =>
            {
                dict.AddOrUpdate(modelTypeKey, t =>
                {
                    // new item added (template was not compiled with the given type).
                    _assemblies.Add(template.TemplateAssembly);
                    return template;
                }, (t, old) =>
                {
                    // item was already added before
                    return template;
                });
                return dict;
            });
        }

        public void CacheTemplate(ICompiledTemplate template, ITemplateKey templateKey)
        {
            var modelTypeKey = GetModelTypeKey(template.ModelType);
            CacheTemplateHelper(template, templateKey, modelTypeKey);
            if (template.TemplateType.BaseType.GenericTypeArguments.Length > 0)
            {
                var alternativeKey = GetModelTypeKey(template.TemplateType.BaseType.GenericTypeArguments[0]);
                if (alternativeKey != modelTypeKey)
                {
                    // could be a template with an @model directive.
                    CacheTemplateHelper(template, templateKey, template.TemplateType.BaseType.GenericTypeArguments[0]);
                }
            }
        }

        public bool TryRetrieveTemplate(ITemplateKey templateKey, Type modelType, out ICompiledTemplate compiledTemplate)
        {
            compiledTemplate = null;
            var uniqueKey = templateKey.GetUniqueKeyString();
            var modelTypeKey = GetModelTypeKey(modelType);
            ConcurrentDictionary<Type, ICompiledTemplate> dict;
            if (!_cache.TryGetValue(uniqueKey, out dict))
            {
                return false;
            }
            return dict.TryGetValue(modelTypeKey, out compiledTemplate);
        }


        public void Dispose()
        {
            _loader.Dispose();
        }
    }
}
