using System.Text;
using System.Text.Json;
using LSFlow.Messaging.Interfaces;
using LSFlow.Messaging.Rabbit.Configurations;
using LSFlow.Outbox.Models;
using RabbitMQ.Client;

namespace LSFlow.Messaging.Rabbit;

public class RabbitPublisher(Client client) : IPublisher
{
    public async Task Publish(List<OutboxMessage> messages)
    {
        var channel = await client.GetChannel();
        var publishTasks = new List<(ValueTask, OutboxMessage)>();
        foreach (var message in messages)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var publishTask = channel.BasicPublishAsync(exchange: message.Destination, routingKey: message.Key, body: body, mandatory: true);
            publishTasks.Add((publishTask, message));
        }
        await AwaitPublishes(publishTasks);
    }

    private static async Task AwaitPublishes(List<(ValueTask publishTask, OutboxMessage message)> publishTasks)
    {
        foreach (var pt in publishTasks)
        {
            try
            {
                await pt.publishTask;
                pt.message.IsProcessed = true;
            }
            catch (Exception ex)
            {
                pt.message.IsProcessed = false;
            }
        }
    }
}