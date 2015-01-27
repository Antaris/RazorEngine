
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
using System.Runtime.CompilerServices;
using RazorEngine.Compilation.ImpromptuInterface.Build;
using System.Reflection;

namespace RazorEngine.Compilation.ImpromptuInterface.Optimization
{


    internal static partial class InvokeHelper
    {


        internal static readonly Type[] FuncKinds;
        internal static readonly Type[] ActionKinds;
        internal static readonly IDictionary<Type, int> FuncArgs;
        internal static readonly IDictionary<Type, int> ActionArgs;

        static InvokeHelper()
        {
            FuncKinds = new[]
                            {
								typeof(Func<>), //0
								typeof(Func<,>), //1
								typeof(Func<,,>), //2
								typeof(Func<,,,>), //3
								typeof(Func<,,,,>), //4
								typeof(Func<,,,,,>), //5
								typeof(Func<,,,,,,>), //6
								typeof(Func<,,,,,,,>), //7
								typeof(Func<,,,,,,,,>), //8
								typeof(Func<,,,,,,,,,>), //9
								typeof(Func<,,,,,,,,,,>), //10
								typeof(Func<,,,,,,,,,,,>), //11
								typeof(Func<,,,,,,,,,,,,>), //12
								typeof(Func<,,,,,,,,,,,,,>), //13
								typeof(Func<,,,,,,,,,,,,,,>), //14
								typeof(Func<,,,,,,,,,,,,,,,>), //15
								typeof(Func<,,,,,,,,,,,,,,,,>), //16
                            };

            ActionKinds = new[]
                            {
                                typeof(Action), //0
								typeof(Action<>), //1
								typeof(Action<,>), //2
								typeof(Action<,,>), //3
								typeof(Action<,,,>), //4
								typeof(Action<,,,,>), //5
								typeof(Action<,,,,,>), //6
								typeof(Action<,,,,,,>), //7
								typeof(Action<,,,,,,,>), //8
								typeof(Action<,,,,,,,,>), //9
								typeof(Action<,,,,,,,,,>), //10
								typeof(Action<,,,,,,,,,,>), //11
								typeof(Action<,,,,,,,,,,,>), //12
								typeof(Action<,,,,,,,,,,,,>), //13
								typeof(Action<,,,,,,,,,,,,,>), //14
								typeof(Action<,,,,,,,,,,,,,,>), //15
								typeof(Action<,,,,,,,,,,,,,,,>), //16
                            };


            FuncArgs = FuncKinds.Zip(Enumerable.Range(0, FuncKinds.Length), (key, value) => new { key, value }).ToDictionary(k => k.key, v => v.value);
            ActionArgs = ActionKinds.Zip(Enumerable.Range(0, ActionKinds.Length), (key, value) => new { key, value }).ToDictionary(k => k.key, v => v.value);

        }




        internal static void InvokeMemberAction(ref CallSite callsite,
                                                    Type binderType,
                                                    int knownType,
                                                    LazyBinder binder,
                                                    String_OR_InvokeMemberName name,
                                                    bool staticContext,
                                                    Type context,
                                                    string[] argNames,
                                                    object target,
                                                    params object[] args)
        {

            var tSwitch = args.Length;
            switch (tSwitch)
            {
                #region Optimizations
                case 0:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target);
                        break;
                    }
                case 1:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0]);
                        break;
                    }
                case 2:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1]);
                        break;
                    }
                case 3:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2]);
                        break;
                    }
                case 4:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3]);
                        break;
                    }
                case 5:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4]);
                        break;
                    }
                case 6:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5]);
                        break;
                    }
                case 7:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                        break;
                    }
                case 8:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object, object, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object, object, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                        break;
                    }
                case 9:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
                        break;
                    }
                case 10:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
                        break;
                    }
                case 11:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
                        break;
                    }
                case 12:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
                        break;
                    }
                case 13:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
                        break;
                    }
                case 14:
                    {
                        var tCallSite = (CallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Action<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
                        break;
                    }
                #endregion
                default:
                    var tArgTypes = Enumerable.Repeat(typeof(object), tSwitch);
                    var tDelagateType = BuildProxy.GenerateCallSiteFuncType(tArgTypes, typeof(void));
                    Impromptu.InvokeCallSite(CreateCallSite(tDelagateType, binderType, knownType, binder, name, context, argNames), target, args);
                    break;

            }
        }










        internal static TReturn InvokeMemberTargetType<TTarget, TReturn>(
                                        ref CallSite callsite,
                                        Type binderType,
                                        int knownType,
                                        LazyBinder binder,
                                       String_OR_InvokeMemberName name,
                                     bool staticContext,
                                     Type context,
                                     string[] argNames,
                                     TTarget target, params object[] args)
        {



            var tSwitch = args.Length;

            switch (tSwitch)
            {
                #region Optimizations
                case 0:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target);
                    }
                case 1:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0]);
                    }
                case 2:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1]);
                    }
                case 3:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2]);
                    }
                case 4:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3]);
                    }
                case 5:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4]);
                    }
                case 6:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5]);
                    }
                case 7:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                    }
                case 8:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                    }
                case 9:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
                    }
                case 10:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
                    }
                case 11:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
                    }
                case 12:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
                    }
                case 13:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
                    }
                case 14:
                    {
                        var tCallSite = (CallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>>)callsite;
                        if (tCallSite == null)
                        {
                            tCallSite = CreateCallSite<Func<CallSite, TTarget, object, object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>>(binderType, knownType, binder, name, context, argNames, staticContext);
                            callsite = tCallSite;
                        }
                        return tCallSite.Target(tCallSite, target, args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
                    }
                #endregion
                default:
                    var tArgTypes = Enumerable.Repeat(typeof(object), tSwitch);
                    var tDelagateType = BuildProxy.GenerateCallSiteFuncType(tArgTypes, typeof(TTarget));
                    return Impromptu.InvokeCallSite(CreateCallSite(tDelagateType, binderType, knownType, binder, name, context, argNames), target, args);

            }
        }






