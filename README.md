# SqsListener
patterns of listening to AWS SQS queues. Extracted From justSaying

Bridging the gap between the example of [receiving a message](http://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSGettingStartedGuide/ReceiveMessage.html)

And a working system that will be expected to run and handle all messages continually while balancing load, running on
multiple instances in an ASG, scaled up and down for load, survive instances terminating and other issues.
