//-----------------------------------------------------------------------------
// <copyright file="ModelSpan.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Spans
{
    using System;
    using System.Web.Razor.Parser;
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Razor.Text;

    /// <summary>
    /// Defines a span that matches a model.
    /// </summary>
    public class ModelSpan : CodeSpan, IEquatable<ModelSpan>
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelSpan"/> class.
        /// </summary>
        /// <param name="start">The start location.</param>
        /// <param name="content">The span content.</param>
        /// <param name="modelTypeName">The model type name.</param>
        public ModelSpan(SourceLocation start, string content, string modelTypeName)
            : base(start, content)
        {
            this.ModelTypeName = modelTypeName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelSpan"/> class.
        /// </summary>
        /// <param name="context">The current parser context.</param>
        /// <param name="modelTypeName">The model type name.</param>
        internal ModelSpan(ParserContext context, string modelTypeName)
            : this(context.CurrentSpanStart, context.ContentBuffer.ToString(), modelTypeName)
        {
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets the model type name.
        /// </summary>
        public string ModelTypeName { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Determines if the specified object is equal to the current instance.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>True if the object is equal to the current instance, otherwise false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ModelSpan);
        }

        /// <summary>
        /// Determines if the specified <see cref="ModelSpan"/> is equal to the current instance.
        /// </summary>
        /// <param name="span">The span to check.</param>
        /// <returns>True if the object is equal to the current instance, otherwise false.</returns>
        public bool Equals(ModelSpan span)
        {
            if (span == null)
            {
                return false;
            }

            return base.Equals(span) && string.Equals(this.ModelTypeName, span.ModelTypeName, StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the unique hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (this.ModelTypeName ?? string.Empty).GetHashCode();
        }

        #endregion
    }
}
