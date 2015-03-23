// 
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

using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using RazorEngine.Compilation.ImpromptuInterface.Build;
using RazorEngine.Compilation.ImpromptuInterface.Dynamic;
using RazorEngine.Compilation.ImpromptuInterface.Internal;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;
using RazorEngine.Compilation.ImpromptuInterface.Optimization;
using Microsoft.CSharp.RuntimeBinder;
using System.Text.RegularExpressions;
namespace RazorEngine.Compilation.ImpromptuInterface
{
    using System;




    /// <summary>
    /// Main API
    /// </summary>
    public static class Impromptu
    {


        private static readonly Type ComObjectType;

        private static readonly dynamic ComBinder;

        static Impromptu()
        {
            try
            {
                ComObjectType = typeof(object).Assembly.GetType("System.__ComObject");
                ComBinder = new Dynamic.ImpromptuLateLibraryType(
                "System.Dynamic.ComBinder, System.Dynamic, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            }
            catch
            {

            }
        }
        /// <summary>
        /// Creates a cached call site at runtime.
        /// </summary>
        /// <param name="delegateType">Type of the delegate.</param>
        /// <param name="binder">The CallSite binder.</param>
        /// <param name="name">Member Name</param>
        /// <param name="context">Permissions Context type</param>
        /// <param name="argNames">The arg names.</param>
        /// <param name="staticContext">if set to <c>true</c> [static context].</param>
        /// <param name="isEvent">if set to <c>true</c> [is event].</param>
        /// <returns>The CallSite</returns>
        /// <remarks>
        /// Advanced usage only for serious custom dynamic invocation.
        /// </remarks>
        /// <seealso cref="CreateCallSite{T}"/>
        public static CallSite CreateCallSite(Type delegateType, CallSiteBinder binder, String_OR_InvokeMemberName name,
                                              Type context, string[] argNames = null, bool staticContext = false,
                                              bool isEvent = false)
        {

            return InvokeHelper.CreateCallSite(delegateType, binder.GetType(), InvokeHelper.Unknown, () => binder, name, context, argNames,
                                               staticContext,
                                               isEvent);
        }

        /// <summary>
        /// Creates the call site.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="binder">The binder.</param>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        /// <param name="argNames">The arg names.</param>
        /// <param name="staticContext">if set to <c>true</c> [static context].</param>
        /// <param name="isEvent">if set to <c>true</c> [is event].</param>
        /// <returns></returns>
        /// /// 
        /// <example>
        /// Unit test that exhibits usage
        /// <code><![CDATA[
        /// string tResult = String.Empty;
        /// var tPoco = new MethOutPoco();
        /// var tBinder =
        /// Binder.InvokeMember(BinderFlags.None, "Func", null, GetType(),
        /// new[]
        /// {
        /// Info.Create(
        /// InfoFlags.None, null),
        /// Info.Create(
        /// InfoFlags.IsOut |
        /// InfoFlags.UseCompileTimeType, null)
        /// });
        /// var tSite = Impromptu.CreateCallSite<DynamicTryString>(tBinder);
        /// tSite.Target.Invoke(tSite, tPoco, out tResult);
        /// Assert.AreEqual("success", tResult);
        /// ]]></code>
        /// </example>
        /// <seealso cref="CreateCallSite"/>
        public static CallSite<T> CreateCallSite<T>(CallSiteBinder binder, String_OR_InvokeMemberName name, Type context,
                                                    string[] argNames = null, bool staticContext = false,
                                                    bool isEvent = false) where T : class
        {
            return InvokeHelper.CreateCallSite<T>(binder.GetType(), InvokeHelper.Unknown, () => binder, name, context, argNames, staticContext,
                                                  isEvent);
        }

#if DISABLED
        public static dynamic DynamicLinq(object enumerable)
        {
            if (!enumerable.GetType().GetInterfaces().Where(it => it.IsGenericType)
                .Any(it => it.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                var tEnum = enumerable as System.Collections.IEnumerable;
                if (tEnum != null)
                {
                    enumerable = tEnum.Cast<object>();
                }
            }

            return new LinqInstanceProxy(enumerable);
        }

        public static ILinq<T> Linq<T>(IEnumerable<T> enumerable)
        {
            return new LinqInstanceProxy(enumerable).ActLike<ILinq<T>>();
        }
#endif

        /// <summary>
        /// Dynamically Invokes a member method using the DLR
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name. Can be a string it will be implicitly converted</param>
        /// <param name="args">The args.</param>
        /// <returns> The result</returns>
        /// <example>   
        /// Unit test that exhibits usage:
        /// <code>
        /// <![CDATA[
        ///    dynamic tExpando = new ExpandoObject();
        ///    tExpando.Func = new Func<int, string>(it => it.ToString());
        ///
        ///    var tValue = 1;
        ///    var tOut = Impromptu.InvokeMember(tExpando, "Func", tValue);
        ///
        ///    Assert.AreEqual(tValue.ToString(), tOut);
        /// ]]>
        /// </code>
        /// </example>
        public static dynamic InvokeMember(object target, String_OR_InvokeMemberName name, params object[] args)
        {
            string[] tArgNames;
            Type tContext;
            bool tStaticContext;
            target = target.GetTargetContext(out tContext, out tStaticContext);
            args = Util.GetArgsAndNames(args, out tArgNames);
            CallSite tCallSite = null;

            return InvokeHelper.InvokeMemberCallSite(target, name, args, tArgNames, tContext, tStaticContext,
                                                     ref tCallSite);
        }


        /// <summary>
        /// Invokes the binary operator.
        /// </summary>
        /// <param name="leftArg">The left arg.</param>
        /// <param name="op">The op.</param>
        /// <param name="rightArg">The right Arg.</param>
        /// <returns></returns>
        public static dynamic InvokeBinaryOperator(dynamic leftArg, ExpressionType op, dynamic rightArg)
        {
            switch (op)
            {
                case ExpressionType.Add:
                    return leftArg + rightArg;
                case ExpressionType.AddAssign:
                    leftArg += rightArg;
                    return leftArg;
                case ExpressionType.AndAssign:
                    leftArg &= rightArg;
                    return leftArg;
                case ExpressionType.Divide:
                    return leftArg / rightArg;
                case ExpressionType.DivideAssign:
                    leftArg /= rightArg;
                    return leftArg;
                case ExpressionType.Equal:
                    return leftArg == rightArg;
                case ExpressionType.ExclusiveOr:
                    return leftArg ^ rightArg;
                case ExpressionType.ExclusiveOrAssign:
                    leftArg ^= rightArg;
                    return leftArg;
                case ExpressionType.GreaterThan:
                    return leftArg > rightArg;
                case ExpressionType.GreaterThanOrEqual:
                    return leftArg >= rightArg;
                case ExpressionType.LeftShift:
                    return leftArg << rightArg;
                case ExpressionType.LeftShiftAssign:
                    leftArg <<= rightArg;
                    return leftArg;
                case ExpressionType.LessThan:
                    return leftArg < rightArg;
                case ExpressionType.LessThanOrEqual:
                    return leftArg <= rightArg;
                case ExpressionType.Modulo:
                    return leftArg % rightArg;
                case ExpressionType.ModuloAssign:
                    leftArg %= rightArg;
                    return leftArg;
                case ExpressionType.Multiply:
                    return leftArg * rightArg;
                case ExpressionType.MultiplyAssign:
                    leftArg *= rightArg;
                    return leftArg;
                case ExpressionType.NotEqual:
                    return leftArg != rightArg;
                case ExpressionType.OrAssign:
                    leftArg |= rightArg;
                    return leftArg;
                case ExpressionType.RightShift:
                    return leftArg >> rightArg;
                case ExpressionType.RightShiftAssign:
                    leftArg >>= rightArg;
                    return leftArg;
                case ExpressionType.Subtract:
                    return leftArg - rightArg;
                case ExpressionType.SubtractAssign:
                    leftArg -= rightArg;
                    return leftArg;
                case ExpressionType.Or:
                    return leftArg | rightArg;
                case ExpressionType.And:
                    return leftArg & rightArg;
                default:
                    throw new ArgumentException("Unsupported Operator", "op");
            }
        }

        /// <summary>
        /// Invokes the unary opartor.
        /// </summary>
        /// <param name="arg">The arg.</param>
        /// <param name="op">The op.</param>
        /// <returns></returns>
        public static dynamic InvokeUnaryOperator(ExpressionType op, dynamic arg)
        {
            switch (op)
            {
                case ExpressionType.Not:
                    return !arg;
                case ExpressionType.Negate:
                    return -arg;
                case ExpressionType.Decrement:
                    return --arg;
                case ExpressionType.Increment:
                    return ++arg;
                default:
                    throw new ArgumentException("Unsupported Operator", "op");
            }
        }

        /// <summary>
        /// Invokes the specified target using the DLR;
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static dynamic Invoke(object target, params object[] args)
        {
            string[] tArgNames;
            Type tContext;
            bool tStaticContext;
            target = target.GetTargetContext(out tContext, out tStaticContext);
            args = Util.GetArgsAndNames(args, out tArgNames);
            CallSite tCallSite = null;

            return InvokeHelper.InvokeDirectCallSite(target, args, tArgNames, tContext, tStaticContext, ref tCallSite);
        }


        /// <summary>
        /// Dynamically Invokes indexer using the DLR.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="indexes">The indexes.</param>
        /// <returns></returns>
        public static dynamic InvokeGetIndex(object target, params object[] indexes)
        {
            string[] tArgNames;
            Type tContext;
            bool tStaticContext;
            target = target.GetTargetContext(out tContext, out tStaticContext);
            indexes = Util.GetArgsAndNames(indexes, out tArgNames);
            CallSite tCallSite = null;

            return InvokeHelper.InvokeGetIndexCallSite(target, indexes, tArgNames, tContext, tStaticContext,
                                                       ref tCallSite);
        }


        /// <summary>
        /// Convenience version of InvokeSetIndex that separates value and indexes.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="value">The value</param>
        /// <param name="indexes">The indexes </param>
        /// <returns></returns>
        public static object InvokeSetValueOnIndexes(object target, object value, params object[] indexes)
        {
            var tList = new List<object>(indexes) { value };
            return InvokeSetIndex(target, indexesThenValue: tList.ToArray());
        }

        /// <summary>
        /// Invokes setindex.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="indexesThenValue">The indexes then value.</param>
        public static object InvokeSetIndex(object target, params object[] indexesThenValue)
        {
            if (indexesThenValue.Length < 2)
            {
                throw new ArgumentException("Requires atleast one index and one value", "indexesThenValue");
            }

            string[] tArgNames;
            Type tContext;
            bool tStaticContext;
            target = target.GetTargetContext(out tContext, out tStaticContext);
            indexesThenValue = Util.GetArgsAndNames(indexesThenValue, out tArgNames);

            CallSite tCallSite = null;
            return InvokeHelper.InvokeSetIndexCallSite(target, indexesThenValue, tArgNames, tContext, tStaticContext,
                                                ref tCallSite);
        }

        /// <summary>
        /// Dynamically Invokes a member method which returns void using the DLR
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        /// <param name="args">The args.</param>
        /// <example>
        /// Unit test that exhibits usage:
        /// <code>
        /// <![CDATA[
        ///    var tTest = "Wrong";
        ///    var tValue = "Correct";
        ///
        ///    dynamic tExpando = new ExpandoObject();
        ///    tExpando.Action = new Action<string>(it => tTest = it);
        ///
        ///    Impromptu.InvokeMemberAction(tExpando, "Action", tValue);
        ///
        ///    Assert.AreEqual(tValue, tTest);
        /// ]]>
        /// </code>
        /// </example>
        public static void InvokeMemberAction(object target, String_OR_InvokeMemberName name, params object[] args)
        {
            string[] tArgNames;
            Type tContext;
            bool tStaticContext;

            target = target.GetTargetContext(out tContext, out tStaticContext);
            args = Util.GetArgsAndNames(args, out tArgNames);

            CallSite tCallSite = null;
            InvokeHelper.InvokeMemberActionCallSite(target, name, args, tArgNames, tContext, tStaticContext,
                                                    ref tCallSite);
        }

        /// <summary>
        /// Invokes the action using the DLR
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="args">The args.</param>
        public static void InvokeAction(object target, params object[] args)
        {
            string[] tArgNames;
            Type tContext;
            bool tStaticContext;

            target = target.GetTargetContext(out tContext, out tStaticContext);
            args = Util.GetArgsAndNames(args, out tArgNames);

            CallSite tCallSite = null;
            InvokeHelper.InvokeDirectActionCallSite(target, args, tArgNames, tContext, tStaticContext, ref tCallSite);
        }


        /// <summary>
        /// Dynamically Invokes a set member using the DLR.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <example>
        /// Unit test that exhibits usage:
        /// <code>
        /// <![CDATA[
        ///    dynamic tExpando = new ExpandoObject();
        ///
        ///    var tSetValue = "1";
        ///
        ///    Impromptu.InvokeSet(tExpando, "Test", tSetValue);
        ///
        ///    Assert.AreEqual(tSetValue, tExpando.Test);
        /// ]]>
        /// </code>
        /// </example>
        /// <remarks>
        /// if you call a static property off a type with a static context the csharp dlr binder won't do it, so this method reverts to reflection
        /// </remarks>
        public static object InvokeSet(object target, string name, object value)
        {
            Type tContext;
            bool tStaticContext;
            target = target.GetTargetContext(out tContext, out tStaticContext);
            tContext = tContext.FixContext();


            CallSite tCallSite = null;
            return InvokeHelper.InvokeSetCallSite(target, name, value, tContext, tStaticContext, ref tCallSite);
        }
#if DISABLED
        /// <summary>
        /// Invokes the set on the end of a property chain.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="propertyChain">The property chain.</param>
        /// <param name="value">The value.</param>
        public static object InvokeSetChain(object target, string propertyChain, object value)
        {
            var tProperties = _chainRegex.FluentMatches(propertyChain).ToList();
            var tGetProperties = tProperties.Take(tProperties.Count - 1);


            var tTarget = target;
            foreach (var tProperty in tGetProperties)
            {
                var tGetter = tProperty.Getter;
                var tIntIndexer = tProperty.IntIndexer;
                var tStringIndexer = tProperty.StringIndexer;

                if (tGetter != null)
                    tTarget = InvokeGet(tTarget, tGetter);
                else if (tIntIndexer != null)
                    tTarget = InvokeGetIndex(tTarget, Impromptu.CoerceConvert(tIntIndexer, typeof(int)));
                else if (tStringIndexer != null)
                    tTarget = InvokeGetIndex(tTarget, tStringIndexer);
                else
                {
                    throw new Exception(string.Format("Could Not Parse :'{0}'", propertyChain));
                }
            }

            var tSetProperty = tProperties.Last();

            var tSetGetter = tSetProperty.Getter;
            var tSetIntIndexer = tSetProperty.IntIndexer;
            var tSetStringIndexer = tSetProperty.StringIndexer;

            if (tSetGetter != null)
                return InvokeSet(tTarget, tSetGetter, value);
            if (tSetIntIndexer != null)
                return InvokeSetIndex(tTarget, Impromptu.CoerceConvert(tSetIntIndexer, typeof(int)), value);
            if (tSetStringIndexer != null)
                return InvokeSetIndex(tTarget, tSetStringIndexer, value);

            throw new Exception(string.Format("Could Not Parse :'{0}'", propertyChain));
        }






        private static readonly dynamic _invokeSetAll = new InvokeSetters();
        /// <summary>
        /// Call Like method invokes set on target and a list of property/value. Invoke with dictionary, anonymous type or named arguments.
        /// </summary>
        /// <value>The invoke set all.</value>
        public static dynamic InvokeSetAll
        {
            get { return _invokeSetAll; }
        }

        /// <summary>
        /// Wraps a target to partial apply a method (or target if you can invoke target directly eg delegate).
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="totalArgCount">The total arg count.</param>
        /// <returns></returns>
        public static dynamic Curry(object target, int? totalArgCount = null)
        {
            if (target is Delegate && !totalArgCount.HasValue)
                return Curry((Delegate)target);
            return new Curry(target, totalArgCount);
        }

        /// <summary>
        /// Wraps a delegate to partially apply it.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static dynamic Curry(Delegate target)
        {
            return new Curry(target, target.Method.GetParameters().Length);
        }
#endif

        /// <summary>
        /// Dynamically Invokes a get member using the DLR.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        /// <returns>The result.</returns>
        /// <example>
        /// Unit Test that describes usage
        /// <code>
        /// <![CDATA[
        ///    var tSetValue = "1";
        ///    var tAnon = new { Test = tSetValue };
        ///
        ///    var tOut =Impromptu.InvokeGet(tAnon, "Test");
        ///
        ///    Assert.AreEqual(tSetValue, tOut);
        /// ]]>
        /// </code>
        /// </example>
        public static dynamic InvokeGet(object target, string name)
        {
            Type tContext;
            bool tStaticContext;
            target = target.GetTargetContext(out tContext, out tStaticContext);
            tContext = tContext.FixContext();
            CallSite tSite = null;
            return InvokeHelper.InvokeGetCallSite(target, name, tContext, tStaticContext, ref tSite);
        }
#if SILVERLIGHT
  private static readonly Regex _chainRegex
           = new Regex(@"((\.?(?<Getter>\w+))|(\[(?<IntIndexer>\d+)\])|(\['(?<StringIndexer>\w+)'\]))");
#else
        private static readonly Regex _chainRegex
                 = new Regex(@"((\.?(?<Getter>\w+))|(\[(?<IntIndexer>\d+)\])|(\['(?<StringIndexer>\w+)'\]))", RegexOptions.Compiled);
#endif
#if DISABLED
        /// <summary>
        /// Invokes the getter property chain.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="propertyChain">The property chain.</param>
        /// <returns></returns>
        public static dynamic InvokeGetChain(object target, string propertyChain)
        {
            var tProperties = _chainRegex.FluentMatches(propertyChain);
            var tTarget = target;
            foreach (var tProperty in tProperties)
            {
                var tGetter = tProperty.Getter;
                var tIntIndexer = tProperty.IntIndexer;
                var tStringIndexer = tProperty.StringIndexer;

                if (tGetter != null)
                    tTarget = InvokeGet(tTarget, tGetter);
                else if (tIntIndexer != null)
                    tTarget = InvokeGetIndex(tTarget, Impromptu.CoerceConvert(tIntIndexer, typeof(int)));
                else if (tStringIndexer != null)
                    tTarget = InvokeGetIndex(tTarget, tStringIndexer);
                else
                {
                    throw new Exception(string.Format("Could Not Parse :'{0}'", propertyChain));
                }
            }
            return tTarget;
        }
#endif

        /// <summary>
        /// Determines whether the specified name on target is event. This allows you to know whether to InvokeMemberAction
        ///  add_{name} or a combo of {invokeget, +=, invokeset} and the corresponding remove_{name} 
        /// or a combon of {invokeget, -=, invokeset}
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// 	<c>true</c> if the specified target is event; otherwise, <c>false</c>.
        /// </returns>
        public static bool InvokeIsEvent(object target, string name)
        {
            Type tContext;
            bool tStaticContext;
            target = target.GetTargetContext(out tContext, out tStaticContext);
            tContext = tContext.FixContext();
            CallSite tCallSite = null;
            return InvokeHelper.InvokeIsEventCallSite(target, name, tContext, ref tCallSite);
        }
        /// <summary>
        /// Invokes add assign with correct behavior for events.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        [Obsolete("Use InvokeAddAssignMember")]
        public static void InvokeAddAssign(object target, string name, object value)
        {
            InvokeAddAssignMember(target, name, value);
        }

        /// <summary>
        /// Invokes add assign with correct behavior for events.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public static void InvokeAddAssignMember(object target, string name, object value)
        {
            CallSite callSiteAdd = null;
            CallSite callSiteGet = null;
            CallSite callSiteSet = null;
            CallSite callSiteIsEvent = null;
            Type context;
            bool staticContext;
            target = target.GetTargetContext(out context, out staticContext);

            var args = new[] { value };
            string[] argNames;
            args = Util.GetArgsAndNames(args, out argNames);

            InvokeHelper.InvokeAddAssignCallSite(target, name, args, argNames, context, staticContext, ref callSiteIsEvent, ref callSiteAdd, ref callSiteGet, ref callSiteSet);
        }

        /// <summary>
        /// Invokes subtract assign with correct behavior for events.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        [Obsolete("use InvokeSubtractAssignMember instead")]
        public static void InvokeSubtractAssign(object target, string name, object value)
        {
            InvokeSubtractAssignMember(target, name, value);
        }
        /// <summary>
        /// Invokes subtract assign with correct behavior for events.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public static void InvokeSubtractAssignMember(object target, string name, object value)
        {
            Type context;
            bool staticContext;
            target = target.GetTargetContext(out context, out staticContext);

            var args = new[] { value };
            string[] argNames;

            args = Util.GetArgsAndNames(args, out argNames);


            CallSite callSiteIsEvent = null;
            CallSite callSiteRemove = null;
            CallSite callSiteGet = null;
            CallSite callSiteSet = null;


            InvokeHelper.InvokeSubtractAssignCallSite(target, name, args, argNames, context, staticContext, ref callSiteIsEvent, ref callSiteRemove, ref callSiteGet, ref  callSiteSet);
        }

        /// <summary>
        /// Invokes  convert using the DLR.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="type">The type.</param>
        /// <param name="explicit">if set to <c>true</c> [explicit].</param>
        /// <returns></returns>
        public static dynamic InvokeConvert(object target, Type type, bool @explicit = false)
        {
            Type tContext;
            bool tDummy;
            target = target.GetTargetContext(out tContext, out tDummy);

            CallSite tCallSite = null;
            return InvokeHelper.InvokeConvertCallSite(target, @explicit, type, tContext, ref tCallSite);

        }

        internal static readonly IDictionary<Type, Delegate> CompiledExpressions = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Coerce to delegate.
        /// </summary>
        /// <param name="invokeableObject"></param>
        /// <param name="delegateType"></param>
        /// <returns></returns>
        public static dynamic CoerceToDelegate(object invokeableObject, Type delegateType)
        {
            if (!typeof(Delegate).IsAssignableFrom(delegateType.BaseType))
            {
                return null;
            }
            var tDelMethodInfo = delegateType.GetMethod("Invoke");
            var tReturnType = tDelMethodInfo.ReturnType;
            var tAction = tReturnType == typeof(void);
            var tParams = tDelMethodInfo.GetParameters();
            var tLength = tDelMethodInfo.GetParameters().Length;
            Delegate tBaseDelegate = tAction
                                         ? InvokeHelper.WrapAction(invokeableObject, tLength)
                                         : InvokeHelper.WrapFunc(tReturnType, invokeableObject, tLength);


            if (!InvokeHelper.IsActionOrFunc(delegateType) || tParams.Any(it => it.ParameterType.IsValueType))
            //Conditions that aren't contravariant;
            {
                Delegate tGetResult;

                if (!CompiledExpressions.TryGetValue(delegateType, out tGetResult))
                {
                    var tParamTypes = tParams.Select(it => it.ParameterType).ToArray();
                    var tDelParam = Expression.Parameter(tBaseDelegate.GetType());
                    var tInnerParams = tParamTypes.Select(Expression.Parameter).ToArray();

                    var tI = Expression.Invoke(tDelParam,
                                               tInnerParams.Select(it => (Expression)Expression.Convert(it, typeof(object))));
                    var tL = Expression.Lambda(delegateType, tI, tInnerParams);

                    tGetResult =
                        Expression.Lambda(Expression.GetFuncType(tBaseDelegate.GetType(), delegateType), tL,
                                          tDelParam).Compile();
                    CompiledExpressions[delegateType] = tGetResult;
                }

                return tGetResult.DynamicInvoke(tBaseDelegate);
            }
            return tBaseDelegate;

        }

#if DISABLED
        public static dynamic CoerceConvert(object target, Type type)
        {
            if (target != null && !type.IsInstanceOfType(target) && DBNull.Value != target)
            {

                var delegateConversion = CoerceToDelegate(target, type);

                if (delegateConversion != null)
                    return delegateConversion;


                if (type.IsInterface)
                {
                    if (target is IDictionary<string, object> && !(target is ImpromptuDictionaryBase))
                    {
                        target = new ImpromptuDictionary((IDictionary<string, object>)target);
                    }
                    else
                    {
                        target = new ImpromptuGet(target);
                    }


                    target = Impromptu.DynamicActLike(target, type);
                }
                else
                {

                    try
                    {
                        object tResult;

                        tResult = Impromptu.InvokeConvert(target, type, @explicit: true);

                        target = tResult;
                    }
                    catch (RuntimeBinderException)
                    {
                        Type tReducedType = type;
                        if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        {
                            tReducedType = type.GetGenericArguments().First();
                        }


                        if (target is IConvertible && typeof(IConvertible).IsAssignableFrom(tReducedType) && !typeof(Enum).IsAssignableFrom(tReducedType))
                        {

                            target = Convert.ChangeType(target, tReducedType, Thread.CurrentThread.CurrentCulture);

                        }
                        else
                        {  //finally check type converter since it's the slowest.

#if !SILVERLIGHT
                            var tConverter = TypeDescriptor.GetConverter(tReducedType);
#else
                                    
                                    TypeConverter tConverter = null;
                                    var tAttributes = tReducedType.GetCustomAttributes(typeof(TypeConverterAttribute), false);
                                    var tAttribute  =tAttributes.OfType<TypeConverterAttribute>().FirstOrDefault();
                                    if(tAttribute !=null)
                                    {
                                        tConverter =
                                            Impromptu.InvokeConstructor(Type.GetType(tAttribute.ConverterTypeName));
                                    }

                                  
#endif
                            if (tConverter != null && tConverter.CanConvertFrom(target.GetType()))
                            {
                                target = tConverter.ConvertFrom(target);
                            }

#if SILVERLIGHT                                   
                                    else if (target is string)
                                    {

                                        var tDC = new SilverConvertertDC(target as String);
                                        var tFE = new SilverConverterFE
                                        {
                                            DataContext = tDC
                                        };


                                        var tProp = SilverConverterFE.GetProperty(tReducedType);

                                        tFE.SetBinding(tProp, new System.Windows.Data.Binding("StringValue"));

                                        var tResult = tFE.GetValue(tProp);

                                        if(tResult != null)
                                        {
                                            target = tResult;
                                        }
                                    }

#endif
                        }
                    }
                }
            }
            else if (((target == null) || target == DBNull.Value) && type.IsValueType)
            {
                target = Impromptu.InvokeConstructor(type);
            }
            else if (!type.IsInstanceOfType(target) && DBNull.Value == target)
            {
                return null;
            }
            return target;
        }
#endif

        /// <summary>
        /// (Obsolete)Invokes the constructor. misspelling
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        [Obsolete("use InvokeConstructor, this was a spelling mistake")]
        public static dynamic InvokeConstuctor(Type type, params object[] args)
        {
            return InvokeConstructor(type, args);
        }


        /// <summary>
        /// Invokes the constuctor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static dynamic InvokeConstructor(Type type, params object[] args)
        {
            string[] tArgNames;
            bool tValue = type.IsValueType;
            if (tValue && args.Length == 0)  //dynamic invocation doesn't see constructors of value types
            {
                return Activator.CreateInstance(type);
            }

            args = Util.GetArgsAndNames(args, out tArgNames);
            CallSite tCallSite = null;


            return InvokeHelper.InvokeConstructorCallSite(type, tValue, args, tArgNames, ref tCallSite);
        }


        /// <summary>
        /// FastDynamicInvoke extension method. Runs up to runs up to 20x faster than <see cref="System.Delegate.DynamicInvoke"/> .
        /// </summary>
        /// <param name="del">The del.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        public static object FastDynamicInvoke(this Delegate del, params object[] args)
        {
            if (del.Method.ReturnType == typeof(void))
            {

                InvokeHelper.FastDynamicInvokeAction(del, args);
                return null;
            }
            return InvokeHelper.FastDynamicInvokeReturn(del, args);
        }

        /// <summary>
        /// Given a generic parameter count and whether it returns void or not gives type of Action or Func
        /// </summary>
        /// <param name="paramCount">The param count.</param>
        /// <param name="returnVoid">if set to <c>true</c> [return void].</param>
        /// <returns>Type of Action or Func</returns>
        public static Type GenericDelegateType(int paramCount, bool returnVoid = false)
        {
            var tParamCount = returnVoid ? paramCount : paramCount - 1;
            if (tParamCount > 16)
                throw new ArgumentException(String.Format("{0} only handle at  most {1} parameters", returnVoid ? "Action" : "Func", returnVoid ? 16 : 17), "paramCount");
            if (tParamCount < 0)
                throw new ArgumentException(String.Format("{0} must have at least {1} parameter(s)", returnVoid ? "Action" : "Func", returnVoid ? 0 : 1), "paramCount");


            return returnVoid
                ? InvokeHelper.ActionKinds[tParamCount]
                : InvokeHelper.FuncKinds[tParamCount];
        }

        /// <summary>
        /// Gets the member names of properties. Not all IDynamicMetaObjectProvider have support for this.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="dynamicOnly">if set to <c>true</c> [dynamic only]. Won't add reflected properties</param>
        /// <returns></returns>
        public static IEnumerable<string> GetMemberNames(object target, bool dynamicOnly = false)
        {
            var tList = new List<string>();
            if (!dynamicOnly)
            {
                tList.AddRange(target.GetType().GetProperties().Select(it => it.Name));
            }

            var tTarget = target as IDynamicMetaObjectProvider;
            if (tTarget != null)
            {
                tList.AddRange(tTarget.GetMetaObject(Expression.Constant(tTarget)).GetDynamicMemberNames());
            }
            else
            {

                if (ComObjectType != null && ComObjectType.IsInstanceOfType(target))
                {
                    tList.AddRange(ComBinder.GetDynamicDataMemberNames(target));
                }
            }
            return tList;
        }

        /// <summary>
        /// Dynamically invokes a method determined by the CallSite binder and be given an appropriate delegate type
        /// </summary>
        /// <param name="callSite">The Callsite</param>
        /// <param name="target">The target.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// <remarks>
        /// Advanced use only. Use this method for serious custom invocation, otherwise there are other convenience methods such as
        /// <see cref="InvokeMember"></see>, <see cref="InvokeGet"></see>, <see cref="InvokeSet"></see> and <see cref="InvokeMemberAction"></see>
        /// </remarks>
        public static dynamic InvokeCallSite(CallSite callSite, object target, params object[] args)
        {


            var tParameters = new List<object> { callSite, target };
            tParameters.AddRange(args);

            MulticastDelegate tDelegate = ((dynamic)callSite).Target;

            return tDelegate.FastDynamicInvoke(tParameters.ToArray());
        }

        /// <summary>
        /// Dynamically invokes a method determined by the CallSite binder and be given an appropriate delegate type
        /// </summary>
        /// <param name="callSite">The Callsite</param>
        /// <param name="target">The target.</param>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        /// <remarks>
        /// Advanced use only. Use this method for serious custom invocation, otherwise there are other convenience methods such as
        /// <see cref="InvokeMember"></see>, <see cref="InvokeGet"></see>, <see cref="InvokeSet"></see> and <see cref="InvokeMemberAction"></see>
        /// </remarks>
        [Obsolete("Use InvokeCallSite instead;")]
        public static dynamic Invoke(CallSite callSite, object target, params object[] args)
        {


            var tParameters = new List<object> { callSite, target };
            tParameters.AddRange(args);

            MulticastDelegate tDelegate = ((dynamic)callSite).Target;

            return tDelegate.FastDynamicInvoke(tParameters.ToArray());
        }

        /// <summary>
        /// Extension Method that Wraps an existing object with an Explicit interface definition
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="originalDynamic">The original object can be annoymous type, System.DynamicObject as well as any others.</param>
        /// <param name="otherInterfaces">Optional other interfaces.</param>
        /// <returns></returns>
        public static TInterface ActLike<TInterface>(this object originalDynamic, params Type[] otherInterfaces) where TInterface : class
        {
            Type tContext;
            bool tDummy;
            originalDynamic = originalDynamic.GetTargetContext(out tContext, out tDummy);
            tContext = tContext.FixContext();

            var tProxy = BuildProxy.BuildType(tContext, typeof(TInterface), otherInterfaces);



            return
                (TInterface)
                InitializeProxy(tProxy, originalDynamic, new[] { typeof(TInterface) }.Concat(otherInterfaces));
        }


        /// <summary>
        /// Unwraps the act like proxy (if wrapped).
        /// </summary>
        /// <param name="proxiedObject">The proxied object.</param>
        /// <returns></returns>
        public static dynamic UndoActLike(this object proxiedObject)
        {

            var actLikeProxy = proxiedObject as IActLikeProxy;
            if (actLikeProxy != null)
            {
                return actLikeProxy.Original;
            }
            return proxiedObject;
        }


        /// <summary>
        /// Extension Method that Wraps an existing object with an Interface of what it is implicitly assigned to.
        /// </summary>
        /// <param name="originalDynamic">The original dynamic.</param>
        /// <param name="otherInterfaces">The other interfaces.</param>
        /// <returns></returns>
        public static dynamic ActLike(this object originalDynamic, params Type[] otherInterfaces)
        {
            return new ActLikeCaster(originalDynamic, otherInterfaces);
        }


        /// <summary>
        /// Makes static methods for the passed in property spec, designed to be used with old api's that reflect properties.
        /// </summary>
        /// <param name="originalDynamic">The original dynamic.</param>
        /// <param name="propertySpec">The property spec.</param>
        /// <returns></returns>
        public static dynamic ActLikeProperties(this object originalDynamic, IDictionary<string, Type> propertySpec)
        {
            Type tContext;
            bool tDummy;
            originalDynamic = originalDynamic.GetTargetContext(out tContext, out tDummy);
            tContext = tContext.FixContext();

            var tProxy = BuildProxy.BuildType(tContext, propertySpec);



            return
                InitializeProxy(tProxy, originalDynamic, propertySpec: propertySpec);
        }



        /// <summary>
        /// Private helper method that initializes the proxy.
        /// </summary>
        /// <param name="proxytype">The proxytype.</param>
        /// <param name="original">The original.</param>
        /// <param name="interfaces">The interfaces.</param>
        /// <param name="propertySpec">The property spec.</param>
        /// <returns></returns>
        internal static object InitializeProxy(Type proxytype, object original, IEnumerable<Type> interfaces = null, IDictionary<string, Type> propertySpec = null)
        {
            var tProxy = (IActLikeProxyInitialize)Activator.CreateInstance(proxytype);
            tProxy.Initialize(original, interfaces, propertySpec);
            return tProxy;
        }



        /// <summary>
        /// This Extension method is called off the calling context to perserve permissions with the object wrapped with an explicit interface definition.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="caller">The caller.</param>
        /// <param name="originalDynamic">The original dynamic.</param>
        /// <param name="otherInterfaces">The other interfaces.</param>
        /// <returns></returns>
        /// <example>
        /// UnitTest That describes usage
        /// <code>
        /// <![CDATA[
        ///     var tTest = new TestWithPrivateMethod();
        ///     var tNonExposed = this.CallActLike<IExposePrivateMethod>(tTest);
        ///     Assert.Throws<RuntimeBinderException>(() => tNonExposed.Test());
        /// ]]>
        /// </code>
        /// </example>
        [Obsolete("Using InvokeContext wrapper to change permission context from target")]
        public static TInterface CallActLike<TInterface>(this object caller, object originalDynamic, params Type[] otherInterfaces) where TInterface : class
        {
            return originalDynamic.WithContext(caller).ActLike<TInterface>(otherInterfaces);
        }

        /// <summary>
        /// Chainable Linq to Objects Method, allows you to wrap a list of objects with an Explict interface defintion
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="originalDynamic">The original dynamic.</param>
        /// <param name="otherInterfaces">The other interfaces.</param>
        /// <returns></returns>
        public static IEnumerable<TInterface> AllActLike<TInterface>(this IEnumerable<object> originalDynamic, params Type[] otherInterfaces) where TInterface : class
        {
            return originalDynamic.Select(it => it.ActLike<TInterface>(otherInterfaces));
        }

        /// <summary>
        /// Static Method that wraps an existing dyanmic object with a explicit interface type
        /// </summary>
        /// <param name="originalDynamic">The original dynamic.</param>
        /// <param name="otherInterfaces">The other interfaces.</param>
        /// <returns></returns>
        public static dynamic DynamicActLike(object originalDynamic, params Type[] otherInterfaces)
        {
            Type tContext;
            bool tDummy;
            originalDynamic = originalDynamic.GetTargetContext(out tContext, out tDummy);
            tContext = tContext.FixContext();

            var tProxy = BuildProxy.BuildType(tContext, otherInterfaces.First(), otherInterfaces.Skip(1).ToArray());

            return InitializeProxy(tProxy, originalDynamic, otherInterfaces);

        }

        /// <summary>
        /// This Extension method is called off the calling context to perserve permissions with the object wrapped with an explicit interface definition.
        /// </summary>
        /// <param name="caller">The caller.</param>
        /// <param name="originalDynamic">The original dynamic.</param>
        /// <param name="otherInterfaces">The other interfaces.</param>
        /// <returns></returns>
        [Obsolete("Using WithContext() wrapper to change permission context from target")]
        public static dynamic CallDynamicActLike(this object caller, object originalDynamic, params Type[] otherInterfaces)
        {

            return DynamicActLike(originalDynamic.WithContext(caller), otherInterfaces);

        }


        /// <summary>
        /// Chainable Linq to Objects Method, allows you to wrap a list of objects, and preserve method permissions with a caller, with an Explict interface defintion
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface.</typeparam>
        /// <param name="originalDynamic">The original dynamic.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="otherInterfaces">The other interfaces.</param>
        /// <returns></returns>
        [Obsolete("Using WithContext() wrapper to change permission context from target")]
        public static IEnumerable<TInterface> AllCallActLike<TInterface>(this IEnumerable<object> originalDynamic, object caller, params Type[] otherInterfaces) where TInterface : class
        {
            return originalDynamic.Select(it => it.WithContext(caller).ActLike<TInterface>(otherInterfaces));
        }

#if SILVERLIGHT5

        /// <summary>
        /// Gets the custom Type.
        /// </summary>
        /// <returns></returns>
        public static Type GetDynamicCustomType(this object target)
        {
            return new ImpromptuRuntimeType(target.GetType(), target);
        }
#endif

    }

}
