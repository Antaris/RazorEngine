//-----------------------------------------------------------------------------
// <copyright file="TypeContext.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Compilation
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
        /// Initializes a new instance of the <see cref="TypeContext"/> class.
        /// </summary>
        internal TypeContext()
        {
            this.ClassName = CompilerServicesUtility.GenerateClassName();
            this.Namespaces = new HashSet<string>();
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
        /// Gets or sets the base template type.
        /// </summary>
        public Type TemplateType { get; set; }

        #endregion
    }
}
