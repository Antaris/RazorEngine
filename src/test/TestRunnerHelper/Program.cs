using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Dynamic;
using RazorEngine.Compilation;
using RazorEngine.Configuration;

namespace TestRunnerHelper
{
    public class Program
    {
        public class TemplateContext<T> : TemplateBase<T>
        {
            public string Section1
            {
                set {
                    ((CustomDataHolder)ViewBag.DataHolder).Section1 = value;
                }
            }
        }

        public class CustomDataHolder
        {
            public string Section1 { get; set; }
        }

        static void Main(string[] args)
        {
            using (var service = RazorEngineService.Create
                (new TemplateServiceConfiguration() { Debug = true }))
            {
                const string template = @"
@{ Layout = ""extractLayouts""; }
@section section1{
<text>sample content</text>
}
            ";
                const string sectionExtracting = @"
@inherits TestRunnerHelper.Program.TemplateContext<dynamic>
@{
    string result;
    using (var mem = new System.IO.StringWriter())
    {
        System.Diagnostics.Debugger.Break();
        var section1 = RenderSection(""section1"");
        section1.WriteTo(mem);
        mem.Flush();
        Section1 = mem.ToString();
    }
}

@RenderSection(""section1"")";
                service.Compile(sectionExtracting, "extractLayouts", null);

                var holder = new CustomDataHolder();
                dynamic viewbag = new DynamicViewBag();
                viewbag.DataHolder = holder;
                // Mono CSC seems to be confused and needs the casts.
                var body = service.RunCompile(template, "templateKey", (Type)null, (object)null, (DynamicViewBag)viewbag);


                if (!holder.Section1.Contains("sample content"))
                {
                    throw new Exception("Expected section content");
                }
            }
        }
    }
}
