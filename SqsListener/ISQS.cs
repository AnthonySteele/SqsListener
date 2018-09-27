using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;

public interface ISQS
{
    Task<ReceiveMessageResponse> ReceiveMessageAsync(ReceiveMessageRequest request, CancellationToken cancellationToken);
    Task<DeleteMessageResponse> DeleteMessageAsync(DeleteMessageRequest request, CancellationToken cancellationToken);
}
