using RazorEngine.Compilation.ImpromptuInterface;
using NUnit.Framework;
using RazorEngine.Compilation;
using RazorEngine.Compilation.ReferenceResolver;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Tests.TestTypes;
using RazorEngine.Text;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Test.RazorEngine.TestTypes.BaseTypes;
using System.Runtime.Remoting;
using System.Collections.Concurrent;
using System.Threading;

namespace Test.RazorEngine
{
    /// <summary>
    /// Tests for the RazorEngineService API.
    /// </summary>
    [TestFixture]
    public class RazorEngineServiceTestFixture
    {
        /// <summary>
        /// Helper API to run tests.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="withConfig"></param>
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

        /// <summary>
        /// Test Class.
        /// </summary>
        public interface IMyInterface
        {
            /// <summary>
            /// Test Class.
            /// </summary>
            string Test();
        }
        /// <summary>
        /// Test Class.
        /// </summary>
        public class MyClass : IMyInterface
        {
            /// <summary>
            /// Test Class.
            /// </summary>
            public string Test()
            {
                return "test";
            }
            /// <summary>
            /// Test Class.
            /// </summary>
            public string More()
            {
                return "more";
            }
        }

        /// <summary>
        /// Test that DynamicActLike also gives access to all previous method.
        /// This is a remainder that we changed the Impromptu code and make 
        /// ActLikeProxy inherit from ImpromptuForwarder (and setting the Target property).
        /// If this test ever fails make sure to fix that because we need this behavior.
        /// </summary>
        [Test]
        public void RazorEngineService_ActLikeTest()
        {
            dynamic m = new ExpandoObject();
            m.Test = new Func<string>(() => "mytest");
            m.More = new Func<string>(() => "mymore");
            dynamic _m = Impromptu.DynamicActLike(m, typeof(IMyInterface));
            Assert.AreEqual("mytest", _m.Test());
            var __m = (IMyInterface)_m;
            Assert.AreEqual("mytest", __m.Test());


            dynamic o = new MyClass();
            dynamic _o = Impromptu.DynamicActLike(o, typeof(IMyInterface));
            Assert.AreEqual("test", _o.Test());
            var __o = (IMyInterface)_o;
            Assert.AreEqual("test", __o.Test());

            Assert.AreEqual("more", _o.More());
            Assert.AreEqual("mymore", _m.More());
        }

        /// <summary>
        /// Check that xml configuration has a template manager.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RazorEngineService_CheckConfiguration()
        {
            var config = new TemplateServiceConfiguration();
            config.TemplateManager = null;
            RazorEngineService.Create(config);
        }

        /// <summary>
        /// Tests that the fluent configuration can configure a template service with a specific encoding.
        /// </summary>
        [Test]
        public void RazorEngineService_DynamicIEnumerable()
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
        public void RazorEngineService_AnonymousTypeWithinTemplate_2()
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
        public void RazorEngineService_AnonymousTypeWithinTemplate()
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
                string result = service.RunCompile(template, "test", null, model, null);

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


        class TestHelperReferenceResolver : IReferenceResolver
        {
            public IEnumerable<CompilerReference> GetReferences
            (
                TypeContext context,
                IEnumerable<CompilerReference> includeAssemblies = null
            )
            {
                // We need to return this standard set or even simple views blow up on
                // a missing reference to System.Linq.
                var loadedAssemblies = (new UseCurrentAssembliesReferenceResolver()).GetReferences(null);
                foreach (var reference in loadedAssemblies)
                    yield return reference;
                yield return CompilerReference.From("test/TestHelper.dll");
            }
        }

        /// <summary>
        /// Tests that we can use types from other assemblies in templates.
        /// </summary>
        [Test]
        public void RazorEngineService_CheckThatWeCanUseUnknownTypes()
        {
            RunTestHelper(service =>
            {
                var assembly = Assembly.LoadFrom("test/TestHelper.dll");
                Type modelType = assembly.GetType("TestHelper.TestClass", true);
                var model = Activator.CreateInstance(modelType);
                var template = @"
@{
    var t = new TestHelper.TestClass();
}
@t.TestProperty";
                string compiled = service.RunCompile(template, Guid.NewGuid().ToString(), modelType, model);
                
            }, config =>
            {
                config.ReferenceResolver = new TestHelperReferenceResolver();
            });
        }


