namespace RazorEngine.Compilation
{
    using RazorEngine.Templating;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a type context that describes a template to compile.
    /// </summary>
    public class TypeContext
    {
        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TypeContext"/>.
        /// </summary>
        internal TypeContext()
        {
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
        #endregion
    }
}
