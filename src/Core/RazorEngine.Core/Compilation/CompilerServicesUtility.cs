//-----------------------------------------------------------------------------
// <copyright file="CompilerServicesUtility.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
namespace RazorEngine.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides service methods for compilation.
    /// </summary>
    public static class CompilerServicesUtility
    {
        #region Fields

        /// <summary>
        /// The type
        /// </summary>
        private static readonly Type DynamicType = typeof(DynamicObject);

        /// <summary>
        /// The type 
        /// </summary>
        private static readonly Type ExpandoType = typeof(ExpandoObject);

        #endregion

        #region Methods

        /// <summary>
        /// Determines if the specified type is an anonymous type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is an anonymous type, otherwise false.</returns>
        public static bool IsAnonymousType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return type.IsClass
                    && type.IsSealed
                    && type.BaseType == typeof(object)
                    && type.Name.StartsWith("<>", StringComparison.Ordinal)
                    && type.IsDefined(typeof(CompilerGeneratedAttribute), true);
        }

        /// <summary>
        /// Determines if the specified type is a dynamic type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is an anonymous type, otherwise false.</returns>
        public static bool IsDynamicType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return DynamicType.IsAssignableFrom(type)
                    || ExpandoType.IsAssignableFrom(type)
                    || IsAnonymousType(type);
        }

        /// <summary>
        /// Generates a random class name.
        /// </summary>
        /// <returns>A new random class name.</returns>
        public static string GenerateClassName()
        {
            return "RazorEngineAuto" + Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Gets the public or protected constructors of the specified type.
        /// </summary>
        /// <param name="type">The target type.</param>
        /// <returns>An enumerable of constructors.</returns>
        public static IEnumerable<ConstructorInfo> GetConstructors(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            return constructors;
        }

        /// <summary>
        /// Gets an enumerable of all assemblies loaded in the current domain.
        /// </summary>
        /// <returns>An enumerable of loaded assemblies.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Reviewed. Suppression is OK here.")]
        public static IEnumerable<Assembly> GetLoadedAssemblies()
        {
            var domain = AppDomain.CurrentDomain;
            return domain.GetAssemblies();
        }

        #endregion
    }
}