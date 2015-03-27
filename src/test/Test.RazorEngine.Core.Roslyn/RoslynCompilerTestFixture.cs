using NUnit.Framework;
using RazorEngine.Compilation;
using RazorEngine.Configuration;
using RazorEngine.Roslyn;
using RazorEngine.Templating;
using RazorEngine.Tests.TestTypes;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.RazorEngine.Core.Roslyn
{
    /// <summary>
    /// Some tests for the Roslyn Compiler backend.
    /// </summary>
    [TestFixture]
    public class RoslynCompilerTestFixture
    {
        /// <summary>
        /// Helper to run tests.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="withConfig"></param>
        public static void RunTestHelper(Action<IRazorEngineService> test, Action<TemplateServiceConfiguration> withConfig = null)
        {
            var defaultConfig = new Action<TemplateServiceConfiguration>((config) => {
                    config.Debug = true;
                    config.CompilerServiceFactory = new RoslynCompilerServiceFactory();
                });
            if (withConfig == null)
            {
                withConfig = defaultConfig;
            }
            else
            {
                var oldFunc = withConfig;
                withConfig = (config) =>
                {
                    defaultConfig(config);
                    oldFunc(config);
                };
            }
            try
            {
                var config = new TemplateServiceConfiguration();
                withConfig(config);
                using (var service = RazorEngineService.Create(config))
                {
                    test(service);
                }
            }
            catch (TemplateCompilationException e)
            {
                var source = e.CompilationData.SourceCode;
                Console.WriteLine("Generated source file: \n\n{0}", source ?? "SOURCE CODE NOT AVAILABLE!");
                e.CompilationData.DeleteAll();
                throw;
            }
        }


        /// <summary>
        /// Tests that a IEnumerable with anonymous objects works.
        /// </summary>
        [Test]
        public void Roslyn_DynamicIEnumerable()
        {
            RunTestHelper(service =>
            {
                const string template = @"@Enumerable.Count(Model.Data)";
                const string expected = "3";
                var anonArray = new[] { new { InnerData = 1 }, new { InnerData = 2 }, new { InnerData = 3 } };
                var model = new { Data = anonArray.Select(a => a) };
                string result = service.RunCompile(template, "test", null, model, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            });
        }

        /// <summary>
        /// Test that anonymous types within the template work.
        /// </summary>
        [Test]
        public void Roslyn_AnonymousTypeWithinTemplate_2()
        {
            RunTestHelper(service =>
            {
                const string template_child = "@Enumerable.Count(Model.Data)";
                const string template_parent = @"@Include(""Child"", new { Data = Model.Animals})";
                const string expected = "3";
                service.Compile(template_child, "Child", null);
                var anonArray = new[] { new Animal { Type = "1" }, new Animal { Type = "2" }, new Animal { Type = "3" } };
                var model = new AnimalViewModel { Animals = anonArray };
                string result = service.RunCompile(template_parent, "test", typeof(AnimalViewModel), model, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            });
        }

        /// <summary>
        /// Test that anonymous types within the template work.
        /// </summary>
        [Test]
        public void Roslyn_AnonymousTypeWithinTemplate()
        {
            RunTestHelper(service =>
            {
                const string template_child = "@Enumerable.Count(Model.Data)";
                const string template_parent = @"@Include(""Child"", new { Data = Model.Data})";
                const string expected = "3";
                service.Compile(template_child, "Child", null);
                var anonArray = new[] { new { InnerData = 1 }, new { InnerData = 2 }, new { InnerData = 3 } };
                var model = new { Data = anonArray.Select(a => a) };
                string result = service.RunCompile(template_parent, "test", null, model, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            });
        }

        /// <summary>
        /// Tests that a specific encoding works.
        /// </summary>
        [Test]
        public void Roslyn_WithSpecificEncoding()
        {
            RunTestHelper(service =>
            {
                const string template = "<h1>Hello @Model.String</h1>";
                const string expected = "<h1>Hello Matt & World</h1>";

                var model = new { String = "Matt & World" };
                string result = service.RunCompile(template, "test", null, model, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }, (c) => c.EncodedStringFactory = new RawStringFactory());
        }

        /// <summary>
        /// Tests that the compilation message is useful.
        /// </summary>
        [Test]
        public void Roslyn_GetInformativeErrorMessage()
        {
            RunTestHelper(service =>
            {
                const string template = "@foreach (var i in Model.Unknown) { @i }";
                var exn = Assert.Throws<TemplateCompilationException>(() =>
                {
                    string result = service.RunCompile(template, "test", typeof(object), new object());
                });
                exn.CompilationData.DeleteAll();
                var msg = exn.Message;
                var errorMessage =
                    string.Format(
                        "An expected substring ({{0}}) was not found in: {0}",
                        msg.Replace("{", "{{").Replace("}", "}}"));

                // Compiler error
                Assert.IsTrue(
                    msg.Contains("does not contain a definition for"),
                    string.Format(errorMessage, "compiler error"));
                // Template
                Assert.IsTrue(msg.Contains(template),
                    string.Format(errorMessage, "template"));
                // Temp files
                Assert.IsTrue(msg.Contains("Temporary files of the compilation can be found"),
                    string.Format(errorMessage, "temp files"));
                // C# source code
                Assert.IsTrue(msg.Contains("namespace " + CompilerServiceBase.DynamicTemplateNamespace),
                    string.Format(errorMessage, "C# source"));
            });
        }

        /// <summary>
        /// Tests that a runtime exception is useful.
        /// </summary>
        [Test]
        public void Roslyn_GetInformativeRuntimeErrorMessage()
        {
            Assert.Ignore("Fixme: Include debug info in roslyn build");
            RunTestHelper(service =>
            {
                const string template = "@foreach (var i in Model.Unknown) { @i }";
                string file = Path.GetTempFileName();
                try
                {
                    File.WriteAllText(file, template);
                    var source = new LoadedTemplateSource(template, file);
                    var exn = Assert.Throws<Microsoft.CSharp.RuntimeBinder.RuntimeBinderException>(() =>
                    {
                        string result = service.RunCompile(source, "test", null, new object());
                    });
                    // We now have a reference to our template in the stacktrace
                    var stack = exn.StackTrace.ToLowerInvariant();
                    var expected = file.ToLowerInvariant();
                    Assert.IsTrue(
                        stack.Contains(expected),
                        "Could not find reference to template (" + expected + ") in stacktrace: \n" +
                        stack);
                }
                finally
                {
                    File.Delete(file);
                }
            });
        }

        /// <summary>
        /// Tests whether we can delete tempfiles when DisableTempFileLocking is true.
        /// </summary>
        [Test]
        public void Roslyn_TestDisableTempFileLocking()
        {
            var cache = new DefaultCachingProvider(t => { });
            var template = "@Model.Property";
            RunTestHelper(service =>
            {
                var model = new { Property = 0 };
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("0", result.Trim());
            }, config =>
            {
                config.Debug = false;
                config.CachingProvider = cache;
                config.DisableTempFileLocking = true;
            });
            ICompiledTemplate compiledTemplate;
            Assert.IsTrue(cache.TryRetrieveTemplate(new NameOnlyTemplateKey("key", ResolveType.Global, null), null, out compiledTemplate));
            var data = compiledTemplate.CompilationData;
            var folder = data.TmpFolder;
            Assert.IsTrue(Directory.Exists(folder));
            data.DeleteAll();
            Assert.IsFalse(Directory.Exists(folder));
        }
    }
}
