using System.Text.Json;
using LSFlow.Messaging.Interfaces;
using Payments.API.Events.Input;

namespace Payments.API;

public class EventDispatcher(IServiceProvider provider) : IEventDispatcher
{
    private readonly Dictionary<string, Type> _eventTypeMapping = new()
    {
        { "OrderCreated", typeof(OrderCreated) }
    };

    public async Task DispatchAsync(string eventType, string payload, CancellationToken cancellationToken = default)
    {
        if (!_eventTypeMapping.TryGetValue(eventType, out var type))
            throw new InvalidOperationException($"No handler registered for eventType {eventType}");

        var @event = JsonSerializer.Deserialize(payload, type) ?? throw new NullReferenceException("Unable to deserialize event");

        var handlerType = typeof(IEventHandler<>).MakeGenericType(type);
        dynamic handler = provider.GetRequiredService(handlerType);

        await handler.HandleAsync((dynamic)@event, cancellationToken);
    }
}