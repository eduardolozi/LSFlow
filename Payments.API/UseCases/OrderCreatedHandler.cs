using LSFlow.Messaging.Interfaces;
using Payments.API.Events.Input;
using Payments.API.Models;
using Payments.API.Models.Enums;

namespace Payments.API.UseCases;

public class OrderCreatedHandler(AppDbContext dbContext) : IEventHandler<OrderCreated>
{
    public async Task HandleAsync(OrderCreated @event, CancellationToken cancellationToken = default)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            UserId = @event.UserId,
            Status = PaymentStatus.Pending,
            OrderId = @event.OrderId,
            Amount = @event.Price,
            PaymentDate = DateTime.UtcNow
        };
        
        await dbContext.Payments.AddAsync(payment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}