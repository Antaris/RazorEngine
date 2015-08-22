using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Configuration.Xml;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRunnerHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new TemplateServiceConfiguration();
            var xml = new XmlTemplateServiceConfiguration("myapp");
            foreach (var ns in xml.Namespaces)
            {
                config.Namespaces.Add(ns);
            }
            Engine.Razor = RazorEngineService.Create(config);
            if (!Engine.Razor.IsTemplateCached("test", null))
            {

            }
        }
    }
}
