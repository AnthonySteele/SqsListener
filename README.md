# SqsListener

Patterns of listening to AWS SQS queues. Extracted from [JustSaying](https://github.com/justeat/JustSaying)

Bridging the gap between the example of [receiving one message once on one machine](http://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSGettingStartedGuide/ReceiveMessage.html) and a message pump that will be expected to run continually, handling all messages while balancing load, running across
multiple instances in an ASG, scaled up and down for load, survive instances terminating and other instance issues.


Small pieces, loosely joined. Toolkits not wrappers.
