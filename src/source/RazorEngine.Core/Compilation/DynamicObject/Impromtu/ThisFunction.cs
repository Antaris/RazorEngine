
//  Copyright 2010  Ekon Benefits
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
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction(dynamic @this);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1>(dynamic @this, T1 arg1);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2>(dynamic @this, T1 arg1, T2 arg2);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3>(dynamic @this, T1 arg1, T2 arg2, T3 arg3);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5, in T6>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate void ThisAction<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<out TResult>(dynamic @this);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, out TResult>(dynamic @this, T1 arg1);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, out TResult>(dynamic @this, T1 arg1, T2 arg2);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, in T6, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15);
    /// <summary>
    /// Special Delegate used to make impromptu object methods first parameter is this.
    /// </summary>
    public delegate TResult ThisFunc<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, out TResult>(dynamic @this, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);

    /// <summary>
    /// Extension method for Dealing with Special Delegate Type
    /// </summary>
    public static class ThisDelegate
    {
        private static readonly HashSet<Type> _specialThisDels = new HashSet<Type>(){
				typeof(ThisAction),
				typeof(ThisFunc<>),
			
				typeof(ThisAction<,>),
				typeof(ThisFunc<,>),
			
				typeof(ThisAction<,,>),
				typeof(ThisFunc<,,>),
			
				typeof(ThisAction<,,,>),
				typeof(ThisFunc<,,,>),
			
				typeof(ThisAction<,,,,>),
				typeof(ThisFunc<,,,,>),
			
				typeof(ThisAction<,,,,,>),
				typeof(ThisFunc<,,,,,>),
			
				typeof(ThisAction<,,,,,,>),
				typeof(ThisFunc<,,,,,,>),
			
				typeof(ThisAction<,,,,,,,>),
				typeof(ThisFunc<,,,,,,,>),
			
				typeof(ThisAction<,,,,,,,,>),
				typeof(ThisFunc<,,,,,,,,>),
			
				typeof(ThisAction<,,,,,,,,,>),
				typeof(ThisFunc<,,,,,,,,,>),
			
				typeof(ThisAction<,,,,,,,,,,>),
				typeof(ThisFunc<,,,,,,,,,,>),
			
				typeof(ThisAction<,,,,,,,,,,,>),
				typeof(ThisFunc<,,,,,,,,,,,>),
			
				typeof(ThisAction<,,,,,,,,,,,,>),
				typeof(ThisFunc<,,,,,,,,,,,,>),
			
				typeof(ThisAction<,,,,,,,,,,,,,>),
				typeof(ThisFunc<,,,,,,,,,,,,,>),
			
				typeof(ThisAction<,,,,,,,,,,,,,,>),
				typeof(ThisFunc<,,,,,,,,,,,,,,>),
			
				typeof(ThisAction<,,,,,,,,,,,,,,,>),
				typeof(ThisFunc<,,,,,,,,,,,,,,,>),
				typeof(ThisFunc<,,,,,,,,,,,,,,,,>),
		};

        /// <summary>
        /// Determines whether [is special this delegate] [the specified del].
        /// </summary>
        /// <param name="del">The del.</param>
        /// <returns>
        /// 	<c>true</c> if [is special this delegate] [the specified del]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSpecialThisDelegate(this Delegate del)
        {
            var tType = del.GetType();
            if (!tType.IsGenericType) return false;
            var tGenDel = del.GetType().GetGenericTypeDefinition();
            var tReturn = _specialThisDels.Contains(tGenDel);
            return tReturn;

        }
    }
}
