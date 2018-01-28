using NUnit.Framework;
using RazorEngine.Compilation;
using RazorEngine.Templating;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.RazorEngine
{
    /// <summary>
    /// Helper class to check the state of the remote AppDomain.
    /// </summary>
    public class AppDomainHelper_Cleanup : global::RazorEngine.CrossAppDomainObject
    {
        /// <summary>
        /// Check if everything is cleaned up.
        /// </summary>
        public string RazorEngineService_CleanUpWorks(CrossAppDomainCleanUp.IPrinter printer)
        {
            CrossAppDomainCleanUp.CurrentPrinter = printer;
            var cache = new DefaultCachingProvider();
            RazorEngineServiceTestFixture.RunTestHelper(service =>
            {
                string template = @"@Model.Name";

                var result = service.RunCompile(template, "test", null, new { Name = "test" });
                Assert.AreEqual("test", result);
            }, config =>
            {
                config.CachingProvider = cache;
            });
            ICompiledTemplate compiledTemplate;
            Assert.IsTrue(cache.TryRetrieveTemplate(new NameOnlyTemplateKey("test", ResolveType.Global, null), null, out compiledTemplate));
            var folder = compiledTemplate.CompilationData.TmpFolder;
            Assert.IsTrue(Directory.Exists(folder));
            return folder;
        }
    }

    /// <summary>
    /// Tests if razorEngine cleans up the generated folders.
    /// </summary>
    [TestFixture]
    public class RazorEngineCleanupTests
    {
        /// <summary>
        /// Tests that a bad template cannot do stuff
        /// </summary>
        [Test]
        public void RazorEngineService_CleanUpWorks()
        {
            var current = AppDomain.CurrentDomain;
            var appDomain = AppDomain.CreateDomain("TestDomain.IsolatedRazorEngineService_CleanUpWorks", current.Evidence, current.SetupInformation);

            ObjectHandle handle =
                Activator.CreateInstanceFrom(
                    appDomain, typeof(AppDomainHelper_Cleanup).Assembly.ManifestModule.FullyQualifiedName,
                    typeof(AppDomainHelper_Cleanup).FullName
                );
            string tmp_folder = null;
            using (var remoteHelper = (AppDomainHelper_Cleanup)handle.Unwrap())
            {
                tmp_folder = remoteHelper.RazorEngineService_CleanUpWorks(new CrossAppDomainCleanUp.Printer());
                Assert.IsTrue(Directory.Exists(tmp_folder));
            }

            AppDomain.Unload(appDomain);
            GC.Collect();
            System.Threading.Thread.Sleep(1000);
            Assert.IsFalse(Directory.Exists(tmp_folder));
        }

        /// <summary>
        /// Tests whether we can delete tempfiles when DisableTempFileLocking is true.
        /// </summary>
        [Test]
        public void RazorEngineService_TestDisableTempFileLocking()
        {
            var cache = new DefaultCachingProvider(t => { });
            var template = "@Model.Property";
            RazorEngineServiceTestFixture.RunTestHelper(service =>
            {
                var model = new { Property = 0 };
                string result = service.RunCompile(template, "key", null, model);
                Assert.AreEqual("0", result.Trim());
            }, config =>
            {
                config.CachingProvider = cache;
                config.DisableTempFileLocking = true;
            });
            ICompiledTemplate compiledTemplate;
            Assert.IsTrue(cache.TryRetrieveTemplate(new NameOnlyTemplateKey("key", ResolveType.Global, null), null, out compiledTemplate));
            var data = compiledTemplate.CompilationData;
            var folder = data.TmpFolder;
            Assert.IsTrue(Directory.Exists(folder));
            data.DeleteAll();
            Assert.IsFalse(Directory.Exists(folder));
        }

        // The test below requires a COM object to run that is not available on the CI server.
        // To run, add the following as a COM reference to the project: - C:\WINDOWS\Microsoft.NET\Framework\vXXXXXX\mscoree.tlb

        //[Serializable]
        //public class TemplateModel
        //{
        //    public string Name { get; set; }
        //}

        //[Test]
        //public void RazorEngineService_CleanupDomainIsUnloaded()
        //{
        //    Func<bool> CleanupDomainExists = () =>
        //      GetAppDomains()
        //        .Any(x => x.FriendlyName.StartsWith("CleanupHelperDomain_"));

        //    Assert.False(CleanupDomainExists(), "Cleanup helper app domain should not exist before test is run.");
        //    using (var service = IsolatedRazorEngineService.Create())
        //    {
        //        service.AddTemplate("TestTemplate",
        //          "Hello @Model.Name " + new String(Enumerable.Repeat('A', 100000).ToArray()));
        //        service.Compile("TestTemplate", typeof(TemplateModel));

        //        Assert.True(CleanupDomainExists(), "Cleanup helper app domain should exist after a template is compiled");
        //    }

        //    Thread.Sleep(300);
        //    Assert.False(CleanupDomainExists(), "Cleanup helper app domain must be properly unloaded after service is disposed");
        //}

        //private static IList<AppDomain> GetAppDomains()
        //{
        //    IList<AppDomain> _IList = new List<AppDomain>();
        //    IntPtr enumHandle = IntPtr.Zero;
        //    mscoree.ICorRuntimeHost host = new mscoree.CorRuntimeHost();
        //    try
        //    {
        //        host.EnumDomains(out enumHandle);
        //        object domain = null;
        //        while (true)
        //        {
        //            host.NextDomain(enumHandle, out domain);
        //            if (domain == null) break;
        //            AppDomain appDomain = (AppDomain)domain;
        //            _IList.Add(appDomain);
        //        }
        //        return _IList;
        //    }
        //    finally
        //    {
        //        host.CloseEnum(enumHandle);
        //        Marshal.ReleaseComObject(host);
        //    }
        //}
    }
}
