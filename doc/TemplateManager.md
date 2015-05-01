# `TemplateManager` and `ICachingProvider`

This section explains how `TemplateManager` work, which ones are available by default and how to write your own.

## Template resolving

There are 3 situations where RazorEngine needs to resolve templates:

 * Layouts
 * Includes
 * `RunCompile`/`Compile` (for `Run` it needs to be cached already)

RazorEngine unifies this process with the `ITemplateManager` interface and does the following:

```
                               
string -> ITemplateManager.GetKey -> ICachingProvider.TryRetrieveTemplate
       // When not cached continue with
       -> ITemplateManager.Resolve -> ITemplateSource
       -> compile ITemplateSource to ICompiledTemplate
       -> ICachingProvider.CacheTemplate
```

The `GetKey` step enables a `TemplateManager` to add customized data to the key for the caching layer to take into account.


## Available TemplateManagers

* `DelegateTemplateManager`: (default) Used as the default for historical reasons, easy solution when using dynamic template razor strings.
* `ResolvePathTemplateManager`: Used to resolve templates from a given list of directory paths. 
  Doesn't support adding templates dynamically via string. You can use a full path instead of a template name.
* `WatchingResolvePathTemplateManager`: Same as ResolvePathTemplateManager but watches the filesystem and invalidates the cache.
  Note that this introduces a memory leak to your application, so only use this is you have an AppDomain recycle strategy in place
  or for debugging purposes.

  > Mono doesn't always detect changes, if you have problems report a bug to mono and try to use 
    `Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled");`
    Related:
    http://stackoverflow.com/questions/16859372/why-doesnt-the-servicestack-razor-filesystemwatcher-work-on-mono-mac-os-x
    http://stackoverflow.com/questions/16519000/filesystemwatcher-under-mono-watching-subdirs
    `<Insert your bug report here>`

## Available CachingProviders

* `DefaultCachingProvider`: (default) Simple in-memory caching strategy.
* `InvalidatingCachingProvider`: Same as `DefaultCachingProvider` but with additional methods to invalidate cached templates.
  Note that invalidating cached templates doesn't actually free the memory (loaded assemblies), so only use this
  for debugging purposes or if you have an `AppDomain` recycle strategy in place.

## Writing your own

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

