using System;

namespace QueueListener
{
    public interface IListenerLogger
    {
        void MessagesReceived(int messagesCount, long recieveTimeMilliseconds);
        void Throttling(int workerCount, long watchElapsedMilliseconds);
        void Timeout();
        void Exception(Exception exception, bool isCancelling);
    }
}