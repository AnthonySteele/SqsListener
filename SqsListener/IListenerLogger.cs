using System;

namespace SqsListener
{
    public interface IListenerLogger
    {
        void ListenLoopStart();
        void ListenLoopEnd();
        void MessagesReceived(TimeSpan duration, int messageCount);

        void Timeout();
        void Exception(Exception exception, bool isCancelling);
        void Throttled(TimeSpan duration);
    }
}
