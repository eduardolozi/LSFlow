using System.Text.Json;
using LSFlow.IntegrationTests.Events.Enum;
using LSFlow.IntegrationTests.Utils;
using LSFlow.Outbox.Models;

namespace LSFlow.IntegrationTests.Events;

public class PaymentEvent
{
    public Guid Id { get; set; }
    public DateTime PaymentAt { get; set; }
    public decimal Price { get; set; }
    public Guid UserId { get; set; }
    public PaymentStatus Status { get; set; }

    public OutboxMessage ToOutboxMessage(string exchange, string routingKey)
    {
        return new OutboxMessage()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Data = JsonSerializer.Serialize(this),
            Destination = exchange,
            Key = routingKey,
            EventType = Status.GetDescription(),
            IsProcessed = false
        };
    }
}