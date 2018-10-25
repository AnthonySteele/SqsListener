using System;

namespace SqsListener
{
    public class NullListenerLogger : IListenerLogger
    {
        public void MessagesReceived(TimeSpan duration, int messageCount)
        {
        }

        public void Throttled(TimeSpan duration)
        {
        }

        public void Timeout()
        {
        }

        public void Exception(Exception exception, bool isCancelling)
        {
        }

        public void ListenLoopStart()
        {
        }

        public void ListenLoopEnd()
        {
        }
    }
}
