using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using SqsListener;
using Xunit;

namespace SQSListenerLoadTests
{
    public class DataTests
    {
        [Fact]
        public async Task TestWithOneMessage()
        {
            var responses = MakeResponse(1);
            var dummySQS = new DummySQS(responses);

            int receivedCount = 0;

            async Task<bool> Handler(Message message)
            {
                await Task.Delay(100);
                Console.WriteLine($"Handled message {message.Body}");

                Interlocked.Increment(ref receivedCount);
                return true;
            }

            var wrappedHandler = Handlers.Wrap(Handler, dummySQS, OnTiming, OnException);

            var listener = new SimpleListener(dummySQS,
                wrappedHandler,
                CancelAfterSeconds(1),
                new NullListenerLogger());

            await listener.Listen();

            Assert.Equal(1, receivedCount);
        }

        [Fact]
        public async Task TestWith100Messages()
        {
            var responses = MakeResponse(100);
            var dummySQS = new DummySQS(responses);

            int receivedCount = 0;

            async Task<bool> Handler(Message message)
            {
                await Task.Delay(50);
                Console.WriteLine($"Handled message {message.Body}");

                Interlocked.Increment(ref receivedCount);
                return true;
            }

            var wrappedHandler = Handlers.Wrap(Handler, dummySQS, OnTiming, OnException);

            var listener = new SimpleListener(dummySQS,
                wrappedHandler,
                CancelAfterSeconds(5),
                new NullListenerLogger());

            await listener.Listen();

            Assert.Equal(100, receivedCount);
        }

        private static List<ReceiveMessageResponse> MakeResponse(int count)
        {
            var responses = new List<ReceiveMessageResponse>();

            for (int index = 0; index < count; index++)
            {
                var response = new ReceiveMessageResponse
                {
                    Messages = new List<Message>
                    {
                        new Message
                        {
                            Body = "some message"
                        }
                    }
                };

                responses.Add(response);
            }

            return responses;
        }

        private CancellationToken CancelAfterSeconds(int seconds)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
            return cts.Token;
        }


        private void OnException(Exception ex)
        {

        }

        private void OnTiming(TimeSpan t)
        {

        }
    }
}
