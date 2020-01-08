namespace RazorEngine.Tests.TestTypes.Issues
{
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    using NUnit.Framework;

    using Templating;
    using Test.RazorEngine;

    /// <summary>
    /// Provides tests for the Release 3.6
    /// </summary>
    [TestFixture]
    public class Release_3_6_TestFixture
    {
        /// <summary>
        /// A test class which should not be serializable.
        /// </summary>
        public class Unserializable
        {
            
        }

        /// <summary>
        /// See https://github.com/Antaris/RazorEngine/issues/267
        /// </summary>
        [Test]
        public void RazorEngineService_Issue267()
        {
            var template = @"test";
            RazorEngineServiceTestFixture.RunTestHelper(service =>
            {
                try
                {   
                    System.Runtime.Remoting.Messaging.CallContext.LogicalSetData("Unserializable", new Unserializable());
                    service.Compile(template, "key", null);
                    string result = service.Run("key", null, (object)null);
                    Assert.AreEqual("test", result.Trim());
                }
                finally
                {
                    System.Runtime.Remoting.Messaging.CallContext.FreeNamedDataSlot("Unserializable");
                }
            });
        }

        /// <summary>
        /// See https://github.com/Antaris/RazorEngine/issues/267
        /// </summary>
        [Test]
        public void RazorEngineService_Issue267Ext()
        {
            var template = @"test";
            try
            {
                System.Runtime.Remoting.Messaging.CallContext.LogicalSetData("Unserializable", new Unserializable());
                RazorEngineServiceTestFixture.RunTestHelper(service =>
                {
                    service.Compile(template, "key", null);
                    string result = service.Run("key", null, (object)null);
                    Assert.AreEqual("test", result.Trim());
                });
            }
            finally
            {
                System.Runtime.Remoting.Messaging.CallContext.FreeNamedDataSlot("Unserializable");
            }
        }

        /// <summary>
        /// Anonymous classes do not work when wrapped in dynamic (via a method call).
        /// 
        /// Issue 67: https://github.com/Antaris/RazorEngine/issues/67
        /// </summary>
        [Test]
        public void Issue67_CollectionOfAnonymous()
        {
            RazorEngineServiceTestFixture.RunTestHelper(service =>
            {
                const string template = @"@foreach(var x in Model){
@x.Number
}";
                string expected = "123";
                var list = new List<Person>() {
                    new Person() { Age = 1 },
                    new Person() { Age = 2 },
                    new Person() { Age = 3 }
                };
                dynamic collectionOfAnonymous = list.Select(p => new { Number = p.Age }).ToList();
                var arrayOfAnonymous = collectionOfAnonymous.ToArray();
                string applied_1 = service.RunCompile(template, "test1", null, (object)collectionOfAnonymous);
                //string applied_2 = service.RunCompileOnDemand(template, "test2", (Type)collectionOfAnonymous.GetType(), (object)collectionOfAnonymous);
                string applied_3 = service.RunCompile(template, "test3", null, (object)arrayOfAnonymous);
                //string applied_4 = service.RunCompileOnDemand(template, "test4", (Type)arrayOfAnonymous.GetType(), (object)arrayOfAnonymous);

                Assert.AreEqual(expected, applied_1);
                //Assert.AreEqual(expected, applied_2);
                Assert.AreEqual(expected, applied_3);
                //Assert.AreEqual(expected, applied_4);
            });
        }

        /// <summary>
        /// Using @Raw within href attribute doesn't work.
        /// 
        /// Issue 181: https://github.com/Antaris/RazorEngine/issues/181
        /// </summary>
        [Test]
        public void Issue181_RawDoesntWork()
        {
            using (var service = RazorEngineService.Create())
            {
                const string link = "http://home.mysite.com/?a=b&c=1&new=1&d=3&e=0&g=1";
                const string template_1 = "@Raw(Model.Data)";
                const string expected_1 = link;
                const string template_2 = "<a href=\"@Raw(Model.Data)\">@Raw(Model.Data)</a>";
                string expected_2 = string.Format("<a href=\"{0}\">{0}</a>", link);

                var model = new { Data = link };
                var template1Key = service.GetKey(nameof(template_1));
                var template2Key = service.GetKey(nameof(template_2));

                service.AddTemplate(template1Key, template_1);
                service.AddTemplate(template2Key, template_2);

                var result_1 = service.RunCompile(template1Key, model: model);
                var result_2 = service.RunCompile(template2Key, model: model);

                Assert.AreEqual(expected_1, result_1);
                Assert.AreEqual(expected_2, result_2);
            }
        }
        
#if NET45
        /// <summary>
        /// Parser fails on layout sections if closing brace is not on a new line.
        /// This is a bug on Microsoft.Asp.Razor 2.0.30506.0.
        ///  
        /// Issue 93: https://github.com/Antaris/RazorEngine/issues/93
        /// </summary>
        [Test]
        public void Issue93_SectionParsing()
        {
            using (var service = RazorEngineService.Create())
            {
                string parent = @"@RenderSection(""MySection"", false)";
                string section_test_1 = "@{ Layout = \"ParentLayout\"; }@section MySection { My section content }";
                string section_test_2 = "@{ Layout = \"ParentLayout\"; }@section MySection {\nMy section content\n}";
                string section_test_3 = "@{ Layout = \"ParentLayout\"; }@section MySection {\nMy section content\na}";
                string expected_1 = " My section content ";
                string expected_2 = "\nMy section content\n";
                string expected_3 = "\nMy section content\na";
                var parentKey = service.GetKey("ParentLayout");
                service.AddTemplate(parentKey, parent);

                var result_1 = service.RunCompile(section_test_1, nameof(section_test_1));
                var result_2 = service.RunCompile(section_test_2, nameof(section_test_2));
                var result_3 = service.RunCompile(section_test_3, nameof(section_test_3));

                Assert.AreEqual(expected_1, result_1);
                Assert.AreEqual(expected_2, result_2);
                Assert.AreEqual(expected_3, result_3);
            }
        }
#endif

        /// <summary>
        /// Using nested sections doesn't work.
        /// 
        /// Issue 163: https://github.com/Antaris/RazorEngine/issues/163
        /// </summary>
        [Test]
        public void Issue163_SectionRedefinition()
        {
            RazorEngineServiceTestFixture.RunTestHelper(service =>
            {
                string parentLayoutTemplate = @"<script scr=""/Scripts/jquery.js""></script>@RenderSection(""Scripts"", false)";
                string childLayoutTemplate =
                    @"@{ Layout = ""ParentLayout""; }@section Scripts {<script scr=""/Scripts/childlayout.js""></script>@RenderSection(""Scripts"", false)}";

                var parentKey = service.GetKey("ParentLayout");
                var childKey = service.GetKey("ChildLayout");

                service.AddTemplate(parentKey, parentLayoutTemplate);
                service.AddTemplate(childKey, childLayoutTemplate);

                service.Compile(parentKey);
                service.Compile(childKey);

                // Page with no section defined (e.g. page has no own scripts)
                string pageWithoutOwnScriptsTemplate = @"@{ Layout = ""ChildLayout""; }";
                string expectedPageWithoutOwnScriptsResult =
                    @"<script scr=""/Scripts/jquery.js""></script><script scr=""/Scripts/childlayout.js""></script>";

                using (var writer = new StringWriter())
                {
                    var pageKey = service.GetKey(nameof(pageWithoutOwnScriptsTemplate));

                    service.AddTemplate(pageKey, pageWithoutOwnScriptsTemplate);
                    string actualPageWithoutOwnScriptsResult = service.RunCompile(pageKey);

                    Assert.AreEqual(expectedPageWithoutOwnScriptsResult, actualPageWithoutOwnScriptsResult);
                }

                // Page with section redefenition (page has own additional scripts)
                string pageWithOwnScriptsTemplate = @"@{ Layout = ""ChildLayout""; }@section Scripts {<script scr=""/Scripts/page.js""></script>}";
                string expectedPageWithOwnScriptsResult =
                    @"<script scr=""/Scripts/jquery.js""></script><script scr=""/Scripts/childlayout.js""></script><script scr=""/Scripts/page.js""></script>";

                using (var writer = new StringWriter())
                {
                    var pageKey2 = service.GetKey(nameof(pageWithOwnScriptsTemplate));

                    service.AddTemplate(pageKey2, pageWithOwnScriptsTemplate);
                    service.RunCompile(pageKey2, writer);

                    string actualPageWithOwnScriptsResult = writer.ToString();
                    Assert.AreEqual(expectedPageWithOwnScriptsResult, actualPageWithOwnScriptsResult);
                }
            });
        }

        /* It doesn't seem like anyone actually needs this.
        /// <summary>
        /// Using RenderBody multiply times doesn't work.
        /// </summary>
        [Test]
        [Ignore]
        public void Issue_MultipleRenderBody()
        {
            TemplateServiceTestFixture.RunTestHelper(service =>
            {
                const string parent = @"@RenderBody() and @RenderBody()";
                service.Compile(parent, null, "ParentLayout");
                string child = @"@{ Layout = ""ParentLayout""; }test";
                service.Compile(child, null, "ChildLayout");

                const string expected = @"test and test";
                string result = service.Run("ChildLayout", null, null);
                Assert.AreEqual(expected, result);
            });
        }
         */

        /* This is a bug on Microsoft.Asp.Razor which only happens when we have an empty script tag
         * TODO: Uncomment when fixed
        /// <summary>
        /// RenderSection is failing when there's HTML in the section.
        /// 
        /// Issue 193: https://github.com/Antaris/RazorEngine/issues/193
        /// </summary>
        [Test]
        public void Issue193_SectionParsing()
        {
            using (var service = new TemplateService())
            {
                service.Compile("@RenderBody() @RenderSection(\"Scripts\")", null, "layout-working");
                service.Compile("@{ Layout = \"layout-working\"; } body @section Scripts {" +
                    "a script" +
                    "}", null, "page-working");
                var workingresult = service.Run("page-working", null, null);

                service.Compile("@RenderBody() @RenderSection(\"Scripts\")", null, "layout");
                service.Compile("@{ Layout = \"layout\"; } body @section Scripts {" +
                    "<script/>" +
                    "}", null, "page");
                var result = service.Run("page", null, null);
            }
        }
        */

    }
}
