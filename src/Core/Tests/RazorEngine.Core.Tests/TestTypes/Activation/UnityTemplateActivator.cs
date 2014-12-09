namespace RazorEngine.Tests.TestTypes.Activation
{
    using System;

    using Microsoft.Practices.Unity;

    using Templating;

    /// <summary>
    /// Defines an activator that supports Unity.
    /// </summary>
    public class UnityTemplateActivator : IActivator
    {
        #region Fields
        private readonly UnityContainer _container;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="UnityTemplateActivator"/>.
        /// </summary>
        /// <param name="container">The unity container.</param>
        public UnityTemplateActivator(UnityContainer container)
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
}