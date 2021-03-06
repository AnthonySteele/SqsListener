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
            var dummySqs = new FixedDataSqs(Enumerable.Empty<ReceiveMessageResponse>());

            var listener = new SimpleListener(
                dummySqs,
                Handler,
                CancelAfterSeconds(1),
                new NullListenerLogger());

            await listener.Listen();
        }

        [Fact]
        public async Task RunForFiveSeconds()
        {
            var dummySQS = new FixedDataSqs(Enumerable.Empty<ReceiveMessageResponse>());

            var handler = Handlers.Wrap(Handler, dummySQS, OnTiming, OnException);

            var listener = new SimpleListener(dummySQS,
                handler,
                CancelAfterSeconds(5),
                new NullListenerLogger());

            await listener.Listen();
        }

        private CancellationToken CancelAfterSeconds(int seconds)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
            return cts.Token;
        }

        private async Task<bool> Handler(Message message)
        {
            await Task.Delay(100);
            Console.WriteLine($"Handled message {message.Body}");
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
