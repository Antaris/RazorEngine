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

namespace RazorEngine.Compilation.ImpromptuInterface.Dynamic
{
    /// <summary>
    /// This interface can be used on your custom dynamic objects if you want to know the interface you are impromptu-ly implementing.
    /// </summary>
    public interface IDynamicKnowLike
    {
        ///<summary>
        /// Property used to pass interface information to proxied object
        ///</summary>
        IEnumerable<Type> KnownInterfaces { set; }

        /// <summary>
        /// Sets the known property spec.
        /// </summary>
        /// <value>The known property spec.</value>
        IDictionary<string, Type> KnownPropertySpec { set; }
    }
}