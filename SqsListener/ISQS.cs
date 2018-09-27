using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;

public interface ISQS
{
    Task<ReceiveMessageResponse> ReceiveMessageAsync(CancellationToken cancellationToken);
    Task<DeleteMessageResponse> DeleteMessageAsync(string receiptHandle);
}