        /// <summary>
        /// Tests that we can use types from other assemblies in templates.
        /// Even when the type can be loaded.
        /// </summary>
        [Test]
        public void RazorEngineService_CheckThatWeCanUseUnknownTypesAtExecuteTime()
        {
            RunTestHelper(service =>
            {
                var template = @"
@{
    var t = new TestHelper.TestClass();
}
@t.TestProperty";
                string compiled = service.RunCompile(template, Guid.NewGuid().ToString());

            }, config =>
            {
                config.ReferenceResolver = new TestHelperReferenceResolver();
            });
        }

        /// <summary>
        /// Tests that we fail with the right exception
        /// </summary>
        [Test]
        public void RazorEngineService_CheckParsingFails()
        {
            RunTestHelper(service =>
            {
                // Tag must be closed!
                var template = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
@if (true)
{
    <Compile>
}";
                Assert.Throws<TemplateParsingException>(() =>
                {
                    string compiled = service.RunCompile(template, Guid.NewGuid().ToString());
                });
            });
        }

        /// <summary>
        /// Tests that we fail with the right exception
        /// </summary>
        [Test]
        public void RazorEngineService_CustomLocalizerHelper_OverrideModelType()
        {
            RunTestHelper(service =>
            {
                // Tag must be closed!
                var model = new TemplateViewData() { Language = "lang", Model = new Person() { Forename = "forname", Surname = "surname" } };
                var template = @"@Model.Forename @Include(""test"") @Localizer.Language";

                service.Compile("@Model.Surname", "test", typeof(Person));
                string result = service.RunCompile(template, Guid.NewGuid().ToString(), typeof(Person), model);
                Assert.AreEqual("forname surname lang", result);
            }, config =>
            {
                config.BaseTemplateType = typeof(AddLanguageInfo_OverrideModelType<>);
            });
        }


        /// <summary>
        /// Tests that overriding Include and ResolveLayout can be used to hook custom data into a custom base class.
        /// </summary>
        [Test]
        public void RazorEngineService_CustomLocalizerHelper_OverrideInclude()
        {
            RunTestHelper(service =>
            {
                // Tag must be closed!
                var model = new TemplateViewData() { Language = "lang", Model = new Person() { Forename = "forname", Surname = "surname" } };
                var template = @"@Model.Forename @Include(""test"") @Localizer.Language";

                service.Compile("@Model.Surname", "test", typeof(Person));
                string result = service.RunCompile(template, Guid.NewGuid().ToString(), typeof(Person), model);
                Assert.AreEqual("forname surname lang", result);
            }, config =>
            {
                config.BaseTemplateType = typeof(AddLanguageInfo_OverrideInclude<>);
            });
        }


        /// <summary>
        /// Tests that we can use ViewBag to hook new data into a custom TemplateBase class.
        /// </summary>
        [Test]
        public void RazorEngineService_CustomLocalizerHelper_ViewBag()
        {
            RunTestHelper(service =>
            {
                var model = new Person() { Forename = "forname", Surname = "surname" };
                var template = @"@Model.Forename @Include(""test"") @Localizer.Language";

                service.Compile("@Model.Surname", "test", typeof(Person));
                dynamic viewbag = new DynamicViewBag();
                viewbag.Language = "lang";
                string result = service.RunCompile(template, Guid.NewGuid().ToString(), typeof(Person), model, (DynamicViewBag) viewbag);
                Assert.AreEqual("forname surname lang", result);
            }, config =>
            {
                config.BaseTemplateType = typeof(AddLanguageInfo_Viewbag<>);
            });
        }

        /// <summary>
        /// Tests that we can access the Viewbag from within the SetModel method.
        /// </summary>
        [Test]
        public void RazorEngineService_CheckViewbagAccessFromSetModel()
        {
            RunTestHelper(service =>
            {
                var model = new Person() { Forename = "forname", Surname = "surname" };
                var template = @"@Model.Forename @Include(""test"") @Localizer.Language";

                service.Compile("@Model.Surname", "test", typeof(Person));
                dynamic viewbag = new DynamicViewBag();
                viewbag.Language = "lang";
                string result = service.RunCompile(template, Guid.NewGuid().ToString(), typeof(Person), model, (DynamicViewBag)viewbag);
                Assert.AreEqual("forname surname lang", result);
            }, config =>
            {
                config.BaseTemplateType = typeof(AddLanguageInfo_Viewbag_SetModel<>);
            });
        }

