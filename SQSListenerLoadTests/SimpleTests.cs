using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using QueueListener;
using Xunit;

namespace SQSListenerLoadTests
{
    public class SimpleTests
    {
        [Fact]
        public async Task RunForASecond()
        {
            var dummySQS = new DummySQS(Enumerable.Empty<ReceiveMessageResponse>());

            var listener = new SimpleListener(dummySQS,
                Handler,
                CancelAfterSeconds(1),
                new ListenerLogger());

            await listener.Listen();
        }

        [Fact]
        public async Task RunForFiveSeconds()
        {
            var dummySQS = new DummySQS(Enumerable.Empty<ReceiveMessageResponse>());

            var listener = new SimpleListener(dummySQS,
                Handler,
                CancelAfterSeconds(5),
                new ListenerLogger());

            await listener.Listen();
        }

        private CancellationToken CancelAfterSeconds(int seconds)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(seconds));
            return cts.Token;
        }

        private async Task Handler(Message arg)
        {
            await Task.Delay(100);
        }
    }
}
