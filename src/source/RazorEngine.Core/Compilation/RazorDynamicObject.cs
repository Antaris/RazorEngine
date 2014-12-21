namespace RazorEngine.Compilation
{
    using ImpromptuInterface;
    using ImpromptuInterface.Dynamic;
    using ImpromptuInterface.Optimization;
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

    /// <summary>
    /// Wraps a dynamic object for serialization of dynamic objects and anonymous type support.
    /// But this type will also make (static) types work which are not serializable.
    /// </summary>
    [Serializable]
    public class RazorDynamicObject : ImpromptuObject
    {
        public static dynamic Cast<T>(object o)
        {
            T data = (T)o;
            return data;
        }

        public static object DynamicCast(object o, Type targetType)
        {
            var castMethod = typeof(RazorDynamicObject).GetMethod("Cast").MakeGenericMethod(targetType);
            return castMethod.Invoke(null, new object[] { o });
        }

        private static BindingFlags Flags =
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.DeclaredOnly;


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

        public static bool IsPrimitive(object result)
        {
            if (result == null)
            {
                return true;
            }
            var t = result.GetType();
            return t.IsPrimitive ||
                result is string ||
                result is Decimal ||
                result is DateTime ||
                result is DateTimeOffset;
        }

        internal class MarshalWrapper : MarshalByRefObject
        {
            object component;
            bool allowMissing;
            Type runtimeType;
            public MarshalWrapper(object wrapped, bool allowMissingMembers)
            {
                allowMissing = allowMissingMembers;
                component = wrapped;
                runtimeType = wrapped.GetType();
            }

            private bool TryFindInvokeMember(Type typeToSearch, string name, object[] args, Type[] paramTypes, out object result) {
                var members = typeToSearch.GetMember(name, RazorDynamicObject.Flags);
                var found = false;
                result = null;
                foreach (var member in members)
                {
                    var methodInfo = member as MethodInfo;
                    if (!found && methodInfo != null && RazorDynamicObject.CompatibleWith(methodInfo.GetParameters(), paramTypes))
                    {
                        result = methodInfo.Invoke(component, args);
                        found = true;
                        break;
                    }
                    var property = member as PropertyInfo;
                    if (!found && property != null)
                    {
                        var setMethod = property.GetSetMethod(true);
                        if (setMethod != null && RazorDynamicObject.CompatibleWith(setMethod.GetParameters(), paramTypes))
                        {
                            result = setMethod.Invoke(component, args);
                            found = true;
                            break;
                        }
                        var getMethod = property.GetGetMethod(true);
                        if (getMethod != null && RazorDynamicObject.CompatibleWith(getMethod.GetParameters(), paramTypes))
                        {
                            result = getMethod.Invoke(component, args);
                            found = true;
                            break;
                        }
                    }
                }
                return found;
            }

            public object GetResult(Invocation invocation)
            {
                object result = null;
                string name = invocation.Name.Name;
                object[] args = invocation.Args;
                Type[] paramTypes = args.Select(o => o.GetType()).ToArray();
                try
                {
                    // First we try to resolve via DLR
                    dynamic target = component;
                    result = invocation.InvokeWithStoredArgs(component);
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
                                result = DynamicCast(component, targetType);
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
                                    if (!TryFindInvokeMember(runtimeType, name, args, paramTypes, out result))
                                    {
                                        // search all interfaces as well
                                        foreach (var @interface in runtimeType.GetInterfaces().Where(i => i.IsPublic))
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
                        if (allowMissing)
                        {
                            return RazorDynamicObject.Create("", allowMissing);
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
                    return RazorDynamicObject.Create(result, allowMissing);
                }
            }
        }


        private MarshalWrapper component;
        internal RazorDynamicObject(object wrapped, bool allowMissingMembers = false)
            : base ()
        {
            component = new MarshalWrapper(wrapped, allowMissingMembers);
        }

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
                    var newType = ImpromptuInterface.Build.BuildProxy.BuildType(
                        typeof(RazorDynamicObject), ints.First(), ints.Skip(1).ToArray());
                    return newType;
                }
            }
            return typeof(object);
        }

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
        
        private static bool IsWrapped(object wrapped)
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

        public static object Create(object wrapped, bool allowMissingMembers = false)
        {
            if (IsWrapped(wrapped))
            {
                return wrapped;
            }
            var wrapper = new RazorDynamicObject(wrapped, allowMissingMembers);
            var interfaces = 
                wrapped.GetType().GetInterfaces()
                .Where(t => t.IsPublic && t != typeof(IDynamicMetaObjectProvider))
                .Select(MapInterface).ToArray();
            if (interfaces.Length > 0)
            {
                return Impromptu.DynamicActLike(wrapper, interfaces);
            }
            return wrapper;
        }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="ImpromptuForwarder"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected RazorDynamicObject(SerializationInfo info, StreamingContext context)
            : base(info,context)
        {
            component = info.GetValue<MarshalWrapper>("Component");
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
            base.GetObjectData(info,context);
            info.AddValue("Component", component);
        }

        private bool RemoteInvoke(Invocation invocation, out object result)
        {
            result = null;
            try
            {
                result = component.GetResult(invocation);
                return true;
            }
            catch (RuntimeBinderException)
            {
                return false;
            }
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            return RemoteInvoke(new Invocation(InvocationKind.Get, binder.Name), out result);
        }

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
                        result = tempResult;
                    }
                }
                return true;
            }
            return false;
        }

        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        {
            return RemoteInvoke(new Invocation(InvocationKind.Set, binder.Name, value), out value);
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
        
        public override string ToString()
        {
            object result;
            RemoteInvoke(
                new Invocation(InvocationKind.InvokeMemberUnknown, "ToString", new object[]{}), 
                out result);
            return (string)result;
        }
    }
}