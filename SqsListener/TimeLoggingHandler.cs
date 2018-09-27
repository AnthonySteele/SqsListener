using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.SQS.Model;


namespace QueueListener
{
    public class TimeLoggingHandler
    {
        private readonly Func<Message, Task> _innerHandler;
        private readonly IListenerLogger _logger;

        public TimeLoggingHandler(
            Func<Message, Task> innerHandler, IListenerLogger logger)
        {
            _innerHandler = innerHandler;
            _logger = logger;
        }

        public async Task Handle(Message message)
        {
            var timer = Stopwatch.StartNew();

            try
            {
                await _innerHandler(message);
            }
            finally 
            {
                timer.Stop();
                _logger.MessageProcessed(timer.ElapsedMilliseconds);
            }
        }
    }
}
