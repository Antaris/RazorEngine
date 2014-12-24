
# RazorEngine Isolation API

RazorEngine provides an additional layer of security by allowing you to run the generated code within an secured sandbox.
All you need to do is use `IsolatedRazorEngineService.Create` instead of `RazorEngineService.Create`. 
This will immediately switch the compilation and execution to another AppDomain.
However this new AppDomain still has no permission restrictions so what you want to do is provide either your own IAppDomainFactory 
or use the Func<AppDomain> overloads of `IsolatedRazorEngineService.Create`:

    [lang=csharp]
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

You can use the above method like this to create an AppDomain with partial trust (internet security zone):

    [lang=csharp]
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

As you can see this template will throw a SecurityException as there is no way for it
to write into a file of the local harddrive.