using RazorEngine.Tests.TestTypes.BaseTypes;

namespace RazorEngine.Tests
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    using NUnit.Framework;

    using Compilation;
    using Configuration;
    using Templating;
    using Text;
    using TestTypes;

    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="TemplateBase"/> type.
    /// </summary>
    [TestFixture]
    public class RazorEngineHostTestFixture
    {
        #region Tests
        /// <summary>
        /// Tests that the <see cref="RazorEngineHost"/> supports the @model directive.
        /// </summary>
        /// <remarks>
        /// As with it's MVC counterpart, we've added support for the @model declaration. This is to enable scenarios
        /// where the model type might be unknown, but we can pass in an instance of <see cref="object" /> and allow
        /// the @model directive to switch the model type.
        /// </remarks>
        [Test]
        public void RazorEngineHost_SupportsModelSpan_UsingCSharpCodeParser()
        {
            using (var service = new TemplateService())
            {
                const string template = "@model List<RazorEngine.Tests.TestTypes.Person>\n@Model.Count";
                const string expected = "1";

                var model = new List<Person> { new Person() { Forename = "Matt", Age = 27 } };
                string result = service.Parse(template, (object)model, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        [Test]
        public void RazorEngineHost_SupportsModelSpan_WithBaseType_NotGeneric_UsingCSharpCodeParser()
        {
            var config = new TemplateServiceConfiguration();
            config.BaseTemplateType = typeof(TemplateBase);
            using (var service = new TemplateService(config))
            {
                const string template = "@model RazorEngine.Tests.TestTypes.Person\n@Model.Forename";
                const string expected = "Matt";

                var model = new Person {Forename = "Matt"};
                string result = service.Parse(template, (object)model, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that the <see cref="RazorEngineHost"/> supports the @ModelType directive.
        /// </summary>
        /// <remarks>
        /// As with it's MVC counterpart, we've added support for the @ModelType declaration. This is to enable scenarios
        /// where the model type might be unknown, but we can pass in an instance of <see cref="object" /> and allow
        /// the @model directive to switch the model type.
        /// </remarks>
        [Category("VBNET")]
        [Test]
        public void RazorEngineHost_SupportsModelSpan_UsingVBCodeParser()
        {
            var config = new TemplateServiceConfiguration
                             {
                                 Language = Language.VisualBasic
                             };

            using (var service = new TemplateService(config))
            {
                const string template = "@ModelType List(Of RazorEngine.Tests.TestTypes.Person)\n@Model.Count";
                const string expected = "1";

                var model = new List<Person> { new Person() { Forename = "Matt", Age = 27 } };
                string result = service.Parse(template, (object)model, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        [Category("VBNET")]
        [Test]
        public void RazorEngineHost_SupportsModelSpan_WithBaseType_NotGeneric_UsingVBCodeParser()
        {
            var config = new TemplateServiceConfiguration();
            config.BaseTemplateType = typeof(TemplateBase);
            config.Language = Language.VisualBasic;

            using (var service = new TemplateService(config))
            {
                const string template = "@ModelType List(Of RazorEngine.Tests.TestTypes.Person)\n@Model.Count";
                const string expected = "1";

                var model = new List<Person> { new Person() { Forename = "Matt", Age = 27 } };
                string result = service.Parse(template, (object)model, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }
        #endregion
    }
}