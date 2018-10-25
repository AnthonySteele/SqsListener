using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace SqsListener
{
    public class Sqs : ISQS
    {
        private readonly IAmazonSQS _sqs;
        private readonly string _queueUrl;

        public Sqs(IAmazonSQS sqs, string queueUrl)
        {
            _sqs = sqs;
            _queueUrl = queueUrl;
        }

        public Task<ReceiveMessageResponse> ReceiveMessagesAsync(int max, CancellationToken cancellationToken)
        {
            var receiveRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = max,
                WaitTimeSeconds = SqsConstants.WaitTimeSeconds
            };

            return _sqs.ReceiveMessageAsync(receiveRequest, cancellationToken);
        }

        public Task<DeleteMessageResponse> DeleteMessageAsync(string receiptHandle)
        {
            var deleteRequest = new DeleteMessageRequest
            {
                QueueUrl = _queueUrl,
                ReceiptHandle = receiptHandle
            };

            return _sqs.DeleteMessageAsync(deleteRequest);
        }
    }
}
