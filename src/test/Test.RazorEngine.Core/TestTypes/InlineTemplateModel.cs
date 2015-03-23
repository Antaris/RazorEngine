namespace RazorEngine.Tests.TestTypes
{
    using System;
    using RazorEngine.Templating;

    /// <summary>
    /// Test class.
    /// </summary>
    public class InlineTemplateModel
    {
        /// <summary>
        /// Test class.
        /// </summary>
        public Func<dynamic, TemplateWriter> InlineTemplate { get; set; }
    }
}
