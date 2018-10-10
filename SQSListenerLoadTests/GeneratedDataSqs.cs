using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using SqsListener;

namespace SQSListenerLoadTests
{
    class GeneratedDataSqs : ISQS
    {
        private readonly Func<ReceiveMessageResponse> _generator;
        private readonly CancellationToken _ctx;

        public GeneratedDataSqs(Func<ReceiveMessageResponse> generator, CancellationToken ctx)
        {
            _generator = generator;
            _ctx = ctx;
        }

        public async Task<ReceiveMessageResponse> ReceiveMessageAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(1);

            if (_ctx.IsCancellationRequested)
            {
                return null;
            }

            return _generator();
        }

        public async Task<DeleteMessageResponse> DeleteMessageAsync(string receiptHandle)
        {
            await Task.Delay(1);
            return null;
        }
    }
}
