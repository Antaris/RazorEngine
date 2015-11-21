using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Dynamic;
using RazorEngine.Compilation;

namespace TestRunnerHelper
{
    class Program
    {
        
        static void Main(string[] args)
        {
            using (var service = IsolatedRazorEngineService.Create())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";
                const string expected = "<h1>Animal Type: Cat</h1>";

                dynamic model = new ExpandoObject();
                model.Type = "Cat";
                var result = service.RunCompile(template, "test", null, (object)RazorDynamicObject.Create(model));
                if (!Equals(expected, result))
                {
                    throw new Exception(string.Format("{0} expected but got {1}", expected, result));
                }
            }
        }
    }
}
