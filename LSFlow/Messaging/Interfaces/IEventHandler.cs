namespace LSFlow.Messaging.Interfaces;

// This interface needs to be implemented in the application that uses the IConsumer
public interface IEventHandler<TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

/*
 EXAMPLE OF IMPLEMENTATION
-----------------------------------------------------------------------------------------------------------------------
public class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    public Task HandleAsync(OrderCreated @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Processando pedido {@event.Id}");
        return Task.CompletedTask;
    }
}
*/