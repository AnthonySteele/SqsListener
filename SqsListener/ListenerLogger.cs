using System;

namespace QueueListener
{
    public class ListenerLogger : IListenerLogger
    {
        public void MessageReceived(long recieveTimeMilliseconds)
        {
        }

        public void MessageProcessed(long recieveTimeMilliseconds)
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
