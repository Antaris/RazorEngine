using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RazorEngine.Compilation
{

    /// <summary>
    /// Helper class to cleanup locked files and folders after the current AppDomain has been unloaded.
    /// (Because of locking they can't be deleted while the current AppDomain is up, see https://github.com/Antaris/RazorEngine/issues/244)
    /// </summary>
    public class CrossAppDomainCleanUp : IDisposable
    {
        /// <summary>
        /// Simple helper object to print status messages across appdomains.
        /// </summary>
        public interface IPrinter
        {
            /// <summary>
            /// Print a status message
            /// </summary>
            /// <param name="format"></param>
            /// <param name="args"></param>
            void Print(string format, params object[] args);
            /// <summary>
            /// Print a error message.
            /// </summary>
            /// <param name="format"></param>
            /// <param name="args"></param>
            void PrintError(string format, params object[] args);
        }
        /// <summary>
        /// A simple Printer which wrints in stdout and stderr.
        /// </summary>
        public class Printer : CrossAppDomainObject, IPrinter
        {
            /// <summary>
            /// Creates a new simple printer.
            /// </summary>
            public Printer()
            {
            }

            void IPrinter.Print(string format, params object[] args)
            {
                Console.WriteLine(format, args);
            }
            void IPrinter.PrintError(string format, params object[] args)
            {
                Console.Error.WriteLine(format, args);
            }
        }

        /// <summary>
        /// A simple printer writing only in stderr.
        /// </summary>
        [Serializable]
        public class ErrorOnlyPrinter : IPrinter
        {
            /// <summary>
            /// Create a new ErrorOnlyPrinter.
            /// </summary>
            public ErrorOnlyPrinter()
            {
            }

            void IPrinter.Print(string format, params object[] args)
            {
            }

            void IPrinter.PrintError(string format, params object[] args)
            {
                Console.Error.WriteLine(format, args);
            }
        }

        /// <summary>
        /// A new empty printer, which prints nothing.
        /// </summary>
        [Serializable]
        public class EmptyPrinter : IPrinter
        {
            /// <summary>
            /// Creates a new EmptyPrinter
            /// </summary>
            public EmptyPrinter()
            {
            }

            void IPrinter.Print(string format, params object[] args)
            {
            }
            void IPrinter.PrintError(string format, params object[] args)
            {
            }
        }

        /// <summary>
        /// Helper class to check the state of the remote AppDomain.
        /// </summary>
        [SecurityCritical]
        public class CleanupHelper : global::RazorEngine.CrossAppDomainObject
        {
            /// <summary>
            /// A helper to be able to subscribe to the DomainUnload event.
            /// Additionally we use this object to check if the printer lives in the wrong appdomain 
            /// (which would lead to an application crash).
            /// </summary>
            [SecurityCritical]
            public class SubscribeHelper : CrossAppDomainObject
            {
                private CleanupHelper _helper = null;
                /// <summary>
                /// Subscribe to the DomainUnload event and call the helper back.
                /// </summary>
                /// <param name="helper"></param>
                public void Subscribe(CleanupHelper helper)
                {
                    if (helper == null)
                    {
                        throw new ArgumentNullException("helper");
                    }
                    _helper = helper;
                    SubscribePrivate();
                }

                /// <summary>
                /// Check if the given printer object is valid.
                /// </summary>
                /// <param name="printer"></param>
                public void CheckPrinter(IPrinter printer)
                {
                    CheckPrinterPrivate(printer);
                }

                [SecuritySafeCritical]
                private void CheckPrinterPrivate(IPrinter printer)
                {
                    (new PermissionSet(PermissionState.Unrestricted)).Assert();
                    if (!System.Runtime.Remoting.RemotingServices.IsObjectOutOfAppDomain(printer) ||
                        !System.Runtime.Remoting.RemotingServices.IsTransparentProxy(printer))
                    {
                        throw new InvalidOperationException("Printer lives in the appdomain which we wait to exit (printer will be dead when we need it)!");
                    }
                }
                [SecuritySafeCritical]
                private void SubscribePrivate()
                {
                    (new PermissionSet(PermissionState.Unrestricted)).Assert();

                    AppDomain.CurrentDomain.DomainUnload += domain_DomainUnload;
                }
                void domain_DomainUnload(object sender, EventArgs e)
                {
                    if (_helper != null)
                    {
                        _helper.domain_DomainUnload(sender, e);
                    }
                }
            }


            private readonly List<string> _toCleanup = new List<string>();
            private AppDomain _domain;
            private IPrinter _printer;
            private void CheckInit()
            {
                if (_domain == null)
                {
                    throw new InvalidOperationException("You need to init me first!");
                }
            }
            /// <summary>
            /// Init the current helper object with the given AppDomain.
            /// </summary>
            /// <param name="domain"></param>
            /// <param name="printer"></param>
            [SecurityCritical]
            public void Init(AppDomain domain, IPrinter printer = null)
            {
                (new PermissionSet(PermissionState.Unrestricted)).Assert();
                //(new System.Security.Permissions.ReflectionPermission(ReflectionPermissionFlag.AllFlags)).Assert();
                if (_domain != null)
                {
                    throw new InvalidOperationException("CleanupHelper can be used only for a single domain.");
                }
                if (domain == null)
                {
                    throw new ArgumentNullException("domain");
                }
                if (printer == null)
                {
                    printer = new ErrorOnlyPrinter();
                }
                _printer = printer;
                if (domain.IsDefaultAppDomain())
                {
                    //throw new InvalidOperationException("CleanupHelper will not work on the default AppDomain")
                    printer.PrintError("We can't cleanup temp files if you use RazorEngine on the default Appdomain.");
                    printer.PrintError("Create a new AppDomain and use RazorEngine from there!");
                }
                printer.Print("init cleanup helper for {0} in {1} ...", domain.FriendlyName, AppDomain.CurrentDomain.FriendlyName);
                this._domain = domain;

                //_domain.DomainUnload += domain_DomainUnload;
                ObjectHandle handle =
                    Activator.CreateInstanceFrom(
                        _domain, typeof(SubscribeHelper).Assembly.ManifestModule.FullyQualifiedName,
                        typeof(SubscribeHelper).FullName
                    );
                using (var subscribeHelper = (SubscribeHelper)handle.Unwrap())
                {
                    if (System.Runtime.Remoting.RemotingServices.IsTransparentProxy(printer))
                    {
                        subscribeHelper.CheckPrinter(printer);
                    }
                    subscribeHelper.Subscribe(this);
                }
            }


            [SecurityCritical]
            void domain_DomainUnload(object sender, EventArgs e)
            {
                var senderDomain = (AppDomain)sender;
                var friendlyName = senderDomain.FriendlyName;
                _printer.Print("Domain {0} is unloading starting task...", friendlyName);
                // NOTE: a background thread or task will get killed before it can finish the work!
                var t = new Thread(() => 
                {
                    try
                    {
                        _printer.Print("waiting a bit for {0}...", friendlyName);
                        Thread.Sleep(300);
                        _printer.Print("cleanup after {0}...", friendlyName);

                        foreach (var item in _toCleanup)
                        {
                            if (File.Exists(item))
                            {
                                try
                                {
                                    File.Delete(item);
                                }
                                catch (IOException exn)
                                {
                                    _printer.PrintError("Could not delete {0}: {1}", item, exn.ToString());
                                }
                                catch (UnauthorizedAccessException exn)
                                {
                                    _printer.PrintError("Could not delete {0}: {1}", item, exn.ToString());
                                }
                            }
                            if (Directory.Exists(item))
                            {
                                try
                                {
                                    Directory.Delete(item, true);
                                }
                                catch (IOException exn)
                                {
                                    _printer.PrintError("Could not delete {0}: {1}", item, exn.ToString());
                                }
                                catch (UnauthorizedAccessException exn)
                                {
                                    _printer.PrintError("Could not delete {0}: {1}", item, exn.ToString());
                                }
                            }
                        }
                        _printer.Print("unload ourself {0}...", AppDomain.CurrentDomain.FriendlyName);
                    }
                    catch (Exception exn)
                    {
                        _printer.PrintError("Unhandled error while cleaning up after {0}: {1}", friendlyName, exn.ToString());
                    }
                    // We are not needed anymore.
                    //AppDomain.Unload(AppDomain.CurrentDomain);
                });
                t.IsBackground = false;
                t.Start();
            }

            /// <summary>
            /// Register the given path for cleanup.
            /// </summary>
            /// <param name="path"></param>
            public void RegisterCleanupPath(string path)
            {
                _printer.Print("Register cleanup path {0} ...", path);
                CheckInit();
                _toCleanup.Add(path);
            }
        }

        private readonly AppDomain _domain;
        private readonly CleanupHelper _helper;
        /*
        [SecurityCritical]
        public static AppDomain GetDefaultAppDomain()
        {
            (new PermissionSet(PermissionState.Unrestricted)).Assert();
            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                return AppDomain.CurrentDomain;
            }
            var isMono = Type.GetType("Mono.Runtime") != null;
            if (isMono)
            {
                var prop = typeof(System.AppDomain).GetProperty("DefaultDomain", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                return (AppDomain)prop.GetMethod.Invoke(null, null);
            }
            else
            {
                mscoree.ICorRuntimeHost host = new mscoree.CorRuntimeHostClass();
                object domain;
                host.GetDefaultDomain(out domain);
                return (AppDomain)domain;
            }
        }*/

        /// <summary>
        /// Get the StrongName of the given assembly.
        /// </summary>
        /// <param name="ass"></param>
        /// <returns></returns>
        public static StrongName FromAssembly(Assembly ass)
        {
            var name = ass.GetName();
            byte[] pk = name.GetPublicKey();
            var blob = new StrongNamePublicKeyBlob(pk);
            return new StrongName(blob, name.Name, name.Version);
        }

        /// <summary>
        /// Create a new CrossAppDomainCleanUp object for the current AppDomain.
        /// </summary>
        /// <param name="toWatch">the appDomain to watch for unload.</param>
        /// <param name="printer"></param>
        [SecurityCritical]
        public CrossAppDomainCleanUp(AppDomain toWatch, IPrinter printer)
        {
            if (toWatch == null) throw new ArgumentNullException("toWatch");
            if (printer == null) throw new ArgumentNullException("printer");

            (new PermissionSet(PermissionState.Unrestricted)).Assert();
            var current = toWatch;

            AppDomainSetup adSetup = new AppDomainSetup();
            adSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            
            StrongName razorEngineAssembly = FromAssembly(typeof(RazorEngine.Templating.RazorEngineService).Assembly);
#if RAZOR4
            StrongName razorAssembly = FromAssembly(typeof(Microsoft.AspNet.Razor.RazorTemplateEngine).Assembly);
#else
            StrongName razorAssembly = FromAssembly(typeof(System.Web.Razor.RazorTemplateEngine).Assembly);
#endif

            _domain = AppDomain.CreateDomain(
                "CleanupHelperDomain_" + Guid.NewGuid().ToString(), null, 
                current.SetupInformation, new PermissionSet(PermissionState.Unrestricted),
                razorEngineAssembly, razorAssembly);

            ObjectHandle handle =
                Activator.CreateInstanceFrom(
                    _domain, typeof(CleanupHelper).Assembly.ManifestModule.FullyQualifiedName,
                    typeof(CleanupHelper).FullName
                );
            _helper = (CleanupHelper)handle.Unwrap();
            _helper.Init(current, CrossAppDomainCleanUp.CurrentPrinter);
        }

        /// <summary>
        /// Register the given path for cleanup.
        /// </summary>
        /// <param name="path"></param>

        [SecuritySafeCritical]
        public void RegisterCleanupPath(string path)
        {
            _helper.RegisterCleanupPath(path);
        }

        /// <summary>
        /// Dispose the current instance.
        /// </summary>

        [SecuritySafeCritical]
        public void Dispose()
        {
            _helper.Dispose();
        }

        [SecuritySafeCritical]
        private static CrossAppDomainCleanUp CreateInitial()
        {
            return new CrossAppDomainCleanUp(AppDomain.CurrentDomain, currentPrinter);
        }

        private static IPrinter currentPrinter = new ErrorOnlyPrinter();

        /// <summary>
        /// Gets or sets the printer that is used by default when creating new CrossAppDomainCleanUp objects.
        /// Do not use this property unless you know what you are doing.
        /// Settings this to a serializable object is safe, however setting this to a marshalbyrefobject
        /// can lead to errors if the object lives in the domain that is watched for unloading
        /// </summary>
        public static IPrinter CurrentPrinter { get { return currentPrinter; } set { currentPrinter = value; } }

        private static Lazy<CrossAppDomainCleanUp> cleanup = new Lazy<CrossAppDomainCleanUp>(CreateInitial);

        /// <summary>
        /// A cleanup instance for the current AppDomain
        /// </summary>
        public static CrossAppDomainCleanUp CurrentCleanup { get { return cleanup.Value; } }
    }
}
