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
        private static readonly Type GenericEnumerableType = typeof(IEnumerable<>);
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
                    && (type.Name.StartsWith("<>", StringComparison.Ordinal)
                        || type.Name.StartsWith("VB$Anonymous", StringComparison.Ordinal))
                    && type.IsDefined(typeof(CompilerGeneratedAttribute), true));
        }

        /// <summary>
        /// Checks if the given type is a anonymous type or a generic type containing a 
        /// reference type as generic type argument
        /// </summary>
        /// <param name="t">the type to check</param>
        /// <returns>true when there exists a reference to an anonymous type.</returns>
        public static bool IsAnonymousTypeRecursive(Type t)
        {
            return t != null && (CompilerServicesUtility.IsAnonymousType(t) ||
                // part of generic
                t.GetGenericArguments().Any(arg => IsAnonymousTypeRecursive(arg)) ||
                // Array is special
                (t.IsArray && IsAnonymousTypeRecursive(t.GetElementType())));
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
            return String.Format("{0}{1}", CompilerServiceBase.ClassNamePrefix, guid.ToString("N"));
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

        /// <summary>
        /// Resolves the C# name of the given type.
        /// </summary>
        /// <param name="type">the type to emit.</param>
        /// <returns>The full type name or dynamic if the type is an instance of an dynamic type.</returns>
        public static string ResolveCSharpTypeName(Type type)
        {
            if (IsIteratorType(type))
                type = GetIteratorInterface(type);

            if (IsDynamicType(type))
                return "dynamic";

            var rawFullName = CSharpGetRawTypeName(type);
            if (!type.IsGenericType)
                return rawFullName;

            return rawFullName
                  + "<"
                  + string.Join(", ", type.GetGenericArguments().Select(ResolveCSharpTypeName))
                  + ">";
        }

        /// <summary>
        /// Resolves the VB.net name of the given type.
        /// </summary>
        /// <param name="type">the type to emit.</param>
        /// <returns>The full type name or Object if the type is an instance of an dynamic type.</returns>
        public static string ResolveVBTypeName(Type type)
        {
            if (IsIteratorType(type))
                type = GetIteratorInterface(type);

            if (IsDynamicType(type))
                return "Object";

            var rawFullName = VBGetRawTypeName(type);
            if (!type.IsGenericType)
                return rawFullName;

            return rawFullName
                  + "(Of "
                  + string.Join(", ", type.GetGenericArguments().Select(ResolveVBTypeName))
                  + ")";
        }

        /// <summary>
        /// Gets the Iterator type for the given compiler generated iterator.
        /// </summary>
        /// <param name="type">The target type.</param>
        /// <returns>Tries to return IEnumerable of T if possible.</returns>
        public static Type GetIteratorInterface(Type type)
        {
            Type firstInterface = null;
            // try to find IEnumerable<>
            foreach (var @interface in type.GetInterfaces())
            {
                if (firstInterface == null)
                    firstInterface = @interface;

                if (@interface.IsGenericType && !@interface.IsGenericTypeDefinition && @interface.GetGenericTypeDefinition() == GenericEnumerableType)
                    return @interface;
            }
            // ok just use the first generic one
            foreach (var @interface in type.GetInterfaces())
            {
                if (@interface.IsGenericType)
                    return @interface;
            }
            // ok use the first one or the whole type.
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

        /// <summary>
        /// Return the raw type name (including namespace) without any generic arguments.
        /// Returns the typename in a way it can be used in C# code.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string CSharpGetRawTypeName(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("templateType");

            string fullName = type.FullName;
            if (type.IsGenericTypeDefinition || type.IsGenericType)
                fullName = type.FullName.Substring(0, type.FullName.IndexOf('`'));

            return fullName.Replace("+", ".");;
        }


        /// <summary>
        /// Return the raw type name (including namespace) without any generic arguments.
        /// Returns the typename in a way it can be used in VB.net code.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string VBGetRawTypeName(Type type) { return CSharpGetRawTypeName(type); }

        /// <summary>
        /// Return the raw type name (including namespace) with the given modelTypeName as generic argument (if applicable).
        /// Returns the typename in a way it can be used in C# code.
        /// </summary>
        /// <param name="templateType"></param>
        /// <param name="modelTypeName"></param>
        /// <param name="throwWhenNotGeneric"></param>
        /// <returns></returns>
        public static string CSharpCreateGenericType(Type templateType, string modelTypeName, bool throwWhenNotGeneric)
        {

            var templateTypeName = CSharpGetRawTypeName(templateType);

            if (!templateType.IsGenericTypeDefinition || !templateType.IsGenericType)
            {
                if (throwWhenNotGeneric)
                {
                    throw new NotSupportedException("The given base type is not generic!");
                }
                return templateTypeName;
            }

            return templateTypeName + "<" + modelTypeName + ">";
        }
        
        /// <summary>
        /// Return the raw type name (including namespace) with the given modelTypeName as generic argument (if applicable).
        /// Returns the typename in a way it can be used in VB.net code.
        /// </summary>
        /// <param name="templateType"></param>
        /// <param name="modelTypeName"></param>
        /// <param name="throwWhenNotGeneric"></param>
        /// <returns></returns>
        public static string VBCreateGenericType(Type templateType, string modelTypeName, bool throwWhenNotGeneric)
        {
            var templateTypeName = VBGetRawTypeName(templateType);

            if (!templateType.IsGenericTypeDefinition || !templateType.IsGenericType)
            {
                if (throwWhenNotGeneric)
                {
                    throw new NotSupportedException("The given base type is not generic!");
                }
                return templateTypeName;
            }

            return templateTypeName + "(Of " + modelTypeName + ")";
        }

        #endregion
    }
}
