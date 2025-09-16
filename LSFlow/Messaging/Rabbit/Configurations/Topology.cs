using LSFlow.Messaging.Rabbit.Models;
using RabbitMQ.Client;
using ExchangeType = LSFlow.Messaging.Rabbit.Models.ExchangeType;

namespace LSFlow.Messaging.Rabbit.Configurations;

public class Topology(Client client)
{
    public async Task AddExchange(string exchangeName, string exchangeType, bool durable = true, bool autoDelete = false)
    {
        if (!ExchangeType.Exists(exchangeType))
            throw new ArgumentException($"Invalid exchange type: {exchangeType}", nameof(exchangeType));
        
        var channel = await client.GetChannel();
        await channel.ExchangeDeclareAsync(exchangeName, exchangeType, durable, autoDelete);
        await channel.CloseAsync();
    }

    public async Task AddQueue(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false, QueueArguments? arguments = null)
    {
        var channel = await client.GetChannel();
        await channel.QueueDeclareAsync(queueName, durable, exclusive, autoDelete, arguments?.ConvertToDictionary());
        await channel.CloseAsync();
    }
}