using System.Text.Json;
using LSFlow.Outbox.Models;

namespace Orders.API.Events;

public class OrderCreated
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Price { get; set; }

    public OutboxMessage ToOutboxMessage()
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Destination = "orders",
            Key = "",
            CreatedAt = OrderDate,
            EventType = GetType().Name,
            IsProcessed = false,
            Data = JsonSerializer.Serialize(this)
        };
    }
}