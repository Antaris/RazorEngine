using ImpromptuInterface;
using NUnit.Framework;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.RazorEngine
{
    [TestFixture]
    public class RazorEngineServiceTestFixture
    {
        public static void RunTestHelper(Action<IRazorEngineService> test, Action<TemplateServiceConfiguration> withConfig = null)
        {
            if (withConfig == null)
            {
                withConfig = (config) => { config.Debug = true; };
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

        public interface IMyInterface
        {
            string Test();
        }
        public class MyClass : IMyInterface
        {
            public string Test()
            {
                return "test";
            }
            public string More()
            {
                return "more";
            }
        }

        /// <summary>
        /// Tests that the fluent configuration can configure a template service with a specific encoding.
        /// </summary>
        [Test]
        public void RazorEngineService_ActLikeTest()
        {
            Assert.Ignore();
            dynamic m = new ExpandoObject();
            m.Test = new Func<string>(() => "mytest");
            m.More = new Func<string>(() => "mymore");
            dynamic _m = Impromptu.DynamicActLike(m, typeof(IMyInterface));
            Assert.AreEqual("mytest", _m.Test());
            dynamic o = new MyClass();
            dynamic _o = Impromptu.DynamicActLike(o, typeof(IMyInterface));
            Assert.AreEqual("test", _o.Test());

            Assert.AreEqual("more", _o.More());
            Assert.AreEqual("mymore", _m.More());
        }

        /// <summary>
        /// Tests that the fluent configuration can configure a template service with a specific encoding.
        /// </summary>
        [Test]
        public void RazorEngineService_DynamicIEnumerable()
        {
            Assert.Ignore();
            RunTestHelper(service =>
            {
                const string template = @"@Enumerable.Count(Model.Data)";
                const string expected = "3";
                var anonArray = new[] { new { InnerData = 1 }, new { InnerData = 2 }, new { InnerData = 3 } };
                var model = new { Data = anonArray.Select(a => a) };
                string result = service.RunCompileOnDemand(template, "test", null, model, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }, (c) => c.EncodedStringFactory = new RawStringFactory());
        }


        /// <summary>
        /// Tests that the fluent configuration can configure a template service with a specific encoding.
        /// </summary>
        [Test]
        public void RazorEngineService_WithSpecificEncoding()
        {
            RunTestHelper(service =>
            {
                const string template = "<h1>Hello @Model.String</h1>";
                const string expected = "<h1>Hello Matt & World</h1>";

                var model = new { String = "Matt & World" };
                string result = service.RunCompileOnDemand(template, "test", null, model, null);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }, (c) => c.EncodedStringFactory = new RawStringFactory());
        }

        /// <summary>
        /// Tests that a simple template with an iterator model can be parsed.
        /// </summary>
        [Test]
        public void RazorEngineService_GetInformativeErrorMessage()
        {
            RunTestHelper(service =>
            {
                const string template = "@foreach (var i in Model.Unknown) { @i }";
                var exn = Assert.Throws<TemplateCompilationException>(() =>
                {
                    string result = service.RunCompileOnDemand(template, "test", typeof(object), new object());
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
                Assert.IsTrue(msg.Contains("namespace CompiledRazorTemplates.Dynamic {"),
                    string.Format(errorMessage, "C# source"));
            });
        }

        /// <summary>
        /// Tests that a simple template with an iterator model can be parsed.
        /// </summary>
        [Test]
        public void RazorEngineService_GetInformativeRuntimeErrorMessage()
        {
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
                        string result = service.RunCompileOnDemand(source, "test", null, new object());
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
    }
}
