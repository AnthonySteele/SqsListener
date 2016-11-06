using System;

namespace QueueListener
{
    public class RecieveSpecification
    {
        public RecieveSpecification(
            string queueUrl,
            int waitTimeSeconds,
            int maxMessagesPerRead,
            int maxConcurrentWorkers)
        {
            QueueUrl = queueUrl;
            WaitTimeSeconds = waitTimeSeconds;
            MaxMessagesPerRead = maxMessagesPerRead;
            MaxConcurrentWorkers = maxConcurrentWorkers;
        }

        public static RecieveSpecification MakeASimpleOne(string queueUrl)
        {
            return new RecieveSpecification(
                queueUrl,
                20,
                10,
                Environment.ProcessorCount * 12);
        }

        public string QueueUrl { get; private set; }
        public int WaitTimeSeconds { get; private set; }
        public int MaxMessagesPerRead { get; private set; }
        public int MaxConcurrentWorkers { get; private set; }
    }
}