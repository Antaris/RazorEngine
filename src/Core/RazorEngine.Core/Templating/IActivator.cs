namespace RazorEngine.Templating
{
    /// <summary>
    /// Defines the required contract for implementing an activator.
    /// </summary>
    public interface IActivator
    {
        #region Methods
        /// <summary>
        /// Creates an instance of the specifed template.
        /// </summary>
        /// <param name="context">The instance context.</param>
        /// <returns>An instance of <see cref="ITemplate"/>.</returns>
        ITemplate CreateInstance(InstanceContext context);
        #endregion
    }
}