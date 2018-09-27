using System;

namespace QueueListener
{
    public static class SimpleListenerConstants
    {
        public static int WaitTimeSeconds = 20;
        public static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(WaitTimeSeconds + 5);
        public static readonly TimeSpan NoDataPause = TimeSpan.FromMilliseconds(50);
    }
}
