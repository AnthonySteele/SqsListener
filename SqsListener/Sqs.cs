using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using QueueListener;

public class Sqs : ISQS
{
    private readonly IAmazonSQS _sqs;
    private readonly string _queueUrl;

    public Sqs(IAmazonSQS sqs, string queueUrl)
    {
        _sqs = sqs;
        _queueUrl = queueUrl;
    }

    public Task<ReceiveMessageResponse> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        var receiveRequest = new ReceiveMessageRequest
        {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = SimpleListenerConstants.WaitTimeSeconds
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
