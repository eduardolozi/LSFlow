using System.Text.Json;
using LSFlow.Outbox.Models;

namespace Payments.API.Events.Output;

public class PaymentRejected
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public DateTime RejectionDate { get; set; }
    public required string RejectionReason { get; set; }
    
    public OutboxMessage ToOutboxMessage()
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Destination = "payments",
            Key = "payment.rejected",
            EventType = GetType().Name,
            CreatedAt = DateTime.Now,
            IsProcessed = false,
            Data = JsonSerializer.Serialize(this)
        };
    }
}