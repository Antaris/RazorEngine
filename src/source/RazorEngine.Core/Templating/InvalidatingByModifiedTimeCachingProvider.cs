using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// An memory leaking invalidating caching provider (See <see cref="ICachingProvider"/>).
    /// This implementation does a very simple in-memory caching and allows you to release templates
    /// by trading with memory. File modification time is used to check if cached template is valid.
    /// WARNING:
    /// Use this caching provider only on AppDomains you recycle regularly, or to
    /// improve the debugging experience.
    /// Never use this in production without any recycle strategy.
    /// </summary>
    public class InvalidatingByModifiedTimeCachingProvider : InvalidatingCachingProvider, ICachingProvider
    {
        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public InvalidatingByModifiedTimeCachingProvider()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="registerForCleanup"></param>
        public InvalidatingByModifiedTimeCachingProvider(Action<string> registerForCleanup)
            : base(registerForCleanup)
        {
        }

        /// <summary>
        /// Try to retrieve a template from the cache. See <see cref="ICachingProvider.TryRetrieveTemplate"/>.
        /// If cached template has different modification time, then the cache is invalidated.
        /// </summary>
        /// <param name="templateKey"></param>
        /// <param name="modelType"></param>
        /// <param name="compiledTemplate"></param>
        /// <returns></returns>
        public new bool TryRetrieveTemplate(ITemplateKey templateKey, Type modelType, out ICompiledTemplate compiledTemplate)
        {
            ICompiledTemplate foundTemplate;
            if (base.TryRetrieveTemplate(templateKey, modelType, out foundTemplate))
            {
                if (KeysHaveEqualModifiedTime(foundTemplate.Key, templateKey))
                {
                    compiledTemplate = foundTemplate;
                    return true;
                }

                InvalidateCache(foundTemplate.Key);
            }

            compiledTemplate = null;
            return false;
        }

        private bool KeysHaveEqualModifiedTime(ITemplateKey key1, ITemplateKey key2)
        {
            var keyWithTime1 = (FullPathWithModifiedTimeTemplateKey)key1;
            var keyWithTime2 = (FullPathWithModifiedTimeTemplateKey)key2;

            return keyWithTime1.ModifiedTime == keyWithTime2.ModifiedTime;
        }
    }
}
