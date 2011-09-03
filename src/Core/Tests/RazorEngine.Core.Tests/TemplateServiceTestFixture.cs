namespace RazorEngine.Tests
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;

    using NUnit.Framework;

    using Compilation;
    using Templating;
    using TestTypes;

    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="TemplateService"/> type.
    /// </summary>
    [TestFixture]
    public class TemplateServiceTestFixture
    {
        #region Setup
        /// <summary>
        /// Configures the test fixture.
        /// </summary>
        [SetUp]
        public void ConfigureFixture()
        {
            CompilerServiceBuilder.SetCompilerServiceFactory(new DefaultCompilerServiceFactory());
        }
        #endregion

        #region Tests
        /// <summary>
        /// Tests that a simple template without a model can be parsed.
        /// </summary>
        [Test]
        public void TemplateService_CanParseSimpleTemplate_WithNoModel()
        {
            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Hello World</h1>";
                const string expected = template;

                string result = service.Parse(template);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template with a model can be parsed.
        /// </summary>
        [Test]
        public void TemplateService_CanParseSimpleTemplate_WithComplexModel()
        {
            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Hello @Model.Forename</h1>";
                const string expected = "<h1>Hello Matt</h1>";

                var model = new Person { Forename = "Matt" };
                string result = service.Parse(template, model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template with a model (of declared type <see cref="object"/>) can be parsed.
        /// </summary>
        [Test]
        public void TemplateService_CanParseSimpleTemplate_WithObjectModel()
        {
            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Hello @Model.Forename</h1>";
                const string expected = "<h1>Hello Matt</h1>";

                object model = new Person { Forename = "Matt" };
                string result = service.Parse(template, model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template with an anonymous model can be parsed.
        /// </summary>
        [Test]
        public void TemplateService_CanParseSimpleTemplate_WithAnonymousModel()
        {
            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Hello @Model.Forename</h1>";
                const string expected = "<h1>Hello Matt</h1>";

                var model = new { Forename = "Matt" };
                string result = service.Parse(template, model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template with a expando model can be parsed.
        /// </summary>
        [Test]
        public void TemplateService_CanParseSimpleTemplate_WithExpandoModel()
        {
            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Hello @Model.Forename</h1>";
                const string expected = "<h1>Hello Matt</h1>";

                dynamic model = new ExpandoObject();
                model.Forename = "Matt";

                string result = service.Parse(template, model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template with a dynamic model can be parsed.
        /// </summary>
        [Test]
        public void TemplateService_CanParseSimpleTemplate_WithDynamicModel()
        {
            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Hello @Model.Forename</h1>";
                const string expected = "<h1>Hello Matt</h1>";

                dynamic model = new ValueObject(new Dictionary<string, object> { { "Forename", "Matt" } });
                string result = service.Parse(template, model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template with html-encoding can be parsed.
        /// </summary>
        /// <remarks>
        /// Text encoding is performed when writing objects to the template result (not literals). This test should 
        /// show that the template service is correctly providing the appropriate encoding factory to process
        /// the object's .ToString() and automatically encode it.
        /// </remarks>
        [Test]
        public void TemplateService_CanParseSimpleTemplate_UsingHtmlEncoding()
        {
            using (var service = new TemplateService(Language.CSharp, Encoding.Html))
            {
                const string template = "<h1>Hello @Model.String</h1>";
                const string expected = "<h1>Hello Matt &amp; World</h1>";

                var model = new { String = "Matt & World" };
                string result = service.Parse(template, model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template with no-encoding can be parsed.
        /// </summary>
        /// <remarks>
        /// Text encoding is performed when writing objects to the template result (not literals). This test should 
        /// show that the template service is correctly providing the appropriate encoding factory to process
        /// the object's .ToString() and automatically encode it.
        /// </remarks>
        [Test]
        public void TemplateService_CanParseSimpleTemplate_UsingRawEncoding()
        {
            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Hello @Model.String</h1>";
                const string expected = "<h1>Hello Matt & World</h1>";

                var model = new { String = "Matt & World" };
                string result = service.Parse(template, model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that the template service can parse multiple templates in sequence.
        /// </summary>
        [Test]
        public void TemplateService_CanParseMultipleTemplatesInSequence_WitNoModels()
        {
            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Hello World</h1>";
                var templates = Enumerable.Repeat(template, 10);

                var results = service.ParseMany(templates, false);

                Assert.That(templates.SequenceEqual(results), "Rendered templates do not match expected.");
            }
        }

        /// <summary>
        /// Tests that the template service can parse multiple templates in parallel.
        /// </summary>
        [Test]
        public void TemplateService_CanParseMultipleTemplatesInParallel_WitNoModels()
        {
            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Hello World</h1>";
                var templates = Enumerable.Repeat(template, 10);

                var results = service.ParseMany(templates, true);

                Assert.That(templates.SequenceEqual(results), "Rendered templates do not match expected."); 
            }
        }

        /// <summary>
        /// Tests that the template service can parse multiple templates in sequence with complex models.
        /// </summary>
        [Test]
        public void TemplateService_CanParseMultipleTemplatesInSequence_WithComplexModels()
        {
            const int maxTemplates = 10;

            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Age: @Model.Age</h1>";
                var expected = Enumerable.Range(1, maxTemplates).Select(i => string.Format("<h1>Age: {0}</h1>", i));
                var templates = Enumerable.Repeat(template, maxTemplates);
                var models = Enumerable.Range(1, maxTemplates).Select(i => new Person { Age = i });

                var results = service.ParseMany(templates, models, false);
                Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.");
            }
        }

        /// <summary>
        /// Tests that the template service can parse multiple templates in parallel with complex models.
        /// </summary>
        [Test]
        public void TemplateService_CanParseMultipleTemplatesInParallel_WithComplexModels()
        {
            const int maxTemplates = 10;

            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Age: @Model.Age</h1>";
                var expected = Enumerable.Range(1, maxTemplates).Select(i => string.Format("<h1>Age: {0}</h1>", i));
                var templates = Enumerable.Repeat(template, maxTemplates);
                var models = Enumerable.Range(1, maxTemplates).Select(i => new Person { Age = i });

                var results = service.ParseMany(templates, models, true);
                Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.");
            }
        }

        /// <summary>
        /// Tests that the template service can parse and run multiple templates based off a single source template.
        /// This is processed in parallel.
        /// </summary>
        [Test]
        public void TemplateService_CanParseSingleTemplateInParallel_WithMultipleModels()
        {
            const int maxTemplates = 10;

            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Age: @Model.Age</h1>";
                var expected = Enumerable.Range(1, maxTemplates).Select(i => string.Format("<h1>Age: {0}</h1>", i));
                var models = Enumerable.Range(1, maxTemplates).Select(i => new Person { Age = i });

                var results = service.ParseMany(template, models, true /* Parallel */);
                Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.");
            }
        }

        /// <summary>
        /// Tests that the template service can parse and run multiple templates based off a single source template.
        /// This is processed in sequence.
        /// </summary>
        [Test]
        public void TemplateService_CanParseSingleTemplateInSequence_WithMultipleModels()
        {
            const int maxTemplates = 10;

            using (var service = new TemplateService(Language.CSharp, Encoding.Raw))
            {
                const string template = "<h1>Age: @Model.Age</h1>";
                var expected = Enumerable.Range(1, maxTemplates).Select(i => string.Format("<h1>Age: {0}</h1>", i));
                var models = Enumerable.Range(1, maxTemplates).Select(i => new Person { Age = i });

                var results = service.ParseMany(template, models, false /* Sequence */);
                Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.");
            }
        }
        #endregion
    }
}