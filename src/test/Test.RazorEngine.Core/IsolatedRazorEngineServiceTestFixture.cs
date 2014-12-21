using NUnit.Framework;
using RazorEngine.Compilation;
using RazorEngine.Templating;
using RazorEngine.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Razor;

namespace Test.RazorEngine
{
    [TestFixture]
    public class IsolatedRazorEngineServiceTestFixture
    {
        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        [SetUp]
        public void SetUp()
        {
            if (!SecurityManager.SecurityEnabled)
                Assert.Ignore("SecurityManager.SecurityEnabled is OFF");
            if (IsRunningOnMono())
                Assert.Ignore("IsolatedRazorEngineServiceTestFixture is not supported on mono");
        }

        public static AppDomain SandboxCreator()
        {
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
            AppDomainSetup adSetup = new AppDomainSetup();
            adSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            AppDomain newDomain = AppDomain.CreateDomain("Sandbox", null, adSetup, permSet, razorEngineAssembly, razorAssembly);
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

                string result = service.RunCompileOnDemand(template, "test");

                Assert.That(result == expected, "Result does not match expected: " + result);
            }
        }

        /// <summary>
        /// Tests that a simple template without a model can be parsed.
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
                    service.RunCompileOnDemand(template, "test");
                });

                Assert.IsFalse(File.Exists(file));
            }
        }

        /// <summary>
        /// Tests that a simple template without a model can be parsed.
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
                    service.RunCompileOnDemand(template, "test");
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
                var test = service.RunCompileOnDemand(template, "test");
                
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

                string result = service.RunCompileOnDemand(template, "test");

                Assert.That(result == expected, "Result does not match expected: " + result);
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
                string result = service.RunCompileOnDemand(template, "test", typeof(Person), model);

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
                    service.RunCompileOnDemand(template, "test", typeof(Animal), model);
                });
            }
        }

        /// <summary>
        /// Tests that a simple template with an anonymous model cannot be parsed.
        /// </summary>
        /// <remarks>
        /// This may seem pointless to test, as the <see cref="IsolatedTemplateService"/> will explicitly
        /// check and throw the exception, it's worth creating a test for future reference. It's also
        /// something we can check should we ever find a way to support dynamic/anonymous objects
        /// across application domain boundaries.
        /// </remarks>
        [Test]
        public void IsolatedRazorEngineService_CannotParseSimpleTemplate_WithAnonymousModel()
        {
            using (var service = IsolatedRazorEngineService.Create())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";
                const string expected = "<h1>Animal Type: Cat</h1>";

                var model = new { Type = "Cat" };
                var result = service.RunCompileOnDemand(template, "test", null, new RazorDynamicObject(model));
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
        public void IsolatedRazorEngineService_CannotParseSimpleTemplate_WithExpandoModel()
        {
            using (var service = IsolatedRazorEngineService.Create())
            {
                const string template = "<h1>Animal Type: @Model.Type</h1>";
                const string expected = "<h1>Animal Type: Cat</h1>";

                dynamic model = new ExpandoObject();
                model.Type = "Cat";
                var result = service.RunCompileOnDemand(template, "test", null, new RazorDynamicObject(model));
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
                string result = service.RunCompileOnDemand(template, "test", null, new RazorDynamicObject(model));
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
    }
}
