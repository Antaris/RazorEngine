using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Templating
{
    /// <summary>
    /// A TemplateManager resolving templates by path, given a list of folders to look into.
    /// </summary>
    public class ResolvePathTemplateManager : ITemplateManager
    {
        private readonly ReadOnlyCollection<string> layoutRoots;
        /// <summary>
        /// Initializes a new TemplateManager.
        /// </summary>
        /// <param name="layoutRoots">the list of folders to look for templates.</param>
        public ResolvePathTemplateManager(IEnumerable<string> layoutRoots)
        {
            this.layoutRoots = new ReadOnlyCollection<string>(new List<string>(layoutRoots));
        }

        internal ResolvePathTemplateManager(ReadOnlyCollection<string> list)
        {
            this.layoutRoots = list;
        }

        /// <summary>
        /// Resolve the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ITemplateSource Resolve(ITemplateKey key)
        {
            var full = key as FullPathTemplateKey;
            if (full == null)
            {
                throw new NotSupportedException("You can only use FullPathTemplateKey with this manager");
            }
            var template = File.ReadAllText(full.FullPath);
            return new LoadedTemplateSource(template, full.FullPath);
        }

        /// <summary>
        /// Get the given key.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="resolveType"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context)
        {
            return new FullPathTemplateKey(name, ResolveFilePath(name), resolveType, context);
        }

        /// <summary>
        /// Resolve full file path using layout roots.
        /// </summary>
        /// <param name="name">file name</param>
        /// <returns>full file path</returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected string ResolveFilePath(string name)
        {
            if (File.Exists(name))
            {
                return name;
            }

            foreach (var root in layoutRoots)
            {
                var path = Path.Combine(root, name);

                if (File.Exists(path))
                {
                    return path;
                }

                //Check if a file with the csharp extension exists
                var csPath = path + ".cshtml";
                if (File.Exists(csPath))
                {
                    return csPath;
                }

                //Check if a file with the visual basic extension exists
                var vbPath = path + ".vbhtml";
                if (File.Exists(vbPath))
                {
                    return vbPath;
                }
            }

            throw new InvalidOperationException(string.Format("Could not resolve template {0}", name));
        }

        /// <summary>
        /// Throws NotSupportedException.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="source"></param>
        public void AddDynamic(ITemplateKey key, ITemplateSource source)
        {
            throw new NotSupportedException("Adding templates dynamically is not supported! Instead you probably want to use the full-path in the name parameter?");
        }
    }
}
