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
    /// <summary>
    /// Provides quick access to the functionality of the <see cref="RazorEngineService"/> class.
    /// </summary>
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

        /// <summary>
        /// Gets a given key from the <see cref="ITemplateManager"/> implementation.
        /// See <see cref="ITemplateManager.GetKey"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resolveType"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ITemplateKey GetKey(string name, ResolveType resolveType = ResolveType.Global, ITemplateKey context = null)
        {
            return _service.GetKey(name, resolveType, context);
        }

        /// <summary>
        /// Checks if a given template is already cached.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static bool IsTemplateCached(ITemplateKey key, Type modelType)
        {
            return _service.IsTemplateCached(key, modelType);
        }

        /// <summary>
        /// Checks if a given template is already cached.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static bool IsTemplateCached(string name, Type modelType)
        {
            return _service.IsTemplateCached(name, modelType);
        }

        /// <summary>
        /// Adds a given template to the template manager as dynamic template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="templateSource"></param>
        public static void AddTemplate(ITemplateKey key, ITemplateSource templateSource)
        {
            _service.AddTemplate(key, templateSource);
        }

        /// <summary>
        /// Adds a given template to the template manager as dynamic template.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="templateSource"></param>
        public static void AddTemplate(string name, ITemplateSource templateSource)
        {
            _service.AddTemplate(name, templateSource);
        }

        /// <summary>
        /// Adds a given template to the template manager as dynamic template.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="templateSource"></param>
        public static void AddTemplate(ITemplateKey key, string templateSource)
        {
            _service.AddTemplate(key, templateSource);
        }

        /// <summary>
        /// Adds a given template to the template manager as dynamic template.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="templateSource"></param>
        public static void AddTemplate(string name, string templateSource)
        {
            _service.AddTemplate(name, templateSource);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Compile"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        public static void Compile(ITemplateKey key, Type modelType = null)
        {
            _service.Compile(key, modelType);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Compile"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        public static void Compile(string name, Type modelType = null)
        {
            _service.Compile(name, modelType);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Compile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.Compile"/>.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        public static void Compile(ITemplateSource templateSource, ITemplateKey key, Type modelType = null)
        {
            _service.Compile(templateSource, key, modelType);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Compile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.Compile"/>.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        public static void Compile(string templateSource, ITemplateKey key, Type modelType = null)
        {
            _service.Compile(templateSource, key, modelType);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Compile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.Compile"/>.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        public static void Compile(ITemplateSource templateSource, string name, Type modelType = null)
        {
            _service.Compile(templateSource, name, modelType);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Compile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.Compile"/>.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        public static void Compile(string templateSource, string name, Type modelType = null)
        {
            _service.Compile(templateSource, name, modelType);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void RunCompile(ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompile(key, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void RunCompile(string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompile(name, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void RunCompile(ITemplateSource templateSource, ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompile(templateSource, key, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void RunCompile(string templateSource, ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompile(templateSource, key, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void RunCompile(ITemplateSource templateSource, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompile(templateSource, name, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void RunCompile(string templateSource, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.RunCompile(templateSource, name, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static string RunCompile(ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompile(key, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static string RunCompile(string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompile(name, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static string RunCompile(ITemplateSource templateSource, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompile(templateSource, key, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static string RunCompile(string templateSource, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompile(templateSource, key, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static string RunCompile(ITemplateSource templateSource, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompile(templateSource, name, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static string RunCompile(string templateSource, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.RunCompile(templateSource, name, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Run"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void Run(ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.Run(key, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Run"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void Run(string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            _service.Run(name, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Run"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static string Run(ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.Run(key, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Run"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static string Run(string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return _service.Run(name, modelType, model, viewBag);
        }

        /// <summary>
        /// Sets the backend service for the static methods within the <see cref="RazorService"/> class.
        /// </summary>
        /// <param name="service">the new backend service.</param>
        public static void SetRazorEngine(IRazorEngineService service)
        {
            Contract.Requires(service != null);

            _service = service;
        }

    }
}
