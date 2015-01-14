# Upgrading RazorEngine.


## Upgrading to 4.x.

While Razor 4 is still beta we do not recommend upgrading to 4.x on production systems.
4.x releases will have the same features/API of 3.x releases but will use Razor-4 instead of Razor-2 and Razor-3.
Some obsolete APIs will be removed so see the `Upgrading to 3.5.0` guide.

## Upgrading to 3.5.0.

First of all 3.5.0 should be fully (binary) backwards compatible with 3.4.x releases. 
So if you have bugs after upgrading (missing method exceptions etc.) please open an issue!

All documentation is now based on the latest API. If you find the need for documentation please consider upgrading.
To give you time to adapt we will support the old API in the 3.x releases.
An obsolete warning means that this API will be removed (possibly already in 4.x!), but stays in the 3.x releases.
Here are some suggestions and bad practices you should get rid of after upgrading to 3.5.0:

### Upgrade to the new API.

Here are the biggest changes:

- Instead of using the static `Razor` class use `Engine.Razor`

- `ITemplateService` is now replaced by `IRazorEngineService` 
 * `new TemplateService(*)` is now replaced by `RazorEngineService.Create(*)` 
 * `new IsolatedTemplateService(*)` is now replaced by `IsolatedRazorEngineService.Create(*)` 

- `ITemplateResolver` is now replaced by `ITemplateManager` (see separate section).

- The Caching API can now be replaced by implementing `ICachingProvider` and caching depends on the template-key and the model-type used for compilation.
  > In previous versions RazorEngine would throw subtle exceptions when different types are used for the same template.
  > This can happen in different scenarios (for example multiple templates with the same layout but different types).
  > Note just because _it just works_ with 3.5.0 you probably don't want to compile templates (Layouts or Includes) multiple times (as compiling is pretty slow compared to just running).
  > Your application probably runs faster with using `dynamic` instead of multiple compiles.
  > This can reverse when you run a compiled template very often, you could be faster by compiling the layout multiple times with static types.

  > You can _kind of_ revert to the old behavior by providing an `ICachingProvider` implementation that throws when someone tries to compile a template multiple times with different types.

The new `IRazorEngineService` interface, which replaces the old `ITemplateService` interface has a lot of extension methods which can be summarized:

> You need to open the `RazorEngine.Templating` namespace to be able to use the extension methods.

* `Parse` is now called `RunCompile`

* The default methods (`Run`, `Compile` and `RunCompile`) have `ITemplateKey` and `TextWriter` parameters.

  - `TextWriter` parameters allow to stream the output into a `TextWriter`. 
    If you just want the output as `string` there is an extension method without the `TextWriter` parameter and a
	`string` return type.

  - `ITemplateKey` specifies the key of the template. 
    The meaning of `ITemplateKey` depends on the concrete `ITemplateManager` implementation (see below).
	The default implementation only uses the name (a simple `string`) of the template for resolving.

	> By default resolving will always fail if the template was not added with `AddTemplate`.

    You almost always want to use the extension methods which have a `string` parameter
	instead of an `ITemplateKey` parameter.

	> You only _really_ need the `ITemplateKey` overload when you have a resolver which loads nested templates (includes/layouts) in another way as
	> global templates, but you want to precompile some nested templates.
	> For example to precompile a layout you would use the `IRazorEngineService.GetKey` method with a parameter of `ResolveType.Layout` to get the 
	> `ITemplateKey` instance and then use this instance to call the `Compile` method.

* There are more extension methods (for `Compile` and `RunCompile`) which take an additional `ITemplateSource` parameter. 
  Those methods just call `IRazorEngineService.AddTemplate` beforehand.
  
  > You need to open the `RazorEngine.Templating` namespace to be able to use the extension methods.
  
  - for every (extension) method taking a `ITemplateSource` parameter there is another extension method which takes a simple `string`
    (the source code of the template). 
	Those will simply call the `ITemplateSource` overload with `new LoadedTemplateSource(templateSource)`.

	> To improve debugging you should use `new LoadedTemplateSource(templateSource, templateFile)` instead (see the debugging section in the Quickstart tutorial).


* `AddTemplate` also has the above extension methods (to specify `ITemplateKey` or `ITemplateSource` via `string`).

* `modelType` now has to be specified explicitly on all methods. `null` means we compile the model with `dynamic` if applicable.
  
  > In previous releases we tried to figure out the modelType from the given model. 
  > However sometimes you want to use a static instance but compile the template with dynamic (to prevent multiple compilations for example).
  > You can now do that by specifing `null`.
  > Another thing you can do now is specify a base type instead of the given instance type and then use the compiled template for multiple sub-type instances.
  
  The parameter will be ignored for compilation when the template has a `@Inherits` or a `@model` directive. 
  
  > The parameter could be used by the caching layer, so make sure you use the same type instance every time to prevent re-compilations.
  > Caching is another reason why this parameter now always has to be specified.

  The modelType parameter must be `null` for anonymous types (and therefore `dynamic` will be used). 
  Using `model.GetType()` is not possible with anonymous types as the compiler generated classes are marked as internal.

### Use `ITemplateManager` or add all your templates before usage.

If you can you should resolve your templates via an `ITemplateManager` implementation (see the Quickstart tutorial) and convert your exisiting
`ITemplateResolver` implementations to the new interface.

This way you can get rid of all `IRazorEngineService` extension methods with an `templatesource` (`string` and `ITemplateSource`) parameter, which will simplify your code.

If you want to have the template-source-code within your application-source-code for some reason you should call `AddTemplate` on app-startup 
and again get rid of all methods with a `templatesource` (`string` and `ITemplateSource`) parameter.

The extensions methods with a `templatesource` (`string` and `ITemplateSource`) parameter are provided to get started with `RazorEngine` very quickly.
But they could block you in the long run when you want a `ITemplateManager` implementation with a `AddTemplate` method that throws `NotSupportedException`.

### Code examples

For example

```csharp
var result = Razor.Parse(razorTemplate, model, cache_name)
```

is now either (when the modeltype is known or you want to precompile on startup)

```csharp
// Once at startup (not required when using an ITemplateManager which knows how to resolve cache_name)
Engine.Razor.AddTemplate(cache_name, razorTemplate)
// On startup
Engine.Razor.Compile(cache_name, typeof(MyModel) /* or model.GetType() or null for 'dynamic'*/)

// instead of the Razor.Parse call
var result = Engine.Razor.Run(cache_name, typeof(MyModel) /* or model.GetType() or null for 'dynamic'*/, model)
```

or (when you want lazy compilation, like `Parse`)

```csharp
// Once at startup (not required when using an ITemplateManager which knows how to resolve cache_name)
Engine.Razor.AddTemplate(cache_name, razorTemplate)
	
// instead of the Razor.Parse call
var result = Engine.Razor.RunCompile(cache_name, typeof(MyModel) /* or model.GetType() or null for 'dynamic'*/, model)
```

The semantic equivalent one-liner would be (only to be used to get started with `RazorEngine` quickly):

```csharp
// This will just call AddTemplate for you (every time), note that the ITemplateManager has to support AddTemplate
// and it has to handle multiple calls to AddTemplate gracefully to make this work.
// The default implementation will throw an exception when you use the same cache_name for different templates.
var result = Engine.Razor.RunCompile(razorTemplate, cache_name, model.GetType() /* typeof(MyModel) or or null for 'dynamic'*/, model)
```


### Mono support

If you need support on the mono platform you should provide a `IReferenceResolver` implementation (see the Quickstart tutorial).

### Roslyn support (beta)

See the Roslyn support page (on the right).