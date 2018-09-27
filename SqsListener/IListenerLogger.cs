using System;

namespace QueueListener
{
    public interface IListenerLogger
    {
        void MessageReceived(long recieveTimeMilliseconds);
        void MessageProcessed(long recieveTimeMilliseconds);

        void Throttling(int workerCount, long watchElapsedMilliseconds);
        void Timeout();
        void Exception(Exception exception, bool isCancelling);
    }
}
