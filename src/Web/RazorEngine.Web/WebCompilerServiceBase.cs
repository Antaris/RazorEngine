using System.Reflection;
using RazorEngine.Templating;

namespace RazorEngine.Web
{
    using System;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Razor;
    using System.Web.Razor.Parser;

    using Compilation;

    /// <summary>
    /// Defines a compiler service that used the ASP.NET BuildProvider system.
    /// </summary>
    public abstract class WebCompilerServiceBase : CompilerServiceBase
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="WebCompilerServiceBase"/>.
        /// </summary>
        /// <param name="virtualFileExtension">The virtual file extensions.</param>
        /// <param name="codeLanguage">The razor code language.</param>
        /// <param name="markupParser">The markup parser.</param>
        protected WebCompilerServiceBase(string virtualFileExtension, RazorCodeLanguage codeLanguage, Func<MarkupParser> markupParser) : base(codeLanguage, markupParser)
        {
            if (string.IsNullOrWhiteSpace(virtualFileExtension))
                throw new ArgumentException("Virtual file extension is required.");

            VirtualFileExtension = virtualFileExtension;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the file extension for the virtual file.
        /// </summary>
        public string VirtualFileExtension { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Compiles the type defined in the specified type context.
        /// </summary>
        /// <param name="context">The type context which defines the type to compile.</param>
        /// <returns>The compiled type.</returns>
        public override Tuple<Type, Assembly> CompileType(TypeContext context)
        {
            string virtualFile = "~/__razor/" + context.ClassName + "." + VirtualFileExtension;
            HttpContext.Current.Items.Add(context.ClassName, context);

            var assembly = BuildManager.GetCompiledAssembly(virtualFile);
            
            
            return Tuple.Create(
                assembly.GetType("CompiledRazorTemplates.Dynamic." + context.ClassName),
                assembly);

            //var t =  BuildManager.GetCompiledType(virtualFile);
            //if (context == null) throw new ArgumentNullException("context");

            //var result = Compile(context);
            //var compileResult = result.Item1;

            //if (compileResult.Errors != null && compileResult.Errors.Count > 0)
            //    throw new TemplateCompilationException(compileResult.Errors, result.Item2, context.TemplateContent);

            //return Tuple.Create(
            //    compileResult.CompiledAssembly.GetType("CompiledRazorTemplates.Dynamic." + context.ClassName),
            //    compileResult.CompiledAssembly);
        }
        #endregion
    }
}