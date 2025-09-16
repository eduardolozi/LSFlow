namespace LSFlow.Messaging.Rabbit.Models;

public class QueueBinding
{
    public required string QueueName { get; init; }
    public required string ExchangeName { get; init; }
    public List<string>? RoutingKeys { get; init; }
}