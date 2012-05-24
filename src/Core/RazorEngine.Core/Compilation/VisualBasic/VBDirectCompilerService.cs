﻿//-----------------------------------------------------------------------------
// <copyright file="VBDirectCompilerService.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Compilation.VisualBasic
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Web.Razor.Parser;
    using Microsoft.VisualBasic;

    /// <summary>
    /// Defines a direct compiler service for the VB syntax.
    /// </summary>
    // ReSharper disable InconsistentNaming
    public class VBDirectCompilerService : DirectCompilerServiceBase
    // ReSharper restore InconsistentNaming
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="VBDirectCompilerService"/> class.
        /// </summary>
        /// <param name="strictMode">Specifies whether the strict mode parsing is enabled.</param>
        /// <param name="markupParserFactory">The markup parser to use.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed in base class: DirectCompilerServiceBase")]
        public VBDirectCompilerService(bool strictMode = true, Func<MarkupParser> markupParserFactory = null)
            : base(new VBRazorCodeLanguage(strictMode), new VBCodeProvider(), markupParserFactory)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds a type name for the specified generic type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="isDynamic">Specifies whether the type is dynamic.</param>
        /// <returns>
        /// The string type name (including namespace and generic type parameters).
        /// </returns>
        public override string BuildTypeNameInternal(Type type, bool isDynamic)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            if (!type.IsGenericType)
            {
                return type.FullName;
            }

            return type.Namespace
                   + "."
                   + type.Name.Substring(0, type.Name.IndexOf('`'))
                   + "(Of "
                   + (isDynamic ? "Object" : string.Join(", ", type.GetGenericArguments().Select(t => this.BuildTypeNameInternal(t, CompilerServicesUtility.IsDynamicType(t)))))
                   + ")";
        }
        #endregion
    }
}