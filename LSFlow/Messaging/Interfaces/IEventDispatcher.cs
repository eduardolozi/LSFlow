namespace LSFlow.Messaging.Interfaces;

// This interface needs to be implemented in the application that uses the IConsumer
public interface IEventDispatcher
{
    Task DispatchAsync(string eventType, string payload, CancellationToken cancellationToken = default);
}

/*
EXAMPLE OF IMPLEMENTATION
-----------------------------------------------------------------------------------------------------------------------
public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _provider;
    private readonly Dictionary<string, Type> _eventTypeMapping;

    public EventDispatcher(IServiceProvider provider)
    {
        _provider = provider;

        _eventTypeMapping = new Dictionary<string, Type>
        {
            { "OrderCreated", typeof(OrderCreated) },
            { "PaymentProcessed", typeof(PaymentProcessed) }
        };
    }

    public async Task DispatchAsync(string eventType, string payload, CancellationToken cancellationToken = default)
    {
        if (!_eventTypeMapping.TryGetValue(eventType, out var type))
            throw new InvalidOperationException($"No handler registered for eventType {eventType}");

        var @event = JsonSerializer.Deserialize(payload, type);

        var handlerType = typeof(IEventHandler<>).MakeGenericType(type);
        dynamic handler = _provider.GetRequiredService(handlerType);

        await handler.HandleAsync((dynamic)@event, cancellationToken);
    }
}
*/