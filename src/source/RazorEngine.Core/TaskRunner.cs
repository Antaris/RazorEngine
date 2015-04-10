using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngine
{
    /// <summary>
    /// Helper for missing net40 methods, REMOVE me when we are net45 only.
    /// </summary>
    internal class TaskRunner
    {
        /// <summary>
        /// Runs the given delegate in a new task (like Task.Run but works on net40).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Task<T> Run<T>(Func<T> t)
        {
#if NET40
            var task = new Task<T>(t);
            task.Start();
            return task;
#else
            return Task.Run(t);
#endif
        }

        /// <summary>
        /// Runs the given delegate in a new task (like Task.Run but works on net40).
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Task Run(Action t)
        {
#if NET40
            var task = new Task(t);
            task.Start();
            return task;
#else
            return Task.Run(t);
#endif
        }
    }
}