#if !__MonoCS__
        internal static Delegate WrapFuncHelper<TReturn>(dynamic invokable, int length)
        {
            switch (length)
            {
                #region Optimizations
                case 0:
                    return new Func<TReturn>(() => invokable());
                case 1:
                    return new Func<object, TReturn>((a1) => invokable(a1));
                case 2:
                    return new Func<object, object, TReturn>((a1, a2) => invokable(a1, a2));
                case 3:
                    return new Func<object, object, object, TReturn>((a1, a2, a3) => invokable(a1, a2, a3));
                case 4:
                    return new Func<object, object, object, object, TReturn>((a1, a2, a3, a4) => invokable(a1, a2, a3, a4));
                case 5:
                    return new Func<object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5) => invokable(a1, a2, a3, a4, a5));
                case 6:
                    return new Func<object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6) => invokable(a1, a2, a3, a4, a5, a6));
                case 7:
                    return new Func<object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7) => invokable(a1, a2, a3, a4, a5, a6, a7));
                case 8:
                    return new Func<object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8) => invokable(a1, a2, a3, a4, a5, a6, a7, a8));
                case 9:
                    return new Func<object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9));
                case 10:
                    return new Func<object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10));
                case 11:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11));
                case 12:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12));
                case 13:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13));
                case 14:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14));
                case 15:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15));
                case 16:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16));
                #endregion
                default:
                    return new DynamicFunc<TReturn>(args => (TReturn)Impromptu.Invoke((object)invokable, args));
            }
        }
