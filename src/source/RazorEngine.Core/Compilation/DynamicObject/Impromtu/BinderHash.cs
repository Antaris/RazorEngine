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

namespace RazorEngine.Compilation.ImpromptuInterface.Optimization
{
    internal class BinderHash
    {


        protected BinderHash(Type delegateType, String_OR_InvokeMemberName name, Type context, string[] argNames, Type binderType, bool staticContext, bool isEvent, bool knownBinder)
        {
            KnownBinder = knownBinder;
            BinderType = binderType;
            StaticContext = staticContext;
            DelegateType = delegateType;
            Name = name;
            Context = context;
            ArgNames = argNames;
            IsEvent = isEvent;

        }



        public static BinderHash Create(Type delType, String_OR_InvokeMemberName name, Type context, string[] argNames, Type binderType, bool staticContext, bool isEvent, bool knownBinder)
        {
            return new BinderHash(delType, name, context, argNames, binderType, staticContext, isEvent, knownBinder);
        }

        public bool KnownBinder { get; protected set; }
        public Type BinderType { get; protected set; }
        public bool StaticContext { get; protected set; }
        public bool IsEvent { get; protected set; }
        public Type DelegateType { get; protected set; }
        public String_OR_InvokeMemberName Name { get; protected set; }
        public Type Context { get; protected set; }
        public string[] ArgNames { get; protected set; }

        public virtual bool Equals(BinderHash other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            var tArgNames = ArgNames;
            var tOtherArgNames = other.ArgNames;

            return
                !(tOtherArgNames == null ^ tArgNames == null)
                && other.IsEvent == IsEvent
                && other.StaticContext == StaticContext
                && Equals(other.Context, Context)
                && (KnownBinder || Equals(other.BinderType, BinderType))
                && Equals(other.DelegateType, DelegateType)
                && Equals(other.Name, Name)
                && (tArgNames == null
                // ReSharper disable AssignNullToNotNullAttribute
                //Exclusive Or Makes Sure this doesn't happen

                                 || tOtherArgNames.SequenceEqual(tArgNames));
            // ReSharper restore AssignNullToNotNullAttribute
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is BinderHash)) return false;
            return Equals((BinderHash)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var tArgNames = ArgNames;

                int result = (tArgNames == null ? 0 : tArgNames.Length * 397);
                result = (result ^ StaticContext.GetHashCode());
                result = (result * 397) ^ DelegateType.GetHashCode();
                result = (result * 397) ^ Context.GetHashCode();
                result = (result * 397) ^ Name.GetHashCode();
                return result;
            }
        }
    }

    internal class GenericBinderHashBase : BinderHash
    {
        protected GenericBinderHashBase(Type delegateType, String_OR_InvokeMemberName name, Type context, string[] argNames, Type binderType, bool staticContext, bool isEvent, bool knownBinder)
            : base(delegateType, name, context, argNames, binderType, staticContext, isEvent, knownBinder)
        {
        }
    }

    internal class BinderHash<T> : GenericBinderHashBase where T : class
    {

        public static BinderHash<T> Create(String_OR_InvokeMemberName name, Type context, string[] argNames, Type binderType, bool staticContext, bool isEvent, bool knownBinder)
        {
            return new BinderHash<T>(name, context, argNames, binderType, staticContext, isEvent, knownBinder);
        }

        protected BinderHash(String_OR_InvokeMemberName name, Type context, string[] argNames, Type binderType, bool staticContext, bool isEvent, bool knownBinder)
            : base(typeof(T), name, context, argNames, binderType, staticContext, isEvent, knownBinder)
        {
        }


        public override bool Equals(BinderHash other)
        {
            if (other is GenericBinderHashBase)
            {
                if (other is BinderHash<T>)
                {
                    return
                           !(other.ArgNames == null ^ ArgNames == null)
                           && other.IsEvent == IsEvent
                           && other.StaticContext == StaticContext
                           && (KnownBinder || Equals(other.BinderType, BinderType))
                           && Equals(other.Context, Context)
                           && Equals(other.Name, Name)
                           && (ArgNames == null
                        // ReSharper disable AssignNullToNotNullAttribute
                        //Exclusive Or Makes Sure this doesn't happen
                                 || other.ArgNames.SequenceEqual(ArgNames));
                    // ReSharper restore AssignNullToNotNullAttribute
                }
                return false;
            }
            return base.Equals(other);
        }
    }
}