        /// <summary>
        /// Tests that nested base classes work.
        /// </summary>
        [Test]
        public void RazorEngineService_TestNestedBaseClass()
        {
            RunTestHelper(service =>
            {
                var model = new Person() { Forename = "forname", Surname = "surname" };
                var template = @"@TestProperty";

                string result = service.RunCompile(template, Guid.NewGuid().ToString(), typeof(Person), model);
                Assert.AreEqual("mytest", result);
            }, config =>
            {
                config.BaseTemplateType = typeof(HostingClass.NestedBaseClass<>);
            });
        }

        /// <summary>
        /// Tests that nested base classes work.
        /// </summary>
        [Test]
        public void RazorEngineService_TestNestedModelClass()
        {
            RunTestHelper(service =>
            {
                var template = @"@Model.TestProperty";
                string result = service.RunCompile(template, "key", typeof(HostingClass.NestedClass), 
                    new HostingClass.NestedClass() { TestProperty = "test" });
                Assert.AreEqual("test", result);
            });
        }

        /// <summary>
        /// Tests that nested base classes work.
        /// </summary>
        [Test]
        public void RazorEngineService_TestNestedGenericModelClass()
        {
            RunTestHelper(service =>
            {
                var template = @"@Model.TestProperty";
                string result = service.RunCompile(template, "key", typeof(HostingClass.GenericNestedClass<string>),
                    new HostingClass.GenericNestedClass<string>() { TestProperty = "test" });
                Assert.AreEqual("test", result);
            });
        }

        /// <summary>
        /// Tests that enumerating dynamic works.
        /// </summary>
        [Test]
        public void RazorEngineService_TestEnumeratingDynamicObject()
        {
            RunTestHelper(service =>
            {
                var list = new List<Person>();
                list.Add(new Person() { Forename = "test1" });
                list.Add(new Person() { Forename = "test2" });
                var model = new { Types = list };
                var template = @"@{
var l = new System.Collections.Generic.List<dynamic>();
foreach (var t in Model.Types) { l.Add(t); }
}@foreach (var m in l) {<text>@m.Forename</text>}";
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("test1test2", result);
            });
        }

