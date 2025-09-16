using Microsoft.AspNetCore.Mvc;
using Orders.API.Events;
using Orders.API.Models;

namespace Orders.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Post([FromBody] CreateOrder createOrder)
    {
        var order = createOrder.ToOrder();
        dbContext.Orders.Add(order);

        var outboxMessage = new OrderCreated
        {
            OrderId = order.Id,
            Price = order.Price,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
        }.ToOutboxMessage();
        await dbContext.OutboxMessages.AddAsync(outboxMessage);
        await dbContext.SaveChangesAsync();

        return Ok();
    }
}