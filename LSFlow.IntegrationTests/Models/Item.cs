namespace LSFlow.IntegrationTests.Models;

public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
}