using NUnit.Framework;
using RazorEngine.Compilation;
using RazorEngine.Templating;
using RazorEngine.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using RazorEngine.Configuration;
#if RAZOR4
using Microsoft.AspNet.Razor;
#else
using System.Web.Razor;
#endif

namespace Test.RazorEngine
{
    /// <summary>
    /// Tests the IsolatedRazorEngineService.
    /// </summary>
    [TestFixture]
    public class IsolatedRazorEngineServiceTestFixture
    {
        /// <summary>
        /// Check if we are running on mono.
        /// </summary>
        /// <returns></returns>
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        /// <summary>
        /// Check if we have either an SecurityManager enabled or are running on mono
        /// and ignore the test.
        /// </summary>
        public static void CheckMono()
        {
#pragma warning disable 0618 // Mono recommends to do this.
            if (!SecurityManager.SecurityEnabled)
                Assert.Ignore("SecurityManager.SecurityEnabled is OFF");
#pragma warning restore 0618
            if (IsRunningOnMono())
                Assert.Ignore("IsolatedRazorEngineServiceTestFixture is not supported on mono");
        }

        /// <summary>
        /// Creates a sandbox for testing.
        /// </summary>
        /// <returns></returns>
        public static AppDomain SandboxCreator()
        {
            CheckMono();
#if RAZOR4
            Assert.Ignore("IsolatedRazorEngineServiceTestFixture is not tested with razor 4 as it is not signed!");
#endif

#if MONO
            // Mono has no AddHostEvidence or GetHostEvidence.
            // We do not run the tests anyway.
            return null;
#else
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
            // We trust ourself as well
            StrongName testAssembly = typeof(IsolatedRazorEngineServiceTestFixture).Assembly.Evidence.GetHostEvidence<StrongName>();
            AppDomainSetup adSetup = new AppDomainSetup();
            adSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            AppDomain newDomain = AppDomain.CreateDomain("Sandbox", null, adSetup, permSet, razorEngineAssembly, razorAssembly, testAssembly);
            return newDomain;
#endif
        }

