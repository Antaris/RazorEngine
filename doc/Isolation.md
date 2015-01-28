
# RazorEngine Isolation API

RazorEngine provides an additional layer of security by allowing you to run the generated code within an secured sandbox.
All you need to do is use `IsolatedRazorEngineService.Create` instead of `RazorEngineService.Create`. 
This will immediately switch the compilation and execution to another AppDomain.
However this new AppDomain still has no permission restrictions so what you want to do is provide either your own IAppDomainFactory 
or use the Func<AppDomain> overloads of `IsolatedRazorEngineService.Create`:

```csharp
public static AppDomain SandboxCreator()
{
    Evidence ev = new Evidence();
    ev.AddHostEvidence(new Zone(SecurityZone.Internet));
    PermissionSet permSet = SecurityManager.GetStandardSandbox(ev);
    // We have to load ourself with full trust
    StrongName razorEngineAssembly = typeof(RazorEngineService).Assembly.Evidence.GetHostEvidence<StrongName>();
    // We have to load Razor with full trust (so all methods are SecurityCritical)
    // This is because we apply AllowPartiallyTrustedCallers to RazorEngine, because
    // We need the untrusted (transparent) code to be able to inherit TemplateBase.
    // Because in the normal environment/appdomain we run as full trust and the Razor assembly has no security attributes
    // it will be completely SecurityCritical. 
    // This means we have to mark a lot of our members SecurityCritical (which is fine).
    // However in the sandbox domain we have partial trust and because razor has no Security attributes that means the
    // code will be transparent (this is where we get a lot of exceptions, because we now have different security attributes)
    // To work around this we give Razor full trust in the sandbox as well.
    StrongName razorAssembly = typeof(RazorTemplateEngine).Assembly.Evidence.GetHostEvidence<StrongName>();
    AppDomainSetup adSetup = new AppDomainSetup();
    adSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
    AppDomain newDomain = AppDomain.CreateDomain("Sandbox", null, adSetup, permSet, razorEngineAssembly, razorAssembly);
    return newDomain;
}
```

You can use the above method like this to create an AppDomain with partial trust (internet security zone):

```csharp
public void IsolatedRazorEngineService_BadTemplate_InSandbox()
{
    using (var service = IsolatedRazorEngineService.Create(SandboxCreator))
    {
        string file = Path.Combine(Environment.CurrentDirectory, Path.GetRandomFileName());
                
        string template = @"
@using System.IO
@{
File.WriteAllText(""$file$"", ""BAD DATA"");
}".Replace("$file$", file.Replace("\\", "\\\\"));
        Assert.Throws<SecurityException>(() =>
        {
            service.RunCompile(template, "test");
        });

        Assert.IsFalse(File.Exists(file));
    }
}
```

As you can see this template will throw a `SecurityException` as there is no way for it
to write into a file of the local harddrive.

## SecurityException and MethodAccessException.

One important thing to understand is how .NET handles partial trust scenarios.
You should definitely read http://msdn.microsoft.com/en-us/library/vstudio/dd233102%28v=vs.100%29.aspx#additional .
However to give some RazorEngine specific hints/additions:

If your assembly is running with full trust all your code (types/members) will be SecurityCritical!
If you add your own assembly to the list of full trust assemblies the template is not able to use types within that assembly as model.
You can work around that by extracting the types to another assembly (with partial trust)
or work with the `AllowPartiallyTrustedCallers` attribute.

Another way is to use `dynamic` and wrap your instance in an `RazorDynamicObject`.
Of course you can also just remove your assembly from the full trust list if you dont need full trust within the sandbox.

## SerializationException

Whenever you are in an isolation scenario all your models need to traverse into another AppDomain, that means your model needs to be serializable!
When you call methods on your model and you use an MarshalByRef scenario all your parameters and return types must be serializable as well!

If you want to use a model instance which is not serializable you can use `dynamic` and wrap it in an `RazorDynamicObject` 
this should magically take care of everything for you.
The only thing not supported is to cast the model within the template in its type (all interfaces will work however).
You can even call methods with serializable parameters (return type must not be serializable).

## Configuration

RazorEngine provides an `IConfigCreator` interface to configure an `IsolatedRazorEngineService`.

```csharp
/// <summary>
/// A helper interface to get a custom configuration into a new AppDomain.
/// Classes inheriting this interface should be Serializable 
/// (and not inherit from MarshalByRefObject).
/// </summary>
public interface IConfigCreator
{
    /// <summary>
    /// Create a new configuration instance.
    /// This method should be executed in the new AppDomain.
    /// </summary>
    /// <returns></returns>
    ITemplateServiceConfiguration CreateConfiguration();
}

/// <summary>
/// A simple <see cref="IConfigCreator"/> implementation to configure the <see cref="Language"/> and the <see cref="Encoding"/>.
/// </summary>
[Serializable]
public class LanguageEncodingConfigCreator : IConfigCreator
{
    private Language language;
    private Encoding encoding;

    /// <summary>
    /// Initializes a new <see cref="LanguageEncodingConfigCreator"/> instance
    /// </summary>
    /// <param name="language"></param>
    /// <param name="encoding"></param>
    public LanguageEncodingConfigCreator(Language language = Language.CSharp, Encoding encoding = Encoding.Html)
    {
        this.language = language;
        this.encoding = encoding;
    }

    /// <summary>
    /// Create the configuration.
    /// </summary>
    /// <returns></returns>
    public ITemplateServiceConfiguration CreateConfiguration()
    {
        return new TemplateServiceConfiguration()
        {
            Language = language,
            EncodedStringFactory = RazorEngineService.GetEncodedStringFactory(encoding)
        };
    }
}
```

The only thing to mention here is that the implementation must be serializable but not inherit from `MarshalByRefObject`.