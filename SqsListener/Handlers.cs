using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace SqsListener
{
    public static class Handlers
    {
        public static Func<Message, Task> Wrap(
            Func<Message, Task<bool>> innerHandler,
            ISQS sqs,
            Action<TimeSpan> onTiming,
            Action<Exception> onException)
        {
            return async message =>
            {
                var timer = Stopwatch.StartNew();

                try
                {
                    bool result;
                    try
                    {
                        result = await innerHandler(message);
                    }
                    catch (Exception ex)
                    {
                        onException(ex);
                        result = false;
                    }

                    if (result)
                    {
                        await sqs.DeleteMessageAsync(message.ReceiptHandle);
                    }
                }
                finally
                {
                    timer.Stop();
                    onTiming(timer.Elapsed);
                }
            };
        }
    }
}
