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
    /// <summary>
    /// Extensions for the <see cref="IRazorEngineService"/>.
    /// </summary>
    public static class RazorEngineServiceExtensions
    {
        /// <summary>
        /// Checks if a given template is already cached.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static bool IsTemplateCached(this IRazorEngineService service, string name, Type modelType)
        {
            var key = service.GetKey(name);
            return service.IsTemplateCached(key, modelType);
        }

        /// <summary>
        /// Adds a given template to the template manager as dynamic template.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="name"></param>
        /// <param name="templateSource"></param>
        public static void AddTemplate(this IRazorEngineService service, string name, ITemplateSource templateSource)
        {
            var key = service.GetKey(name);
            service.AddTemplate(key, templateSource);
        }

        /// <summary>
        /// Adds a given template to the template manager as dynamic template.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="key"></param>
        /// <param name="templateSource"></param>
        public static void AddTemplate(this IRazorEngineService service, ITemplateKey key, string templateSource)
        {
            service.AddTemplate(key, new LoadedTemplateSource(templateSource));
        }

        /// <summary>
        /// Adds a given template to the template manager as dynamic template.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="name"></param>
        /// <param name="templateSource"></param>
        public static void AddTemplate(this IRazorEngineService service, string name, string templateSource)
        {
            service.AddTemplate(name, new LoadedTemplateSource(templateSource));
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Compile"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        public static void Compile(this IRazorEngineService service, string name, Type modelType = null)
        {
            service.Compile(service.GetKey(name), modelType);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Compile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.Compile"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        public static void Compile(this IRazorEngineService service, ITemplateSource templateSource, ITemplateKey key, Type modelType = null)
        {
            service.AddTemplate(key, templateSource);
            service.Compile(key, modelType);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Compile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.Compile"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        public static void Compile(this IRazorEngineService service, string templateSource, ITemplateKey key, Type modelType = null)
        {
            service.AddTemplate(key, templateSource);
            service.Compile(key, modelType);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Compile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.Compile"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        public static void Compile(this IRazorEngineService service, ITemplateSource templateSource, string name, Type modelType = null)
        {
            service.AddTemplate(name, templateSource);
            service.Compile(name, modelType);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Compile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.Compile"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        public static void Compile(this IRazorEngineService service, string templateSource, string name, Type modelType = null)
        {
            service.AddTemplate(name, templateSource);
            service.Compile(name, modelType);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="name"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void RunCompile(this IRazorEngineService service, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.RunCompile(service.GetKey(name), writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void RunCompile(this IRazorEngineService service, ITemplateSource templateSource, ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(key, templateSource);
            service.RunCompile(key, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void RunCompile(this IRazorEngineService service, string templateSource, ITemplateKey key, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(key, templateSource);
            service.RunCompile(key, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void RunCompile(this IRazorEngineService service, ITemplateSource templateSource, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(name, templateSource);
            service.RunCompile(name, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void RunCompile(this IRazorEngineService service, string templateSource, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(name, templateSource);
            service.RunCompile(name, writer, modelType, model, viewBag);
        }

        /// <summary>
        /// Helper method to provide a TextWriter and return the written data.
        /// </summary>
        /// <param name="withWriter"></param>
        /// <returns></returns>
        private static string WithWriter(Action<TextWriter> withWriter)
        {
            using (var writer = new System.IO.StringWriter())
            {
                withWriter(writer);
                return writer.ToString();
            }
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public static string RunCompile(this IRazorEngineService service, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return WithWriter(writer => service.RunCompile(key, writer, modelType, model, viewBag));
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public static string RunCompile(this IRazorEngineService service, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return WithWriter(writer => service.RunCompile(name, writer, modelType, model, viewBag));
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public static string RunCompile(this IRazorEngineService service, ITemplateSource templateSource, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(key, templateSource);
            return service.RunCompile(key, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public static string RunCompile(this IRazorEngineService service, string templateSource, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(key, templateSource);
            return service.RunCompile(key, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public static string RunCompile(this IRazorEngineService service, ITemplateSource templateSource, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(name, templateSource);
            return service.RunCompile(name, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which calls <see cref="RazorEngineService.AddTemplate"/> before calling <see cref="RazorEngineService.RunCompile"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public static string RunCompile(this IRazorEngineService service, string templateSource, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.AddTemplate(name, templateSource);
            return service.RunCompile(name, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Run"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="name"></param>
        /// <param name="writer"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        public static void Run(this IRazorEngineService service, string name, TextWriter writer, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            service.Run(service.GetKey(name), writer, modelType, model, viewBag);
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Run"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="key"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public static string Run(this IRazorEngineService service, ITemplateKey key, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return WithWriter(writer => service.Run(key, writer, modelType, model, viewBag));
        }

        /// <summary>
        /// See <see cref="RazorEngineService.Run"/>.
        /// Convenience method which creates a <see cref="TextWriter"/> and returns the result as string.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="name"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public static string Run(this IRazorEngineService service, string name, Type modelType = null, object model = null, DynamicViewBag viewBag = null)
        {
            return WithWriter(writer => service.Run(name, writer, modelType, model, viewBag));
        }

        /// <summary>
        /// Adds and compiles a new template using the specified <paramref name="templateSource"/> and returns a <see cref="TemplateRunner{TModel}"/>.
        /// </summary>
        /// <typeparam name="TModel">The model type</typeparam>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <returns></returns>
        public static ITemplateRunner<TModel> CompileRunner<TModel>(this IRazorEngineService service, string templateSource)
        {
            var name = $"{typeof(TModel).Name}_{Guid.NewGuid()}";
            return service.CompileRunner<TModel>(templateSource, name);
        }

        /// <summary>
        /// Adds and compiles a new template using the specified <paramref name="templateSource"/> and returns a <see cref="TemplateRunner{TModel}"/>.
        /// </summary>
        /// <typeparam name="TModel">The model type</typeparam>
        /// <param name="service"></param>
        /// <param name="templateSource"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ITemplateRunner<TModel> CompileRunner<TModel>(this IRazorEngineService service, string templateSource, string name)
        {
            var key = service.GetKey(name);

            service.AddTemplate(key, templateSource);
            service.Compile(key, typeof(TModel));

            return new TemplateRunner<TModel>(service, key);
        }
    }
}