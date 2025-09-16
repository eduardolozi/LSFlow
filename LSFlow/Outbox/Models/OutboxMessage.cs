namespace LSFlow.Outbox.Models;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; }
    public string Data { get; set; }
    public string Destination { get; set; }     // Kafka Topics, Rabbit Exchanges
    public string Key { get; set; }             // Kafka PartitionKey, Rabbit RoutingKey
    public DateTime CreatedAt { get; set; }
    public bool IsProcessed { get; set; }
}