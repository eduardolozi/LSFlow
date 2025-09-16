using LSFlow.Outbox.Models;

namespace LSFlow.Messaging.Interfaces;

public interface IPublisher
{
    Task Publish(string topicOrExchange, string routingKey, OutboxMessage message);
}