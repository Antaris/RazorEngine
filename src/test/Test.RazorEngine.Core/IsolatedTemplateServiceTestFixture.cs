namespace RazorEngine.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Runtime.Serialization;

    using NUnit.Framework;

    using Compilation;
    using Templating;
    using TestTypes;
    using Test.RazorEngine;
    using System.IO;
    using System.Security;

    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="IsolatedTemplateService"/> type.
    /// </summary>
    [TestFixture]
    [Obsolete("Needs to be updated/removed.")]
    public class IsolatedTemplateServiceTestFixture
    {
        #region Tests

        /// <summary>
        /// Tests that a bad template cannot write where it wants.
        /// </summary>
        [Test]
        public void IsolatedTemplateService_BadTemplate_InSandbox()
        {
            using (var service = new IsolatedTemplateService(IsolatedRazorEngineServiceTestFixture.SandboxCreator))
            {
                string file = Path.Combine(Environment.CurrentDirectory, Path.GetRandomFileName());

                string template = @"
@using System.IO
@{
File.WriteAllText(""$file$"", ""BAD DATA"");
}".Replace("$file$", file.Replace("\\", "\\\\"));
                Assert.Throws<SecurityException>(() =>
                {
                    service.Parse(template, null, null, "test");
                });

                Assert.IsFalse(File.Exists(file));
            }
        }

        /// <summary>
        /// Tests that a very bad template cannot change its permission.
        /// </summary>
        [Test]
        public void IsolatedTemplateService_VeryBadTemplate_InSandbox()
        {
            using (var service = new IsolatedTemplateService(IsolatedRazorEngineServiceTestFixture.SandboxCreator))
            {
                string file = Path.Combine(Environment.CurrentDirectory, Path.GetRandomFileName());

                string template = @"
@using System.IO
@using System.Security
@using System.Security.Permissions
@{
(new PermissionSet(PermissionState.Unrestricted)).Assert();
File.WriteAllText(""$file$"", ""BAD DATA"");
}".Replace("$file$", file.Replace("\\", "\\\\"));
                Assert.Throws<InvalidOperationException>(() =>
                { // cannot create a file in template
                    service.Parse(template, null, null, "test");
                });

                Assert.IsFalse(File.Exists(file));
            }
        }


        /// <summary>
        /// Tests that a simple template without a model can be parsed.
        /// </summary>
        [Test]
        public void IsolatedTemplateService_CanParseSimpleTemplate_WithNoModel()
        {
            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Hello World</h1>";
                const string expected = template;

                string result = service.Parse(template, null, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template with a model can be parsed.
        /// </summary>
        [Test]
        public void IsolatedTemplateService_CanParseSimpleTemplate_WithComplexSerialisableModel()
        {
            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Hello @Model.Forename</h1>";
                const string expected = "<h1>Hello Matt</h1>";

                var model = new Person { Forename = "Matt" };
                string result = service.Parse(template, model, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template with a non-serialisable model cannot be parsed.
        /// </summary>
        [Test] 
        public void IsolatedTemplateService_CannotParseSimpleTemplate_WithComplexNonSerialisableModel()
        {
            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";

                Assert.Throws<SerializationException>(() =>
                {
                    var model = new Animal { Type = "Cat" };
                    service.Parse(template, model, null, null);
                });
            }
        }

        /// <summary>
        /// Tests that a simple template with an anonymous model cannot be parsed.
        /// </summary>
        /// <remarks>
        /// This may seem pointless to test, as the <see cref="IsolatedTemplateService"/> will explicitly
        /// check and throw the exception, it's worth creating a test for future reference. It's also
        /// something we can check should we ever find a way to support dynamic/anonymous objects
        /// across application domain boundaries.
        /// </remarks>
        [Test]
        public void IsolatedTemplateService_CannotParseSimpleTemplate_WithAnonymousModel()
        {
            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";

                Assert.Throws<ArgumentException>(() =>
                {
                    var model = new { Type = "Cat" };
                    service.Parse(template, model, null, null);
                });
            }
        }

        /// <summary>
        /// Tests that a simple template with an expando model cannot be parsed.
        /// </summary>
        /// <remarks>
        /// This may seem pointless to test, as the <see cref="IsolatedTemplateService"/> will explicitly
        /// check and throw the exception, it's worth creating a test for future reference. It's also
        /// something we can check should we ever find a way to support dynamic/anonymous objects
        /// across application domain boundaries.
        /// </remarks>
        [Test]
        public void IsolatedTemplateService_CannotParseSimpleTemplate_WithExpandoModel()
        {
            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";

                Assert.Throws<ArgumentException>(() =>
                {
                    dynamic model = new ExpandoObject();
                    model.Type = "Cat";
                    service.Parse(template, model, null, null);
                });
            }
        }

        /// <summary>
        /// Tests that a simple template with a dynamic model cannot be parsed.
        /// </summary>
        /// <remarks>
        /// This may seem pointless to test, as the <see cref="IsolatedTemplateService"/> will explicitly
        /// check and throw the exception, it's worth creating a test for future reference. It's also
        /// something we can check should we ever find a way to support dynamic/anonymous objects
        /// across application domain boundaries.
        /// </remarks>
        [Test]
        public void IsolatedTemplateService_CannotParseSimpleTemplate_WithDynamicModel()
        {
            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";

                Assert.Throws<ArgumentException>(() =>
                {
                    dynamic model = new ValueObject(new Dictionary<string, object> { { "Type", "Cat" } });
                    service.Parse(template, model, null, null);
                });
            }
        }

        /// <summary>
        /// Tests that an isolated template service cannot use the same application domain as the 
        /// main application domain.
        /// </summary>
        /// <remarks>
        /// An isolated template service will unload it's child application domain on Dispose. We need to ensure
        /// it doesn't attempt to unload the current application domain that it is running in. This may or may
        /// not be the main application domain (but is very likely to be).
        /// </remarks>
        [Test]
        public void IsolatedTemplateService_WillThrowException_WhenUsingMainAppDomain()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (var service = new IsolatedTemplateService(() => AppDomain.CurrentDomain))
                { }
            });
        }

        /// <summary>
        /// Tests that an isolated template service cannot use a null application domain.
        /// </summary>
        /// <remarks>
        /// I had considered using the default <see cref="IAppDomainFactory"/> to spawn a default
        /// application domain to load templates into when a null value is returned, but behaviourly this didn't 
        /// seem like the right thing to do. If you're using an <see cref="IsolatedTemplateService"/>, 
        /// you should expect it to have a valid application domain, so passing null should cause an exception.
        /// </remarks>
        [Test]
        public void IsolatedTemplateService_WillThrowException_WhenUsingNullAppDomain()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (var service = new IsolatedTemplateService(() => null))
                { }
            });
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
        public void IsolatedTemplateService_CanParseSimpleTemplate_UsingHtmlEncoding()
        {

            using (var service = new IsolatedTemplateService(Encoding.Html))
            {
                const string template = "<h1>Hello @Model.Forename</h1>";
                const string expected = "<h1>Hello Matt &amp; World</h1>";

                var model = new Person { Forename = "Matt & World" };
                string result = service.Parse(template, model, null, null);

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
        public void IsolatedTemplateService_CanParseSimpleTemplate_UsingRawEncoding()
        {
            using (var service = new IsolatedTemplateService(Encoding.Raw))
            {
                const string template = "<h1>Hello @Model.Forename</h1>";
                const string expected = "<h1>Hello Matt & World</h1>";

                var model = new Person { Forename = "Matt & World" };
                string result = service.Parse(template, model, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that the template service can parse multiple templates in sequence.
        /// </summary>
        [Test]
        public void IsolatedTemplateService_CanParseMultipleTemplatesInSequence_WitNoModels()
        {
            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Hello World</h1>";
                var templates = Enumerable.Repeat(template, 10).ToArray();

                var results = service.ParseMany(templates, null, null, null, false);

                Assert.That(templates.SequenceEqual(results), "Rendered templates do not match expected.");
            }
        }

        /// <summary>
        /// Tests that the template service can parse multiple templates in parallel.
        /// </summary>
        [Test]
        public void IsolatedTemplateService_CanParseMultipleTemplatesInParallel_WitNoModels()
        {
            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Hello World</h1>";
                var templates = Enumerable.Repeat(template, 10).ToArray();

                var results = service.ParseMany(templates, null, null, null, true);

                Assert.That(templates.SequenceEqual(results), "Rendered templates do not match expected.");
            }
        }

        /// <summary>
        /// Tests that the template service can parse multiple templates in sequence with complex models.
        /// </summary>
        [Test]
        public void IsolatedTemplateService_CanParseMultipleTemplatesInSequence_WithComplexModels()
        {
            const int maxTemplates = 10;

            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Age: @Model.Age</h1>";
                var expected = Enumerable.Range(1, maxTemplates).Select(i => string.Format("<h1>Age: {0}</h1>", i));
                var templates = Enumerable.Repeat(template, maxTemplates).ToArray();
                var models = Enumerable.Range(1, maxTemplates).Select(i => new Person { Age = i }).ToArray();

                var results = service.ParseMany(templates, models, null, null, false);
                Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.");
            }
        }

        /// <summary>
        /// Tests that the template service can parse multiple templates in parallel with complex models.
        /// </summary>
        [Test]
        public void IsolatedTemplateService_CanParseMultipleTemplatesInParallel_WithComplexModels()
        {
            const int maxTemplates = 10;

            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Age: @Model.Age</h1>";
                var expected = Enumerable.Range(1, maxTemplates).Select(i => string.Format("<h1>Age: {0}</h1>", i));
                var templates = Enumerable.Repeat(template, maxTemplates).ToArray();
                var models = Enumerable.Range(1, maxTemplates).Select(i => new Person { Age = i }).ToArray();

                var results = service.ParseMany(templates, models, null, null, true);
                Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.");
            }
        }

        /// <summary>
        /// Tests that the template service can parse and run multiple templates based off a single source template.
        /// This is processed in parallel.
        /// </summary>
        [Test]
        public void IsolatedTemplateService_CanParseSingleTemplateInParallel_WithMultipleModels()
        {
            const int maxTemplates = 10;

            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Age: @Model.Age</h1>";
                var expected = Enumerable.Range(1, maxTemplates).Select(i => string.Format("<h1>Age: {0}</h1>", i));
                var models = Enumerable.Range(1, maxTemplates).Select(i => new Person { Age = i }).ToArray();

                var results = service.ParseMany(Enumerable.Repeat(template, maxTemplates).ToArray(), models, null, null, true /* Parallel */);
                Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.");
            }
        }

        /// <summary>
        /// Tests that the template service can parse and run multiple templates based off a single source template.
        /// This is processed in sequence.
        /// </summary>
        [Test]
        public void IsolatedTemplateService_CanParseSingleTemplateInSequence_WithMultipleModels()
        {
            const int maxTemplates = 10;

            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Age: @Model.Age</h1>";
                var expected = Enumerable.Range(1, maxTemplates).Select(i => string.Format("<h1>Age: {0}</h1>", i));
                var models = Enumerable.Range(1, maxTemplates).Select(i => new Person { Age = i }).ToArray();

                var results = service.ParseMany(Enumerable.Repeat(template, maxTemplates).ToArray(), models, null, null, false /* Sequence */);
                Assert.That(expected.SequenceEqual(results), "Parsed templates do not match expected results.");
            }
        }
        #endregion
    }
}