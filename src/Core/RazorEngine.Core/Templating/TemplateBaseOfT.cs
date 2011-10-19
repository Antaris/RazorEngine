namespace RazorEngine.Templating
{
    using System;
    using System.Dynamic;

    using Compilation;
    using Text;

    /// <summary>
    /// Provides a base implementation of a template with a model.
    /// </summary>
    /// <typeparam name="T">The model type.</typeparam>
    public class TemplateBase<T> : TemplateBase, ITemplate<T>
    {
        #region Fields
        private object model;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TemplateBase{T}"/>.
        /// </summary>
        protected TemplateBase()
        {
            HasDynamicModel = GetType()
                .IsDefined(typeof(HasDynamicModelAttribute), true);
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
            get { return (T)model; }
            set
            {
                if (HasDynamicModel && !(value is DynamicObject) && !(value is ExpandoObject))
                    model = new RazorDynamicObject { Model = value };
                else
                    model = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Includes the template with the specified name.
        /// </summary>
        /// <param name="name">The name of the template to include.</param>
        /// <returns>The template writer helper.</returns>
        public override TemplateWriter Include(string name)
        {
            var instance = TemplateService.Resolve(name, Model);
            if (instance == null)
                throw new ArgumentException("No template could be resolved with name '" + name + "'");

            return new TemplateWriter(tw => tw.Write(instance.Run(new ExecuteContext())));
        }
        #endregion
    }
}