namespace RazorEngine.Templating
{
    using System.Dynamic;

    using Compilation;
    using System;
    using System.Security;

    /// <summary>
    /// Provides a base implementation of a template with a model.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public class TemplateBase<T> : TemplateBase, ITemplate<T>
    {
        #region Fields

        private object currentModel;
        private bool _needReplaceNullModel = false;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateBase{T}"/>.
        /// </summary>
        protected TemplateBase()
        {
            if (typeof(T) == typeof(object) &&
                GetType().IsDefined(typeof (HasDynamicModelAttribute), true))
            {
                // It is possible that we think that we have a dynamic model 
                // (because null was given and the attribute is in place), 
                // but the template contains the @model directive and
                // therefore we have a static typed template.
                HasDynamicModel = true;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Determines whether this template has a dynamic model.
        /// </summary>
        protected bool HasDynamicModel { get; private set; }

        internal override Type ModelType
        {
            get
            {
                return HasDynamicModel ? typeof(DynamicObject) : typeof(T);
            }
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public T Model
        {
            get { return (T)currentModel; }
            set { currentModel = value; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Set the model.
        /// </summary>
        /// <param name="model"></param>
        public override void SetModel(object model)
        {
            Model = (T)model;
        }

        /// <summary>
        /// Includes the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template type in cache.</param>
        /// <param name="model">The model or NULL if there is no model for the template.</param>
        /// <returns>The template writer helper.</returns>
#pragma warning disable 1573
        // ReSharper disable OptionalParameterHierarchyMismatch
        public override TemplateWriter Include(string name, object model, Type modelType)
            // ReSharper restore OptionalParameterHierarchyMismatch
#pragma warning restore 1573
        {
            model = _needReplaceNullModel && model == null ? Model : model;
            modelType = _needReplaceNullModel && model == null ? ModelType : modelType;
            return base.Include(name, model, modelType);
        }

        /// <summary>
        /// Includes the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template type in cache.</param>
        /// <param name="model">The model or NULL if there is no model for the template.</param>
        /// <returns>The template writer helper.</returns>
        public virtual TemplateWriter Include(string name, object model)
        {
            return Include(name, model, null);
        }

        /// <summary>
        /// Includes the template with the specified name, uses the current model and model-type.
        /// </summary>
        /// <param name="name">The name of the template type in cache.</param>
        /// <returns>The template writer helper.</returns>
        public virtual TemplateWriter Include(string name)
        {
            _needReplaceNullModel = true;
            try
            {
                return Include(name, null, null);
            }
            finally
            {
                _needReplaceNullModel = false;
            }
        }


        /// <summary>
        /// Resolves the layout template.
        /// </summary>
        /// <param name="name">The name of the layout template.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        protected override ITemplate ResolveLayout(string name)
        {
            return InternalTemplateService.Resolve(name, (T)currentModel, ModelType, (DynamicViewBag)ViewBag, ResolveType.Layout);
        }
        #endregion
    }
}