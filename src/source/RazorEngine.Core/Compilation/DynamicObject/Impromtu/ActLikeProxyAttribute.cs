// 
//  Copyright 2011 Ekon Benefits
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
//using RazorEngine.Compilation.ImpromptuInterface.Internal.Support;

namespace RazorEngine.Compilation.ImpromptuInterface.Build
{
    /// <summary>
    /// Meta info describing proxy usage. Can be used to preload proxy.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [Serializable]
    public sealed class ActLikeProxyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActLikeProxyAttribute"/> class.
        /// </summary>
        /// <param name="interfaces">The interfaces.</param>
        /// <param name="context">The context.</param>
        public ActLikeProxyAttribute(Type[] interfaces, Type context)
        {
            Interfaces = interfaces;
            Context = context;
        }

        /// <summary>
        /// Gets or sets the interfaces.
        /// </summary>
        /// <value>The interfaces.</value>
        public Type[] Interfaces { get; set; }
        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>The context.</value>
        public Type Context { get; set; }
    }
}
