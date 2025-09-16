using LSFlow.IntegrationTests.Events;
using LSFlow.Messaging.Interfaces;

namespace LSFlow.IntegrationTests.UseCases;

public class PaymentEventHandler(AppDbContext dbContext) : IEventHandler<PaymentEvent>
{
    public Task HandleAsync(PaymentEvent @event, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}