using System;
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

        private int _idleCount;

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
                await WaitForCapacity();
                if (_ctx.IsCancellationRequested)
                {
                    break;
                }
                await ListenOnce();
            }

            _logger.ListenLoopEnd();
        }

        private async Task ListenOnce()
        {
            try
            {
                var receiveTimer = Stopwatch.StartNew();
                var sqsResponse = await ReceiveWithTimeout();
                receiveTimer.Stop();

                if ((sqsResponse?.Messages != null) && sqsResponse.Messages.Any())
                {
                    _idleCount = 0;
                    _logger.MessageReceived(receiveTimer.ElapsedMilliseconds);
                    HandleMessage(sqsResponse.Messages.First());
                }
                else
                {
                    await Idle();
                    _idleCount++;
                }
            }
            catch (Exception ex)
            {
                var isCancelling = _ctx.IsCancellationRequested;
                _logger.Exception(ex, isCancelling);
            }
        }

        private async Task Idle()
        {
            const int idleDuration = 50;
            const int maxIdleCount = 10;

            // Called when there are no messages
            // and therefor there may be no messages in the near future
            // so don't poll aws in a tight loop when there's nothing
            // ramp up  the idle: if it's the first idle loop, idle for 0ms
            // increase by 50ms each consecutive time, up to 0.5 seconds
            var thisIdleCount = Math.Min(_idleCount, maxIdleCount);
            var delay = TimeSpan.FromMilliseconds(idleDuration * thisIdleCount);
            _logger.Idle(_idleCount);
            await Task.Delay(delay, _ctx);
        }

        private async Task<ReceiveMessageResponse> ReceiveWithTimeout()
        {
            var receiveTimeoutCancellation = new CancellationTokenSource(ReceiveTimeout);

            try
            {
                using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _ctx, receiveTimeoutCancellation.Token))
                {
                    return await _sqs.ReceiveMessageAsync(linkedCts.Token);
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