        /// <summary>
        /// Tests that casting back to IEnumerable works.
        /// </summary>
        [Test]
        public void RazorEngineService_TestImplicitCastIEnumerable()
        {
            RunTestHelper(service =>
            {
                var list = new List<Person>();
                list.Add(new Person() { Forename = "test1" });
                list.Add(new Person() { Forename = "test2" });
                var model = new { Types = list };
                var template = @"@{
var l = new System.Collections.Generic.List<dynamic>();
IEnumerable<dynamic> types = Model.Types;
l.AddRange(types);
}@foreach (var m in l) {<text>@m.Forename</text>}";
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("test1test2", result);
            });
        }

        /// <summary>
        /// Tests that casting back to IEnumerable works.
        /// </summary>
        [Test]
        public void RazorEngineService_TestCastingToIEnumerable()
        {
            RunTestHelper(service =>
            {
                var list = new List<Person>();
                list.Add(new Person() { Forename = "test1" });
                list.Add(new Person() { Forename = "test2" });
                var model = new { Types = list };
                var template = @"@{
var l = new System.Collections.Generic.List<dynamic>();
l.AddRange((IEnumerable<dynamic>)Model.Types);
}@foreach (var m in l) {<text>@m.Forename</text>}";
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("test1test2", result);
            });
        }

        /// <summary>
        /// Tests that we can get back the concrete type if we want it.
        /// </summary>
        [Test]
        public void RazorEngineService_TestImplicitCastToConcreteType()
        {
            RunTestHelper(service =>
            {
                var model = new { Person = new Person() { Forename = "test1" } };
                var template = @"@{
RazorEngine.Tests.TestTypes.Person p = Model.Person;
}
@p.Forename";
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("test1", result.TrimStart());
            });
        }

        /// <summary>
        /// Tests that we can get back the concrete type if we want it.
        /// </summary>
        [Test]
        public void RazorEngineService_TestExplicitCastToConcreteType()
        {
            RunTestHelper(service =>
            {
                var model = new { Person = new Person() { Forename = "test1" } };
                var template = @"@{
var p = (RazorEngine.Tests.TestTypes.Person)Model.Person;
}
@p.Forename";
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("test1", result.TrimStart());
            });
        }

        /// <summary>
        /// Enum for testing purposes.
        /// </summary>
        public enum MyEnum
        {
            /// <summary>
            /// State 1
            /// </summary>
            State1, 
            /// <summary>
            /// State 2
            /// </summary>
            State2
        }

        /// <summary>
        /// Tests that we can compare dynamic object with an enum.
        /// </summary>
        [Test]
        public void RazorEngineService_TestCompareWithEnumAfterUnwrap()
        {
            RunTestHelper(service =>
            {
                var model = new { State = MyEnum.State1 };
                var template =
@"@using RazorEngine.Compilation
@{
if (RazorDynamicObject.Unwrap(Model.State) == Test.RazorEngine.RazorEngineServiceTestFixture.MyEnum.State1)
{<text>correct</text>}else{<text>wrong</text>}}";
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("correct", result.TrimStart());
            });
        }
        
        /// <summary>
        /// Tests that we can compare dynamic object with an enum.
        /// </summary>
        [Test]
        //[Ignore]
        public void RazorEngineService_TestCompareWithEnum()
        {
            RunTestHelper(service =>
            {
                var model = new { State = MyEnum.State1 };
                var template =
@"@{
if (Model.State == Test.RazorEngine.RazorEngineServiceTestFixture.MyEnum.State1)
{<text>correct</text>}else{<text>wrong</text>}}";
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("correct", result.TrimStart());
            });
        }

        /// <summary>
        /// Tests that we can compare dynamic object with an enum.
        /// </summary>
        [Test]
        public void RazorEngineService_TestCompareWithEnumImplicitCastBefore()
        {
            RunTestHelper(service =>
            {
                var model = new { State = MyEnum.State1 };
                var template =
@"@{Test.RazorEngine.RazorEngineServiceTestFixture.MyEnum v = Model.State;
if (v == Test.RazorEngine.RazorEngineServiceTestFixture.MyEnum.State1){<text>correct</text>}else{<text>wrong</text>}}";
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("correct", result.TrimStart());
            });
        }

        /// <summary>
        /// Tests that we can compare dynamic object with an enum.
        /// </summary>
        [Test]
        public void RazorEngineService_TestCompareWithEnumExplicitCastBefore()
        {
            RunTestHelper(service =>
            {
                var model = new { State = MyEnum.State1 };
                var template =
@"@{
if ((Test.RazorEngine.RazorEngineServiceTestFixture.MyEnum)Model.State == Test.RazorEngine.RazorEngineServiceTestFixture.MyEnum.State1){<text>correct</text>}else{<text>wrong</text>}}";
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("correct", result.TrimStart());
            });
        }

#if !RAZOR4 // @helper was removed from Razor 4
        /// <summary>
        /// Tests that debugging works with @helper syntax.
        /// </summary>
        [Test]
        public void RazorEngineService_TestDebuggingWithHelperWithTemplateFile()
        {
            var cache = new DefaultCachingProvider();
            var file = System.IO.Path.GetTempFileName();

            var template =
@"@helper Display(int price) {
    if (price == 0) {
        <text>free</text>
    } else {
        <text>@price</text>
    }
}
@Display(Model.MyPrice)";
            File.WriteAllText(file, template);
            RunTestHelper(service =>
            {
                var model = new { MyPrice = 0 };
                string result = service.RunCompile(new LoadedTemplateSource(template, file), "key", null, model);
                Assert.AreEqual("free", result.Trim());
            }, config => config.CachingProvider = cache);
            ICompiledTemplate compiledTemplate;
            Assert.IsTrue(cache.TryRetrieveTemplate(new NameOnlyTemplateKey("key", ResolveType.Global, null), null, out compiledTemplate));
            // Contains line pragmas with the template file.
            Assert.IsTrue(compiledTemplate.CompilationData.SourceCode.Contains(file), "");
            File.Delete(file);
        }

        /// <summary>
        /// Tests that debugging works with @helper syntax.
        /// </summary>
        [Test]
        public void RazorEngineService_TestDebuggingWithHelper()
        {
            var cache = new DefaultCachingProvider();
            var template =
@"@helper Display(int price) {
    if (price == 0) {
        <text>free</text>
    } else {
        <text>@price</text>
    }
}
@Display(Model.MyPrice)";
            RunTestHelper(service =>
            {
                var model = new { MyPrice = 0 };
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("free", result.Trim());
            }, config => config.CachingProvider = cache);
            ICompiledTemplate compiledTemplate;
            Assert.IsTrue(cache.TryRetrieveTemplate(new NameOnlyTemplateKey("key", ResolveType.Global, null), null, out compiledTemplate));
            // #line hidden should be removed, so make debugging work, see https://github.com/Antaris/RazorEngine/issues/253.
            Assert.IsFalse(compiledTemplate.CompilationData.SourceCode.Contains("#line hidden"));
        }
