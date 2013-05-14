namespace RazorEngine.Mvc
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using Templating;

    /// <summary>
    /// Provides a base implementation of an MVC-compatible template.
    /// </summary>
    public abstract class MvcTemplateBase : TemplateBase
    {
        #region Properties
        /// <summary>
        /// Gets the <see cref="HtmlHelper{Object}"/> for this template.
        /// </summary>
        public new HtmlHelper<object> Html { get; private set; }

        /// <summary>
        /// Gets the <see cref="UrlHelper"/> for this template.
        /// </summary>
        public UrlHelper Url { get; private set; }
        #endregion

        #region Methods
        public void InitHelpers()
        {
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            var handler = httpContext.CurrentHandler as MvcHandler;
            if (handler == null)
                throw new InvalidOperationException("Unable to run template outside of ASP.NET MVC");
        }
        #endregion
    }
}