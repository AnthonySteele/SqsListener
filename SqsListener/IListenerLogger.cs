using System;

namespace SqsListener
{
    public interface IListenerLogger
    {
        void ListenLoopStart();
        void ListenLoopEnd();
        void MessageReceived(TimeSpan duration);

        void Timeout();
        void Exception(Exception exception, bool isCancelling);
        void Idle(int idleCount);
        void Throttled(TimeSpan duration);
    }
}
