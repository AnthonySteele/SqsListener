using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using SqsListener;

namespace QueueListener
{
    public class SimpleListener
    {
        private readonly ISQS _sqs;
        private readonly string _queueUrl;
        private readonly Func<Message, Task> _messageHandler;
        private readonly CancellationToken _ctx;
        private readonly IListenerLogger _logger;

        private readonly TaskList _tasks = new TaskList();

        public SimpleListener(
            ISQS sqs,
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
                await WaitForCapacity();
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

                if (sqsResponse.Messages.Any())
                {
                    _logger.MessageReceived(receiveTimer.ElapsedMilliseconds);
                    HandleMessage(sqsResponse.Messages.First());
                }
                else
                {
                    await Task.Delay(SimpleListenerConstants.NoDataPause, _ctx);
                }
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
                MaxNumberOfMessages = 1,
                WaitTimeSeconds = SimpleListenerConstants.WaitTimeSeconds
            };
        }

        private void HandleMessage(Message message)
        {
            // start it, but don't wait for completion
            var task = _messageHandler(message);
            _tasks.Add(task);
        }

        private async Task WaitForCapacity()
        {
            _tasks.ClearCompleted();
            if (!_tasks.CanAdd)
            {
                await _tasks.WhenAny();
                _tasks.ClearCompleted();
            }
        }
    }
}
