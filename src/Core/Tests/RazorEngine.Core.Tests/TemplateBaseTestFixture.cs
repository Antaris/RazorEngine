namespace RazorEngine.Tests
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    using NUnit.Framework;

    using Configuration;
    using Templating;
    using Text;
    using TestTypes;

    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="TemplateBase"/> type.
    /// </summary>
    [TestFixture]
    public class TemplateBaseTestFixture
    {
        #region Tests
        /// <summary>
        /// Tests that a call to <see cref="TemplateBase.Raw" /> will output the raw text when using html encoding.
        /// </summary>
        /// <remarks>
        /// <see cref="TemplateBase"/> includes a <see cref="TemplateBase.Raw"/> method which returns an instance of
        /// <see cref="RawString"/> which should bypass the string encoding when writing values.
        /// </remarks>
        [Test]
        public void TemplateBase_CanUseRawOutput_WithHtmlEncoding()
        {
            using (var service = new TemplateService())
            {
                const string template = "<h1>Hello @Raw(Model.Name)</h1>";
                const string expected = "<h1>Hello <</h1>";

                var model = new { Name = "<" };
                string result = service.Parse(template, model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }
        #endregion
    }
}