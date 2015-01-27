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
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;

namespace RazorEngine.Compilation.ImpromptuInterface
{
    /// <summary>
    /// Use for Named arguments passed to InvokeMethods
    /// </summary>
    [Serializable]
    public class InvokeArg
    {
        /// <summary>
        /// Performs an explicit conversion from <see cref="KeyValuePair{String,Object}"/> to <see cref="RazorEngine.Compilation.ImpromptuInterface.InvokeArg"/>.
        /// </summary>
        /// <param name="pair">The pair.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator InvokeArg(KeyValuePair<string, object> pair)
        {
            return new InvokeArg(pair.Key, pair.Value);
        }

        /// <summary>
        /// Create Function can set to variable to make cleaner syntax;
        /// </summary>
        public static readonly Func<string, object, InvokeArg> Create =
            new Func<string, object, InvokeArg>((n, v) => new InvokeArg(n, v));


        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeArg"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public InvokeArg(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the argument name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the argument value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; private set; }
    }

    /// <summary>
    /// InvokeArg that makes it easier to Cast from any IDictionaryValue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class InvokeArg<T> : InvokeArg
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvokeArg&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public InvokeArg(string name, object value) : base(name, value) { }

        /// <summary>
        /// Performs an explicit conversion from <see cref="KeyValuePair{String,Object}"/> to <see cref="InvokeArg{T}"/>.
        /// </summary>
        /// <param name="pair">The pair.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator InvokeArg<T>(KeyValuePair<string, T> pair)
        {
            return new InvokeArg<T>(pair.Key, pair.Value);
        }

    }
}
