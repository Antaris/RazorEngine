# Layout and Partial templates

This section explains how you can use Layout templates and embed templates one in another.

## Layout template

Defining a layout is quite simple.
First make sure to set the `Layout` property in the template using the layout template name.
Note that using `Layout` within a layout template is supported.
Second make sure the `string` you set for the Layout property can actually resolved by the template manager.
For the default manager this means you added the template (with `AddTemplate`):

```csharp
using RazorEngine;
using RazorEngine.Templating;
using System;

namespace TestRunnerHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = Engine.Razor;
            // In this example I'm using the default configuration, but you should choose a different template manager: http://antaris.github.io/RazorEngine/TemplateManager.html
            service.AddTemplate("layout", "<h1>@RenderBody()</h1>");
            service.AddTemplate("template", @"@{Layout = ""layout"";}my template");
            service.Compile("template");
            var result = service.Run("template");
            Console.WriteLine("Result is: {0}", result);
        }
    }
}
```

> The code will print `<h1>my template</h1>`. 

but other managers resolve template differently, please read http://antaris.github.io/RazorEngine/TemplateManager.html for details!
As you can see, all you need to do in the layout template is call `@RenderBody()` on the place you want your child template to appear.

## Partial templates

All there is to do is to make a `@Include` call from within the template:

`Include(string cacheName, object model = null, Type modelType = null)`

As for layouts you need to ensure that the first string parameter can be resolved to a template:

```csharp
using RazorEngine;
using RazorEngine.Templating;
using System;

namespace TestRunnerHelper
{
    public class SubModel
    {
        public string SubModelProperty { get; set; }
    }

    public class MyModel
    {
        public string ModelProperty { get; set; }
        public SubModel SubModel { get; set; }
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            var service = Engine.Razor;
            // In this example I'm using the default configuration, but you should choose a different template manager: http://antaris.github.io/RazorEngine/TemplateManager.html
            service.AddTemplate("part", @"my template");
            // If you leave the second and third parameters out the current model will be used.
            // If you leave the third we assume the template can be used for multiple types and use "dynamic".
            // If the second parameter is null (which is default) the third parameter is ignored.
            // To workaround in the case you want to specify type "dynamic" without specifying a model use Include("p", new object(), null)
            service.AddTemplate("template", @"<h1>@Include(""part"", @Model.SubModel, typeof(TestRunnerHelper.SubModel))</h1>");
            service.Compile("template", typeof(MyModel));
            service.Compile("part", typeof(SubModel));
            var result = service.Run("template", typeof(MyModel), new MyModel { ModelProperty = "model", SubModel = new SubModel { SubModelProperty = "submodel"} });
            Console.WriteLine("Result is: {0}", result);
        }
    }
}
```

## General considerations

As the above code showed a way to fully type all the templates you should consider adding `@inherits`
directives to the templates as explained in https://antaris.github.io/RazorEngine/IntellisenseAndResharper.html
