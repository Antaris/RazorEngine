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