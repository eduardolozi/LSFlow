using LSFlow.Outbox.Models;

namespace LSFlow.Messaging.Interfaces;

public interface IPublisher
{
    Task Publish(List<OutboxMessage> messages);
}