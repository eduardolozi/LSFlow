using LSFlow.IntegrationTests.Models.Enums;

namespace LSFlow.IntegrationTests.Models;

public class Order
{
    public Guid Id { get; set; }
    public List<Item> Items { get; set; } = [];
    public Guid UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Price { get; set; }

    public void CalculatePrice()
    {
        Price = Items.Sum(x => x.UnitPrice * x.Quantity);
    }
}