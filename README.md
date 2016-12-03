# RazorEngine

[![Join the chat at https://gitter.im/Antaris/RazorEngine](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Antaris/RazorEngine?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

This project is searching for new maintainers, so if you want to help write on gitter or start sending PRs :)

*latest* documentation available on https://antaris.github.io/RazorEngine/.


## Build status

Develop Branch (`master`)

[![Build Status](https://travis-ci.org/Antaris/RazorEngine.svg?branch=master)](https://travis-ci.org/Antaris/RazorEngine)
[![Build status](https://ci.appveyor.com/api/projects/status/39bi38wonhwolrgy/branch/master?svg=true)](https://ci.appveyor.com/project/Antaris/razorengine/branch/master)

Release Branch (`releases`)

[![Build Status](https://travis-ci.org/Antaris/RazorEngine.svg?branch=releases)](https://travis-ci.org/Antaris/RazorEngine)
[![Build status](https://ci.appveyor.com/api/projects/status/39bi38wonhwolrgy/branch/releases?svg=true)](https://ci.appveyor.com/project/Antaris/razorengine/branch/releases)


## Quickstart

First install the nuget package (>=3.5.0).

	Install-Package RazorEngine

A templating engine built on Microsoft's Razor parsing engine, RazorEngine allows you to use Razor syntax to build dynamic templates.
You can find an introduction [here](http://www.asp.net/web-pages/overview/getting-started/introducing-razor-syntax-%28c%29).
All you need to do is use the static `Engine` class (the `Engine.Razor` instance) in the 'RazorEngine' namespace:

```csharp
using RazorEngine;
using RazorEngine.Templating; // For extension methods.

string template = "Hello @Model.Name, welcome to RazorEngine!";
var result =
	Engine.Razor.RunCompile(template, "templateKey", null, new { Name = "World" });
```

> The `RunCompile` method used here is an extension method and you need to open the `RazorEngine.Templating` namespace.

The `"templateKey"` must be unique and after running the above example you can re-run the cached template with this key.

```csharp
// using RazorEngine.Templating; // Dont forget to include this.
var result =
	Engine.Razor.Run("templateKey", null, new { Name = "Max" });
```

The null parameter is the `modelType` and `null` in this case means we use `dynamic` as the type of the model.
You can use a static model as well by providing a type object.

```csharp
// using RazorEngine.Templating; // Dont forget to include this.
var result =
	Engine.Razor.RunCompile("templateKey", typeof(Person), new Person { Name = "Max" });
```

Note that we now re-compile the model with a different type. 
When you do not run the same template a lot of times (like several 1000 times), compiling uses the most time.
So the benefit you get from a static type will most likely not compensate the additional compile time.
Therefore you should either stick to one type for a template (best of both worlds) or just use (the slower) `dynamic` (`null`).
You can specify the `modelType` of a template with the `@model` directive. 
When you do this the `modelType` parameter is ignored, but you should use the same type instance (or `null`) 
on every call to prevent unnecessary re-compilations because of type mismatches in the caching layer.

## Configuration

You can configure RazorEngine with the `TemplateServiceConfiguration` class.

```csharp
var config = new TemplateServiceConfiguration();
// .. configure your instance

var service = RazorEngineService.Create(config);
```

If you want to use the static `Engine` class with this new configuration:

```csharp
Engine.Razor = service;
```


### General Configuration

By default RazorEngine is configured to encode using Html. 
This supports the majority of users but with some configuration changes you can also set it to encode using Raw format 
which is better suited for templates that generate things like javascript, php, C# and others.

```csharp
config.Language = Language.VisualBasic; // VB.NET as template language.
config.EncodedStringFactory = new RawStringFactory(); // Raw string encoding.
config.EncodedStringFactory = new HtmlEncodedStringFactory(); // Html encoding.
```

### Debugging

One thing you might want to enable is the debugging feature:

```csharp
config.Debug = true;
```

When `Debug` is true you can straight up debug into the generated code. 
RazorEngine also supports debugging directly into the template files (normally `.cshtml` files).
As you might see in the above code there is no file to debug into.
To provide RazorEngine with the necessary information you need to tell where the file can be found:

```csharp
// using RazorEngine.Templating; // Dont forget to include this.
string template = "Hello @Model.Name, welcome to RazorEngine!";
string templateFile = "C:/mytemplate.cshtml"
var result =
	Engine.Razor.RunCompile(new LoadedTemplateSource(template, templateFile), "templateKey", null, new { Name = "World" });
```

This time when debugging the template you will jump right into the template file.

### Set a template manager

The API is designed around the idea that you do not have the templates sitting around in the source code 
(while you can do that as seen above).
The main interface to provide RazorEngine with templates is the `ITemplateManager` interface.	
You should either pick one of the available implementations or write your own.
See [TemplateManager and Caching documentation](http://antaris.github.io/RazorEngine/TemplateManager.html) for details.

## Temporary files

RazorEngine tries hard to delete the temporary files it creates, but this is not always possible.
This is especially true if you run RazorEngine from the default `AppDomain`.
RazorEngine will warn you in this situation by writing to the stderr. 
One way to switch into a new AppDomain is to use the following snippet:

```csharp
static int Main(string[] args)
{
    if (AppDomain.CurrentDomain.IsDefaultAppDomain())
    {
        // RazorEngine cannot clean up from the default appdomain...
        Console.WriteLine("Switching to secound AppDomain, for RazorEngine...");
        AppDomainSetup adSetup = new AppDomainSetup();
        adSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        var current = AppDomain.CurrentDomain;
        // You only need to add strongnames when your appdomain is not a full trust environment.
        var strongNames = new StrongName[0];

        var domain = AppDomain.CreateDomain(
            "MyMainDomain", null,
            current.SetupInformation, new PermissionSet(PermissionState.Unrestricted),
            strongNames);
        var exitCode = domain.ExecuteAssembly(Assembly.GetExecutingAssembly().Location);
        // RazorEngine will cleanup. 
        AppDomain.Unload(domain);
        return exitCode;
    }
    // Continue with your code.
}
```

Depending on your scenario you probably need to edit it to your needs.
Note that you need to `Unload` the domain to trigger cleanup.

For the following scenario:

 * Your templates are limited in number.
 * You fully trust your templates / don't need isolation.
 * You don't need any kind of debugging support.
 * Your templates do not change in runtime.

You can use `config.DisableTempFileLocking = true` as well. This will work in any AppDomain (including the default one).
To remove the RazorEngine warnings you can additionally use `config.CachingProvider = new DefaultCachingProvider(t => {})`.

See also https://github.com/Antaris/RazorEngine/issues/244 for more details.


## More

On the right side you can find links to advanced topics and additional [documentation](http://antaris.github.io/RazorEngine/).
You should definitely read "About Razor" and "Template basics".

