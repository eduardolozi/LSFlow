using LSFlow.Outbox;

namespace LSFlow.Messaging.Interfaces;

public interface IConsumer
{
    Task Consume(CancellationToken cancellationToken);
}