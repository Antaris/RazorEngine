using RazorEngine.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    public static class TemplateServiceExtensions
    {
        public static ITemplateKey GetKey(this ICachedTemplateService service, string name, ResolveType resolveType = ResolveType.Global, ICompiledTemplate context = null)
        {
            return service.Core.GetKey(name, resolveType, context);
        }

        public static void AddTemplate(this ICachedTemplateService service, string name, ITemplateSource source)
        {
            service.Core.Configuration.TemplateManager.AddDynamic(service.GetKey(name), source);
        }

        public static void AddTemplate(this ICachedTemplateService service, string name, string templateSource)
        {
            service.AddTemplate(name, new TemplateSource(templateSource, name));
        }

        public static ICompiledTemplate CompileAndCache(this ICachedTemplateService service, string name, Type modelType)
        {
            return service.CompileAndCache(service.GetKey(name), modelType);
        }

        public static void RunCompileOnDemand(this ICachedTemplateService service, string name, Type modelType, TextWriter writer, object model, DynamicViewBag viewBag)
        {
            service.RunCompileOnDemand(service.GetKey(name), modelType, writer, model, viewBag);
        }

        public static void RunCachedTemplate(this ICachedTemplateService service, string name, Type modelType, TextWriter writer, object model, DynamicViewBag viewBag)
        {
            service.RunCachedTemplate(service.GetKey(name), modelType, writer, model, viewBag);
        }


        public static string RunCompileOnDemand(this ICachedTemplateService service, string name, Type modelType, object model, DynamicViewBag viewBag)
        {
            using(var writer = new System.IO.StringWriter())
	        {
                service.RunCompileOnDemand(name, modelType, writer, model, viewBag);
                return writer.ToString();
            }
        }

        public static string RunCachedTemplate(this ICachedTemplateService service, string name, Type modelType, object model, DynamicViewBag viewBag)
        {
            using(var writer = new System.IO.StringWriter())
	        {
                service.RunCachedTemplate(name, modelType, writer, model, viewBag);
                return writer.ToString();
            }
        }

        public static string RunCompileOnDemand(this ICachedTemplateService service, ITemplateKey key, Type modelType, object model, DynamicViewBag viewBag)
        {
            using (var writer = new System.IO.StringWriter())
            {
                service.RunCompileOnDemand(key, modelType, writer, model, viewBag);
                return writer.ToString();
            }
        }

        public static string RunCachedTemplate(this ICachedTemplateService service, ITemplateKey key, Type modelType, object model, DynamicViewBag viewBag)
        {
            using (var writer = new System.IO.StringWriter())
            {
                service.RunCachedTemplate(key, modelType, writer, model, viewBag);
                return writer.ToString();
            }
        }
    }
}
