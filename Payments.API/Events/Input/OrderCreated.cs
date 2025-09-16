using System.Text.Json;
using LSFlow.Outbox.Models;

namespace Payments.API.Events.Input;

public class OrderCreated
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Price { get; set; }
}