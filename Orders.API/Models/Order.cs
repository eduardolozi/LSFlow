using System.Text.Json.Serialization;
using Orders.API.Models.Enums;

namespace Orders.API.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Price { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public Guid? PaymentId { get; set; }
}

public record CreateOrder(Guid UserId, decimal Price, DateTime OrderDate)
{
    public Order ToOrder()
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            Price = Price,
            OrderDate = OrderDate,
            Status = OrderStatus.Pending
        };
    }
};