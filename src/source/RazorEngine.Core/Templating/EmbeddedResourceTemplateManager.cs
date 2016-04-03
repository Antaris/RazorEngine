using System;
using System.IO;

namespace RazorEngine.Templating
{
    /// <summary>
    /// A TemplateManager loading templates from embedded resources.
    /// </summary>
    public class EmbeddedResourceTemplateManager : ITemplateManager
    {
        /// <summary>
        /// Initializes a new TemplateManager.
        /// </summary>
        /// <param name="rootType">The type from the assembly that contains embedded resources that will act as a root type for Assembly.GetManifestResourceStream() calls.</param>
        public EmbeddedResourceTemplateManager(Type rootType)
        {
            if (rootType == null)
                throw new ArgumentNullException(nameof(rootType));

            this.RootType = rootType;
        }

        /// <summary>
        /// The type from the assembly that contains embedded resources
        /// </summary>
        public Type RootType { get; }

        /// <summary>
        /// Resolve the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ITemplateSource Resolve(ITemplateKey key)
        {
            using (var stream = this.RootType.Assembly.GetManifestResourceStream(this.RootType, key.Name + ".cshtml"))
            {
                if(stream == null)
                    throw new TemplateLoadingException(string.Format("Couldn't load resource '{0}.{1}.cshtml' from assembly {2}", this.RootType.Namespace, key.Name, this.RootType.Assembly.FullName));

                using (var reader = new StreamReader(stream))
                {
                    return new LoadedTemplateSource(reader.ReadToEnd());
                }
            }
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
            return new NameOnlyTemplateKey(name, resolveType, context);
        }

        /// <summary>
        /// Throws NotSupportedException.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="source"></param>
        public void AddDynamic(ITemplateKey key, ITemplateSource source)
        {
            throw new NotSupportedException("Adding templates dynamically is not supported. This Manager only supports embedded resources.");
        }
    }
}
