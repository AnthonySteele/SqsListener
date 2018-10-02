using System;

namespace SqsListener
{
    public interface IListenerLogger
    {
        void MessageReceived(long recieveTimeMilliseconds);

        void Timeout();
        void Exception(Exception exception, bool isCancelling);
        void Idle(int idleCount);
        void ListenLoopStart();
        void ListenLoopEnd();
    }
}
