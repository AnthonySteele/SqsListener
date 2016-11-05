using System;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace QueueListener
{
    public class DeleteOnSuccessHandler
    {
        private readonly IAmazonSQS _sqs;
        private readonly string _queueUrl;
        private readonly Func<Message, Task<bool>> _innerHandler;
        private readonly Action<Exception> _exceptionLogger;

        public DeleteOnSuccessHandler(IAmazonSQS sqs,
            string queueUrl, 
            Func<Message, Task<bool>> innerHandler,
            Action<Exception> exceptionLogger)
        {
            _sqs = sqs;
            _queueUrl = queueUrl;
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
                await DeleteMessageFromQueue(message.ReceiptHandle);
            }
        }

        private async Task DeleteMessageFromQueue(string receiptHandle)
        {
            var deleteRequest = new DeleteMessageRequest
                {
                    QueueUrl = _queueUrl,
                    ReceiptHandle = receiptHandle
                };

            await _sqs.DeleteMessageAsync(deleteRequest);
        }
    }
}
