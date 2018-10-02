using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace SQSListenerLoadTests
{
    class DummySQS: ISQS
    {
        private readonly Queue<ReceiveMessageResponse> _pending;

        public DummySQS(IEnumerable<ReceiveMessageResponse> items)
        {
            _pending = new Queue<ReceiveMessageResponse>(items);
        }

        public async Task<ReceiveMessageResponse> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);
            if (_pending.Count == 0)
            {
                return null;
            }

            return _pending.Dequeue();
        }

        public async Task<DeleteMessageResponse> DeleteMessageAsync(string receiptHandle)
        {
            await Task.Delay(1);
            return null;
        }
    }
}
