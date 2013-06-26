using System.Linq;

namespace RazorEngine.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Collections;

    /// <summary>
    /// Provides service methods for compilation.
    /// </summary>
    public static class CompilerServicesUtility
    {
        #region Fields
        private static readonly Type DynamicType = typeof(DynamicObject);
        private static readonly Type ExpandoType = typeof(ExpandoObject);
        private static readonly Type EnumerableType = typeof(IEnumerable);
        private static readonly Type EnumeratorType = typeof(IEnumerator);
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
                throw new ArgumentNullException("type");

            return (type.IsClass
                    && type.IsSealed
                    && type.BaseType == typeof(object)
                    && type.Name.StartsWith("<>", StringComparison.Ordinal)
                    && type.IsDefined(typeof(CompilerGeneratedAttribute), true));
        }

        /// <summary>
        /// Determines if the specified type is a dynamic type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is an anonymous type, otherwise false.</returns>
        public static bool IsDynamicType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return (DynamicType.IsAssignableFrom(type)
                    || ExpandoType.IsAssignableFrom(type)
                    || IsAnonymousType(type));
        }

        /// <summary>
        /// Determines if the specified type is a compiler generated iterator type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if the type is an iterator type, otherwise false.</returns>
        public static bool IsIteratorType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return type.IsNestedPrivate
                && type.Name.StartsWith("<", StringComparison.Ordinal)
                && (EnumerableType.IsAssignableFrom(type) || EnumeratorType.IsAssignableFrom(type));
        }

        /// <summary>
        /// Generates a random class name.
        /// </summary>
        /// <returns>A new random class name.</returns>
        public static string GenerateClassName()
        {
            Guid guid = Guid.NewGuid();
            return Regex.Replace(guid.ToString("N"), @"[^A-Za-z]*", "");
        }

        /// <summary>
        /// Gets the public or protected constructors of the specified type.
        /// </summary>
        /// <param name="type">The target type.</param>
        /// <returns>An enumerable of constructors.</returns>
        public static IEnumerable<ConstructorInfo> GetConstructors(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var constructors = type
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            return constructors;
        }

        public static string ResolveCSharpTypeName(Type type)
        {
            if (IsIteratorType(type))
                type = GetFirstGenericInterface(type);

            if (IsDynamicType(type))
                return "dynamic";

            if (!type.IsGenericType)
                return type.FullName;

            return type.Namespace
                  + "."
                  + type.Name.Substring(0, type.Name.IndexOf('`'))
                  + "<"
                  + string.Join(", ", type.GetGenericArguments().Select(ResolveCSharpTypeName))
                  + ">";
        }

        public static string ResolveVBTypeName(Type type)
        {
            if (IsIteratorType(type))
                type = GetFirstGenericInterface(type);

            if (IsDynamicType(type))
                return "Object";

            if (!type.IsGenericType)
                return type.FullName;

            return type.Namespace
                  + "."
                  + type.Name.Substring(0, type.Name.IndexOf('`'))
                  + "(Of"
                  + string.Join(", ", type.GetGenericArguments().Select(ResolveVBTypeName))
                  + ")";
        }

        /// <summary>
        /// Gets the first generic interface of the specified type if one exists.
        /// </summary>
        /// <param name="type">The target type.</param>
        /// <returns>The first generic interface if one exists, otherwise the first interface or the target type itself if there are no interfaces.</returns>
        public static Type GetFirstGenericInterface(Type type)
        {
            Type firstInterface = null;
            foreach (var @interface in type.GetInterfaces())
            {
                if (firstInterface == null)
                    firstInterface = @interface;

                if (@interface.IsGenericType)
                    return @interface;
            }
            return @firstInterface ?? type;
        }

        /// <summary>
        /// Gets an enumerable of all assemblies loaded in the current domain.
        /// </summary>
        /// <returns>An enumerable of loaded assemblies.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static IEnumerable<Assembly> GetLoadedAssemblies()
        {
            var domain = AppDomain.CurrentDomain;
            return domain.GetAssemblies();
        }
        #endregion
    }
}