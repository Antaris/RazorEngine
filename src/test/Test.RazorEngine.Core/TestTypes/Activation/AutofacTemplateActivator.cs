namespace RazorEngine.Tests.TestTypes.Activation
{
    using System;
#if NET45
    using Autofac;

    using Templating;

    /// <summary>
    /// Defines an activator that supports Unity.
    /// </summary>
    public class AutofacTemplateActivator : IActivator
    {
        #region Fields
        private readonly IContainer _container;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="AutofacTemplateActivator"/>.
        /// </summary>
        /// <param name="container">The unity container.</param>
        public AutofacTemplateActivator(IContainer container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            _container = container;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Creates an instance of the specifed template.
        /// </summary>
        /// <param name="context">The instance context.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        public ITemplate CreateInstance(InstanceContext context)
        {
            return (ITemplate)_container.Resolve(context.TemplateType);
        }
        #endregion
    }
#endif
}