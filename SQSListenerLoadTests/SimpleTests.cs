using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using SqsListener;
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
                new NullListenerLogger());

            await listener.Listen();
        }

        [Fact]
        public async Task RunForFiveSeconds()
        {
            var dummySQS = new DummySQS(Enumerable.Empty<ReceiveMessageResponse>());

            var handler = Handlers.Wrap(dummySQS, Handler, OnTiming, OnException);

            var listener = new SimpleListener(dummySQS,
                handler,
                CancelAfterSeconds(5),
                new NullListenerLogger());

            await listener.Listen();
        }

        private CancellationToken CancelAfterSeconds(int seconds)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(seconds));
            return cts.Token;
        }

        private async Task<bool> Handler(Message message)
        {
            await Task.Delay(100);
            return true;
        }

        private void OnException(Exception ex)
        {

        }

        private void OnTiming(TimeSpan t)
        {

        }
    }
}
