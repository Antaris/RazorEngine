namespace RazorEngine.Templating
{
    using System.Dynamic;

    using Compilation;

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
            HasDynamicModel = GetType().IsDefined(typeof (HasDynamicModelAttribute), true);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Determines whether this template has a dynamic model.
        /// </summary>
        protected bool HasDynamicModel { get; private set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public T Model
        {
            get { return (T)currentModel; }
            set
            {
                if (HasDynamicModel && !(value is DynamicObject) && !(value is ExpandoObject))
                    currentModel = new RazorDynamicObject
                                   {
                                       Model = value, 
                                       AllowMissingPropertiesOnDynamic = TemplateService.Configuration.AllowMissingPropertiesOnDynamic
                                   };
                else
                    currentModel = value;
            }
        }

        #endregion

        /// <summary>
        /// Includes the template with the specified name.
        /// </summary>
        /// <param name="cacheName">The name of the template type in cache.</param>
        /// <param name="model">The model or NULL if there is no model for the template.</param>
        /// <returns>The template writer helper.</returns>
        public override TemplateWriter Include(string cacheName, object model = null)
        {
            return base.Include(cacheName, model ?? Model);
        }

        #region Methods
        /// <summary>
        /// Resolves the layout template.
        /// </summary>
        /// <param name="name">The name of the layout template.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        protected override ITemplate ResolveLayout(string name)
        {
            return TemplateService.Resolve(name, (T)currentModel);
        }
        #endregion
    }
}