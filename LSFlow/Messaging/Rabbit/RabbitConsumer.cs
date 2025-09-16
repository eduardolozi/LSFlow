using System.Text;
using System.Text.Json;
using LSFlow.Messaging.Interfaces;
using LSFlow.Messaging.Rabbit.Configurations;
using LSFlow.Messaging.Rabbit.Models;
using LSFlow.Outbox;
using LSFlow.Outbox.Interfaces;
using LSFlow.Outbox.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LSFlow.Messaging.Rabbit;

public class RabbitConsumer(Client client, List<QueueBinding> bindings, IEventDispatcher eventDispatcher, IOutboxDbContext dbContext) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var channel = await client.GetChannel();
        
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<OutboxMessage>(payload);

            if (message != null)
            {
                var alreadyProcessed = await dbContext
                    .ProcessedMessages
                    .AnyAsync(x => x.Id == message.Id, cancellationToken);
                
                if (!alreadyProcessed)
                {
                    // await eventDispatcher.DispatchAsync(message.EventType, message.Data, cancellationToken);
                    await dbContext.ProcessedMessages.AddAsync(
                        new ProcessedMessage
                        {
                            Id = message.Id,
                            EventType = message.EventType,
                            ProcessedAt = DateTime.UtcNow
                        },
                        cancellationToken
                    );
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            
            await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
        };

        var queues = new HashSet<string>();
        foreach(var binding in bindings)
        {
            foreach (var rk in binding.RoutingKeys)
            {
                await channel.QueueBindAsync(
                    queue: binding.QueueName,
                    exchange: binding.ExchangeName,
                    routingKey: rk,
                    cancellationToken: cancellationToken
                );
            }
            queues.Add(binding.QueueName);
        }

        foreach (var queue in queues)
        {
            await channel.BasicConsumeAsync(
                queue: queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken
            );
        }
    }
}