        /// <summary>
        /// Tests that a simple template without a model can be parsed.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_WithNoModel_InSandbox()
        {
            using (var service = IsolatedRazorEngineService.Create(SandboxCreator))
            {
                const string template = "<h1>Hello World</h1>";
                const string expected = template;

                string result = service.RunCompile(template, "test");

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple viewbag is working.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_DynamicViewBag_InSandBox()
        {
            using (var service = IsolatedRazorEngineService.Create(SandboxCreator))
            {
                const string template = "<h1>Hello @ViewBag.Test</h1>";
                const string expected = "<h1>Hello TestItem</h1>";
                dynamic viewbag = new DynamicViewBag();
                viewbag.Test = "TestItem";

                string result = service.RunCompile(template, "test", (Type)null, (object)null, (DynamicViewBag)viewbag);
                Assert.AreEqual(expected, result);
            }
        }

        /// <summary>
        /// Tests that a simple viewbag is working.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_Dynamic_InSandBox()
        {
            using (var service = IsolatedRazorEngineService.Create(SandboxCreator))
            {
                const string template = "<h1>Hello @Model.Forename, @ViewBag.Test</h1>";
                const string expected = "<h1>Hello Matt, TestItem</h1>";
                dynamic viewbag = new DynamicViewBag();
                viewbag.Test = "TestItem";
                viewbag.Forename = "Matt";
                string result = service.RunCompile(template, "test", null, (object)viewbag, (DynamicViewBag)viewbag);
                Assert.AreEqual(expected, result);
            }
        }

        /// <summary>
        /// Tests that a bad template cannot do stuff
        /// </summary>
        [Test]
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

        /// <summary>
        /// Helper class to check the state of the remote AppDomain.
        /// </summary>
        public class AssemblyChecker : global::RazorEngine.CrossAppDomainObject
        {
            /// <summary>
            /// The assemblies are called "@(namespace).@(class).dll"
            /// </summary>
            const string CompiledAssemblyPrefix = CompilerServiceBase.DynamicTemplateNamespace + "." + CompilerServiceBase.ClassNamePrefix;
            
            /// <summary>
            /// Check if a compiled assembly exists.
            /// </summary>
            /// <returns></returns>
            public bool ExistsCompiledAssembly() {
                (new PermissionSet(PermissionState.Unrestricted)).Assert();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var razorEngine = 
                    assemblies
                    .Where(a => a.FullName.StartsWith(CompiledAssemblyPrefix))
                    .FirstOrDefault();
                return razorEngine != null;
            }

            /// <summary>
            /// Check if everything is cleaned up.
            /// </summary>
            public void IsolatedRazorEngineService_CleanUpWorks()
            {
                var appDomain = SandboxCreator();
                using (var service = IsolatedRazorEngineService.Create(() => appDomain))
                {
                    string template = @"@Model.Name";

                    var result = service.RunCompile(template, "test", null, new { Name = "test" });
                    Assert.AreEqual("test", result);


                    ObjectHandle handle =
                        Activator.CreateInstanceFrom(
                            appDomain, typeof(AssemblyChecker).Assembly.ManifestModule.FullyQualifiedName,
                            typeof(AssemblyChecker).FullName
                        );

                    using (var localHelper = new AssemblyChecker())
                    {
                        Assert.False(localHelper.ExistsCompiledAssembly());
                    }
                    using (var remoteHelper = (AssemblyChecker)handle.Unwrap())
                    {
                        Assert.IsTrue(remoteHelper.ExistsCompiledAssembly());
                    }
                }
                Assert.Throws<AppDomainUnloadedException>(() => { Console.WriteLine(appDomain.FriendlyName); });
                using (var localHelper = new AssemblyChecker())
                {
                    Assert.False(localHelper.ExistsCompiledAssembly());
                }
            }
        }

        /// <summary>
        /// Tests that a bad template cannot do stuff
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_CleanUpWorks()
        {
            var current = AppDomain.CurrentDomain;
            var appDomain = AppDomain.CreateDomain("TestDomain.IsolatedRazorEngineService_CleanUpWorks", current.Evidence, current.SetupInformation);

            ObjectHandle handle =
                Activator.CreateInstanceFrom(
                    appDomain, typeof(AssemblyChecker).Assembly.ManifestModule.FullyQualifiedName,
                    typeof(AssemblyChecker).FullName
                );
            using (var remoteHelper = (AssemblyChecker)handle.Unwrap())
            {
                remoteHelper.IsolatedRazorEngineService_CleanUpWorks();
            }
        }

        /// <summary>
        /// Tests that a very bad template cannot change its permissions.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_VeryBadTemplate_InSandbox()
        {
            using (var service = IsolatedRazorEngineService.Create(SandboxCreator))
            {
                string file = Path.Combine(Environment.CurrentDirectory, Path.GetRandomFileName());

                string template = @"
@using System.IO
@using System.Security
@using System.Security.Permissions
@{
(new PermissionSet(PermissionState.Unrestricted)).Assert();
File.WriteAllText(""$file$"", ""BAD DATA"");
}".Replace("$file$", file.Replace("\\", "\\\\"));
                Assert.Throws<InvalidOperationException>(() =>
                { // cannot create a file in template
                    service.RunCompile(template, "test");
                });

                Assert.IsFalse(File.Exists(file));
            }
        }

        /// <summary>
        /// Tests that a simple template without a model can be parsed.
        /// </summary>
        [Test]
        public void RazorEngineService_VeryBadTemplate()
        {
            RazorEngineServiceTestFixture.RunTestHelper(service =>
            {
                string file = Path.Combine(Environment.CurrentDirectory, Path.GetRandomFileName());

                string template = @"
@using System.IO
@using System.Security
@using System.Security.Permissions
@{
(new PermissionSet(PermissionState.Unrestricted)).Assert();
File.WriteAllText(""$file$"", ""BAD DATA"");
}".Replace("$file$", file.Replace("\\", "\\\\"));
                var test = service.RunCompile(template, "test");
                
                Assert.IsTrue(File.Exists(file));
                File.Delete(file);
            });
        }

        /// <summary>
        /// Tests that a simple template without a model can be parsed.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_CanParseSimpleTemplate_WithNoModel()
        {
            using (var service = IsolatedRazorEngineService.Create())
            {
                const string template = "<h1>Hello World</h1>";
                const string expected = template;

                string result = service.RunCompile(template, "test");

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Test that we can not access security critical types.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_StaticSecurityCriticalModel_InSandbox()
        {
            using (var service = IsolatedRazorEngineService.Create(SandboxCreator))
            {
                const string template = "<h1>Hello World @Model.Forename</h1>";
                const string expected = "<h1>Hello World TestForename</h1>";
                var model = new Person { Forename = "TestForename" };
                Assert.Throws<MethodAccessException>(() =>
                { // Because we cannot access the template constructor (as there is a SecurityCritical type argument)
                    string result = service.RunCompile(template, "test", typeof(Person), model);
                    Assert.AreEqual(expected, result);
                });
            }
        }

        /// <summary>
        /// Test that we can not access security critical members in model.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_StaticSecurityCriticalModelDynamicType_InSandBox()
        {
            using (var service = IsolatedRazorEngineService.Create(SandboxCreator))
            {
                const string template = "<h1>Hello @Model.Forename</h1>";
                const string expected = "<h1>Hello Matt</h1>";

                var model = new Person { Forename = "Matt" };
                Assert.Throws<SecurityException>(() =>
                { // this time we have no security critical type argument but we still should not be able to access it...
                    string result = service.RunCompile(template, "test", null, model);
                    Assert.AreEqual(expected, result);
                });
            }
        }


        /// <summary>
        /// Test that we can not access security critical types.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_StaticSecurityCriticalModelWrapped_InSandbox()
        {
            using (var service = IsolatedRazorEngineService.Create(SandboxCreator))
            {
                const string template = "<h1>Hello World @Model.Forename</h1>";
                const string expected = "<h1>Hello World TestForename</h1>";
                var model = new Person { Forename = "TestForename" };

                string result = service.RunCompile(template, "test", null, new RazorDynamicObject(model));
                Assert.AreEqual(expected, result);
                
            }
        }

        /// <summary>
        /// Test that we can access security transparent static models in a sandbox.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_StaticModel_InSandbox()
        {
            using (var service = IsolatedRazorEngineService.Create(SandboxCreator))
            {
                const string template = "<h1>Hello World @Model.Name</h1>";
                const string expected = "<h1>Hello World TestForename</h1>";
                // We "abuse" NameOnlyTemplateKey because it is SecurityTransparent and serializable.
                var model = new NameOnlyTemplateKey("TestForename", ResolveType.Global, null);
                string result = service.RunCompile(template, "test", typeof(NameOnlyTemplateKey), model);
                Assert.AreEqual(expected, result);
            }
        }

        /// <summary>
        /// Test that we can access security transparent static models via "dynamic" in a sandbox.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_StaticModelDynamicType_InSandBox()
        {
            using (var service = IsolatedRazorEngineService.Create(SandboxCreator))
            {
                const string template = "<h1>Hello @Model.Name</h1>";
                const string expected = "<h1>Hello Matt</h1>";
                // We "abuse" NameOnlyTemplateKey because it is SecurityTransparent and serializable.
                var model = new NameOnlyTemplateKey("Matt", ResolveType.Global, null);
                string result = service.RunCompile(template, "test", null, model);
                Assert.AreEqual(expected, result);
                
            }
        }

        /// <summary>
        /// Tests that a simple template with a model can be parsed.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_CanParseSimpleTemplate_WithComplexSerializableModel()
        {
            using (var service = IsolatedRazorEngineService.Create())
            {
                const string template = "<h1>Hello @Model.Forename</h1>";
                const string expected = "<h1>Hello Matt</h1>";

                var model = new Person { Forename = "Matt" };
                string result = service.RunCompile(template, "test", typeof(Person), model);

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template with a non-serializable model cannot be parsed.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_CannotParseSimpleTemplate_WithComplexNonSerializableModel()
        {
            using (var service = IsolatedRazorEngineService.Create())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";

                Assert.Throws<SerializationException>(() =>
                {
                    var model = new Animal { Type = "Cat" };
                    service.RunCompile(template, "test", typeof(Animal), model);
                });
            }
        }

        /// <summary>
        /// Tests that a simple template with an anonymous model can be parsed.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_ParseSimpleTemplate_WithAnonymousModel()
        {
            using (var service = IsolatedRazorEngineService.Create())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";
                const string expected = "<h1>Animal Type: Cat</h1>";

                var model = new { Type = "Cat" };
                var result = service.RunCompile(template, "test", null, new RazorDynamicObject(model));
                Assert.AreEqual(expected, result);
            }
        }

        /// <summary>
        /// Tests that a simple template with an anonymous model can be parsed within a sandbox.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_Sandbox_WithAnonymousModel()
        {
            using (var service = IsolatedRazorEngineService.Create(SandboxCreator))
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";
                const string expected = "<h1>Animal Type: Cat</h1>";

                var model = new { Type = "Cat" };
                var result = service.RunCompile(template, "test", null, new RazorDynamicObject(model));
                Assert.AreEqual(expected, result);
            }
        }


        /// <summary>
        /// Tests that a simple template with an expando model cannot be parsed.
        /// </summary>
        /// <remarks>
        /// This may seem pointless to test, as the <see cref="IsolatedTemplateService"/> will explicitly
        /// check and throw the exception, it's worth creating a test for future reference. It's also
        /// something we can check should we ever find a way to support dynamic/anonymous objects
        /// across application domain boundaries.
        /// </remarks>
        [Test]
        public void IsolatedRazorEngineService_ParseSimpleTemplate_WithExpandoModel()
        {
            using (var service = IsolatedRazorEngineService.Create())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";
                const string expected = "<h1>Animal Type: Cat</h1>";

                dynamic model = new ExpandoObject();
                model.Type = "Cat";
                var result = service.RunCompile(template, "test", null, (object)RazorDynamicObject.Create(model));
                Assert.AreEqual(expected, result);
            }
        }

        /// <summary>
        /// Tests that a simple template with a dynamic model cannot be parsed.
        /// </summary>
        /// <remarks>
        /// This may seem pointless to test, as the <see cref="IsolatedTemplateService"/> will explicitly
        /// check and throw the exception, it's worth creating a test for future reference. It's also
        /// something we can check should we ever find a way to support dynamic/anonymous objects
        /// across application domain boundaries.
        /// </remarks>
        [Test]
        public void IsolatedRazorEngineService_CannotParseSimpleTemplate_WithDynamicModel()
        {
            using (var service = IsolatedRazorEngineService.Create())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";
                const string expected = "<h1>Animal Type: Cat</h1>";

                dynamic model = new ValueObject(new Dictionary<string, object> { { "Type", "Cat" } });
                string result = service.RunCompile(template, "test", null, (object)RazorDynamicObject.Create(model));
                Assert.AreEqual(expected, result);
            }
        }

        /// <summary>
        /// Tests that an isolated template service cannot use the same application domain as the 
        /// main application domain.
        /// </summary>
        /// <remarks>
        /// An isolated template service will unload it's child application domain on Dispose. We need to ensure
        /// it doesn't attempt to unload the current application domain that it is running in. This may or may
        /// not be the main application domain (but is very likely to be).
        /// </remarks>
        [Test]
        public void IsolatedRazorEngineService_WillThrowException_WhenUsingMainAppDomain()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (var service = IsolatedRazorEngineService.Create(() => AppDomain.CurrentDomain))
                { }
            });
        }

        /// <summary>
        /// Tests that an isolated template service cannot use a null application domain.
        /// </summary>
        /// <remarks>
        /// I had considered using the default <see cref="IAppDomainFactory"/> to spawn a default
        /// application domain to load templates into when a null value is returned, but behaviourly this didn't 
        /// seem like the right thing to do. If you're using an <see cref="IsolatedTemplateService"/>, 
        /// you should expect it to have a valid application domain, so passing null should cause an exception.
        /// </remarks>
        [Test]
        public void IsolatedRazorEngineService_WillThrowException_WhenUsingNullAppDomain()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (var service = IsolatedRazorEngineService.Create(() => null))
                { }
            });
        }

        /// <summary>
        /// Test Type.
        /// </summary>
        [Serializable]
        public class InsecureConfigCreator : IsolatedRazorEngineService.IConfigCreator
        {
            /// <summary>
            /// Test Type.
            /// </summary>
            public ITemplateServiceConfiguration CreateConfiguration()
            {
                var config = new TemplateServiceConfiguration();
                config.DisableTempFileLocking = true;
                return config;
            }
        }

        /// <summary>
        /// Tests that we cannot create an insecure sandbox.
        /// </summary>
        [Test]
        public void IsolatedRazorEngineService_WillThrowException_WhenUsingDisableFileLocking()
        {
            Assert.Throws<InvalidOperationException>(() =>
                {
                    using (var service = IsolatedRazorEngineService.Create(new InsecureConfigCreator(), SandboxCreator))
                    {
                        const string template = "<h1>Hello World</h1>";
                        const string expected = template;

                        string result = service.RunCompile(template, "test");

                        Assert.That(result == expected, "Result does not match expected: " + result);
                    }
                });
        }

    }
}
