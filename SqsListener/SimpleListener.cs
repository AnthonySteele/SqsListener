using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace QueueListener
{
    public static class SimpleListenerConstants
    {
        public static int MaxNumberOfMessagesPerBatch = 10;
        public static int WaitTimeSeconds = 20;
        public static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(WaitTimeSeconds + 5);

    }

    public class SimpleListener
    {
        private readonly IAmazonSQS _sqs;
        private readonly string _queueUrl;
        private readonly Func<Message, Task> _messageHandler;
        private readonly CancellationToken _ctx;
        private readonly IListenerLogger _logger;

        public SimpleListener(
            IAmazonSQS sqs,
            string queueUrl,
            Func<Message, Task> messageHandler,
            CancellationToken ctx,
            IListenerLogger logger)
        {
            _sqs = sqs;
            _queueUrl = queueUrl;
            _messageHandler = messageHandler;
            _ctx = ctx;
            _logger = logger;
        }

        public async Task Listen()
        {
            while (!_ctx.IsCancellationRequested)
            {
                await ListenOnce();
            }
        }

        private async Task ListenOnce()
        {
            try
            {
                var receiveTimer = Stopwatch.StartNew();
                var request = MakeReceiveMessageRequest();
                var sqsResponse = await ReceiveWithTimeout(request);

                receiveTimer.Stop();

                _logger.MessagesReceived(sqsResponse.Messages.Count, receiveTimer.ElapsedMilliseconds);
                await HandleMessages(sqsResponse.Messages);
            }
            catch (Exception ex)
            {
                var isCancelling = _ctx.IsCancellationRequested;
                _logger.Exception(ex, isCancelling);
            }
        }

        private async Task<ReceiveMessageResponse> ReceiveWithTimeout(ReceiveMessageRequest request)
        {
            var receiveTimeoutCancellation = new CancellationTokenSource(SimpleListenerConstants.ReceiveTimeout);

            try
            {
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _ctx, receiveTimeoutCancellation.Token))
                {
                    return await _sqs.ReceiveMessageAsync(request, linkedCts.Token);
                }
            }
            finally
            {
                if (receiveTimeoutCancellation.Token.IsCancellationRequested)
                {
                    _logger.Timeout();
                }
            }
        }

        private ReceiveMessageRequest MakeReceiveMessageRequest()
        {
            return new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = SimpleListenerConstants.MaxNumberOfMessagesPerBatch,
                WaitTimeSeconds = SimpleListenerConstants.WaitTimeSeconds
            };
        }

        /// <summary>
        /// the problems wityh this are
        /// - messages are handles 1 after another, not launched in parallel
        /// - you wait for the last one to complete handling before getting any more
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        private async Task HandleMessages(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                await _messageHandler(message);
            }
        }
    }
}
