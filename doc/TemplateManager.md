# ITemplateManager and ICachingProvider

This section explains how `ITemplateManager` and `ICachingProvider` work and play together, 
which implementations are available by default and how to write your own.

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
   >  `Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled");`  
   >  Related:  
   >  http://stackoverflow.com/questions/16859372/why-doesnt-the-servicestack-razor-filesystemwatcher-work-on-mono-mac-os-x  
   >  http://stackoverflow.com/questions/16519000/filesystemwatcher-under-mono-watching-subdirs  
   >  `<Insert your bug report here>`  

## Available CachingProviders

* `DefaultCachingProvider`: (default) Simple in-memory caching strategy.
* `InvalidatingCachingProvider`: Same as `DefaultCachingProvider` but with additional methods to invalidate cached templates.
  Note that invalidating cached templates doesn't actually free the memory (loaded assemblies), so only use this
  for debugging purposes or if you have an `AppDomain` recycle strategy in place.

## Writing your own `ITemplateManager`

If the above implementations don't fit your needs you can roll your own:

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
        // template is specified by full path
        //return new FullPathTemplateKey(name, fullPath, resolveType, context);
    }

    public void AddDynamic(ITemplateKey key, ITemplateSource source)
    {
        // You can disable dynamic templates completely. 
        // This just means all convenience methods (Compile and RunCompile) with
        // a TemplateSource will no longer work (they are not really needed anyway).
        throw new NotImplementedException("dynamic templates are not supported!");
    }
}
```

Contributing your implementation back to RazorEngine is highly appreciated.

## Writing your own `ICachingProvider`

RazorEngine provides a robust caching layer out of the box which should suite most use-cases.
If you use "Compile" on application startup and "Run" on every use of the application you can be 
sure that caching works (as "Run" would throw if there is no cached template).
If you want "lazy"-compilation of the templates you would just use "RunCompile".

However if you need some custom features (like caching across multiple runs of the application)
you want to run your own caching implementation.
All you need to do is implement the `ICachingProvider` interface and set an instance of your implementation in the configuration.
As starting point you can use the DefaultCachingProvider (latest code is in the repository): 

```csharp
config.CachingProvider = new DefaultCachingProvider();

/// <summary>
/// The default caching provider (See <see cref="ICachingProvider"/>).
/// This implementation does a very simple in-memory caching.
/// It can handle when the same template is used with multiple model-types.
/// </summary>
public class DefaultCachingProvider : ICachingProvider
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, ICompiledTemplate>> _cache =
        new ConcurrentDictionary<string, ConcurrentDictionary<Type, ICompiledTemplate>>();

    private readonly TypeLoader _loader;
    private readonly ConcurrentBag<Assembly> _assemblies = new ConcurrentBag<Assembly>();

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultCachingProvider"/> class.
    /// </summary>
    public DefaultCachingProvider()
    {
        _loader = new TypeLoader(AppDomain.CurrentDomain, _assemblies);
    }

    /// <summary>
    /// The manages <see cref="TypeLoader"/>. See <see cref="ICachingProvider.TypeLoader"/>
    /// </summary>
    public TypeLoader TypeLoader
    {
        get
        {
            return _loader;
        }
    }

    /// <summary>
    /// Get the key used within a dictionary for a modelType.
    /// </summary>
    public static Type GetModelTypeKey(Type modelType)
    {
        if (modelType == null || 
            typeof(System.Dynamic.IDynamicMetaObjectProvider).IsAssignableFrom(modelType))
        {
            return typeof(System.Dynamic.DynamicObject);
        }
        return modelType;
    }

    private void CacheTemplateHelper(ICompiledTemplate template, ITemplateKey templateKey, Type modelTypeKey)
    {
        var uniqueKey = templateKey.GetUniqueKeyString();
        _cache.AddOrUpdate(uniqueKey, key =>
        {
            // new item added
            _assemblies.Add(template.TemplateAssembly);
            var dict = new ConcurrentDictionary<Type, ICompiledTemplate>();
            dict.AddOrUpdate(modelTypeKey, template, (t, old) => template);
            return dict;
        }, (key, dict) =>
        {
            dict.AddOrUpdate(modelTypeKey, t =>
            {
                // new item added (template was not compiled with the given type).
                _assemblies.Add(template.TemplateAssembly);
                return template;
            }, (t, old) =>
            {
                // item was already added before
                return template;
            });
            return dict;
        });
    }

    /// <summary>
    /// Caches a template. See <see cref="ICachingProvider.CacheTemplate"/>.
    /// </summary>
    /// <param name="template"></param>
    /// <param name="templateKey"></param>
    public void CacheTemplate(ICompiledTemplate template, ITemplateKey templateKey)
    {
        var modelTypeKey = GetModelTypeKey(template.ModelType);
        CacheTemplateHelper(template, templateKey, modelTypeKey);
        var typeArgs = template.TemplateType.BaseType.GetGenericArguments();
        if (typeArgs.Length > 0)
        {
            var alternativeKey = GetModelTypeKey(typeArgs[0]);
            if (alternativeKey != modelTypeKey)
            {
                // could be a template with an @model directive.
                CacheTemplateHelper(template, templateKey, typeArgs[0]);
            }
        }
    }

    /// <summary>
    /// Try to retrieve a template from the cache. See <see cref="ICachingProvider.TryRetrieveTemplate"/>.
    /// </summary>
    /// <param name="templateKey"></param>
    /// <param name="modelType"></param>
    /// <param name="compiledTemplate"></param>
    /// <returns></returns>
    public bool TryRetrieveTemplate(ITemplateKey templateKey, Type modelType, out ICompiledTemplate compiledTemplate)
    {
        compiledTemplate = null;
        var uniqueKey = templateKey.GetUniqueKeyString();
        var modelTypeKey = GetModelTypeKey(modelType);
        ConcurrentDictionary<Type, ICompiledTemplate> dict;
        if (!_cache.TryGetValue(uniqueKey, out dict))
        {
            return false;
        }
        return dict.TryGetValue(modelTypeKey, out compiledTemplate);
    }

    /// <summary>
    /// Dispose the instance.
    /// </summary>
    public void Dispose()
    {
        _loader.Dispose();
    }
}
```

Contributing your implementation back to RazorEngine is highly appreciated.