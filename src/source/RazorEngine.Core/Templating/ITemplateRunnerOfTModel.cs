using System.IO;

namespace RazorEngine.Templating
{
    /// <summary>
    /// Defines the required contract for implementing a typed, runnable template reference.
    /// </summary>
    /// <typeparam name="TModel">The model type</typeparam>
    public interface ITemplateRunner<TModel>
    {
        /// <summary>
        /// Runs the template using the specified <paramref name="model"/>.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="textWriter"></param>
        /// <param name="viewBag"></param>
        void Run(TModel model, TextWriter textWriter, DynamicViewBag viewBag = null);
    }
}