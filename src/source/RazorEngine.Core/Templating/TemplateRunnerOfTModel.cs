using System.IO;

namespace RazorEngine.Templating
{
    /// <summary>
    /// Typed, runnable template reference.
    /// </summary>
    /// <typeparam name="TModel">The model type</typeparam>
    internal class TemplateRunner<TModel> : ITemplateRunner<TModel>
    {
        private readonly IRazorEngineService _service;
        private readonly ITemplateKey _key;

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateRunner{TModel}"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="key"></param>
        public TemplateRunner(IRazorEngineService service, ITemplateKey key)
        {
            _service = service;
            _key = key;
        }

        /// <summary>
        /// Runs the template using the specified <paramref name="model"/>.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="textWriter"></param>
        /// <param name="viewBag"></param>
        public void Run(TModel model, TextWriter textWriter, DynamicViewBag viewBag = null)
        {
            _service.Run(_key, textWriter, typeof(TModel), model, viewBag);
        }
    }
}