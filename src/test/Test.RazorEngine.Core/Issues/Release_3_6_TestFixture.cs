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

    }
}
