using DotNet.Testcontainers.Builders;
using LSFlow.IntegrationTests.Events;
using LSFlow.IntegrationTests.UseCases;
using LSFlow.Messaging.Interfaces;
using LSFlow.Messaging.Rabbit;
using LSFlow.Messaging.Rabbit.Configurations;
using LSFlow.Messaging.Rabbit.Models;
using LSFlow.Outbox;
using LSFlow.Outbox.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Microsoft.Extensions.Hosting;

namespace LSFlow.IntegrationTests.Lifecycle;

public class BaseFixture : IAsyncLifetime
{
    public IServiceProvider ServiceProvider { get; set; }
    private IHost _host;
    public static IServiceCollection Services { get; private set; } = new ServiceCollection();
    
    private PostgreSqlContainer _pgContainer;
    private RabbitMqContainer _rabbitContainer;

    public async Task InitializeAsync()
    {
        _pgContainer = new PostgreSqlBuilder()
            .WithUsername(PostgreSqlBuilder.DefaultUsername)
            .WithPassword(PostgreSqlBuilder.DefaultPassword)
            .WithDatabase(PostgreSqlBuilder.DefaultDatabase)
            .WithPortBinding(0, PostgreSqlBuilder.PostgreSqlPort)
            .Build();
        
        await _pgContainer.StartAsync();

        var pgHostMappedPort = _pgContainer.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort);
        
        _rabbitContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3.11-management")
            .WithUsername(RabbitMqBuilder.DefaultUsername)
            .WithPassword(RabbitMqBuilder.DefaultPassword)
            .WithPortBinding(0, 15672)
            .WithPortBinding(5672, RabbitMqBuilder.RabbitMqPort)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(RabbitMqBuilder.RabbitMqPort))
            .Build();
        
        await _rabbitContainer.StartAsync();
        
        SetRabbitEnvironments();
        
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddDbContext<IOutboxDbContext, AppDbContext>(options =>
                    options.UseNpgsql($"Host={_pgContainer.Hostname};Port={pgHostMappedPort};Username={PostgreSqlBuilder.DefaultUsername};Password={PostgreSqlBuilder.DefaultPassword};Database={PostgreSqlBuilder.DefaultDatabase}")
                );

                services.AddRabbitClient();
                services.AddOutbox();
                
                services.AddScoped<IEventDispatcher, EventDispatcher>();
                services.AddScoped<IEventHandler<PaymentEvent>, PaymentEventHandler>();

                var bindings = new List<QueueBinding>
                {
                    new() { QueueName = "payments.notifications", ExchangeName = "payments", RoutingKeys = ["payment.confirmed", "payment.rejected"] },
                    new() { QueueName = "payments.events", ExchangeName = "payments", RoutingKeys = ["payment.processing", "payment.confirmed", "payment.rejected"] },
                    new () { QueueName = "orders.analytics", ExchangeName = "orders" },
                    new () { QueueName = "orders.events", ExchangeName = "orders" }                              
                };
        
                services.AddRabbitConsumer(bindings);
            })
            .Build();

        ServiceProvider = _host.Services;
        
        await InitializeDatabase(ServiceProvider);
        await DefineRabbitTopology(ServiceProvider);
        
        await _host.StartAsync();
    }

    private static async Task InitializeDatabase(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await db.Database.EnsureCreatedAsync())
            await db.Database.MigrateAsync();
    }
    
    private void SetRabbitEnvironments()
    {
        Environment.SetEnvironmentVariable("RABBIT_USERNAME", RabbitMqBuilder.DefaultUsername);
        Environment.SetEnvironmentVariable("RABBIT_PASSWORD", RabbitMqBuilder.DefaultPassword);
        Environment.SetEnvironmentVariable("RABBIT_HOSTNAME", _rabbitContainer.Hostname);
        Environment.SetEnvironmentVariable("RABBIT_PORT", "5672");
        Environment.SetEnvironmentVariable("RABBIT_VHOST", "/");
    }

    private static async Task DefineRabbitTopology(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var topology = scope.ServiceProvider.GetRequiredService<Topology>();
        await topology.AddExchange("payments", ExchangeType.Topic);
        await topology.AddQueue("payments.notifications");
        await topology.AddQueue("payments.events");
        await topology.AddExchange("orders", ExchangeType.Fanout);
        await topology.AddQueue("orders.events");
        await topology.AddQueue("orders.analytics");
    }

    public async Task DisposeAsync()
    {
        await _pgContainer.StopAsync();
        await _rabbitContainer.StopAsync();
        await _host.StopAsync();
        _host.Dispose();
    }
}