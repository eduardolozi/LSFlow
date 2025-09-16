using LSFlow.Messaging.Interfaces;
using LSFlow.Messaging.Rabbit.Configurations;
using LSFlow.Messaging.Rabbit.Models;
using LSFlow.Outbox.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LSFlow.Messaging.Rabbit;

public static class Extensions
{
    public static void AddRabbitClient(this IServiceCollection services)
    {
        services.AddSingleton<Client>(_ =>
        {
            var username = Environment.GetEnvironmentVariable("RABBIT_USERNAME") ?? throw new InvalidOperationException("RABBIT_USERNAME env was not found");
            var password = Environment.GetEnvironmentVariable("RABBIT_PASSWORD") ?? throw new InvalidOperationException("RABBIT_PASSWORD env was not found");
            var hostname = Environment.GetEnvironmentVariable("RABBIT_HOSTNAME") ?? throw new InvalidOperationException("RABBIT_HOSTNAME env was not found");
            var port = Environment.GetEnvironmentVariable("RABBIT_PORT") ?? throw new InvalidOperationException("RABBIT_PORT env was not found");
            var virtualHost = Environment.GetEnvironmentVariable("RABBIT_VHOST") ?? throw new InvalidOperationException("RABBIT_VHOST env was not found");

            if (!int.TryParse(port, out var portNumber))
                throw new InvalidCastException("RABBIT_PORT env is not a valid port number");
            
            return new Client(
                username,
                password,
                hostname,
                portNumber,
                virtualHost
            );
        });

        services.AddScoped<IPublisher>(provider =>
        {
            var client = provider.GetRequiredService<Client>();
            return new RabbitPublisher(client);
        });

        services.AddScoped<Topology>();
    }

    public static void AddRabbitConsumer(this IServiceCollection services, List<QueueBinding> bindings)
    {
        services.AddSingleton<IHostedService>(provider =>
        {
            var client = provider.GetRequiredService<Client>();
            return new RabbitConsumer(client, bindings, provider);
        });
    }
}