using System.IO;

namespace RazorEngine.Templating
{
    /// <summary>
    /// Extensions for the <see cref="ITemplateRunner{TModel}"/>.
    /// </summary>
    public static class TemplateRunnerExtensions
    {
        /// <summary>
        /// Runs the template using the specified <paramref name="model"/>.
        /// </summary>
        /// <param name="templateRunner"></param>
        /// <param name="model"></param>
        /// <param name="viewBag"></param>
        /// <returns></returns>
        public static string Run<TModel>(this ITemplateRunner<TModel> templateRunner, TModel model, DynamicViewBag viewBag = null)
        {
            using (var textWriter = new StringWriter())
            {
                templateRunner.Run(model, textWriter, viewBag);
                return textWriter.ToString();
            }
        }
    }
}
