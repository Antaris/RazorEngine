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
    public static class RazorEngineServiceExtensions
    {
        public static bool IsTemplateCached(this IRazorEngineService service, ITemplateKey key, Type modelType)
        {
            ICompiledTemplate template;
            return service.Configuration.CachingProvider.TryRetrieveTemplate(key, modelType, out template);
        }

        public static bool IsTemplateCached(this IRazorEngineService service, string name, Type modelType)
        {
            var key = service.GetKey(name);
            return service.IsTemplateCached(key, modelType);
        }

        public static void AddTemplate(this IRazorEngineService service, ITemplateKey key, ITemplateSource templateSource)
        {
            service.Configuration.TemplateManager.AddDynamic(key, templateSource);
        }

        public static void AddTemplate(this IRazorEngineService service, string name, ITemplateSource templateSource)
        {
            var key = service.GetKey(name);
            service.AddTemplate(key, templateSource);
        }

        public static void AddTemplate(this IRazorEngineService service, ITemplateKey key, string templateSource)
        {
            service.AddTemplate(key, new LoadedTemplateSource(templateSource));
        }

        public static void AddTemplate(this IRazorEngineService service, string name, string templateSource)
        {
            service.AddTemplate(name, new LoadedTemplateSource(templateSource));
        }

        public static void CompileAndCache<T>(this IRazorEngineService service, ITemplateKey key)
        {
            service.CompileAndCache(key, typeof(T));
        }

        public static void CompileAndCache(this IRazorEngineService service, string name, Type modelType = null)
        {
            service.CompileAndCache(service.GetKey(name), modelType);
        }

        public static void CompileAndCache<T>(this IRazorEngineService service, string name)
        {
            service.CompileAndCache(service.GetKey(name), typeof(T));
        }

        public static void CompileAndCache(this IRazorEngineService service, ITemplateSource templateSource, ITemplateKey key, Type modelType = null)
        {
            service.AddTemplate(key, templateSource);
            service.CompileAndCache(key, modelType);
        }
        public static void CompileAndCache<T>(this IRazorEngineService service, ITemplateSource templateSource, ITemplateKey key)
        {
            service.CompileAndCache(templateSource, key, typeof(T));
        }

        public static void CompileAndCache(this IRazorEngineService service, string templateSource, ITemplateKey key, Type modelType = null)
        {
            service.AddTemplate(key, templateSource);
            service.CompileAndCache(key, modelType);
        }

        public static void CompileAndCache<T>(this IRazorEngineService service, string templateSource, ITemplateKey key)
        {
            service.CompileAndCache(templateSource, key, typeof(T));
        }

        public static void CompileAndCache(this IRazorEngineService service, ITemplateSource templateSource, string name, Type modelType = null)
        {
            service.AddTemplate(name, templateSource);
            service.CompileAndCache(name, modelType);
        }

        public static void CompileAndCache<T>(this IRazorEngineService service, ITemplateSource templateSource, string name)
        {
            service.CompileAndCache(templateSource, name, typeof(T));
        }

        public static void CompileAndCache(this IRazorEngineService service, string templateSource, string name, Type modelType = null)
        {
            service.AddTemplate(name, templateSource);
            service.CompileAndCache(name, modelType);
        }

        public static void CompileAndCache<T>(this IRazorEngineService service, string templateSource, string name)
        {
            service.CompileAndCache(templateSource, name, typeof(T));
        }


        public static void RunCompileOnDemand<T>(this IRazorEngineService service, ITemplateKey key, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            service.RunCompileOnDemand(key, writer, typeof(T), model, viewBag);
        }

        public static void RunCompileOnDemand(this IRazorEngineService service, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.RunCompileOnDemand(service.GetKey(name), writer, modelType, model, viewBag);
        }

        public static void RunCompileOnDemand<T>(this IRazorEngineService service, string name, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            service.RunCompileOnDemand(name, writer, typeof(T), model, viewBag);
        }

        public static void RunCompileOnDemand(this IRazorEngineService service, ITemplateSource templateSource, ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(key, templateSource);
            service.RunCompileOnDemand(key, writer, modelType, model, viewBag);
        }
        public static void RunCompileOnDemand<T>(this IRazorEngineService service, ITemplateSource templateSource, ITemplateKey key, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            service.RunCompileOnDemand(templateSource, key, writer, typeof(T), model, viewBag);
        }

        public static void RunCompileOnDemand(this IRazorEngineService service, string templateSource, ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(key, templateSource);
            service.RunCompileOnDemand(key, writer, modelType, model, viewBag);
        }

        public static void RunCompileOnDemand<T>(this IRazorEngineService service, string templateSource, ITemplateKey key, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            service.RunCompileOnDemand(templateSource, key, writer, typeof(T), model, viewBag);
        }

        public static void RunCompileOnDemand(this IRazorEngineService service, ITemplateSource templateSource, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(name, templateSource);
            service.RunCompileOnDemand(name, writer, modelType, model, viewBag);
        }

        public static void RunCompileOnDemand<T>(this IRazorEngineService service, ITemplateSource templateSource, string name, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            service.RunCompileOnDemand(templateSource, name, writer, typeof(T), model, viewBag);
        }

        public static void RunCompileOnDemand(this IRazorEngineService service, string templateSource, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(name, templateSource);
            service.RunCompileOnDemand(name, writer, modelType, model, viewBag);
        }

        public static void RunCompileOnDemand<T>(this IRazorEngineService service, string templateSource, string name, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            service.RunCompileOnDemand(templateSource, name, writer, typeof(T), model, viewBag);
        }

        private static string WithWriter(Action<TextWriter> withWriter)
        {
            using (var writer = new System.IO.StringWriter())
            {
                withWriter(writer);
                return writer.ToString();
            }
        }

        public static string RunCompileOnDemand(this IRazorEngineService service, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return WithWriter(writer => service.RunCompileOnDemand(key, writer, modelType, model, viewBag));
        }

        public static string RunCompileOnDemand<T>(this IRazorEngineService service, ITemplateKey key, T model, DynamicViewBag viewBag = null)
        {
            return service.RunCompileOnDemand(key, typeof(T), model, viewBag);
        }

        public static string RunCompileOnDemand(this IRazorEngineService service, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return WithWriter(writer => service.RunCompileOnDemand(name, writer, modelType, model, viewBag));
        }

        public static string RunCompileOnDemand<T>(this IRazorEngineService service, string name, T model, DynamicViewBag viewBag = null)
        {
            return service.RunCompileOnDemand(name, typeof(T), model, viewBag);
        }


        public static string RunCompileOnDemand(this IRazorEngineService service, ITemplateSource templateSource, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(key, templateSource);
            return service.RunCompileOnDemand(key, modelType, model, viewBag);
        }

        public static string RunCompileOnDemand<T>(this IRazorEngineService service, ITemplateSource templateSource, ITemplateKey key, T model, DynamicViewBag viewBag = null)
        {
            return service.RunCompileOnDemand(templateSource, key, typeof(T), model, viewBag);
        }

        public static string RunCompileOnDemand(this IRazorEngineService service, string templateSource, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(key, templateSource);
            return service.RunCompileOnDemand(key, modelType, model, viewBag);
        }
        public static string RunCompileOnDemand<T>(this IRazorEngineService service, string templateSource, ITemplateKey key, T model, DynamicViewBag viewBag = null)
        {
            return service.RunCompileOnDemand(templateSource, key, typeof(T), model, viewBag);
        }

        public static string RunCompileOnDemand(this IRazorEngineService service, ITemplateSource templateSource, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(name, templateSource);
            return service.RunCompileOnDemand(name, modelType, model, viewBag);
        }

        public static string RunCompileOnDemand<T>(this IRazorEngineService service, ITemplateSource templateSource, string name, T model, DynamicViewBag viewBag = null)
        {
            return service.RunCompileOnDemand(templateSource, name, typeof(T), model, viewBag);
        }

        public static string RunCompileOnDemand(this IRazorEngineService service, string templateSource, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(name, templateSource);
            return service.RunCompileOnDemand(name, modelType, model, viewBag);
        }

        public static string RunCompileOnDemand<T>(this IRazorEngineService service, string templateSource, string name, T model, DynamicViewBag viewBag = null)
        {
            return service.RunCompileOnDemand(templateSource, name, typeof(T), model, viewBag);
        }



        public static void RunCachedTemplate<T>(this IRazorEngineService service, ITemplateKey key, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            service.RunCachedTemplate(key, writer, typeof(T), model, viewBag);
        }

        public static void RunCachedTemplate(this IRazorEngineService service, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.RunCachedTemplate(service.GetKey(name), writer, modelType, model, viewBag);
        }

        public static void RunCachedTemplate<T>(this IRazorEngineService service, string name, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            service.RunCachedTemplate(name, writer, typeof(T), model, viewBag);
        }

        public static string RunCachedTemplate(this IRazorEngineService service, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return WithWriter(writer => service.RunCachedTemplate(key, writer, modelType, model, viewBag));
        }

        public static string RunCachedTemplate<T>(this IRazorEngineService service, ITemplateKey key, T model, DynamicViewBag viewBag = null)
        {
            return service.RunCachedTemplate(key, typeof(T), model, viewBag);
        }

        public static string RunCachedTemplate(this IRazorEngineService service, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return WithWriter(writer => service.RunCachedTemplate(name, writer, modelType, model, viewBag));
        }

        public static string RunCachedTemplate<T>(this IRazorEngineService service, string name, T model, DynamicViewBag viewBag = null)
        {
            return service.RunCachedTemplate(name, typeof(T), model, viewBag);
        }
    }
}
