using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine
{
    public static class RazorService
    {
        private static IRazorEngineService _service = RazorEngineService.Create();

        private static IRazorEngineService RazorEngine
        {
            get
            {
                return _service;
            }
        }

        public static void CompileAndCache(ITemplateKey key, Type modelType = null)
        {
            _service.CompileAndCache(key, modelType);
        }

        public static void CompileAndCache<T>(ITemplateKey key)
        {
            _service.CompileAndCache<T>(key);
        }

        public static void CompileAndCache(string name, Type modelType = null)
        {
            _service.CompileAndCache(name, modelType);
        }

        public static void CompileAndCache<T>(string name)
        {
            _service.CompileAndCache<T>(name);
        }

        public static void CompileAndCache(ITemplateSource templateSource, ITemplateKey key, Type modelType = null)
        {
            _service.CompileAndCache(templateSource, key, modelType);
        }

        public static void CompileAndCache<T>(ITemplateSource templateSource, ITemplateKey key)
        {
            _service.CompileAndCache<T>(templateSource, key);
        }

        public static void CompileAndCache(string templateSource, ITemplateKey key, Type modelType = null)
        {
            _service.CompileAndCache(templateSource, key, modelType);
        }

        public static void CompileAndCache<T>(string templateSource, ITemplateKey key)
        {
            _service.CompileAndCache<T>(templateSource, key);
        }

        public static void CompileAndCache(ITemplateSource templateSource, string name, Type modelType = null)
        {
            _service.CompileAndCache(templateSource, name, modelType);
        }

        public static void CompileAndCache<T>(ITemplateSource templateSource, string name)
        {
            _service.CompileAndCache<T>(templateSource, name);
        }

        public static void CompileAndCache(string templateSource, string name, Type modelType = null)
        {
            _service.CompileAndCache(templateSource, name, modelType);
        }

        public static void CompileAndCache<T>(string templateSource, string name)
        {
            _service.CompileAndCache<T>(templateSource, name);
        }


        public static void RunCompileOnDemand(ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand(key, writer, modelType, model, viewBag);
        }

        public static void RunCompileOnDemand<T>(ITemplateKey key, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand<T>(key, writer, model, viewBag);
        }

        public static void RunCompileOnDemand(string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand(name, writer, modelType, model, viewBag);
        }

        public static void RunCompileOnDemand<T>(string name, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand<T>(name, writer, model, viewBag);
        }

        public static void RunCompileOnDemand(ITemplateSource templateSource, ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand(templateSource, key, writer, modelType, model, viewBag);
        }

        public static void RunCompileOnDemand<T>(ITemplateSource templateSource, ITemplateKey key, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand<T>(templateSource, key, writer, model, viewBag);
        }

        public static void RunCompileOnDemand(string templateSource, ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand(templateSource, key, writer, modelType, model, viewBag);
        }

        public static void RunCompileOnDemand<T>(string templateSource, ITemplateKey key, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand<T>(templateSource, key, writer, model, viewBag);
        }

        public static void RunCompileOnDemand(ITemplateSource templateSource, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand(templateSource, name, writer, modelType, model, viewBag);
        }

        public static void RunCompileOnDemand<T>(ITemplateSource templateSource, string name, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand<T>(templateSource, name, writer, model, viewBag);
        }

        public static void RunCompileOnDemand(string templateSource, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand(templateSource, name, writer, modelType, model, viewBag);
        }

        public static void RunCompileOnDemand<T>(string templateSource, string name, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            _service.RunCompileOnDemand<T>(templateSource, name, writer, model, viewBag);
        }


        public static string RunCompileOnDemand(ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand(key, modelType, model, viewBag);
        }

        public static string RunCompileOnDemand<T>(ITemplateKey key, T model, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand<T>(key, model, viewBag);
        }

        public static string RunCompileOnDemand(string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand(name, modelType, model, viewBag);
        }

        public static string RunCompileOnDemand<T>(string name, T model, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand<T>(name, model, viewBag);
        }

        public static string RunCompileOnDemand(ITemplateSource templateSource, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand(templateSource, key, modelType, model, viewBag);
        }

        public static string RunCompileOnDemand<T>(ITemplateSource templateSource, ITemplateKey key, T model, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand<T>(templateSource, key, model, viewBag);
        }

        public static string RunCompileOnDemand(string templateSource, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand(templateSource, key, modelType, model, viewBag);
        }

        public static string RunCompileOnDemand<T>(string templateSource, ITemplateKey key, T model, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand<T>(templateSource, key, model, viewBag);
        }

        public static string RunCompileOnDemand(ITemplateSource templateSource, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand(templateSource, name, modelType, model, viewBag);
        }

        public static string RunCompileOnDemand<T>(ITemplateSource templateSource, string name, T model, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand<T>(templateSource, name, model, viewBag);
        }

        public static string RunCompileOnDemand(string templateSource, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand(templateSource, name, modelType, model, viewBag);
        }

        public static string RunCompileOnDemand<T>(string templateSource, string name, T model, DynamicViewBag viewBag = null)
        {
            return _service.RunCompileOnDemand<T>(templateSource, name, model, viewBag);
        }



        public static void RunCachedTemplate(ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCachedTemplate(key, writer, modelType, model, viewBag);
        }

        public static void RunCachedTemplate<T>(ITemplateKey key, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            _service.RunCachedTemplate<T>(key, writer, model, viewBag);
        }

        public static void RunCachedTemplate(string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCachedTemplate(name, writer, modelType, model, viewBag);
        }

        public static void RunCachedTemplate<T>(string name, TextWriter writer, T model, DynamicViewBag viewBag = null)
        {
            _service.RunCachedTemplate<T>(name, writer, model, viewBag);
        }




        public static string RunCachedTemplate(ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCachedTemplate(key, modelType, model, viewBag);
        }

        public static string RunCachedTemplate<T>(ITemplateKey key, T model, DynamicViewBag viewBag = null)
        {
            return _service.RunCachedTemplate<T>(key, model, viewBag);
        }


        public static string RunCachedTemplate(string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCachedTemplate(name, modelType, model, viewBag);
        }

        public static string RunCachedTemplate<T>(string name, T model, DynamicViewBag viewBag = null)
        {
            return _service.RunCachedTemplate<T>(name, model, viewBag);
        }

        public static void SetRazorEngine(IRazorEngineService service)
        {
            Contract.Requires(service != null);

            _service = service;
        }

    }
}
