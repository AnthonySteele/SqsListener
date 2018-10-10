using System;
using System.Collections.Generic;
using System.Linq;
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
            var responses = MakeResponses(1);
            var dummySQS = new FixedDataSqs(responses);

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
            var responses = MakeResponses(100);
            var dummySQS = new FixedDataSqs(responses);

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

        [Fact]
        public async Task TestWithMessagesFor5Seconds()
        {
            var dummySQS = new GeneratedDataSqs(MakeResponse, CancelAfterSeconds(5));

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

            Assert.True(receivedCount > 100);
        }


        private static List<ReceiveMessageResponse> MakeResponses(int count)
        {
            var responses = new List<ReceiveMessageResponse>();

            for (int index = 0; index < count; index++)
            {
                var response = MakeResponse();
                responses.Add(response);
            }

            return responses;
        }

        private static ReceiveMessageResponse MakeResponse()
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
            return response;
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
