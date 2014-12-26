namespace RazorEngine.Tests.TestTypes
{
    using System;
    using RazorEngine.Templating;

    public class InlineTemplateModel
    {
        public Func<dynamic, TemplateWriter> InlineTemplate { get; set; }
    }
}