#endif

        internal static class MonoConvertCallSite<T>
        {
            internal static CallSite CallSite;
        }

        internal static Delegate WrapFuncHelperMono<TReturn>(dynamic invokable, int length)
        {
            switch (length)
            {
                #region Optimizations
                case 0:
                    return new Func<TReturn>(() =>
                    {
                        object tResult = invokable();
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 1:
                    return new Func<object, TReturn>((a1) =>
                    {
                        object tResult = invokable(a1);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 2:
                    return new Func<object, object, TReturn>((a1, a2) =>
                    {
                        object tResult = invokable(a1, a2);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 3:
                    return new Func<object, object, object, TReturn>((a1, a2, a3) =>
                    {
                        object tResult = invokable(a1, a2, a3);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 4:
                    return new Func<object, object, object, object, TReturn>((a1, a2, a3, a4) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 5:
                    return new Func<object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 6:
                    return new Func<object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5, a6);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 7:
                    return new Func<object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5, a6, a7);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 8:
                    return new Func<object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5, a6, a7, a8);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 9:
                    return new Func<object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 10:
                    return new Func<object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 11:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 12:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 13:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 14:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 15:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                case 16:
                    return new Func<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, TReturn>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16) =>
                    {
                        object tResult = invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
                #endregion
                default:
                    return new DynamicFunc<TReturn>(args =>
                    {
                        object tResult = Impromptu.Invoke((object)invokable, args);
                        return (TReturn)InvokeConvertCallSite(tResult, true, typeof(TReturn), typeof(object), ref MonoConvertCallSite<TReturn>.CallSite);
                    });
            }
        }


        internal static Delegate WrapAction(dynamic invokable, int length)
        {
            switch (length)
            {
                #region Optimizations
                case 0:
                    return new Action(() => invokable());
                case 1:
                    return new Action<object>((a1) => invokable(a1));
                case 2:
                    return new Action<object, object>((a1, a2) => invokable(a1, a2));
                case 3:
                    return new Action<object, object, object>((a1, a2, a3) => invokable(a1, a2, a3));
                case 4:
                    return new Action<object, object, object, object>((a1, a2, a3, a4) => invokable(a1, a2, a3, a4));
                case 5:
                    return new Action<object, object, object, object, object>((a1, a2, a3, a4, a5) => invokable(a1, a2, a3, a4, a5));
                case 6:
                    return new Action<object, object, object, object, object, object>((a1, a2, a3, a4, a5, a6) => invokable(a1, a2, a3, a4, a5, a6));
                case 7:
                    return new Action<object, object, object, object, object, object, object>((a1, a2, a3, a4, a5, a6, a7) => invokable(a1, a2, a3, a4, a5, a6, a7));
                case 8:
                    return new Action<object, object, object, object, object, object, object, object>((a1, a2, a3, a4, a5, a6, a7, a8) => invokable(a1, a2, a3, a4, a5, a6, a7, a8));
                case 9:
                    return new Action<object, object, object, object, object, object, object, object, object>((a1, a2, a3, a4, a5, a6, a7, a8, a9) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9));
                case 10:
                    return new Action<object, object, object, object, object, object, object, object, object, object>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10));
                case 11:
                    return new Action<object, object, object, object, object, object, object, object, object, object, object>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11));
                case 12:
                    return new Action<object, object, object, object, object, object, object, object, object, object, object, object>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12));
                case 13:
                    return new Action<object, object, object, object, object, object, object, object, object, object, object, object, object>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13));
                case 14:
                    return new Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14));
                case 15:
                    return new Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15));
                case 16:
                    return new Action<object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>((a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16) => invokable(a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16));
                #endregion
                default:
                    return new DynamicAction(args => Impromptu.InvokeAction((object)invokable, args));
            }
        }


        internal static object FastDynamicInvokeReturn(Delegate del, dynamic[] args)
        {
            dynamic tDel = del;
            switch (args.Length)
            {
                default:
                    try
                    {
                        return del.DynamicInvoke(args);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }
                #region Optimization
                case 1:
                    return tDel(args[0]);
                case 2:
                    return tDel(args[0], args[1]);
                case 3:
                    return tDel(args[0], args[1], args[2]);
                case 4:
                    return tDel(args[0], args[1], args[2], args[3]);
                case 5:
                    return tDel(args[0], args[1], args[2], args[3], args[4]);
                case 6:
                    return tDel(args[0], args[1], args[2], args[3], args[4], args[5]);
                case 7:
                    return tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                case 8:
                    return tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                case 9:
                    return tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
                case 10:
                    return tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
                case 11:
                    return tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
                case 12:
                    return tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
                case 13:
                    return tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
                case 14:
                    return tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
                case 15:
                    return tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]);
                case 16:
                    return tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14], args[15]);
                #endregion
            }
        }

        internal static void FastDynamicInvokeAction(Delegate del, params dynamic[] args)
        {
            dynamic tDel = del;
            switch (args.Length)
            {
                default:
                    try
                    {
                        del.DynamicInvoke(args);
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw ex.InnerException;
                    }
                    return;
                #region Optimization
                case 1:
                    tDel(args[0]);
                    return;
                case 2:
                    tDel(args[0], args[1]);
                    return;
                case 3:
                    tDel(args[0], args[1], args[2]);
                    return;
                case 4:
                    tDel(args[0], args[1], args[2], args[3]);
                    return;
                case 5:
                    tDel(args[0], args[1], args[2], args[3], args[4]);
                    return;
                case 6:
                    tDel(args[0], args[1], args[2], args[3], args[4], args[5]);
                    return;
                case 7:
                    tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                    return;
                case 8:
                    tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                    return;
                case 9:
                    tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8]);
                    return;
                case 10:
                    tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9]);
                    return;
                case 11:
                    tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
                    return;
                case 12:
                    tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11]);
                    return;
                case 13:
                    tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12]);
                    return;
                case 14:
                    tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13]);
                    return;
                case 15:
                    tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14]);
                    return;
                case 16:
                    tDel(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10], args[11], args[12], args[13], args[14], args[15]);
                    return;
                #endregion
            }
        }
    }
}
