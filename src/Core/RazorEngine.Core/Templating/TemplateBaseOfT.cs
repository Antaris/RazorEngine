//-----------------------------------------------------------------------------
// <copyright file="TemplateBaseOfT.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Templating
{
    using System;
    using System.Dynamic;
    using Compilation;

    /// <summary>
    /// Provides a base implementation of a template with a model.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public class TemplateBase<T> : TemplateBase, ITemplate<T>
    {
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        private object model;

        /// <summary>
        /// 
        /// </summary>
        private readonly Type _modelType = typeof(T);

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateBase&lt;T&gt;"/> class.
        /// </summary>
        protected TemplateBase()
        {
            this.HasDynamicModel = this.GetType()
                .IsDefined(typeof(HasDynamicModelAttribute), true);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public T Model
        {
            get
            {
                return (T)this.model;
            }

            set
            {
                if (this.HasDynamicModel && !(value is DynamicObject) && !(value is ExpandoObject))
                {
                    this.model = new RazorDynamicObject
                    {
                        Model = value
                    };
                }
                else
                {
                    this.model = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this template has a dynamic model.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has dynamic model; otherwise, <c>false</c>.
        /// </value>
        protected bool HasDynamicModel { get; private set; }

        #endregion
    }
}