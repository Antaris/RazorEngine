using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RazorEngine.Compilation
{
    /// <summary>
    /// The sole purpose of this class is to fix https://github.com/Antaris/RazorEngine/issues/267.
    /// </summary>
    [SecurityCritical]
    internal class ExecutionContextLessThread : IDisposable
    {
        [SecurityCritical]
        private class CallHelperSafeHelper<TIn, TOut>
        {
            public Func<TIn, TOut> toCall;
            public TIn inData;
            public TOut outData;
            public void AsAction()
            {
                outData = toCall(inData);
            }
            public TOut AsFunc()
            {
                AsAction();
                return outData;
            }

            public void AsContextCallback(object state)
            {
                AsAction();
            }
        }
        [SecurityCritical]
        private class FuncConv<T>
        {
            public Func<T> toCall;
            public T Call(bool data)
            {
                return toCall();
            }
        }

        [SecurityCritical]
        private class ActionConv
        {
            public Action toCall;
            public bool Call(bool data)
            {
                toCall();
                return true;
            }
        }

        private System.Collections.Concurrent.ConcurrentQueue<Tuple<TaskCompletionSource<bool>, Action>> queue =
            new System.Collections.Concurrent.ConcurrentQueue<Tuple<TaskCompletionSource<bool>, Action>>();
        private Exception messagePumpExn;
        private Thread t;

        [SecurityCritical]
        private void MessagePumpWithoutExecutionContext()
        {
            (new PermissionSet(PermissionState.Unrestricted)).Assert();
            try
            {
                Thread.CurrentPrincipal = null;
                Tuple<TaskCompletionSource<bool>, Action> currentWork;
                while (true)
                {
                    if (queue.TryDequeue(out currentWork))
                    {
                        try
                        {
                            currentWork.Item2();
                            currentWork.Item1.SetResult(true);
                        }
                        catch (ThreadAbortException)
                        {
                            currentWork.Item1.TrySetResult(false);
                        }
                        catch (Exception e)
                        {
                            currentWork.Item1.TrySetException(e);
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                messagePumpExn = e;
                Thread.ResetAbort();
            }
            catch (Exception e)
            {
                messagePumpExn = e;
            }
        }

        private ExecutionContextLessThread()
        {
            var saved = Thread.CurrentPrincipal;
            try
            {
                Thread.CurrentPrincipal = null;
                if (System.Threading.ExecutionContext.IsFlowSuppressed())
                {
                    t = new Thread(new ThreadStart(MessagePumpWithoutExecutionContext));
                    t.IsBackground = true;
                    t.Start();
                }
                else
                {
                    using (var flow = System.Threading.ExecutionContext.SuppressFlow())
                    {
                        t = new Thread(new ThreadStart(MessagePumpWithoutExecutionContext));
                        t.IsBackground = true;
                        t.Start();
                    }
                }
            }
            finally
            {
                Thread.CurrentPrincipal = saved;
            }
        }
        [SecurityCritical]
        public void CallAction(Action work)
        {
            if (!t.IsAlive)
            {
                throw new InvalidOperationException("Our messagepump thread crashed for some reason", messagePumpExn);
            }
            var task = new TaskCompletionSource<bool>();
            queue.Enqueue(Tuple.Create(task, work));
            var success = task.Task.Result;
            if (!success)
            {
                throw new InvalidOperationException("Expected the task to return true!");
            }
        }

        public T CallFunc<T>(Func<T> func)
        {
            var toFunc = new FuncConv<T>() { toCall = func };
            var helper = new CallHelperSafeHelper<bool, T>() { inData = true, toCall = new Func<bool, T>(toFunc.Call) };
            CallAction(helper.AsAction);
            return helper.outData;
        }

        public O CallFunc<I, O>(Func<I, O> func, I inData)
        {
            var helper = new CallHelperSafeHelper<I, O>() { inData = inData, toCall = func };
            CallAction(helper.AsAction);
            return helper.outData;
        }

        public static ExecutionContextLessThread Create()
        {
            return new ExecutionContextLessThread();
        }


        //private static readonly Lazy<ExecutionContextLessThread> defaultThread = new Lazy<ExecutionContextLessThread>(Create);
        //public static ExecutionContextLessThread Default
        //{
        //    get
        //    {
        //        return defaultThread.Value;
        //    }
        //}
        public static void DefaultCallAction(Action a)
        {
            var toFunc = new ActionConv() { toCall = a };
            var helper = new CallHelperSafeHelper<bool, bool>() { inData = true, toCall = new Func<bool, bool>(toFunc.Call) };
            ExecutionContext.Run(EmptyExecutionContext.Empty, helper.AsContextCallback, null);
            //using (var t = Create())
            //{
            //    t.CallAction(a);
            //}
            //Default.CallAction(a);
        }

        public static O DefaultCallFunc<O>(Func<O> f)
        {
            var toFunc = new FuncConv<O>() { toCall = f };
            var helper = new CallHelperSafeHelper<bool, O>() { inData = true, toCall = new Func<bool, O>(toFunc.Call) };
            ExecutionContext.Run(EmptyExecutionContext.Empty, helper.AsContextCallback, null);
            return helper.outData;
            //using (var t = Create())
            //{
            //    return t.CallFunc(f);
            //}
            //return Default.CallFunc(f);
        }

        public static O DefaultCallFunc<I, O>(Func<I, O> f, I d)
        {
            var helper = new CallHelperSafeHelper<I, O>() { inData = d, toCall = f };
            ExecutionContext.Run(EmptyExecutionContext.Empty, helper.AsContextCallback, null);
            return helper.outData;
            //using (var t = Create())
            //{
            //    return t.CallFunc(f, d);
            //}
            //return Default.CallFunc(f, d);
        }

        [SecuritySafeCritical]
        ~ExecutionContextLessThread()
        {
            Dispose();
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            (new PermissionSet(PermissionState.Unrestricted)).Assert();
            if (t.IsAlive)
            {
                t.Abort();
                t.Join();
            }
        }
    }

    internal static class EmptyExecutionContext {
        
        private static ExecutionContext empty;

        [SecurityCritical]
        static EmptyExecutionContext()
        {
            using (var t = ExecutionContextLessThread.Create())
            {
                empty = t.CallFunc(ExecutionContext.Capture);
            }
        }
        public static ExecutionContext Empty { get { return empty.CreateCopy(); } }

    }
}
