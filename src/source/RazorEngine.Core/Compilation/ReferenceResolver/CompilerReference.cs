using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Compilation.ReferenceResolver
{
    /// <summary>
    /// Represents a reference for the compiler
    /// </summary>
    public abstract class CompilerReference
    {
        /// <summary>
        /// Visitor pattern for the <see cref="CompilerReference"/> class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public interface ICompilerReferenceVisitor<T>
        {
            /// <summary>
            /// Handle a direct assembly reference
            /// </summary>
            /// <param name="assembly"></param>
            /// <returns></returns>
            T Visit(Assembly assembly);
            /// <summary>
            /// Handle a file reference.
            /// </summary>
            /// <param name="file"></param>
            /// <returns></returns>
            T Visit(string file);
            /// <summary>
            /// Handle a stream reference.
            /// </summary>
            /// <param name="stream"></param>
            /// <returns></returns>
            T Visit(Stream stream);
            /// <summary>
            /// Handle a byte array reference.
            /// </summary>
            /// <param name="byteArray"></param>
            /// <returns></returns>
            T Visit(byte[] byteArray);
        }

        /// <summary>
        /// The Type of the reference
        /// </summary>
        public enum CompilerReferenceType
        {
            /// <summary>
            /// Reference to a file
            /// </summary>
            FileReference, 
            /// <summary>
            /// Reference to a assembly instace
            /// </summary>
            DirectAssemblyReference,
            /// <summary>
            /// Reference to a assembly stream.
            /// </summary>
            StreamReference, 
            /// <summary>
            /// Reference to a assembly within a byte array.
            /// </summary>
            ByteArrayReference
        }

        /// <summary>
        /// The type of the current reference.
        /// </summary>
        public CompilerReferenceType ReferenceType { get; private set; }

        /// <summary>
        /// execute the given visitor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="visitor"></param>
        /// <returns></returns>
        public abstract T Visit<T>(ICompilerReferenceVisitor<T> visitor);

        internal CompilerReference(CompilerReferenceType assemblyReference)
        {
            ReferenceType = assemblyReference;
        }

        /// <summary>
        /// Create a compiler reference from the given file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static CompilerReference From(string file)
        {
            return new FileReference(file);
        }

        /// <summary>
        /// Create a compiler reference from the given assembly.
        /// NOTE: The CodeDom compiler doesn't support assembly references where assembly.Location is null (roslyn only)!
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static CompilerReference From(Assembly assembly)
        {
            return new DirectAssemblyReference(assembly);
        }

        /// <summary>
        /// Create a compiler reference from the given stream.
        /// NOTE: The CodeDom compiler doesn't support stream references (roslyn only)!
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static CompilerReference From(Stream stream)
        {
            return new StreamReference(stream);
        }

        /// <summary>
        /// Create a compiler reference from the given byte array.
        /// NOTE: The CodeDom compiler doesn't support byte array references (roslyn only)!
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static CompilerReference From(byte[] byteArray)
        {
            return new ByteArrayReference(byteArray);
        }

        /// <summary>
        /// Default implementation for resolving an assembly name.
        /// </summary>
        /// <param name="assemblyName">name of the assembly to resolve</param>
        /// <param name="references">references to check</param>
        /// <returns>the resolved assembly or null</returns>
        internal static Assembly Resolve(string assemblyName, IEnumerable<CompilerReference> references)
        {
            // First try the loaded ones
            foreach (var reference in references)
            {
                var assemblyReference = reference as DirectAssemblyReference;
                if (assemblyReference != null && assemblyReference.Assembly.GetName().FullName == assemblyName)
                {
                    return assemblyReference.Assembly;
                }
            }
            // Then try the files
            foreach (var reference in references)
            {
                var fileReference = reference as FileReference;
                if (fileReference != null && AssemblyName.GetAssemblyName(fileReference.File).FullName == assemblyName)
                {
                    return Assembly.LoadFrom(fileReference.File);
                }
            }
            return null;
        }

        /// <summary>
        /// Try to resolve the reference to a file (throws when this is not possible).
        /// </summary>
        /// <param name="exceptionCreator"></param>
        /// <returns></returns>
        public string GetFile(Func<string, Exception> exceptionCreator = null)
        {
            return this.Visit(new SelectFileVisitor(exceptionCreator));
        }

        private static bool ByteArrayCompare(byte[] a1, byte[] a2)
        {
            if (a1 == a2)
            {
                return true;
            }
            if ((a1 != null) && (a2 != null))
            {
                if (a1.Length != a2.Length)
                {
                    return false;
                }
                for (int i = 0; i < a1.Length; i++)
                {
                    if (a1[i] != a2[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the given object is equal to the current object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var other = obj as CompilerReference;
            if (other == null) return false;

            var thisAssembly = this as DirectAssemblyReference;
            if (thisAssembly != null)
            {
                var assembly = thisAssembly.Assembly;
                var assemblyRef = other as DirectAssemblyReference;
                if (assemblyRef != null)
                {
                    return assembly == assemblyRef.Assembly;
                }
                var fileRef = other as FileReference;
                if (fileRef != null)
                {
                    return fileRef.File == assembly.Location;
                }

                return false;
            }

            var thisFile = this as FileReference;
            if (thisFile != null)
            {
                var file = thisFile.File;
                var fileRef = other as FileReference;
                if (fileRef != null)
                {
                    return fileRef.File == file;
                }

                var assemblyRef = other as DirectAssemblyReference;
                if (assemblyRef != null)
                {
                    return file == assemblyRef.Assembly.Location;
                }
            }
            var thisByte = this as ByteArrayReference;
            if (thisByte != null)
            {
                var byteRef = other as ByteArrayReference;
                if (byteRef != null)
                {
                    return ByteArrayCompare(thisByte.ByteArray, byteRef.ByteArray);
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a hashcode for the current object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var thisAssembly = this as DirectAssemblyReference;
            if (thisAssembly != null)
            {
                if (thisAssembly.Assembly.Location != null)
                {
                    return thisAssembly.Assembly.Location.GetHashCode();
                }
                else
                {
                    thisAssembly.Assembly.GetHashCode();
                }
            }
            var thisFile = this as FileReference;
            if (thisFile != null)
            {
                return thisFile.File.GetHashCode();
            }

            var thisBytes = this as ByteArrayReference;
            if (thisBytes != null)
            {
                return thisBytes.ByteArray.GetHashCode();
            }

            var thisStream = this as StreamReference;
            if (thisStream != null)
            {
                return thisStream.Stream.GetHashCode();
            }

            throw new InvalidOperationException("Unknown CompilerReference!");
        }

        /// <summary>
        /// A visitor for the GetFile function.
        /// </summary>
        private class SelectFileVisitor : CompilerReference.ICompilerReferenceVisitor<string>
        {
            private Func<string, Exception> exceptionCreator;
            public SelectFileVisitor(Func<string, Exception> exceptionCreator = null)
            {
                this.exceptionCreator = exceptionCreator ?? ((msg) => new InvalidOperationException(msg));
            }

            public string Visit(System.Reflection.Assembly assembly)
            {
                var file = assembly.Location;
                if (string.IsNullOrEmpty(file))
                {
                    throw this.exceptionCreator(string.Format("Could not get location from assembly {0}!", assembly.FullName));
                }
                return file;
            }

            public string Visit(string file)
            {
                return file;
            }

            public string Visit(System.IO.Stream stream)
            {
                throw this.exceptionCreator("need file but got stream reference!");
            }

            public string Visit(byte[] byteArray)
            {
                throw this.exceptionCreator("need file but got byteArray reference!");
            }
        }

        /// <summary>
        /// A file reference.
        /// </summary>
        public class FileReference : CompilerReference
        {
            /// <summary>
            /// The referenced file.
            /// </summary>
            public string File { get; private set; }
            internal FileReference(string file)
                : base(CompilerReferenceType.FileReference)
            {
                File = new System.Uri(Path.GetFullPath(file)).LocalPath;
            }

            /// <summary>
            /// Visit the given visitor.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="visitor"></param>
            /// <returns></returns>
            public override T Visit<T>(ICompilerReferenceVisitor<T> visitor)
            {
                return visitor.Visit(File);
            }
        }

        /// <summary>
        /// A direct assembly reference.
        /// </summary>
        public class DirectAssemblyReference : CompilerReference
        {
            /// <summary>
            /// The referenced assembly.
            /// </summary>
            public Assembly Assembly { get; private set; }
            internal DirectAssemblyReference(Assembly assembly)
                : base(CompilerReferenceType.DirectAssemblyReference)
            {
                Assembly = assembly;
            }

            /// <summary>
            /// Visit the visitor.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="visitor"></param>
            /// <returns></returns>
            public override T Visit<T>(ICompilerReferenceVisitor<T> visitor)
            {
                return visitor.Visit(Assembly);
            }
        }

        /// <summary>
        /// A stream reference.
        /// </summary>
        public class StreamReference : CompilerReference
        {
            /// <summary>
            /// The referenced stream.
            /// </summary>
            public Stream Stream { get; private set; }
            internal StreamReference(Stream stream)
                : base(CompilerReferenceType.StreamReference)
            {
                Stream = stream;
            }

            /// <summary>
            /// Visit the given visitor.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="visitor"></param>
            /// <returns></returns>
            public override T Visit<T>(ICompilerReferenceVisitor<T> visitor)
            {
                return visitor.Visit(Stream);
            }
        }

        /// <summary>
        /// A byte array reference.
        /// </summary>
        public class ByteArrayReference : CompilerReference
        {
            /// <summary>
            /// The referenced byte array.
            /// </summary>
            public byte[] ByteArray { get; private set; }
            internal ByteArrayReference(byte[] byteArray)
                : base(CompilerReferenceType.ByteArrayReference)
            {
                ByteArray = byteArray;
            }

            /// <summary>
            /// Visit the given visitor.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="visitor"></param>
            /// <returns></returns>
            public override T Visit<T>(ICompilerReferenceVisitor<T> visitor)
            {
                return visitor.Visit(ByteArray);
            }
        }
    }
}
