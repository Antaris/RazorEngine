using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
//using RazorEngine.Compilation.ImpromptuInterface.Internal.Support;
using RazorEngine.Compilation.ImpromptuInterface.Optimization;

namespace RazorEngine.Compilation.ImpromptuInterface.Dynamic
{
    /// <summary>
    /// Late bind types from libraries not not at compile type
    /// </summary>
    [Serializable]
    public class ImpromptuLateLibraryType : ImpromptuForwarder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImpromptuLateLibraryType"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ImpromptuLateLibraryType(Type type)
            : base(type)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImpromptuLateLibraryType"/> class.
        /// </summary>
        /// <param name="typeName">Qualified Name of the type.</param>
        public ImpromptuLateLibraryType(string typeName)
            : base(Type.GetType(typeName, false))
        {

        }

        /// <summary>
        /// Returns a late bound constructor
        /// </summary>
        /// <value>The late bound constructor</value>
        public dynamic @new
        {
            get { return new ConstructorForward((Type)Target); }
        }

        /// <summary>
        /// Forward argument to constructor including named arguments
        /// </summary>
        public class ConstructorForward : DynamicObject, ICustomTypeProvider
        {
            private readonly Type _type;
            internal ConstructorForward(Type type)
            {
                _type = type;
            }
            /// <summary>
            /// Tries to invoke.
            /// </summary>
            /// <param name="binder"></param>
            /// <param name="args"></param>
            /// <param name="result"></param>
            /// <returns></returns>
            public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
            {
                result = Impromptu.InvokeConstructor(_type, Util.NameArgsIfNecessary(binder.CallInfo, args));
                return true;
            }
#if SILVERLIGHT5
            public Type GetCustomType()
            {
                return this.GetDynamicCustomType();
            }
#endif
        }

        /// <summary>
        /// Gets a value indicating whether this Type is available at runtime.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is available; otherwise, <c>false</c>.
        /// </value>
        public bool IsAvailable
        {
            get { return Target != null; }
        }

        /// <summary>
        /// The call target.
        /// </summary>
        protected override object CallTarget
        {
            get
            {
                return InvokeContext.CreateStatic((Type)Target);
            }
        }


#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="ImpromptuForwarder"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected ImpromptuLateLibraryType(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

#endif
    }
}
