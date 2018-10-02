using System;

namespace SqsListener
{
    public class NullListenerLogger : IListenerLogger
    {
        public void MessageReceived(long recieveTimeMilliseconds)
        {
        }

        public void Timeout()
        {
        }

        public void Exception(Exception exception, bool isCancelling)
        {
        }

        public void Idle(int idleCount)
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
