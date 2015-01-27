using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorEngine.Compilation.ImpromptuInterface.InvokeExt
{
    /// <summary>
    /// Various extension methods for add
    /// </summary>
    public static class InvokeExt
    {
        /// <summary>
        /// Combines target with context.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static InvokeContext WithContext(this object target, Type context)
        {
            return new InvokeContext(target, context);
        }

        /// <summary>
        /// Combines target with context.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static InvokeContext WithContext<TContext>(this object target)
        {
            return new InvokeContext(target, typeof(TContext));
        }

        /// <summary>
        /// Combines target with context.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static InvokeContext WithContext(this object target, object context)
        {
            return new InvokeContext(target, context);
        }

        /// <summary>
        /// Withes the static context.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static InvokeContext WithStaticContext(this Type target, object context = null)
        {

            return new InvokeContext(target, true, context);
        }

        /// <summary>
        /// attaches generic args to string
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="genericArgs">The generic args.</param>
        /// <returns></returns>
        public static InvokeMemberName WithGenericArgs(this string name, params Type[] genericArgs)
        {
            return new InvokeMemberName(name, genericArgs);
        }

        /// <summary>
        /// attaches name of the argument.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static InvokeArg WithArgumentName(this object argument, string name)
        {
            return new InvokeArg(name, argument);
        }

    }
}
