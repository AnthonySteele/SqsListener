# SqsListener
patterns of listening to AWS SQS queues. Extracted From justSaying

Bridging the gap between the example of [receiving a message](http://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSGettingStartedGuide/ReceiveMessage.html)

And a working system that will be expected to run multiple instances in an ASG, survive instances terminating, and handle all messages continually whiole balancing load.
