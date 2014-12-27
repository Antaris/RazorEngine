using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine
{
    /// <summary>
    /// Provides quick access to the functionality of the <see cref="RazorEngineService"/> class.
    /// </summary>
    public static class Engine
    {
        private static object _syncLock = new object();
        private static IRazorEngineService _service;

        /// <summary>
        /// Quick access to RazorEngine. See <see cref="IRazorEngineService"/>.
        /// </summary>
        public static IRazorEngineService Razor
        {
            get
            {
                if (_service == null)
                {
                    lock (_syncLock)
                    {
                        if (_service == null)
                        {
                            _service = RazorEngineService.Create();
                        }
                    }
                }
                return _service;
            }
            set
            {
                _service = value;
            }
        }

        private static IRazorEngineService _isolatedService;

        /// <summary>
        /// Quick access to an isolated RazorEngine. See <see cref="IRazorEngineService"/>.
        /// </summary>
        public static IRazorEngineService IsolatedRazor
        {
            get
            {
                if (_isolatedService == null)
                {
                    lock (_syncLock)
                    {
                        if (_isolatedService == null)
                        {
                            _isolatedService = IsolatedRazorEngineService.Create();
                        }
                    }
                }
                return _isolatedService;
            }
            set
            {
                _isolatedService = value;
            }
        }
    }
}
