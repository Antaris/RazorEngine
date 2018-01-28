using System;
using System.Linq;
using RazorEngine.Templating;

namespace TestMemory
{
  [Serializable]
  public class TemplateModel
  {
    public string Name { get; set; }
  }

  class Program
  {
    static void Main(string[] args)
    {
      using (var service = IsolatedRazorEngineService.Create())
      {
        service.AddTemplate("TestTemplate",
          "Hello @Model.Name " + new String(Enumerable.Repeat('A', 100000).ToArray()));
        service.Compile("TestTemplate", typeof(TemplateModel));

        int counter = 0;

        while (!Console.KeyAvailable)
        {
          var result = service.Run("TestTemplate", typeof(TemplateModel), new TemplateModel {Name = "World"});
          Console.WriteLine("Run: {0}. Result: {1}", ++counter, result.Substring(0, 20));
        }
      }

      Console.ReadKey();
      Console.WriteLine("Key pressed. Press a key again to quit");
      Console.ReadKey();
    }
  }
}
