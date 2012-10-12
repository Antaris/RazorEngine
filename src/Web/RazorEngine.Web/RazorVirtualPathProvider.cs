using System.Collections;
using System.Web.Caching;

namespace RazorEngine.Web
{
    using System;
    using System.Web.Hosting;

    /// <summary>
    /// Defines a virtual path provider used to resolve dynamic compiler calls.
    /// </summary>
    public class RazorVirtualPathProvider : VirtualPathProvider
    {
        /// <summary>
        /// Gets a value that indicates whether a file exists in either the physical or virtual file system.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <returns>
        /// true if the file exists in either the virtual or physical file system; otherwise, false.
        /// </returns>
        public override bool FileExists(string virtualPath)
        {
            return IsRazorVirtualPath(virtualPath) || base.FileExists(virtualPath);
        }

        /// <summary>
        /// Gets the physical system file if it exists.
        /// </summary>
        /// <param name="virtualPath">The path to the virtual file.</param>
        /// <returns>
        /// null if RazorTemplate; otherwise, the base provider will handle the request.
        /// </returns>
        public override VirtualFile GetFile(string virtualPath)
        {
            return IsRazorVirtualPath(virtualPath) ? null : base.GetFile(virtualPath);
        }

        /// <summary>
        /// Creates a cache dependency based on the specified virtual paths.
        /// </summary>
        /// <param name="virtualPath">The path to the primary virtual resource.</param>
        /// <param name="virtualPathDependencies">An array of paths to other resources required by the primary virtual resource.</param>
        /// <param name="utcStart">The UTC time at which the virtual resources were read.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Caching.CacheDependency"/> object for the specified virtual resources.
        /// </returns>
        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            return IsRazorVirtualPath(virtualPath) ? null : Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        /// <summary>
        /// Determines if the virtual path is a razor dynamic call.
        /// </summary>
        /// <param name="virtualPath">The virtual path.</param>
        /// <returns>True if the virtual file is a dynamic razor call, otherwise false.</returns>
        private static bool IsRazorVirtualPath(string virtualPath)
        {
            return virtualPath.StartsWith("/__razor/", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
