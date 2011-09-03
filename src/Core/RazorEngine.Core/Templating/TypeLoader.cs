namespace RazorEngine.Templating
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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
        private readonly AppDomain _appDomain;
        private readonly IEnumerable<Assembly> _assemblies;
        private readonly ConcurrentDictionary<Type, Func<ITemplate>> _constructors;
        private readonly ResolveEventHandler _resolveEventHandler;
        private bool disposed;
        #endregion

        #region Constructor
        /// <summary>
        /// Initialises a new instance of <see cref="TypeLoader"/>
        /// </summary>
        /// <param name="appDomain">The application domain.</param>
        /// <param name="assemblies">The set of assemblies.</param>
        public TypeLoader(AppDomain appDomain, IEnumerable<Assembly> assemblies)
        {
            Contract.Requires(appDomain != null);
            Contract.Requires(assemblies != null);

            _appDomain = appDomain;
            _assemblies = assemblies;
            _constructors = new ConcurrentDictionary<Type, Func<ITemplate>>();
            _resolveEventHandler = (s, e) => ResolveAssembly(e.Name);

            _appDomain.AssemblyResolve += _resolveEventHandler;
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
            Contract.Requires(type != null);

            if (disposed)
                throw new ObjectDisposedException("TypeLoader");

            var ctor = GetConstructor(type);
            return ctor();
        }

        /// <summary>
        /// Releases resources used by this instance.
        /// </summary>
        /// <param name="disposing">Flag to determine whether this instance is being disposed of explicitly.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                _appDomain.AssemblyResolve -= _resolveEventHandler;
                disposed = true;
            }
        }

        /// <summary>
        /// Releases resources used by this reference.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Gets the delegate used to create an instance of the template type. 
        /// This method will consider the cached constructor delegate before creating an instance of one.
        /// </summary>
        /// <param name="type">The template type.</param>
        /// <returns>The delegate instance.</returns>
        private Func<ITemplate> GetConstructor(Type type)
        {
            return _constructors
                .GetOrAdd(type, GetConstructorInternal);
        }

        /// <summary>
        /// Gets the delegate used to create an instance of the template type.
        /// </summary>
        /// <param name="type">The template type.</param>
        /// <returns>The delegate instance.</returns>
        private static Func<ITemplate> GetConstructorInternal(Type type)
        {
            var method = type.GetConstructor(new Type[0]);

            return Expression.Lambda<Func<ITemplate>>(
                Expression.New(method)).Compile();
        }

        /// <summary>
        /// Resolves the assembly with the specified name.
        /// </summary>
        /// <param name="name">The name of the assembly.</param>
        /// <returns>The assembly instance, or null.</returns>
        private Assembly ResolveAssembly(string name)
        {
            return _assemblies
                .Where(a => a.FullName.Equals(name))
                .FirstOrDefault();
        }
        #endregion
    }
}
