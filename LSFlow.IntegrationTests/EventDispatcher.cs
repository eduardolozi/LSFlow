using System.Text.Json;
using LSFlow.IntegrationTests.Events;
using LSFlow.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace LSFlow.IntegrationTests;

public class EventDispatcher(IServiceProvider provider) : IEventDispatcher
{
    private readonly Dictionary<string, Type> _eventTypeMapping = new()
    {
        { "OrderCreated", typeof(OrderCreatedEvent) },
        { "payment.processing", typeof(PaymentEvent) }
    };

    public async Task DispatchAsync(string eventType, string payload, CancellationToken cancellationToken = default)
    {
        if (!_eventTypeMapping.TryGetValue(eventType, out var type))
            throw new InvalidOperationException($"No handler registered for eventType {eventType}");

        var @event = JsonSerializer.Deserialize(payload, type);
        await Task.Delay(100, cancellationToken);
        var handlerType = typeof(IEventHandler<>).MakeGenericType(type);
        dynamic handler = provider.GetRequiredService(handlerType);
        
        await handler.HandleAsync((dynamic)@event!, cancellationToken);
    }
}