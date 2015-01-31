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

        #endregion

        #region Constructor

        /// <summary>
        /// Initialises a new instance of <see cref="TemplateBase{T}"/>.
        /// </summary>
        protected TemplateBase()
        {
            var hasDynamicAttribute = GetType().IsDefined(typeof (HasDynamicModelAttribute), true);
            if (hasDynamicAttribute && typeof(T) == typeof(object))
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

        internal override Type ModeType
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
        /// <param name="cacheName">The name of the template type in cache.</param>
        /// <param name="model">The model or NULL if there is no model for the template.</param>
        /// <param name="modelType"></param>
        /// <returns>The template writer helper.</returns>
        public override TemplateWriter Include(string cacheName, object model = null, Type modelType = null)
        {
            // When model == null we use our current model => we should use the same modelType as well.
            return base.Include(cacheName, model ?? Model, model == null ? ModeType: modelType);
        }

        /// <summary>
        /// Resolves the layout template.
        /// </summary>
        /// <param name="name">The name of the layout template.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        protected override ITemplate ResolveLayout(string name)
        {
            return InternalTemplateService.Resolve(name, (T)currentModel, ModeType, (DynamicViewBag)ViewBag, ResolveType.Layout);
        }
        #endregion
    }
}