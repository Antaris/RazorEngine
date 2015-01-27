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

namespace RazorEngine.Compilation.ImpromptuInterface.Dynamic
{
    /// <summary>
    /// This interface can be used on your custom dynamic objects if you want impromptu interfaces without casting to object or using the static method syntax of ActLike.
    /// Also if you want to change the behavior for slightly for specific types as this will take precident when using the dynamic keyword or your specific type is known staticly.
    /// </summary>
    public interface IActLike
    {
        /// <summary>
        /// This interface can be used on your custom dynamic objects if you want impromptu interfaces without casting to object or using the static method syntax of ActLike.
        /// Also if you want to change the behavior for slightly for specific types as this will take precident when using the dynamic keyword or your specific type is known staticly.
        /// </summary>
        ///<param name="otherInterfaces"></param>
        ///<typeparam name="TInterface"></typeparam>
        ///<returns></returns>
        TInterface ActLike<TInterface>(params Type[] otherInterfaces) where TInterface : class;
    }
}