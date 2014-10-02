# RazorEngine

## Quickstart

A templating engine built on Microsoft's Razor parsing engine, RazorEngine allows you to use Razor syntax to build dynamic templates.
All you need to do is use the static Razor class:

    string template = "Hello @Model.Name, welcome to RazorEngine!";
    string result = Razor.Parse(template, new { Name = "World" });

The templating engine supports strict and anonymous types, as well as customised base templates, for instance:

    Razor.SetTemplateBase(typeof(HtmlTemplateBase<>));
    
    string template = 
    @"<html>
        <head>
          <title>Hello @Model.Name</title>
        </head>
        <body>
          Email: @Html.TextBoxFor(m => m.Email)
        </body>
      </html>";

    var model = new PageModel { Name = "World", Email = "someone@somewhere.com" };
    string result = Razor.Parse(template, model);

## Caching

To improve the speed for often used templates caching the compiled template type is build right into RazorEngine, but you would only compile the template when you need it:

      if (Razor.Resolve(templateName, model) == null) then
          // Custom logic to resolve your template name to an actual template string
          var templateContent = templateResolver.Resolve(templateName); 
          Razor.Compile(templateContent, (model == null ? typeof<object> : model.GetType()), templateName);
      Razor.Run(templateName, model, properties /* can be null as well */);

# Multiple templates (multiple distinct sources)

If you need multiple template sources (for example with conflicting template names) you would just create a new instance of the TemplateService and use 
its instance members instead of the static methods of the static Razor class.

## Configuration

	var config = new TemplateServiceConfiguration();
	// .. configure your instance
	
	var templateservice = new MyTemplateService(config);

If you want to use the static Razor class with this configured template service:

    Razor.SetTemplateService(templateservice);

### Enable debugging

    config.Debug = true;

### Set a resolver
	
A template resolver can do the "templateName -> templateContents" resolving.

    // if templates are often read from disk you propably want to do some caching yourself.
	config.Resolver = new MyITemplateResolver(); 

See also http://stackoverflow.com/questions/10520821/how-are-templates-in-razorengine-cached

### Set a reference Resolver (templates with special assembly references / MONO support)

Sometimes it is required to controll the references added in the compilation step yourself instead of just using all currently loaded assemblies (default).
In these cases you can set an IAssemblyReferenceResolver:

	config.ReferenceResolver = new MyIReferenceResolver();


	class MyIReferenceResolver : IAssemblyReferenceResolver {
	    public IEnumerable<string> GetReferences(TypeContext context) {
			// My templates need some special reference to compile.
			return new [] { "Path-to-my-custom-assembly"; };
		}
	}

It could be usefull to get running on mono to just manually return all the assemblies you need.
The default is to use the ``UseCurrentAssembliesReferenceResolver`` class, which always returns all currently loaded assemblies.
You can get and modify this list with (and return it in your own implementation if you wish):

	var loadedList = (new UseCurrentAssembliesReferenceResolver()).GetReferences(null);