namespace RazorEngine.Compilation
{
    using RazorEngine.Compilation.ImpromptuInterface;
    using RazorEngine.Compilation.ImpromptuInterface.Dynamic;
    using RazorEngine.Compilation.ImpromptuInterface.Optimization;
    using Microsoft.CSharp.RuntimeBinder;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;
    using System.Collections.Generic;

    /// <summary>
    /// Wraps a dynamic object for serialization of dynamic objects and anonymous type support.
    /// But this type will also make (static) types work which are not serializable.
    /// </summary>
    [Serializable]
    public class RazorDynamicObject : ImpromptuObject, IDisposable
    {
        /// <summary>
        /// A helper class to make sure the wrapped object does not leave its <see cref="AppDomain"/>.
        /// </summary>
        internal class MarshalWrapper : CrossAppDomainObject
        {
            private object _component;
            private bool _allowMissing;
            private Type _runtimeType;
            private List<IDisposable> _disposables;

            /// <summary>
            /// Initializes a new instance of the <see cref="MarshalWrapper"/> class.
            /// </summary>
            /// <param name="wrapped">the wrapped object.</param>
            /// <param name="allowMissingMembers">true when we allow missing properties.</param>
            public MarshalWrapper(object wrapped, bool allowMissingMembers)
            {
                _allowMissing = allowMissingMembers;
                _component = wrapped;
                _runtimeType = wrapped.GetType();
                _disposables = new List<IDisposable>();
            }

            /// <summary>
            /// Tries to find a member with the given name, the given arguments 
            /// and the given parameter types and invokes that member.
            /// </summary>
            /// <param name="typeToSearch">the type we search for that member.</param>
            /// <param name="name">the name of the member</param>
            /// <param name="args">the arguments of the member</param>
            /// <param name="paramTypes">the type of the arguments of the member</param>
            /// <param name="result">the result of invoking the found member.</param>
            /// <returns>true if a member was found and invoked.</returns>
            private bool TryFindInvokeMember(Type typeToSearch, string name, object[] args, Type[] paramTypes, out object result)
            {
                var members = typeToSearch.GetMember(name, RazorDynamicObject.Flags);
                var found = false;
                result = null;
                foreach (var member in members)
                {
                    var methodInfo = member as MethodInfo;
                    if (!found && methodInfo != null && RazorDynamicObject.CompatibleWith(methodInfo.GetParameters(), paramTypes))
                    {
                        result = methodInfo.Invoke(_component, args);
                        found = true;
                        break;
                    }
                    var property = member as PropertyInfo;
                    if (!found && property != null)
                    {
                        var setMethod = property.GetSetMethod(true);
                        if (setMethod != null && RazorDynamicObject.CompatibleWith(setMethod.GetParameters(), paramTypes))
                        {
                            result = setMethod.Invoke(_component, args);
                            found = true;
                            break;
                        }
                        var getMethod = property.GetGetMethod(true);
                        if (getMethod != null && RazorDynamicObject.CompatibleWith(getMethod.GetParameters(), paramTypes))
                        {
                            result = getMethod.Invoke(_component, args);
                            found = true;
                            break;
                        }
                    }
                }
                return found;
            }

            /// <summary>
            /// Unwrap the currently wrapped object 
            /// (note that this can cause crossing an app-domain boundary).
            /// </summary>
            /// <returns></returns>
            public object Unwrap()
            {
                return _component;
            }