#endif


        /// <summary>
        /// Tests whether we can delete tempfiles when DisableTempFileLocking is true.
        /// </summary>
        [Test]
        public void RazorEngineService_TestThatWeThrowWhenDebugAndDisableLockingAreEnabled()
        {
            var template = "@Model.Property";
            Assert.Throws<InvalidOperationException>(() =>
            {
                RazorEngineServiceTestFixture.RunTestHelper(service =>
                {
                    var model = new { Property = 0 };
                    string result = service.RunCompile(template, "key", null, model);
                    Assert.AreEqual("0", result.Trim());
                }, config =>
                {
                    config.Debug = true;
                    config.DisableTempFileLocking = true;
                });
            });
        }

        /// <summary>
        /// Test that we can check for missing properties
        /// </summary>
        [Test]
        public void RazorEngineService_CheckForMissingProperty()
        {
            var template = @"
@try {
  // save all properties which might be missing beforehand in variables (otherwise there will be partial ouput).
  var link = Model.Foo.Link; 
  // Use them as you would normally.
  <a href=""@link"">@Model.Foo.Text</a>
}
catch (RuntimeBinderException) {
  <p>@Model.Foo.Text</p>
}";
            RunTestHelper(service =>
            {
                service.Compile(template, "key", null);
                dynamic model = new { Foo = new { Link = "link", Text = "text"} };
                string result = service.Run("key", null, (object)model);
                Assert.AreEqual("<a href=\"link\">text</a>", result.Trim());
                
                model = new { Foo = new { Text = "text" } };
                result = service.Run("key", null, (object)model);
                Assert.AreEqual("<p>text</p>", result.Trim());
            }, config => {
                config.Namespaces.Add("RazorEngine.Compilation");
                config.Namespaces.Add("Microsoft.CSharp.RuntimeBinder");
                //config.AllowMissingPropertiesOnDynamic = true; 
                config.Debug = true; });
        }
        
        /// <summary>
        /// Test that we can check for missing properties with empty string.
        /// </summary>
        [Test]
        public void RazorEngineService_CheckForMissingPropertyEmptyString()
        {
            var template = @"
@if (!string.IsNullOrEmpty(Model.Foo.Link.ToString())) {
  <a href=""@Model.Foo.Link"">@Model.Foo.Text</a>
}
else {
  <p>@Model.Foo.Text</p>
}";
            RunTestHelper(service =>
            {
                service.Compile(template, "key", null);
                dynamic model = new { Foo = new { Link = "link", Text = "text" } };
                string result = service.Run("key", null, (object)model);
                Assert.AreEqual("<a href=\"link\">text</a>", result.Trim());

                model = new { Foo = new { Text = "text" } };
                result = service.Run("key", null, (object)model);
                Assert.AreEqual("<p>text</p>", result.Trim());
            }, config =>
            {
                config.Namespaces.Add("RazorEngine.Compilation");
                config.Namespaces.Add("Microsoft.CSharp.RuntimeBinder");
                config.AllowMissingPropertiesOnDynamic = true; 
                config.Debug = true;
            });
        }

        /// <summary>
        /// Tests InvalidatingCachingProvider.
        /// </summary>
        [Test]
        public void RazorEngineService_TestInvalidatingCachingProvider()
        {
            var cache = new InvalidatingCachingProvider();
            var mgr = new DelegateTemplateManager();
            var template =
@"@if (Model.MyPrice == 0) {
    <text>free</text>
} else {
    <text>@Model.MyPrice</text>
}";
            var template2 =
