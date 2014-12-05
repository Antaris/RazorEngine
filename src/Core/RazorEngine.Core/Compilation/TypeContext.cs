﻿namespace RazorEngine.Compilation
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines a type context that describes a template to compile.
    /// </summary>
    public class TypeContext : MarshalByRefObject
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
        public string TemplateContent { get; set; }

        /// <summary>
        /// Gets or sets the name of the template file.
        /// </summary>
        /// <value>The name of the template file.</value>
        public string TemplateFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include debug information into the generated template in order to support Visual Studio debugger inside the template code.
        /// </summary>
        /// <value><c>true</c> if [include debug information]; otherwise, <c>false</c>.</value>
        public bool IncludeDebugInformation { get; set; }

        /// <summary>
        /// Gets or sets the base template type.
        /// </summary>
        public Type TemplateType { get; set; }
        #endregion
    }
}
