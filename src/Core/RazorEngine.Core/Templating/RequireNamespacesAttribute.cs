//-----------------------------------------------------------------------------
// <copyright file="RequireNamespacesAttribute.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Allows base templates to define require template imports when
    /// generating templates.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class RequireNamespacesAttribute : Attribute
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireNamespacesAttribute"/> class.
        /// </summary>
        /// <param name="namespaces">The set of required namespace imports.</param>
        public RequireNamespacesAttribute(params string[] namespaces)
        {
            if (namespaces == null)
            {
                throw new ArgumentNullException("namespaces");
            }

            var set = new HashSet<string>();
            foreach (string ns in namespaces)
            {
                set.Add(ns);
            }

            this.Namespaces = set;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the set of required namespace imports.
        /// </summary>
        public IEnumerable<string> Namespaces { get; private set; }

        #endregion
    }
}