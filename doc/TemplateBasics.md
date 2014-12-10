# Template basics

## Getting Started
RazorEngine provides a base implementation of a template class, the `TemplateBase`, this is normally superseded by the model specific template class, the `TemplateBase<T>`. For most use cases, we're hoping this will be enough. To get started with template using RazorEngine, you can simply use the static `Razor` type:

    [lang=csharp]
    string template = "<div>Hello @Model.Name</div>";
    var model = new { Name = "Matt" };

    string result = Razor.Parse(template, model);

Which should result in:

    [lang=markup]
	<div>Hello Matt</div>

## Using Anonymous Types
RazorEngine supports anonymous types (those declared as `var` with no identifier, e.g. `var model = { Name = "Matt" };`. The set of statements to use an anonymous model, is exactly the same as a statically type model (as seen above):

    [lang=csharp]
    string template = "<div>Hello @Model.Name</div>";
    var model = new { Name = "Matt" };

    string result = Razor.Parse(template, model);

Thanks to the C# compiler's (and Visual Studio's) type inference, it will determine the model type that is passed to the generic `Razor.Parse<T>(string, T)` method for you, keeping the code clean and concise.

> Note: Anonymous types are not supported when using the `IsolatedTemplateService`

## Using Dynamic Types
RazorEngine has support for dynamic types (those declared as `dynamic`). Again, the structure for using dynamic types is very similar:

    [lang=csharp]
    string template = "<div>Hello @Model.Name</div>";
    dynamic model = new ExpandoObject();
    model.Name = "Matt";

    string result = Razor.Parse(template, model);

> Note: Dynamic types are not supported when using the `IsolatedTemplateService`

## Caching Templates
When you provide a name argument to your call, RazorEngine will cache the compiled type so we can re-use the template type for subsequent executions:

    [lang=csharp]
    string result = Razor.Parse(template, model, "test");

If you reuse the same template again, it will not have to parse and recompile the template, because we've already cached it.

> Note: If you call to parse a template with a name for the cached version, but the template content has changed, RazorEngine will invalidate the cached version and replace with the update template. This allows templates to be cached, changed and re-cached over time.

## Pre-compiling and Running Templates
On the back of the caching feature mentioned above, RazorEngine supports the ability to pre-compile your templates for later execution. Convenience methods called `Compile` and `Run` allow you to make an initial compile and cache of your template (`Compile`) and then execute it later (`Run`):

    [lang=csharp]
    Razor.Compile(template, "testTemplate");

    // And then...
    string result = Razor.Run("testTemplate");

## Configuration

By default RazorEngine is configured to encode using Html. This supports the majority of users but with some configuration changes you can also set it to encode using Raw format which is better suited for templates that generate things like javascript, php, C# and others.

### Old school:

    [lang=csharp]
    var config = new TemplateServiceConfiguration();
    config.EncodedStringFactory = new RawStringFactory();
    
    // create a new TemplateService and pass in the configuration to the constructor
    var myConfiguredTemplateService = new TemplateService(config);
    // set the template service to our configured one
    Razor.SetTemplateService(myConfiguredTemplateService);
    
    // start parsing templates
    string template = "Hello \"@(Model.Name)\"";
    var model = new { Name = "Matt" };

    string result = Razor.Parse(template, model);

Which should result in:

    [lang=markup]
	Hello "Matt"

### Using the fluent API:

    [lang=csharp]
    var config = new FluentTemplateServiceConfiguration(c => c.WithEncoding(RazorEngine.Encoding.Raw));
    
    // create a new TemplateService and pass in the configuration to the constructor
    var myConfiguredTemplateService = new TemplateService(config);
    // set the template service to our configured one
    Razor.SetTemplateService(myConfiguredTemplateService);
    
    // start parsing templates
    string template = "Hello \"@(Model.Name)\"";
    var model = new { Name = "Matt" };

    string result = Razor.Parse(template, model);

Which should result in:

    [lang=markup]
	Hello "Matt"