using System;

namespace QueueListener
{
    public class ListenerLogger : IListenerLogger
    {
        public void MessagesReceived(int messagesCount, long recieveTimeMilliseconds)
        {
        }

        public void Throttling(int workerCount, long watchElapsedMilliseconds)
        {
        }

        public void Timeout()
        {
        }

        public void Exception(Exception exception, bool isCancelling)
        {
        }
    }
}
