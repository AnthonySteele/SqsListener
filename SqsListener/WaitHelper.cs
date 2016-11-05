using System;
using System.Threading;
using System.Threading.Tasks;

namespace QueueListener
{
    public static class WaitHelper
    {
        public static Task AsTask(WaitHandle waitHandle)
        {
            var tcs = new TaskCompletionSource<object>();

            ThreadPool.RegisterWaitForSingleObject(
                waitObject: waitHandle,
                callBack: (o, timeout) => { tcs.SetResult(null); },
                state: null,
                timeout: TimeSpan.FromMilliseconds(int.MaxValue),
                executeOnlyOnce: true);

            return tcs.Task;
        }

        public static void RunInSemaphore(Func<Task> action, SemaphoreSlim semaphore)
        {
            var throttledTask = new Task<Task>(() => ReleaseAfter(action, semaphore));
            semaphore.Wait();
            throttledTask.Start();
        }

        private static async Task ReleaseAfter(Func<Task> action, SemaphoreSlim semaphore)
        {
            try
            {
                await action();
            }
            finally
            {
                semaphore.Release();
            }
        }

    }
}
