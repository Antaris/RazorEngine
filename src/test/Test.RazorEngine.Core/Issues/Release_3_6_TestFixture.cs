namespace RazorEngine.Tests.TestTypes.Issues
{
    using System;
    using System.Collections.Generic;
    using Microsoft.CSharp.RuntimeBinder;

    using NUnit.Framework;

    using Configuration;
    using Templating;

    /// <summary>
    /// Provides tests for the Release 3.6
    /// </summary>
    [TestFixture]
    public class Release_3_6_TestFixture
    {
        /// <summary>
        /// Using @Raw within href attribute doesn't work.
        /// 
        /// Issue 181: https://github.com/Antaris/RazorEngine/issues/181
        /// </summary>
        [Test]
        public void Issue181_RawDoesntWork()
        {
            using (var service = new TemplateService())
            {
                const string link = "http://home.mysite.com/?a=b&c=1&new=1&d=3&e=0&g=1";
                const string template_1 = "@Raw(Model.Data)";
                const string expected_1 = link;
                const string template_2 = "<a href=\"@Raw(Model.Data)\">@Raw(Model.Data)</a>";
                string expected_2 = string.Format("<a href=\"{0}\">{0}</a>", link);

                var model = new { Data = link };

                var result_1 = service.Parse(template_1, model, null, null);
                Assert.AreEqual(expected_1, result_1);
                var result_2 = service.Parse(template_2, model, null, null);
                Assert.AreEqual(expected_2, result_2);
            }
        }

        /// <summary>
        /// Using nested sections doesn't work.
        /// 
        /// Issue 163: https://github.com/Antaris/RazorEngine/issues/163
        /// </summary>
        [Test]
        public void Issue163_SectionRedefenition()
        {
            TemplateServiceTestFixture.RunTestHelper(service =>
            {
                string parentLayoutTemplate = @"<script scr=""/Scripts/jquery.js""></script>@RenderSection(""Scripts"", false)";
                string childLayoutTemplate =
                    @"@{ Layout = ""ParentLayout""; }@section Scripts {<script scr=""/Scripts/childlayout.js""></script>@RenderSection(""Scripts"", false)}";
                service.Compile(parentLayoutTemplate, null, "ParentLayout");
                service.Compile(childLayoutTemplate, null, "ChildLayout");

                // Page with no section defined (e.g. page has no own scripts)
                string pageWithoutOwnScriptsTemplate = @"@{ Layout = ""ChildLayout""; }";
                string expectedPageWithoutOwnScriptsResult =
                    @"<script scr=""/Scripts/jquery.js""></script><script scr=""/Scripts/childlayout.js""></script>";
                string actualPageWithoutOwnScriptsResult = service.Parse(pageWithoutOwnScriptsTemplate, null, null, null);
                Assert.AreEqual(expectedPageWithoutOwnScriptsResult, actualPageWithoutOwnScriptsResult);

                // Page with section redefenition (page has own additional scripts)
                string pageWithOwnScriptsTemplate = @"@{ Layout = ""ChildLayout""; }@section Scripts {<script scr=""/Scripts/page.js""></script>}";
                string expectedPageWithOwnScriptsResult =
                    @"<script scr=""/Scripts/jquery.js""></script><script scr=""/Scripts/childlayout.js""></script><script scr=""/Scripts/page.js""></script>";
                string actualPageWithOwnScriptsResult = service.Parse(pageWithOwnScriptsTemplate, null, null, null);
                Assert.AreEqual(expectedPageWithOwnScriptsResult, actualPageWithOwnScriptsResult);
            });
        }

        /*
        /// <summary>
        /// Using RenderBody multiply times doesn't work.
        /// Note it doesn't seem like anyone actually needs this.
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
    }
}
