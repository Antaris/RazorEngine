# RazorEngine

Documentation also available on http://antaris.github.io/RazorEngine/.

## Build status

Develop Branch

[![Build Status](https://travis-ci.org/Antaris/RazorEngine.svg?branch=develop)](https://travis-ci.org/Antaris/RazorEngine)
[![Build status](https://ci.appveyor.com/api/projects/status/39bi38wonhwolrgy?svg=true)](https://ci.appveyor.com/project/Antaris/razorengine)

Master Branch

[![Build Status](https://travis-ci.org/Antaris/RazorEngine.svg?branch=master)](https://travis-ci.org/Antaris/RazorEngine)
[![Build status](https://ci.appveyor.com/api/projects/status/39bi38wonhwolrgy/branch/master?svg=true)](https://ci.appveyor.com/project/Antaris/razorengine/branch/master)


## Quickstart

First install the nuget package (>=3.5.0).

	Install-Package RazorEngine

A templating engine built on Microsoft's Razor parsing engine, RazorEngine allows you to use Razor syntax to build dynamic templates.
All you need to do is use the static `Engine` class (the `Engine.Razor` instance):

```csharp
using RazorEngine;
using RazorEngine.Templating; // IMPORTANT: The RunCompile method used below is an extension method of this namespace.

string template = "Hello @Model.Name, welcome to RazorEngine!";
var result =
	Engine.Razor.RunCompile(template, "templateKey", null, new { Name = "World" });
```

The `"templateKey"` must be unique and after running the above example you can re-run the cached template with this key.

```csharp
var result =
	Engine.Razor.Run("templateKey", null, new { Name = "Max" });
```

The null parameter is the `modelType` and `null` in this case means we use `dynamic` as the type of the model.
You can use a static model as well by providing a type object.

```csharp
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
As as you might see in the above code there is no file to debug into.
To provide RazorEngine with the necessary information you need to tell where the file can be found:

```csharp
string template = "Hello @Model.Name, welcome to RazorEngine!";
string templateFile = "C:/mytemplate.cshtml"
var result =
	Engine.Razor.RunCompile(new LoadedTemplateSource(template, templateFile), "templateKey", null, new { Name = "World" });
```

This time when debugging the template you will jump right into the template file.

### Set a template manager
	
The API is designed around the idea that you do not have the templates sitting around in the source code (while you can do that as seen above).
The main interface to provide RazorEngine with templates is the `ITemplateManager` interface.

```csharp
config.TemplateManager = new MyTemplateManager(); 

public class MyTemplateManager : ITemplateManager
{
    public ITemplateSource Resolve(ITemplateKey key)
    {
        // Resolve your template here (ie read from disk)
		// if the same templates are often read from disk you propably want to do some caching here.
        string template = "Hello @Model.Name, welcome to RazorEngine!";
        // Provide a non-null file to improve debugging
        return new LoadedTemplateSource(template, null);
    }

    public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context)
    {
        // If you can have different templates with the same name depending on the 
        // context or the resolveType you need your own implementation here!
        // Otherwise you can just use NameOnlyTemplateKey.
        return new NameOnlyTemplateKey(name, resolveType, context);
    }

    public void AddDynamic(ITemplateKey key, ITemplateSource source)
    {
        // You can disable dynamic templates completely, but 
        // then all convenience methods (Compile and RunCompile) with
        // a TemplateSource will no longer work (they are not really needed anyway).
        throw new NotImplementedException("dynamic templates are not supported!");
    }
}
```

### Set a reference resolver 

Templates are first transformed to a source code file and then dynamically compiled by invoking the compiler.
Because you can use source code within the template itself you are free to use any libraries within a template.
However the compiler needs to be able to resolve everything and the default strategy is to reference all currently loaded assemblies.
This can lead to problems when you want to use a library (in the template) which is not referenced in the 
hosting code or not loaded by the runtime (because it is unused).
It is also possible that you run into problems on Mono because mcs behaves differently.
To be able to resolve such issues you can control this behaviour and set your own `IReferenceResolver` implementation.

```csharp
config.ReferenceResolver = new MyIReferenceResolver();

class MyIReferenceResolver : IReferenceResolver {
    public IEnumerable<CompilerReference> GetReferences(TypeContext context, IEnumerable<CompilerReference> includeAssemblies) {
		// TypeContext gives you some context for the compilation (which templates, which namespaces and types)
		// My templates need some special reference to compile.
		return new [] { 
			CompilerReference.From("Path-to-my-custom-assembly"), // file path (string)
			CompilerReference.From(typeof(MyType).Assembly), // Assembly
			CompilerReference.From(assemblyInByteArray), // byte array (roslyn only)
			CompilerReference.From(File.OpenRead(assembly)), // stream (roslyn only)
		};
	}
}
```

It could be usefull to get running on mono to just manually return all the assemblies you need.
The default is to use the `UseCurrentAssembliesReferenceResolver` class, which always returns all currently loaded assemblies.
You can get and modify this list (and return it in your own implementation if you wish):

```csharp
var loadedList = (new UseCurrentAssembliesReferenceResolver()).GetReferences(null)
```

## More

On the right side you can find links to advanced topics and additional documentation.
You should definitely read "About Razor" and "Template basics".

