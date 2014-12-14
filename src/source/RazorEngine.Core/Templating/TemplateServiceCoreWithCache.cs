using RazorEngine.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    internal class TemplateServiceCoreWithCache : TemplateServiceCore
    {
        internal TemplateServiceCoreWithCache(ITemplateServiceConfiguration config, CachedTemplateService cached)
            : base(config, cached)
        {
        }

        internal override ITemplate ResolveInternal(string cacheName, object model, ResolveType resolveType, ICompiledTemplate context)
        {
            var templateKey = GetKey(cacheName, resolveType, context);
            ICompiledTemplate compiledTemplate;
            if (!Configuration.CachingProvider.TryRetrieveTemplate(templateKey, GetTypeFromModelObject(model), out compiledTemplate))
            {
                compiledTemplate = Compile(templateKey, GetTypeFromModelObject(model));
                Configuration.CachingProvider.CacheTemplate(compiledTemplate, templateKey);
            }
            return CreateTemplate(compiledTemplate, model);
        }
    }
}
