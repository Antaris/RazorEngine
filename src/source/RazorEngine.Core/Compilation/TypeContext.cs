namespace RazorEngine.Compilation
{
    using RazorEngine.Templating;
    using ReferenceResolver;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a type context that describes a template to compile.
    /// </summary>
    public class TypeContext
    {
        private readonly Action<IEnumerable<CompilerReference>> _addReferences;

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TypeContext"/>.
        /// </summary>
        /// <param name="addReferences"></param>
        internal TypeContext(Action<IEnumerable<CompilerReference>> addReferences)
        {
            if (addReferences == null) throw new ArgumentNullException(nameof(addReferences));
            _addReferences = addReferences;
            ClassName = CompilerServicesUtility.GenerateClassName();
            Namespaces = new HashSet<string>();
        }

        /// <summary>
        /// Creates a new TypeContext instance with the given classname and the given namespaces.
        /// </summary>
        /// <param name="className"></param>
        /// <param name="namespaces"></param>
        internal TypeContext(string className, ISet<string> namespaces)
        {
            _addReferences = refs => { };
            ClassName = className;
            Namespaces = namespaces;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the class name.
        /// </summary>
        public string ClassName { get; private set; }

        /// <summary>
        /// Gets or sets the model type.
        /// </summary>
        public Type ModelType { get; set; }

        /// <summary>
        /// Gets the set of namespace imports.
        /// </summary>
        public ISet<string> Namespaces { get; private set; }

        /// <summary>
        /// Gets or sets the template content.
        /// </summary>
        public ITemplateSource TemplateContent { get; set; }

        /// <summary>
        /// Gets or sets the base template type.
        /// </summary>
        public Type TemplateType { get; set; }

        /// <summary>
        /// Adds compiler references to the current dynamic assembly resolve list.
        /// </summary>
        /// <param name="references">the references to add to the dynamic resolve list.</param>
        public void AddReferences(IEnumerable<CompilerReference> references)
        {
            _addReferences(references);
        }
        #endregion
    }
}
