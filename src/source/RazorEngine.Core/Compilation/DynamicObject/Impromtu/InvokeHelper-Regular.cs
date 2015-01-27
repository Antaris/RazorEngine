using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using RazorEngine.Compilation.ImpromptuInterface.Build;
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;
using RazorEngine.Compilation.ImpromptuInterface.Internal;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace RazorEngine.Compilation.ImpromptuInterface.Optimization
{

    internal class DummmyNull
    {

    }

    internal static partial class InvokeHelper
    {


        private static readonly IDictionary<BinderHash, CallSite> _unknownBinderCache = new Dictionary<BinderHash, CallSite>(20);
        private static readonly IDictionary<BinderHash, CallSite> _getBinderCache = new Dictionary<BinderHash, CallSite>(20);
        private static readonly IDictionary<BinderHash, CallSite> _setBinderCache = new Dictionary<BinderHash, CallSite>(20);
        private static readonly IDictionary<BinderHash, CallSite> _memberBinderCache = new Dictionary<BinderHash, CallSite>(20);
        private static readonly IDictionary<BinderHash, CallSite> _directBinderCache = new Dictionary<BinderHash, CallSite>(20);
        private static readonly IDictionary<BinderHash, CallSite> _constructorBinderCache = new Dictionary<BinderHash, CallSite>(20);

        internal const int Unknown = 0;
        internal const int KnownGet = 1;
        internal const int KnownSet = 2;
        internal const int KnownMember = 3;
        internal const int KnownDirect = 4;
        internal const int KnownConstructor = 5;

        private static bool TryDynamicCachedCallSite(BinderHash hash, int knownBinderType, out CallSite callSite)
        {
            switch (knownBinderType)
            {
                default:
                    return _unknownBinderCache.TryGetValue(hash, out callSite);

                case KnownGet:
                    return _getBinderCache.TryGetValue(hash, out callSite);

                case KnownSet:
                    return _setBinderCache.TryGetValue(hash, out callSite);

                case KnownMember:
                    return _memberBinderCache.TryGetValue(hash, out callSite);

                case KnownDirect:
                    return _directBinderCache.TryGetValue(hash, out callSite);

                case KnownConstructor:
                    return _constructorBinderCache.TryGetValue(hash, out callSite);

            }

        }

        private static void SetDynamicCachedCallSite(BinderHash hash, int knownBinderType, CallSite callSite)
        {
            switch (knownBinderType)
            {
                default:
                    _unknownBinderCache[hash] = callSite;
                    break;
                case KnownGet:
                    _getBinderCache[hash] = callSite;
                    break;
                case KnownSet:
                    _setBinderCache[hash] = callSite;
                    break;
                case KnownMember:
                    _memberBinderCache[hash] = callSite;
                    break;
                case KnownDirect:
                    _directBinderCache[hash] = callSite;
                    break;
                case KnownConstructor:
                    _constructorBinderCache[hash] = callSite;
                    break;
            }
        }

        private static readonly object _binderCacheLock = new object();

        /// <summary>
        /// LazyBinderType
        /// </summary>
        internal delegate CallSiteBinder LazyBinder();



        public static bool IsActionOrFunc(object target)
        {
            if (target == null)
                return false;
            var tType = target as Type ?? target.GetType();

            if (tType.IsGenericType)
            {
                tType = tType.GetGenericTypeDefinition();
            }

            return FuncArgs.ContainsKey(tType) || ActionArgs.ContainsKey(tType);
        }


        internal static object InvokeMethodDelegate(this object target, Delegate tFunc, object[] args)
        {
            object result;

            try
            {
                result = tFunc.FastDynamicInvoke(
                    tFunc.IsSpecialThisDelegate()
                        ? new[] { target }.Concat(args).ToArray()
                        : args
                    );
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
            return result;
        }





        internal static IEnumerable<CSharpArgumentInfo> GetBindingArgumentList(object[] args, string[] argNames, bool staticContext)
        {

            var tTargetFlag = CSharpArgumentInfoFlags.None;
            if (staticContext)
            {
                tTargetFlag |= CSharpArgumentInfoFlags.IsStaticType | CSharpArgumentInfoFlags.UseCompileTimeType;
            }



            var tList = new BareBonesList<CSharpArgumentInfo>(args.Length + 1)
                        {
                            CSharpArgumentInfo.Create(tTargetFlag, null)
                        };

            //Optimization: linq statement creates a slight overhead in this case
            // ReSharper disable LoopCanBeConvertedToQuery
            // ReSharper disable ForCanBeConvertedToForeach
            for (int i = 0; i < args.Length; i++)
            {
                var tFlag = CSharpArgumentInfoFlags.None;
                string tName = null;
                if (argNames != null && argNames.Length > i)
                    tName = argNames[i];

                if (!String.IsNullOrEmpty(tName))
                {
                    tFlag |= CSharpArgumentInfoFlags.NamedArgument;

                }
                tList.Add(CSharpArgumentInfo.Create(
                    tFlag, tName));
            }
            // ReSharper restore ForCanBeConvertedToForeach
            // ReSharper restore LoopCanBeConvertedToQuery

            return tList;
        }


        internal static CallSite CreateCallSite(
            Type delegateType,
            Type specificBinderType,
             int knownType,
            LazyBinder binder,
            String_OR_InvokeMemberName name,
            Type context,
            string[] argNames = null,
            bool staticContext = false,
            bool isEvent = false

            )
        {

            var tHash = BinderHash.Create(delegateType, name, context, argNames, specificBinderType, staticContext, isEvent, knownType != Unknown);
            lock (_binderCacheLock)
            {
                CallSite tOut;
                if (!TryDynamicCachedCallSite(tHash, knownType, out tOut))
                {
                    tOut = CallSite.Create(delegateType, binder());
                    SetDynamicCachedCallSite(tHash, knownType, tOut);
                }
                return tOut;
            }
        }


        internal static CallSite<T> CreateCallSite<T>(
        Type specificBinderType,
        int knownType,
        LazyBinder binder,
        String_OR_InvokeMemberName name,
        Type context,
        string[] argNames = null,
        bool staticContext = false,
        bool isEvent = false
        )
        where T : class
        {
            var tHash = BinderHash<T>.Create(name, context, argNames, specificBinderType, staticContext, isEvent, knownType != Unknown);
            lock (_binderCacheLock)
            {
                CallSite tOut;
                if (!TryDynamicCachedCallSite(tHash, knownType, out tOut))
                {
                    tOut = CallSite<T>.Create(binder());
                    SetDynamicCachedCallSite(tHash, knownType, tOut);
                }
                return (CallSite<T>)tOut;
            }
        }


        internal delegate object DynamicInvokeMemberConstructorValueType(
            CallSite funcSite,
            Type funcTarget,
            ref CallSite callsite,
            Type binderType,
            int knownType,
            LazyBinder binder,
            String_OR_InvokeMemberName name,
            bool staticContext,
            Type context,
            string[] argNames,
            Type target,
            object[] args);

        internal static readonly IDictionary<Type, CallSite<DynamicInvokeMemberConstructorValueType>> _dynamicInvokeMemberSite = new Dictionary<Type, CallSite<DynamicInvokeMemberConstructorValueType>>();

        internal static dynamic DynamicInvokeStaticMember(Type tReturn, ref CallSite callsite, Type binderType, int knownType, LazyBinder binder,
                                       string name,
                                     bool staticContext,
                                     Type context,
                                     string[] argNames,
                                     Type target, params object[] args)
        {
            CallSite<DynamicInvokeMemberConstructorValueType> tSite;
            if (!_dynamicInvokeMemberSite.TryGetValue(tReturn, out tSite))
            {
                tSite = CallSite<DynamicInvokeMemberConstructorValueType>.Create(
                        Binder.InvokeMember(
                            CSharpBinderFlags.None,
                            "InvokeMemberTargetType",
                            new[] { typeof(Type), tReturn },
                            typeof(InvokeHelper),
                            new[]
                                {
                                    CSharpArgumentInfo.Create(
                                        CSharpArgumentInfoFlags.IsStaticType |
                                        CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                     CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsRef, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                }
                            )
                    );
                _dynamicInvokeMemberSite[tReturn] = tSite;
            }

            return tSite.Target(tSite, typeof(InvokeHelper), ref callsite, binderType, knownType, binder, name, staticContext, context, argNames, target, args);
        }


        internal static TReturn InvokeMember<TReturn>(ref CallSite callsite, Type binderType, int knownType, LazyBinder binder,
                                       String_OR_InvokeMemberName name,
                                     bool staticContext,
                                     Type context,
                                     string[] argNames,
                                     object target, params object[] args)
        {
            return InvokeMemberTargetType<object, TReturn>(ref callsite, binderType, knownType, binder, name, staticContext, context, argNames, target, args);
        }

        internal static object InvokeGetCallSite(object target, string name, Type context, bool staticContext, ref CallSite callsite)
        {
            if (callsite == null)
            {
                var tTargetFlag = CSharpArgumentInfoFlags.None;
                LazyBinder tBinder;
                Type tBinderType;
                int tKnownType;
                if (staticContext) //CSharp Binder won't call Static properties, grrr.
                {
                    var tStaticFlag = CSharpBinderFlags.None;
                    if (Util.IsMono) //Mono only works if InvokeSpecialName is set and .net only works if it isn't
                        tStaticFlag |= CSharpBinderFlags.InvokeSpecialName;

                    tBinder = () => Binder.InvokeMember(tStaticFlag, "get_" + name,
                                                         null,
                                                         context,
                                                         new List<CSharpArgumentInfo>
                                                             {
                                                                 CSharpArgumentInfo.Create(
                                                                     CSharpArgumentInfoFlags.IsStaticType |
                                                                     CSharpArgumentInfoFlags.UseCompileTimeType,
                                                                     null)
                                                             });

                    tBinderType = typeof(InvokeMemberBinder);
                    tKnownType = KnownMember;
                }
                else
                {

                    tBinder = () => Binder.GetMember(CSharpBinderFlags.None, name,
                                                      context,
                                                      new List<CSharpArgumentInfo>
                                                          {
                                                              CSharpArgumentInfo.Create(
                                                                  tTargetFlag, null)
                                                          });
                    tBinderType = typeof(GetMemberBinder);
                    tKnownType = KnownGet;
                }


                callsite = CreateCallSite<Func<CallSite, object, object>>(tBinderType, tKnownType, tBinder, name, context,
                                staticContext: staticContext);
            }
            var tCallSite = (CallSite<Func<CallSite, object, object>>)callsite;
            return tCallSite.Target(tCallSite, target);

        }

        internal static object InvokeSetCallSite(object target, string name, object value, Type context, bool staticContext, ref CallSite callSite)
        {
            if (callSite == null)
            {
                LazyBinder tBinder;
                Type tBinderType;
                if (staticContext) //CSharp Binder won't call Static properties, grrr.
                {

                    tBinder = () =>
                    {
                        var tStaticFlag = CSharpBinderFlags.ResultDiscarded;
                        if (Util.IsMono) //Mono only works if InvokeSpecialName is set and .net only works if it isn't
                            tStaticFlag |= CSharpBinderFlags.InvokeSpecialName;

                        return Binder.InvokeMember(tStaticFlag, "set_" + name,
                                            null,
                                            context,
                                            new List<CSharpArgumentInfo>
                                                              {
                                                                  CSharpArgumentInfo.Create(
                                                                      CSharpArgumentInfoFlags.IsStaticType |
                                                                      CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                                                  CSharpArgumentInfo.Create(

                                                                      CSharpArgumentInfoFlags.None

                                                                      , null)
                                                              });
                    };

                    tBinderType = typeof(InvokeMemberBinder);
                    callSite = CreateCallSite<Action<CallSite, object, object>>(tBinderType, KnownMember, tBinder, name, context, staticContext: true);
                }
                else
                {

                    tBinder = () => Binder.SetMember(CSharpBinderFlags.None, name,
                                               context,
                                               new List<CSharpArgumentInfo>
                                                   {
                                                       CSharpArgumentInfo.Create(
                                                           CSharpArgumentInfoFlags.None, null),
                                                       CSharpArgumentInfo.Create(

                                                           CSharpArgumentInfoFlags.None

                                                           , null)

                                                   });


                    tBinderType = typeof(SetMemberBinder);
                    callSite = CreateCallSite<Func<CallSite, object, object, object>>(tBinderType, KnownSet, tBinder, name, context, staticContext: false);
                }
            }

            if (staticContext)
            {
                var tCallSite = (CallSite<Action<CallSite, object, object>>)callSite;
                tCallSite.Target(callSite, target, value);
                return value;
            }
            else
            {
                var tCallSite = (CallSite<Func<CallSite, object, object, object>>)callSite;
                var tResult = tCallSite.Target(callSite, target, value);
                return tResult;
            }
        }

        internal static object InvokeMemberCallSite(object target, String_OR_InvokeMemberName name, object[] args, string[] tArgNames, Type tContext, bool tStaticContext, ref CallSite callSite)
        {
            LazyBinder tBinder = null;
            Type tBinderType = null;
            if (callSite == null)
            {

                tBinder = () =>
                {
                    var tList = GetBindingArgumentList(args, tArgNames, tStaticContext);
                    var tFlag = CSharpBinderFlags.None;
                    if (name.IsSpecialName)
                    {
                        tFlag |= CSharpBinderFlags.InvokeSpecialName;
                    }
                    return Binder.InvokeMember(tFlag, name.Name, name.GenericArgs,
                                                tContext, tList);
                };
                tBinderType = typeof(InvokeMemberBinder);
            }


            return InvokeMember<object>(ref callSite, tBinderType, KnownMember, tBinder, name, tStaticContext, tContext, tArgNames, target, args);
        }

        internal static object InvokeDirectCallSite(object target, object[] args, string[] tArgNames, Type tContext, bool tStaticContext, ref CallSite callSite)
        {
            LazyBinder tBinder = null;
            Type tBinderType = null;

            if (callSite == null)
            {

                tBinder = () =>
                {
                    var tList = GetBindingArgumentList(args, tArgNames, tStaticContext);
                    var tFlag = CSharpBinderFlags.None;
                    return Binder.Invoke(tFlag, tContext, tList);
                };
                tBinderType = typeof(InvokeBinder);
            }


            return InvokeMember<object>(ref callSite, tBinderType, KnownDirect, tBinder, String.Empty, tStaticContext, tContext, tArgNames, target, args);
        }

        internal static object InvokeGetIndexCallSite(object target, object[] indexes, string[] argNames, Type context, bool tStaticContext, ref CallSite callSite)
        {
            LazyBinder tBinder = null;
            Type tBinderType = null;
            if (callSite == null)
            {

                tBinder = () =>
                {
                    var tList = GetBindingArgumentList(indexes, argNames,
                                                                 tStaticContext);
                    return Binder.GetIndex(CSharpBinderFlags.None, context, tList);
                };
                tBinderType = typeof(GetIndexBinder);

            }

            return InvokeMember<object>(ref callSite, tBinderType, Unknown, tBinder, Invocation.IndexBinderName, tStaticContext, context, argNames, target, indexes);
        }

        internal static object InvokeSetIndexCallSite(object target, object[] indexesThenValue, string[] tArgNames, Type tContext, bool tStaticContext, ref CallSite tCallSite)
        {
            LazyBinder tBinder = null;
            Type tBinderType = null;
            if (tCallSite == null)
            {

                tBinder = () =>
                {
                    var tList = GetBindingArgumentList(indexesThenValue, tArgNames,
                                                                 tStaticContext);
                    return Binder.SetIndex(CSharpBinderFlags.None, tContext, tList);
                };

                tBinderType = typeof(SetIndexBinder);
            }

            return InvokeMember<object>(ref tCallSite, tBinderType, Unknown, tBinder, Invocation.IndexBinderName, tStaticContext, tContext, tArgNames, target, indexesThenValue);
        }

        internal static void InvokeMemberActionCallSite(object target, String_OR_InvokeMemberName name, object[] args, string[] tArgNames, Type tContext, bool tStaticContext, ref CallSite callSite)
        {
            LazyBinder tBinder = null;
            Type tBinderType = null;
            if (callSite == null)
            {

                tBinder = () =>
                {
                    IEnumerable<CSharpArgumentInfo> tList;
                    tList = GetBindingArgumentList(args, tArgNames, tStaticContext);

                    var tFlag = CSharpBinderFlags.ResultDiscarded;
                    if (name.IsSpecialName)
                    {
                        tFlag |= CSharpBinderFlags.InvokeSpecialName;
                    }

                    return Binder.InvokeMember(tFlag, name.Name, name.GenericArgs,
                                               tContext, tList);
                };
                tBinderType = typeof(InvokeMemberBinder);
            }


            InvokeMemberAction(ref callSite, tBinderType, KnownMember, tBinder, name, tStaticContext, tContext, tArgNames, target, args);
        }


        internal static void InvokeDirectActionCallSite(object target, object[] args, string[] tArgNames, Type tContext, bool tStaticContext, ref CallSite callSite)
        {
            LazyBinder tBinder = null;
            Type tBinderType = null;

            if (callSite == null)
            {

                tBinder = () =>
                {
                    IEnumerable<CSharpArgumentInfo> tList;
                    tList = GetBindingArgumentList(args, tArgNames, tStaticContext);

                    var tFlag = CSharpBinderFlags.ResultDiscarded;


                    return Binder.Invoke(tFlag, tContext, tList);
                };
                tBinderType = typeof(InvokeBinder);
            }


            InvokeMemberAction(ref callSite, tBinderType, KnownDirect, tBinder, String.Empty, tStaticContext, tContext, tArgNames, target, args);
        }

        internal class IsEventBinderDummy
        {

        }
        internal static bool InvokeIsEventCallSite(object target, string name, Type tContext, ref CallSite callSite)
        {
            if (callSite == null)
            {
                LazyBinder tBinder = () => Binder.IsEvent(CSharpBinderFlags.None, name, tContext);
                var tBinderType = typeof(IsEventBinderDummy);
                callSite = CreateCallSite<Func<CallSite, object, bool>>(tBinderType, Unknown, tBinder, name, tContext, isEvent: true);
            }
            var tCallSite = (CallSite<Func<CallSite, object, bool>>)callSite;

            return tCallSite.Target(tCallSite, target);
        }

        internal static void InvokeAddAssignCallSite(object target, string name, object[] args, string[] argNames, Type context, bool staticContext, ref CallSite callSiteIsEvent, ref CallSite callSiteAdd, ref CallSite callSiteGet, ref CallSite callSiteSet)
        {

            if (InvokeIsEventCallSite(target, name, context, ref callSiteIsEvent))
            {
                InvokeMemberActionCallSite(target, InvokeMemberName.CreateSpecialName("add_" + name), args, argNames, context, staticContext, ref callSiteAdd);
            }
            else
            {
                dynamic tGet = InvokeGetCallSite(target, name, context, staticContext, ref callSiteGet);
                tGet += (dynamic)(args[0]);
                InvokeSetCallSite(target, name, (object)tGet, context, staticContext, ref callSiteSet);
            }
        }

        internal static void InvokeSubtractAssignCallSite(object target, string name, object[] args, string[] argNames, Type context, bool staticContext, ref CallSite callSiteIsEvent, ref CallSite callSiteRemove, ref CallSite callSiteGet, ref CallSite callSiteSet)
        {
            if (InvokeIsEventCallSite(target, name, context, ref callSiteIsEvent))
            {
                InvokeMemberActionCallSite(target, InvokeMemberName.CreateSpecialName("remove_" + name), args, argNames, context, staticContext, ref callSiteRemove);
            }
            else
            {
                dynamic tGet = InvokeGetCallSite(target, name, context, staticContext, ref callSiteGet);
                tGet -= (dynamic)(args[0]);
                InvokeHelper.InvokeSetCallSite(target, name, tGet, context, staticContext, ref callSiteSet);
            }
        }

        public delegate void DynamicAction(params object[] args);
        public delegate TReturn DynamicFunc<out TReturn>(params object[] args);

        internal static object InvokeConvertCallSite(object target, bool explict, Type type, Type context, ref CallSite callSite)
        {
            if (callSite == null)
            {
                LazyBinder tBinder = () =>
                {
                    var tFlags = explict ? CSharpBinderFlags.ConvertExplicit : CSharpBinderFlags.None;

                    return Binder.Convert(tFlags, type, context);
                };
                Type tBinderType = typeof(ConvertBinder);

                var tFunc = BuildProxy.GenerateCallSiteFuncType(new Type[] { }, type);


                callSite = CreateCallSite(tFunc, tBinderType, Unknown, tBinder,
                                          explict
                                              ? Invocation.ExplicitConvertBinderName
                                              : Invocation.ImplicitConvertBinderName, context);
            }
            dynamic tDynCallSite = callSite;
            return tDynCallSite.Target(callSite, target);

        }

        internal class InvokeConstructorDummy { };

        internal static object InvokeConstructorCallSite(Type type, bool isValueType, object[] args, string[] argNames, ref CallSite callSite)
        {
            LazyBinder tBinder = null;
            Type tBinderType = typeof(InvokeConstructorDummy);
            if (callSite == null || isValueType)
            {
                if (isValueType && args.Length == 0)  //dynamic invocation doesn't see no argument constructors of value types
                {
                    return Activator.CreateInstance(type);
                }


                tBinder = () =>
                {
                    var tList = GetBindingArgumentList(args, argNames, true);
                    return Binder.InvokeConstructor(CSharpBinderFlags.None, type, tList);
                };
            }


            if (isValueType || Util.IsMono)
            {
                CallSite tDummy = null;
                return DynamicInvokeStaticMember(type, ref tDummy, tBinderType, KnownConstructor, tBinder, Invocation.ConstructorBinderName, true, type,
                                                              argNames, type, args);
            }

            return InvokeMemberTargetType<Type, object>(ref callSite, tBinderType, KnownConstructor, tBinder, Invocation.ConstructorBinderName, true, type, argNames,
                                                                     type, args);
        }

        internal static readonly IDictionary<Type, CallSite<DynamicInvokeWrapFunc>> _dynamicInvokeWrapFunc = new Dictionary<Type, CallSite<DynamicInvokeWrapFunc>>();

        internal delegate object DynamicInvokeWrapFunc(
         CallSite funcSite,
         Type funcTarget,
         object invokable,
         int length
         );

        internal static Delegate WrapFunc(Type returnType, object invokable, int length)
        {
            CallSite<DynamicInvokeWrapFunc> tSite;
            if (!_dynamicInvokeWrapFunc.TryGetValue(returnType, out tSite))
            {

                var tMethod = "WrapFuncHelperMono";

#if !__MonoCS__
                //Mono Compiler can't compile or run WrapFuncHelper
                if (!Util.IsMono)
                {
                    tMethod = "WrapFuncHelper";
                }
#endif
                tSite = CallSite<DynamicInvokeWrapFunc>.Create(
                    Binder.InvokeMember(
                        CSharpBinderFlags.None,
                        tMethod,
                        new[] { returnType },
                        typeof(InvokeHelper),
                        new[]
                            {
                                CSharpArgumentInfo.Create(
                                    CSharpArgumentInfoFlags.IsStaticType | CSharpArgumentInfoFlags.UseCompileTimeType,
                                    null),
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                                CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null),
                            }
                        ));
                _dynamicInvokeWrapFunc[returnType] = tSite;
            }
            return (Delegate)tSite.Target(tSite, typeof(InvokeHelper), invokable, length);
        }
    }
}
