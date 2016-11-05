using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace QueueListener
{
    public class Listener
    {
        private readonly IAmazonSQS _sqs;
        private readonly IListenerLogger _logger;
        private readonly RecieveSpecification _recieveSpecification;
        private readonly CancellationToken _ctx;
        private readonly Func<Message, Task> _messageHandler;

        private readonly SemaphoreSlim _semaphore;
        private readonly TimeSpan _receiveTimeout;

        public Listener(
            IAmazonSQS sqs,
            RecieveSpecification recieveSpecification,
            Func<Message, Task> messageHandler,
            CancellationToken ctx,
            IListenerLogger logger)
        {
            _sqs = sqs;
            _recieveSpecification = recieveSpecification;
            _messageHandler = messageHandler;
            _ctx = ctx;
            _logger = logger;

            _semaphore = new SemaphoreSlim(recieveSpecification.MaxConcurrentWorkers, recieveSpecification.MaxConcurrentWorkers);
            _receiveTimeout = TimeSpan.FromSeconds(_recieveSpecification.WaitTimeSeconds + 10);
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
                var currentWorkerCount = await GetCurrentWorkerCount();
                var request = MakeReceiveMessageRequest(currentWorkerCount);

                var receiveTimer = Stopwatch.StartNew();
                var sqsResponse = await ReceiveWithTimeout(request);

                receiveTimer.Stop();

                _logger.MessagesReceived(sqsResponse.Messages.Count, receiveTimer.ElapsedMilliseconds);
                HandleMessages(sqsResponse.Messages);
            }
            catch (Exception ex)
            {
                var isCancelling = _ctx.IsCancellationRequested;
                _logger.Exception(ex, isCancelling);
            }
        }

        private async Task<ReceiveMessageResponse> ReceiveWithTimeout(ReceiveMessageRequest request)
        {
            var receiveTimeoutCancellation = new CancellationTokenSource(_receiveTimeout);

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

        private async Task<int> GetCurrentWorkerCount()
        {
            if (_semaphore.CurrentCount == 0)
            {
                await ThrottleUntilWorkersAreAvailable();
            }

            return _semaphore.CurrentCount;
        }

        private async Task ThrottleUntilWorkersAreAvailable()
        {
            var watch = Stopwatch.StartNew();
            await WaitHelper.AsTask(_semaphore.AvailableWaitHandle);
            watch.Stop();
            _logger.Throttling(_semaphore.CurrentCount, watch.ElapsedMilliseconds);
        }

        private ReceiveMessageRequest MakeReceiveMessageRequest(int availableWorkers)
        {
            return new ReceiveMessageRequest
            {
                QueueUrl = _recieveSpecification.QueueUrl,
                MaxNumberOfMessages = Math.Min(availableWorkers, _recieveSpecification.MaxMessagesPerRead),
                WaitTimeSeconds = _recieveSpecification.WaitTimeSeconds
            };
        }

        private void HandleMessages(IEnumerable<Message> messages)
        {
            foreach (var message in messages)
            {
                WaitHelper.RunInSemaphore(() => _messageHandler(message), _semaphore);
            }
        }
    }
}
