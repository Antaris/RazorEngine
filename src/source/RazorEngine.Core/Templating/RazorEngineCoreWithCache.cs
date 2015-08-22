using RazorEngine.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    internal class RazorEngineCoreWithCache : RazorEngineCore
    {
        internal RazorEngineCoreWithCache(ReadOnlyTemplateServiceConfiguration config, RazorEngineService cached)
            : base(config, cached)
        {
        }

        internal override ITemplate ResolveInternal(string cacheName, object model, Type modelType, DynamicViewBag viewbag, ResolveType resolveType, ITemplateKey context)
        {
            var templateKey = GetKey(cacheName, resolveType, context);
            ICompiledTemplate compiledTemplate;
            if (!Configuration.CachingProvider.TryRetrieveTemplate(templateKey, modelType, out compiledTemplate))
            {
                compiledTemplate = Compile(templateKey, modelType);
                Configuration.CachingProvider.CacheTemplate(compiledTemplate, templateKey);
            }
            return CreateTemplate(compiledTemplate, model, viewbag);
        }
    }
}
