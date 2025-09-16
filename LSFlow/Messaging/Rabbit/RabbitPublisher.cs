using System.Text;
using System.Text.Json;
using LSFlow.Messaging.Interfaces;
using LSFlow.Messaging.Rabbit.Configurations;
using LSFlow.Outbox.Models;
using RabbitMQ.Client;

namespace LSFlow.Messaging.Rabbit;

public class RabbitPublisher(Client client) : IPublisher
{
    public async Task Publish(string exchange, string routingKey, OutboxMessage message)
    {
        var channel = await client.GetChannel();
        
        //study the scenario of mandatory = true, using callback to catch the message not delivered to queue
        var payload = JsonSerializer.Serialize(message);
        await channel.BasicPublishAsync(exchange, routingKey, false, Encoding.UTF8.GetBytes(payload));
        
        await channel.CloseAsync();
    }
}