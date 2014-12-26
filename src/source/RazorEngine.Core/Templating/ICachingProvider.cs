using System;
namespace RazorEngine.Templating
{
    /// <summary>
    /// This interface represents the caching layer.
    /// </summary>
    public interface ICachingProvider : IDisposable
    {
        /// <summary>
        /// Request that a given template should be cached.
        /// </summary>
        /// <param name="template">The template to be cached.</param>
        /// <param name="key">The key of the template.</param>
        void CacheTemplate(ICompiledTemplate template, ITemplateKey key);

        /// <summary>
        /// Try to resolve a template within the cache.
        /// </summary>
        /// <param name="key">the key of the template.</param>
        /// <param name="modelType">the model-type of the template.</param>
        /// <param name="template">the resolved template</param>
        /// <returns>true if a template was found.</returns>
        /// <remarks>
        /// Implementations MUST decide if they allow multiple model-types for the 
        /// same template key and SHOULD throw a exception when a template is requested with the wrong type!
        /// </remarks>
        bool TryRetrieveTemplate(ITemplateKey key, Type modelType, out ICompiledTemplate template);

        /// <summary>
        /// Every caching provider must manage a <see cref="TypeLoader"/> instance.
        /// This instance makes sure that all assemblies can be resolved properly.
        /// </summary>
        TypeLoader TypeLoader { get; }
    }
}
