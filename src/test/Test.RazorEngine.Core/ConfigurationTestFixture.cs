namespace RazorEngine.Tests
{
    using System;
    using System.IO;

    using Moq;
    using NUnit.Framework;

    using Compilation;
    using Configuration;
    using Templating;
    using Text;
    using Configuration.Xml;

    /// <summary>
    /// Defines a test fixture that provides tests for the <see cref="ITemplateServiceConfiguration"/> type.
    /// </summary>
    [TestFixture]
    public class ConfigurationTestFixture
    {
        #region Tests
        /// <summary>
        /// Tests that the fluent configuration supports adding additional namespace imports.
        /// </summary>
        [Test]
        public void FluentTemplateServiceConfiguration_CanAddNamespaces()
        {
            var config = new FluentTemplateServiceConfiguration(c => c.IncludeNamespaces("RazorEngine.Templating"));

            Assert.That(config.Namespaces.Contains("RazorEngine.Templating"));
        }

        /// <summary>
        /// Tests that the fluent configuration can configure a template service with additional namespaces.
        /// </summary>
        [Test]
        public void FluentTemplateServiceConfiguration_CanConfigureTemplateService_WithAdditionalNamespaces()
        {
            var config = new FluentTemplateServiceConfiguration(
                c => c.IncludeNamespaces("System.IO"));
#pragma warning disable 0618 // TODO: Update test.
            using (var service = new TemplateService(config))
#pragma warning restore 0618 // TODO: Update test.
            {
                const string template = @"@Directory.GetFiles(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Personal)), ""*.*"").Length";

                int expected = Directory.GetFiles(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Personal)), "*.*").Length;
                string result = service.Parse(template, null, null, null);

                Assert.AreEqual(expected.ToString(), result);
            }
        }
        
#if !RAZOR4
        /// <summary>
        /// Tests that the fluent configuration can configure a template service with a specific code language.
        /// </summary>
        /// <remarks>
        /// For this test, we're switching to VB, and using a @Code section:
        ///     <code>
        ///         @Code Dim name = "Matt" End Code
        ///         @name
        ///     </code>
        /// ... which should result in:
        ///     <code>
        ///         
        ///         Matt
        ///     </code>
        /// </remarks>
        [Test]
        [Category("VBNET")]
        public void FluentTemplateServiceConfiguration_CanConfigureTemplateService_WithSpecificCodeLanguage()
        {
            var config = new FluentTemplateServiceConfiguration(
                c => c.WithCodeLanguage(Language.VisualBasic));

#pragma warning disable 0618 // TODO: Update test.
            using (var service = new TemplateService(config))
#pragma warning restore 0618 // TODO: Update test.
            {
                const string template = "@Code Dim name = \"Matt\" End Code\n@name";
                const string expected = "\nMatt";

                string result = service.Parse(template, null, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }
#endif

        /// <summary>
        /// Tests that the fluent configuration can configure a template service with a specific encoding.
        /// </summary>
        [Test]
        public void FluentTemplateServiceConfiguration_CanConfigureTemplateService_WithSpecificEncoding()
        {
            var config = new FluentTemplateServiceConfiguration(
                c => c.WithEncoding(Encoding.Raw));

#pragma warning disable 0618 // TODO: Update test.
            using (var service = new TemplateService(config))
#pragma warning restore 0618 // TODO: Update test.
            {
                const string template = "<h1>Hello @Model.String</h1>";
                const string expected = "<h1>Hello Matt & World</h1>";

                var model = new { String = "Matt & World" };
                string result = service.Parse(template, model, null, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that the fluent configuration supports setting a custom activator.
        /// </summary>
        [Test]
        public void FluentTemplateServiceConfiguration_CanSetActivator_UsingActivator()
        {
            var mock = new Mock<IActivator>();

            var config = new FluentTemplateServiceConfiguration(c => c.ActivateUsing(mock.Object));

            Assert.That(config.Activator == mock.Object);
        }

        /// <summary>
        /// Tests that the fluent configuration supports setting the code language.
        /// </summary>
        [Category("VBNET")]
        [Test]
        public void FluentTemplateServiceConfiguration_CanSetCodeLanguage()
        {
            var config = new FluentTemplateServiceConfiguration(c => c.WithCodeLanguage(Language.VisualBasic));

            Assert.That(config.Language == Language.VisualBasic);
        }

        /// <summary>
        /// Tests that the fluent configuration supports setting the compiler service factory.
        /// </summary>
        [Test]
        public void FluentTemplateServiceConfiguration_CanSetCompilerServiceFactory()
        {
            var mock = new Mock<ICompilerServiceFactory>();

            var config = new FluentTemplateServiceConfiguration(c => c.CompileUsing(mock.Object));

            Assert.That(config.CompilerServiceFactory == mock.Object);
        }

        /// <summary>
        /// Tests that the fluent configuration supports setting the encoded string factory.
        /// </summary>
        [Test]
        public void FluentTemplateServiceConfiguration_CanSetEncodedStringFactory()
        {
            var mock = new Mock<IEncodedStringFactory>();

            var config = new FluentTemplateServiceConfiguration(c => c.EncodeUsing(mock.Object));

            Assert.That(config.EncodedStringFactory == mock.Object);
        }

        /// <summary>
        /// Tests that the fluent configuration supports setting the encoded string factory using a predefined encoding.
        /// </summary>
        [Test]
        public void FluentTemplateServiceConfiguration_CanSetEncoding_UsingHtmlEncoding()
        {
            var config = new FluentTemplateServiceConfiguration(c => c.WithEncoding(Encoding.Html));

            Assert.That(config.EncodedStringFactory is HtmlEncodedStringFactory);
        }

        /// <summary>
        /// Tests that the fluent configuration supports setting the encoded string factory using a predefined encoding.
        /// </summary>
        [Test]
        public void FluentTemplateServiceConfiguration_CanSetEncoding_UsingRawEncoding()
        {
            var config = new FluentTemplateServiceConfiguration(c => c.WithEncoding(Encoding.Raw));

            Assert.That(config.EncodedStringFactory is RawStringFactory);
        }

        /// <summary>
        /// Tests that the fluent configuration supports setting a custom activator delegate.
        /// </summary>
        [Test]
        public void FluentTemplateServiceConfigutation_CanSetActivator_UsingDelegate()
        {
            Func<InstanceContext, ITemplate> activator = i => null;

            var config = new FluentTemplateServiceConfiguration(c => c.ActivateUsing(activator));
            var delegateActivator = (DelegateActivator)config.Activator;

            Assert.That(delegateActivator.Activator == activator);
        }
        #endregion
    }
}