using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace SqsListener
{
    public interface ISQS
    {
        Task<ReceiveMessageResponse> ReceiveMessagesAsync(int max, CancellationToken cancellationToken);
        Task<DeleteMessageResponse> DeleteMessageAsync(string receiptHandle);
    }
}
