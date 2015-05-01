using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// The default caching provider (See <see cref="ICachingProvider"/>).
    /// This implementation does a very simple in-memory caching.
    /// It can handle when the same template is used with multiple model-types.
    /// </summary>
    public class DefaultCachingProvider : ICachingProvider
    {
        /// <summary>
        /// We wrap it without calling any memory leaking API.
        /// </summary>
        private InvalidatingCachingProvider inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCachingProvider"/> class.
        /// </summary>
        public DefaultCachingProvider() : this(null)
        {
            inner = new InvalidatingCachingProvider();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCachingProvider"/> class.
        /// </summary>
        /// <param name="registerForCleanup">callback for files which need to be cleaned up.</param>
        public DefaultCachingProvider(Action<string> registerForCleanup)
        {
            inner = new InvalidatingCachingProvider(registerForCleanup);
        }

        /// <summary>
        /// The manages <see cref="TypeLoader"/>. See <see cref="ICachingProvider.TypeLoader"/>
        /// </summary>
        public TypeLoader TypeLoader
        {
            get
            {
                return inner.TypeLoader;
            }
        }

        /// <summary>
        /// Get the key used within a dictionary for a modelType.
        /// </summary>
        public static Type GetModelTypeKey(Type modelType)
        {
            return InvalidatingCachingProvider.GetModelTypeKey(modelType);
        }

        /// <summary>
        /// Caches a template. See <see cref="ICachingProvider.CacheTemplate"/>.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="templateKey"></param>
        public void CacheTemplate(ICompiledTemplate template, ITemplateKey templateKey)
        {
            inner.CacheTemplate(template, templateKey);
        }

        /// <summary>
        /// Try to retrieve a template from the cache. See <see cref="ICachingProvider.TryRetrieveTemplate"/>.
        /// </summary>
        /// <param name="templateKey"></param>
        /// <param name="modelType"></param>
        /// <param name="compiledTemplate"></param>
        /// <returns></returns>
        public bool TryRetrieveTemplate(ITemplateKey templateKey, Type modelType, out ICompiledTemplate compiledTemplate)
        {
            return inner.TryRetrieveTemplate(templateKey, modelType, out compiledTemplate);
        }

        /// <summary>
        /// Dispose the instance.
        /// </summary>
        public void Dispose()
        {
            inner.Dispose();
        }
    }
}
