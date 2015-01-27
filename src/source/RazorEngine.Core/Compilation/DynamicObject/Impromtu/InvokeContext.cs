// 
//  Copyright 2011  Ekon Benefits
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;

namespace RazorEngine.Compilation.ImpromptuInterface
{



    /// <summary>
    /// Specific version of InvokeContext which declares a type to be used to invoke static methods.
    /// </summary>
    public class StaticContext : InvokeContext
    {
        /// <summary>
        /// Performs an explicit conversion from <see cref="System.Type"/> to <see cref="RazorEngine.Compilation.ImpromptuInterface.StaticContext"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator StaticContext(Type type)
        {
            return new StaticContext(type);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticContext"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public StaticContext(Type target)
            : base(target, true, null)
        {
        }
    }

    /// <summary>
    /// Object that stores a context with a target for dynamic invocation
    /// </summary>
    [Serializable]
    public class InvokeContext
    {

        /// <summary>
        /// Create Function can set to variable to make cleaner syntax;
        /// </summary>
        public static readonly Func<object, object, InvokeContext> CreateContext =
            new Func<object, object, InvokeContext>((t, c) => new InvokeContext(t, c));

        /// <summary>
        /// Create Function can set to variable to make cleaner syntax;
        /// </summary>
        public static readonly Func<Type, InvokeContext> CreateStatic =
            new Func<Type, InvokeContext>((t) => new InvokeContext(t, true, null));


        /// <summary>
        /// Create Function can set to variable to make cleaner syntax;
        /// </summary>
        public static readonly Func<Type, object, InvokeContext> CreateStaticWithContext =
            new Func<Type, object, InvokeContext>((t, c) => new InvokeContext(t, true, c));


        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        /// <value>The target.</value>
        public object Target { get; protected set; }
        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>The context.</value>
        public Type Context { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether [static context].
        /// </summary>
        /// <value><c>true</c> if [static context]; otherwise, <c>false</c>.</value>
        public bool StaticContext { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeContext"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="staticContext">if set to <c>true</c> [static context].</param>
        /// <param name="context">The context.</param>
        public InvokeContext(Type target, bool staticContext, object context)
        {
            if (context != null && !(context is Type))
            {
                context = context.GetType();
            }
            Target = target;
            Context = ((Type)context) ?? target;
            StaticContext = staticContext;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeContext"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="context">The context.</param>
        public InvokeContext(object target, object context)
        {
            this.Target = target;

            if (context != null && !(context is Type))
            {
                context = context.GetType();
            }

            Context = (Type)context;
        }
    }
}
