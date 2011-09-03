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

    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="IsolatedTemplateService"/> type.
    /// </summary>
    [TestFixture]
    public class IsolatedTemplateServiceTestFixture
    {
        #region Setup
        /// <summary>
        /// Configures the test fixture.
        /// </summary>
        [SetUp]
        public void ConfigureFixture()
        {
            // This won't work for the isolated template service, as it needs to be initialised
            // in the target application domain. Rethink.
            CompilerServiceBuilder.SetCompilerServiceFactory(new DefaultCompilerServiceFactory());
        }
        #endregion

        #region Tests
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

                string result = service.Parse(template);

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
                string result = service.Parse(template, model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template with a non-serialisable model cannot be parsed.
        /// </summary>
        [Test] 
        public void IsolatedTemplateService_CannotParseSimpleTemplate_WithComplexNonSerialisableModelThat()
        {
            using (var service = new IsolatedTemplateService())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";

                Assert.Throws<SerializationException>(() =>
                {
                    var model = new Animal { Type = "Cat" };
                    service.Parse(template, model);
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
                    service.Parse(template, model);
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
                    service.Parse(template, model);
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
                    service.Parse(template, model);
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
        #endregion
    }
}