@"@if (Model.MyPrice == 0) {
    <text>totally free</text>
} else {
    <text>@Model.MyPrice</text>
}";
            RunTestHelper(service =>
            {
                var model = new { MyPrice = 0 };
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("free", result.Trim());
                cache.InvalidateCache(service.GetKey("key"));
                mgr.RemoveDynamic(service.GetKey("key"));
                string result2 = service.RunCompile(template2, "key", null, model);
                Assert.AreEqual("totally free", result2.Trim());
            }, config => {
                config.CachingProvider = cache;
                config.TemplateManager = mgr;
            });
        }

        /// <summary>
        /// Tests that ResolvePathTemplateManager.
        /// </summary>
        [Test]
        public void RazorEngineService_TestResolvePathTemplateManager()
        {
            var cache = new InvalidatingCachingProvider();
            var temp = Path.GetTempFileName();
            var template2File = Path.GetTempFileName();
            File.Delete(temp);
            try
            {
                Directory.CreateDirectory(temp);
                var mgr = new ResolvePathTemplateManager(new[] { temp });
                var template = @"free";
                var template2 = @"template2";
                File.WriteAllText(template2File, template2);

                var templateFileName = Path.ChangeExtension(Path.GetRandomFileName(), ".cshtml");
                var templateName = Path.GetFileNameWithoutExtension(templateFileName);
                var templateFile = Path.Combine(temp, templateFileName);
                File.WriteAllText(templateFile, template);

                RunTestHelper(service =>
                {
                    var model = new { MyPrice = 0 };
                    string result = service.RunCompile(templateName, null, model);
                    Assert.AreEqual("free", result.Trim());
                    string result2 = service.RunCompile(template2File, null, model);
                    Assert.AreEqual("template2", result2.Trim());
                }, config =>
                {
                    config.CachingProvider = cache;
                    config.TemplateManager = mgr;
                });
            }
            finally
            {
                Directory.Delete(temp, true);
                if (File.Exists(template2File)) { File.Delete(template2File); }
            }
        }

        /// <summary>
        /// Tests ResolvePathTemplateManager.
        /// </summary>
        [Test]
        public void RazorEngineService_TestWatchingResolvePathTemplateManager()
        {
            var temp = Path.GetTempFileName();
            var template2File = Path.GetTempFileName();
            File.Delete(temp);
            var prev = Environment.GetEnvironmentVariable("MONO_MANAGED_WATCHER");
            try
            {
                if (IsolatedRazorEngineServiceTestFixture.IsRunningOnMono())
                {
                    Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled");
                }
                Directory.CreateDirectory(temp);
                var cache = new InvalidatingCachingProvider();
                var mgr = new WatchingResolvePathTemplateManager(new [] { temp }, cache);
                var template = @"initial";
                var templateChanged = @"next";


                var templateFileName = Path.ChangeExtension(Path.GetRandomFileName(), ".cshtml");
                var templateName = Path.GetFileNameWithoutExtension(templateFileName);
                var templateFile = Path.Combine(temp, templateFileName);
                File.WriteAllText(templateFile, template);

                RunTestHelper(service =>
                {
                    var model = new { MyPrice = 0 };
                    string result = service.RunCompile(templateFileName, null, model);
                    Assert.AreEqual("initial", result.Trim());

                    File.WriteAllText(templateFile, templateChanged);
                    Thread.Sleep(2000); // wait for the events to kick in.

                    string result2 = service.RunCompile(templateFileName, null, model);
                    Assert.AreEqual("next", result2.Trim());
                }, config =>
                {
                    config.CachingProvider = cache;
                    config.TemplateManager = mgr;
                });
            }
            finally
            {
                Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", prev);
                Directory.Delete(temp, true);
                if (File.Exists(template2File)) { File.Delete(template2File); }
            }
        }
    }
}
