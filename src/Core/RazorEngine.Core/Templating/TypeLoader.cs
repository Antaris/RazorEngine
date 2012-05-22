//-----------------------------------------------------------------------------
// <copyright file="TypeLoader.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Defines a type loader.
    /// </summary>
    public class TypeLoader : IDisposable
    {
        #region Fields

        /// <summary>
        /// The application domain field
        /// </summary>
        private readonly AppDomain appDomain;

        /// <summary>
        /// The assembly field
        /// </summary>
        private readonly IEnumerable<Assembly> assemblies;

        /// <summary>
        /// The constructor collection field
        /// </summary>
        private readonly ConcurrentDictionary<Type, Func<ITemplate>> constructors;

        /// <summary>
        /// The event resolver field
        /// </summary>
        private readonly ResolveEventHandler resolveEventHandler;

        /// <summary>
        /// The disposing flag field
        /// </summary>
        private bool isdisposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeLoader"/> class.
        /// </summary>
        /// <param name="appDomain">The application domain.</param>
        /// <param name="assemblies">The set of assemblies.</param>
        public TypeLoader(AppDomain appDomain, IEnumerable<Assembly> assemblies)
        {
            /* ReSharper disable InvocationIsSkipped */
            Contract.Requires(appDomain != null);
            Contract.Requires(assemblies != null);
            /* ReSharper restore InvocationIsSkipped */

            this.appDomain = appDomain;
            this.assemblies = assemblies;
            this.constructors = new ConcurrentDictionary<Type, Func<ITemplate>>();
            this.resolveEventHandler = (s, e) => this.ResolveAssembly(e.Name);

            this.appDomain.AssemblyResolve += this.resolveEventHandler;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an instance of the specified type.
        /// </summary>
        /// <param name="type">The type to create.</param>
        /// <returns>An instance of the type.</returns>
        public ITemplate CreateInstance(Type type)
        {
            /* ReSharper disable InvocationIsSkipped */
            Contract.Requires(type != null);
            /* ReSharper restore InvocationIsSkipped */

            if (this.isdisposed)
            {
                throw new ObjectDisposedException("TypeLoader");
            }

            var ctor = this.GetConstructor(type);

            return ctor();
        }

        /// <summary>
        /// Releases resources used by this reference.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by this instance.
        /// </summary>
        /// <param name="disposing">Flag to determine whether this instance is being disposed of explicitly.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isdisposed && disposing)
            {
                this.appDomain.AssemblyResolve -= this.resolveEventHandler;
                this.isdisposed = true;
            }
        }

        /// <summary>
        /// Gets the delegate used to create an instance of the template type.
        /// </summary>
        /// <param name="type">The template type.</param>
        /// <returns>The delegate instance.</returns>
        private static Func<ITemplate> GetConstructorInternal(Type type)
        {
            var method = type.GetConstructor(new Type[0]);

            Debug.Assert(method != null, "method != null");
            return Expression.Lambda<Func<ITemplate>>(
                Expression.New(method)).Compile();
        }

        /// <summary>
        /// Gets the delegate used to create an instance of the template type. 
        /// This method will consider the cached constructor delegate before creating an instance of one.
        /// </summary>
        /// <param name="type">The template type.</param>
        /// <returns>The delegate instance.</returns>
        private Func<ITemplate> GetConstructor(Type type)
        {
            return this.constructors
                .GetOrAdd(type, GetConstructorInternal);
        }

        /// <summary>
        /// Resolves the assembly with the specified name.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <returns>The assembly instance, or null.</returns>
        private Assembly ResolveAssembly(string name)
        {
            return this.assemblies.FirstOrDefault(a => a.FullName.Equals(name));
        }

        #endregion
    }
}
