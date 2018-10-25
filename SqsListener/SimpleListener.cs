using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;

namespace SqsListener
{
    public class SimpleListener
    {
        public static readonly TimeSpan ReceiveTimeout = TimeSpan.FromSeconds(SqsConstants.WaitTimeSeconds + 5);

        private readonly ISQS _sqs;
        private readonly Func<Message, Task> _messageHandler;
        private readonly CancellationToken _ctx;
        private readonly IListenerLogger _logger;

        private readonly TaskList _tasks = new TaskList(10);

        public SimpleListener(
            ISQS sqs,
            Func<Message, Task> messageHandler,
            CancellationToken ctx,
            IListenerLogger logger)
        {
            _sqs = sqs;
            _messageHandler = messageHandler;
            _ctx = ctx;
            _logger = logger;
        }

        public async Task Listen()
        {
            _logger.ListenLoopStart();

            while (!_ctx.IsCancellationRequested)
            {
                await WaitForCapacity()
                    .ConfigureAwait(false);

                if (_ctx.IsCancellationRequested)
                {
                    break;
                }

                await ListenOnce()
                    .ConfigureAwait(false);
            }

            _logger.ListenLoopEnd();
        }

        private async Task ListenOnce()
        {
            try
            {
                var receiveTimer = Stopwatch.StartNew();
                var sqsResponse = await ReceiveWithTimeout()
                    .ConfigureAwait(false);

                receiveTimer.Stop();

                if (sqsResponse?.Messages?.Any() == true)
                {
                    _logger.MessagesReceived(receiveTimer.Elapsed, sqsResponse.Messages.Count);
                    HandleMessages(sqsResponse.Messages);
                }
            }
            catch (Exception ex)
            {
                var isCancelling = _ctx.IsCancellationRequested;
                _logger.Exception(ex, isCancelling);
            }
        }

        private async Task<ReceiveMessageResponse> ReceiveWithTimeout()
        {
            const int maxMessagesPerQuery = 6;
            var maxMessages = Math.Min(_tasks.Capacity, maxMessagesPerQuery);
            var receiveTimeoutCancellation = new CancellationTokenSource(ReceiveTimeout);

            try
            {
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _ctx, receiveTimeoutCancellation.Token))
                {
                    return await _sqs.ReceiveMessagesAsync(maxMessages, linkedCts.Token)
                        .ConfigureAwait(false);
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

        private void HandleMessages(List<Message> messages)
        {
            foreach (var message in messages)
            {
                // start it, but don't wait for completion
                // it is still an open question if we should involve
                // the thread pool at all
                _tasks.Add(MessageHandlerTask(message));
            }
        }

        private Task MessageHandlerTask(Message message)
        {
            return Task.Run(async ()
                    => await _messageHandler(message).ConfigureAwait(false),
                _ctx);
        }

        private async Task WaitForCapacity()
        {
            _tasks.ClearCompleted();

            if (_tasks.Capacity <= 0)
            {
                var capacityTimer = Stopwatch.StartNew();

                await _tasks.WhenAny()
                    .ConfigureAwait(false);
                _tasks.ClearCompleted();

                capacityTimer.Stop();
                _logger.Throttled(capacityTimer.Elapsed);
            }
        }
    }
}
