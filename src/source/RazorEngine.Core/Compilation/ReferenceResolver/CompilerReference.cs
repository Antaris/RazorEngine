using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine.Compilation.ReferenceResolver
{
    public abstract class CompilerReference
    {
        public interface ICompilerReferenceVisitor<T>
        {
            T Visit(Assembly assembly);
            T Visit(string file);
            T Visit(Stream stream);
            T Visit(byte[] byteArray);
        }

        public enum CompilerReferenceType
        {
            FileReference, DirectAssemblyReference, StreamReference, ByteArrayReference
        }

        public CompilerReferenceType ReferenceType { get; private set; }

        public abstract T Visit<T>(ICompilerReferenceVisitor<T> visitor);

        internal CompilerReference(CompilerReferenceType assemblyReference)
        {
            ReferenceType = assemblyReference;
        }

        public static CompilerReference From(string file)
        {
            return new FileReference(file);
        }

        public static CompilerReference From(Assembly assembly)
        {
            return new DirectAssemblyReference(assembly);
        }

        public static CompilerReference From(Stream stream)
        {
            return new StreamReference(stream);
        }

        public static CompilerReference From(byte[] byteArray)
        {
            return new ByteArrayReference(byteArray);
        }

        public string GetFile(Func<string, Exception> exceptionCreator = null)
        {
            return this.Visit(new SelectFileVisitor(exceptionCreator));
        }

        private class SelectFileVisitor : CompilerReference.ICompilerReferenceVisitor<string>
        {
            Func<string, Exception> exceptionCreator;
            public SelectFileVisitor(Func<string, Exception> exceptionCreator = null)
            {
               this.exceptionCreator = exceptionCreator ?? ((msg) => new InvalidOperationException(msg)); 
            }

            public string Visit(System.Reflection.Assembly assembly)
            {
                var file = assembly.Location;
                if (string.IsNullOrEmpty(file))
                {
                    throw this.exceptionCreator("Could not get location from assembly!");
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


        public class FileReference : CompilerReference
        {
            public string File { get; private set; }
            internal FileReference(string file)
                : base(CompilerReferenceType.FileReference)
            {
                File = file;
            }

            public override T Visit<T>(ICompilerReferenceVisitor<T> visitor)
            {
                return visitor.Visit(File);
            }
        }
        public class DirectAssemblyReference : CompilerReference
        {
            public Assembly Assembly { get; private set; }
            internal DirectAssemblyReference(Assembly assembly)
                : base(CompilerReferenceType.DirectAssemblyReference)
            {
                Assembly = assembly;
            }
            public override T Visit<T>(ICompilerReferenceVisitor<T> visitor)
            {
                return visitor.Visit(Assembly);
            }
        }

        public class StreamReference : CompilerReference
        {
            public Stream Stream { get; private set; }
            internal StreamReference(Stream stream)
                : base(CompilerReferenceType.StreamReference)
            {
                Stream = stream;
            }
            public override T Visit<T>(ICompilerReferenceVisitor<T> visitor)
            {
                return visitor.Visit(Stream);
            }
        }

        public class ByteArrayReference : CompilerReference
        {
            public byte[] ByteArray { get; private set; }
            internal ByteArrayReference(byte[] byteArray)
                : base(CompilerReferenceType.ByteArrayReference)
            {
                ByteArray = byteArray;
            }

            public override T Visit<T>(ICompilerReferenceVisitor<T> visitor)
            {
                return visitor.Visit(ByteArray);
            }
        }
    }
}
