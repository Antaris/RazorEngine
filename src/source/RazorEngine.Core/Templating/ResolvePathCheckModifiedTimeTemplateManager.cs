using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// A TemplateManager resolving remplates by path, given a list of folders to look into.
    /// Uses <see cref="FullPathWithModifiedTimeTemplateKey" /> to save template modification time.
    /// </summary>
    public class ResolvePathCheckModifiedTimeTemplateManager : ResolvePathTemplateManager, ITemplateManager
    {
        /// <summary>
        /// Initializes a new TemplateManager
        /// </summary>
        /// <param name="layoutRoots">the list of folders to look for templates.</param>
        public ResolvePathCheckModifiedTimeTemplateManager(IEnumerable<string> layoutRoots)
            : base(layoutRoots)
        {
        }

        /// <summary>
        /// Get the given key.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resolveType"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public new ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context)
        {
            var fullPath = ResolveFilePath(name);
            var modifiedTime = File.GetLastWriteTimeUtc(fullPath);

            return new FullPathWithModifiedTimeTemplateKey(name, fullPath, modifiedTime, resolveType, context);
        }
    }
}
