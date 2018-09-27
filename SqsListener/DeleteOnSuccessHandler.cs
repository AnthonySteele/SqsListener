using System;
using System.Threading.Tasks;
using Amazon.SQS.Model;


namespace QueueListener
{
    public class DeleteOnSuccessHandler
    {
        private readonly ISQS _sqs;
        private readonly Func<Message, Task<bool>> _innerHandler;
        private readonly Action<Exception> _exceptionLogger;

        public DeleteOnSuccessHandler(
            ISQS sqs,
            Func<Message, Task<bool>> innerHandler,
            Action<Exception> exceptionLogger)
        {
            _sqs = sqs;
            _innerHandler = innerHandler;
            _exceptionLogger = exceptionLogger;
        }

        public async Task Handle(Message message)
        {
            bool result;
            try
            {
                result = await _innerHandler(message);
            }
            catch (Exception ex)
            {
                _exceptionLogger(ex);
                result = false;
            }

            if (result)
            {
                await _sqs.DeleteMessageAsync(message.ReceiptHandle);
            }
        }
    }
}
