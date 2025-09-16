using System.Text;
using System.Text.Json;
using LSFlow.Messaging.Interfaces;
using LSFlow.Messaging.Rabbit.Configurations;
using LSFlow.Messaging.Rabbit.Models;
using LSFlow.Outbox;
using LSFlow.Outbox.Interfaces;
using LSFlow.Outbox.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LSFlow.Messaging.Rabbit;

public class RabbitConsumer(Client client, List<QueueBinding> bindings, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await client.GetChannel();

        var queues = new HashSet<string>();
        foreach (var binding in bindings)
        {
            if (binding.RoutingKeys != null && binding.RoutingKeys.Any())
            {
                foreach (var rk in binding.RoutingKeys)
                {
                    await channel.QueueBindAsync(binding.QueueName, binding.ExchangeName, rk, cancellationToken: stoppingToken);
                }
            }
            else
            {
                await channel.QueueBindAsync(binding.QueueName, binding.ExchangeName, "", cancellationToken: stoppingToken);
            }

            queues.Add(binding.QueueName);
        }

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IOutboxDbContext>();
            var eventDispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();

            var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = JsonSerializer.Deserialize<OutboxMessage>(payload);

            if (message != null)
            {
                var alreadyProcessed = await dbContext.ProcessedMessages
                    .AnyAsync(x => x.Id == message.Id, stoppingToken);

                if (!alreadyProcessed)
                {
                    await eventDispatcher.DispatchAsync(message.EventType, message.Data, stoppingToken);

                    await dbContext.ProcessedMessages.AddAsync(new ProcessedMessage
                    {
                        Id = message.Id,
                        EventType = message.EventType,
                        ProcessedAt = DateTime.UtcNow
                    }, stoppingToken);

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }

            await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
        };

        foreach (var queue in queues)
        {
            await channel.BasicConsumeAsync(queue, autoAck: false, consumer, cancellationToken: stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}