            /// <summary>
            /// This method is used to delegate the invocation across the <see cref="AppDomain"/>.
            /// </summary>
            /// <param name="invocation">The invocation to cross the <see cref="AppDomain"/>.</param>
            /// <returns>The result of the invocation on the wrapped instance.</returns>
            [SecuritySafeCritical]
            public object GetResult(Invocation invocation)
            {
                (new PermissionSet(PermissionState.Unrestricted)).Assert();
                object result = null;
                string name = invocation.Name.Name;
                object[] args = invocation.Args;
                Type[] paramTypes = args.Select(o => o.GetType()).ToArray();
                try
                {
                    if (invocation.Kind == InvocationKind.NotSet && invocation.Name.Name == "_BinaryOperator")
                    { // We use that for operators
                        var exp = (ExpressionType)args[0];
                        object other = args[1];
                        result = Impromptu.InvokeBinaryOperator(_component, exp, other);
                    }
                    else if (invocation.Kind == InvocationKind.NotSet && invocation.Name.Name == "_UnaryOperator")
                    { // We use that for operators
                        var exp = (ExpressionType)args[0];
                        result = Impromptu.InvokeUnaryOperator(exp, _component);
                    }
                    else
                    {
                        // First we try to resolve via DLR
                        dynamic target = _component;
                        result = invocation.InvokeWithStoredArgs(_component);
                    }
                }
                catch (RuntimeBinderException)
                {
                    // DLR doesn't like some kind of functions, 
                    // expecially because we have casted the component to object...
                    bool found = false;
                    switch (invocation.Kind)
                    {
                        case InvocationKind.Convert:
                            var targetType = (Type)args[0];
                            bool tExplict = false;
                            if (args.Length == 2)
                                tExplict = (bool)args[1];
                            // try to find explicit or implicit operator.
                            try
                            {
                                result = DynamicCast(_component, targetType);
                                found = true;
                            }
                            catch (Exception)
                            {
                                found = false;
                            }
                            break;
                        //case InvocationKind.IsEvent:
                        //case InvocationKind.AddAssign:
                        //case InvocationKind.SubtractAssign:
                        //case InvocationKind.Invoke:
                        //case InvocationKind.InvokeAction:
                        //case InvocationKind.InvokeUnknown:
                        //case InvocationKind.InvokeMember: // TODO: add testcase
                        //case InvocationKind.InvokeMemberAction: // TODO: add testcase
                        //case InvocationKind.GetIndex: // TODO: add testcase
                        //case InvocationKind.SetIndex: // TODO: add testcase
                        case InvocationKind.Get:
                        case InvocationKind.Set:
                        case InvocationKind.InvokeMemberUnknown:
                            {
                                if (!found)
                                {
                                    if (!TryFindInvokeMember(_runtimeType, name, args, paramTypes, out result))
                                    {
                                        // search all interfaces as well
                                        foreach (var @interface in _runtimeType.GetInterfaces().Where(i => i.IsPublic))
                                        {
                                            if (TryFindInvokeMember(@interface, name, args, paramTypes, out result))
                                            {
                                                found = true;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        found = true;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    if (!found)
                    {
                        if (_allowMissing)
                        {
                            return RazorDynamicObject.Create("", _allowMissing);
                        }
                        throw;
                    }
                }
                if (RazorDynamicObject.IsPrimitive(result))
                {
                    return result;
                }
                else
                {
                    return RazorDynamicObject.Create(result, _disposables.Add, _allowMissing);
                }
            }

            protected override void Dispose(bool disposing)
            {
                foreach (var disposable in _disposables)
                {
                    disposable.Dispose();
                }
                _disposables.Clear();

                base.Dispose(disposing);
            }
        }

        private static BindingFlags Flags =
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.DeclaredOnly;


        private MarshalWrapper _component;
        private bool _disposed;

        internal RazorDynamicObject(object wrapped, bool allowMissingMembers = false)
            : base ()
        {
            _component = new MarshalWrapper(wrapped, allowMissingMembers);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RazorDynamicObject"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected RazorDynamicObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _component = info.GetValue<MarshalWrapper>("Component");
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Component", _component);
        }

        /// <summary>
        /// Try to find a type instance which has no references to anonymous types.
        /// Either we use the type or create a new one which implements the same interfaces.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type MapType(Type @type)
        {
            if (@type.IsPublic)
            {
                return @type;
            }
            var ints = @type.GetInterfaces().Where(t => t.IsPublic).Select(MapInterface).ToArray();
            if (ints.Length > 0)
            {
                if (ints.Length == 1)
                {
                    return ints[0];
                }
                else
                {
                    var newType = RazorEngine.Compilation.ImpromptuInterface.Build.BuildProxy.BuildType(
                        typeof(RazorDynamicObject), ints.First(), ints.Skip(1).ToArray());
                    return newType;
                }
            }
            return typeof(object);
        }

        /// <summary>
        /// Convert the given interface type instance in another interface 
        /// type instance which is free of anonymous types.
        /// </summary>
        /// <param name="interface"></param>
        /// <returns></returns>
        public static Type MapInterface(Type @interface)
        {
            if (!@interface.IsGenericType)
            {
                return @interface;
            }
            var genericType = @interface.GetGenericTypeDefinition();
            var args = @interface.GetGenericArguments().Select(MapType).ToArray();
            return genericType.MakeGenericType(args);
        }
        
        /// <summary>
        /// Check if an instance is already wrapped with <see cref="RazorDynamicObject"/>.
        /// </summary>
        /// <param name="wrapped">the object to check.</param>
        /// <returns></returns>
        public static bool IsWrapped(object wrapped)
        {
            if (wrapped is RazorDynamicObject)
            {
                return true;
            }
            else if (wrapped is IActLikeProxy)
            {
                var actLike = (IActLikeProxy)wrapped;
                object orig = actLike.Original;
                return IsWrapped(orig);
            }
            return false;
        }

        internal static object Create(object wrapped, Action<RazorDynamicObject> created, bool allowMissingMembers = false)
        {
            if (IsWrapped(wrapped))
            {
                return wrapped;
            }
            var wrapper = new RazorDynamicObject(wrapped, allowMissingMembers);
            created(wrapper);
            var interfaces =
                wrapped.GetType().GetInterfaces()
                // remove IDynamicMetaObjectProvider and ISerializable interfaces because ActLikeProxy does already implement them
                .Where(t => t.IsPublic && t != typeof(IDynamicMetaObjectProvider) && t != typeof(ISerializable))
                .Select(MapInterface).ToArray();
            if (interfaces.Length > 0)
            {
                return Impromptu.DynamicActLike(wrapper, interfaces);
            }
            return wrapper;
        }

        /// <summary>
        /// Create a wrapper around an dynamic object.
        /// This wrapper ensures that we can cross the <see cref="AppDomain"/>, 
        /// call internal methods (to make Anonymous types work), 
        /// or call missing methods (when allowMissingMembers is true).
        /// </summary>
        /// <param name="wrapped">The object to wrap.</param>
        /// <param name="allowMissingMembers">true when we should not throw when missing members are invoked.</param>
        /// <returns>the wrapped object.</returns>
        public static object Create(object wrapped, bool allowMissingMembers = false)
        {
            return Create(wrapped, r => { }, allowMissingMembers);
        }

        /// <summary>
        /// A simple generic cast method. Used for the DynamicCast implementation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static dynamic Cast<T>(object o)
        {
            T data = (T)o;
            return data;
        }

        /// <summary>
        /// A tricky dynamic cast (Cast in the runtime with dynamic types).
        /// </summary>
        /// <param name="o"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static object DynamicCast(object o, Type targetType)
        {
            var castMethod = typeof(RazorDynamicObject).GetMethod("Cast").MakeGenericMethod(targetType);
            return castMethod.Invoke(null, new object[] { o });
        }

        /// <summary>
        /// Checks if the fiven ParameterInfo array is compatible with the given type array.
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        public static bool CompatibleWith(ParameterInfo[] parameterInfo, Type[] paramTypes)
        {
            if (parameterInfo.Length != paramTypes.Length)
            {
                return false;
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (!parameterInfo[i].ParameterType.IsAssignableFrom(paramTypes[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returnes true when the type of the given result is primitive.
        /// (ie should not be wrapped in another <see cref="RazorDynamicObject"/> instance)
        /// </summary>
        /// <param name="target">the object to check</param>
        /// <returns></returns>
        public static bool IsPrimitive(object target)
        {
            if (target == null)
            {
                return true;
            }
            var t = target.GetType();
            return t.IsPrimitive ||
                target is string ||
                target is Decimal ||
                target is DateTime ||
                target is DateTimeOffset;
        }

        /// <summary>
        /// Captures the invocation and invokes it on the wrapped object (possibly across the <see cref="AppDomain"/> boundary.
        /// </summary>
        /// <param name="invocation">The invocation</param>
        /// <param name="result">the result</param>
        /// <returns></returns>
        private bool RemoteInvoke(Invocation invocation, out object result)
        {
            result = null;
            try
            {
                result = _component.GetResult(invocation);
                return true;
            }
            catch (RuntimeBinderException)
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to get the member.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            return RemoteInvoke(new Invocation(InvocationKind.Get, binder.Name), out result);
        }

        /// <summary>
        /// Unwraps the current RazorDynamicObject.
        /// </summary>
        /// <returns>the unwrapped object.</returns>
        private object DynamicUnwrap()
        {
            return _component.Unwrap();
        }

        /// <summary>
        /// Unwraps the current dynamic object.
        /// </summary>
        /// <returns>the unwrapped object.</returns>
        public static object Unwrap(object wrapped)
        {
            var o = wrapped as RazorDynamicObject;
            if (o != null)
            {
                return o.DynamicUnwrap();
            }
            else if (wrapped is IActLikeProxy)
            {
                var actLike = (IActLikeProxy)wrapped;
                object orig = actLike.Original;
                return Unwrap(orig);
            }
            else
            {
                throw new InvalidOperationException("The given instance is not wrapped");
            }
        }

        /// <summary>
        /// Tries to convert the current instance.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public override bool TryConvert(System.Dynamic.ConvertBinder binder, out object result)
        {
            var targetType = binder.Type;
            var isExplicit = binder.Explicit;
            result = null;
            object tempResult;
            if (RemoteInvoke(new Invocation(InvocationKind.Convert, (isExplicit ? "op_Explicit" : "op_Implicit"), targetType, isExplicit), out tempResult))
            {
                if (tempResult.GetType() == targetType)
                { // Not wrapped, so most likely a primitive?
                    result = tempResult;
                }
                else
                {
                    if (targetType.IsInterface)
                    {
                        result = Impromptu.DynamicActLike(tempResult, targetType);
                    }
                    else
                    {
                        // We can not present ourself as any class so we just try keep beeing dynamic.
                        if (IsWrapped(tempResult))
                        {
                            // Maybe we need to unwrap here.
                            result = Unwrap(tempResult); 
                        }
                        else
                        {
                            result = tempResult;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to set the member.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        {
            return RemoteInvoke(new Invocation(InvocationKind.Set, binder.Name, value), out value);
        }

        /// <summary>
        /// Forwards the binary operation
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="arg"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            return RemoteInvoke(new Invocation(InvocationKind.NotSet, "_BinaryOperator", new [] {binder.Operation, arg}), out result);
        }

        /// <summary>
        /// Forwads the unary operation
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            return RemoteInvoke(new Invocation(InvocationKind.NotSet, "_UnaryOperator", new[] { binder.Operation }), out result);
        }

        /// <summary>
        /// Tries the invoke member.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="args">The args.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
        {
            return RemoteInvoke(new Invocation(InvocationKind.InvokeMemberUnknown, binder.Name, Util.NameArgsIfNecessary(binder.CallInfo, args)), out result);
        }
        /// <summary>
        /// Tries the index of the get.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="indexes">The indexes.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public override bool TryGetIndex(System.Dynamic.GetIndexBinder binder, object[] indexes, out object result)
        {
            return RemoteInvoke(new Invocation(InvocationKind.GetIndex, Invocation.IndexBinderName, Util.NameArgsIfNecessary(binder.CallInfo, indexes)), out result);
        }
        /// <summary>
        /// Tries the index of the set.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="indexes">The indexes.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override bool TrySetIndex(System.Dynamic.SetIndexBinder binder, object[] indexes, object value)
        {
            object outTemp;
            var tCombinedArgs = indexes.Concat(new[] { value }).ToArray();
            return RemoteInvoke(new Invocation(InvocationKind.GetIndex, Invocation.IndexBinderName, Util.NameArgsIfNecessary(binder.CallInfo, tCombinedArgs)), out outTemp);
        }
        
        /// <summary>
        /// Override ToString and remotely invoke our wrapped instance.
        /// </summary>
        /// <returns>Whatever our wrapped instance returns.</returns>
        public override string ToString()
        {
            object result;
            RemoteInvoke(
                new Invocation(InvocationKind.InvokeMemberUnknown, "ToString", new object[]{}), 
                out result);
            return (string)result;
        }

        
        /// <summary>
        /// Cleans up the <see cref="RazorDynamicObject"/> instance.
        /// </summary>
        ~RazorDynamicObject()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes the current instance.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// Disposes the current instance via the disposable pattern.
        /// </summary>
        /// <param name="disposing">true when Dispose() was called manually.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _component.Dispose();
            }
            _disposed = true;
        }

    }
}