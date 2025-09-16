using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using LSFlow.Messaging.Rabbit;
using LSFlow.Messaging.Rabbit.Configurations;
using LSFlow.Messaging.Rabbit.Models;
using LSFlow.Outbox;
using LSFlow.Outbox.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace LSFlow.IntegrationTests;

public class BaseFixture : IAsyncLifetime
{
    public ServiceProvider ServiceProvider { get; set; }
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
        Services.AddDbContext<IOutboxDbContext, AppDbContext>(options =>
            options.UseNpgsql($"Host={_pgContainer.Hostname};Port={pgHostMappedPort};Username={PostgreSqlBuilder.DefaultUsername};Password={PostgreSqlBuilder.DefaultPassword};Database={PostgreSqlBuilder.DefaultDatabase}")
        );
        
        _rabbitContainer = new RabbitMqBuilder()
            .WithUsername(RabbitMqBuilder.DefaultUsername)
            .WithPassword(RabbitMqBuilder.DefaultPassword)
            .WithPortBinding(0, RabbitMqBuilder.RabbitMqPort)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(RabbitMqBuilder.RabbitMqPort))
            .Build();
        
        await _rabbitContainer.StartAsync();
        var rabbitHostMappedPort = _rabbitContainer.GetMappedPublicPort(RabbitMqBuilder.RabbitMqPort);

        SetRabbitEnvironments(rabbitHostMappedPort);
        
        Services.AddRabbitClient();
        Services.AddOutbox<AppDbContext, RabbitPublisher, EventDispatcher>();
        
        var bindings = new List<QueueBinding>
        {
            new() { QueueName = "payment.notifications", ExchangeName = "payments", RoutingKeys = ["payment.confirmed", "payment.rejected"] },
            new() { QueueName = "payment.events", ExchangeName = "payments", RoutingKeys = ["payment.processing", "payment.confirmed", "payment.rejected"] },
            new () { QueueName = "orders.analytics", ExchangeName = "orders" },
            new () { QueueName = "orders.events", ExchangeName = "orders" }                              
        };
        
        Services.AddRabbitConsumer<AppDbContext, EventDispatcher>(bindings);
        
        ServiceProvider = Services.BuildServiceProvider();

        await InitializeDatabase(ServiceProvider);
        await DefineRabbitTopology(ServiceProvider);
    }

    private static async Task InitializeDatabase(ServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetRequiredService<AppDbContext>();
        
        if (await db.Database.EnsureCreatedAsync())
            await db.Database.MigrateAsync();
    }
    
    private void SetRabbitEnvironments(ushort hostPort)
    {
        Environment.SetEnvironmentVariable("RABBIT_USERNAME", RabbitMqBuilder.DefaultUsername);
        Environment.SetEnvironmentVariable("RABBIT_PASSWORD", RabbitMqBuilder.DefaultPassword);
        Environment.SetEnvironmentVariable("RABBIT_HOSTNAME", _rabbitContainer.Hostname);
        Environment.SetEnvironmentVariable("RABBIT_PORT", $"{hostPort}");
        Environment.SetEnvironmentVariable("RABBIT_VHOST", "/");
    }

    private static async Task DefineRabbitTopology(ServiceProvider serviceProvider)
    {
        var topology = serviceProvider.GetRequiredService<Topology>();
        
        await topology.AddExchange("payment", ExchangeType.Topic);
        await topology.AddQueue("payment.notifications");
        await topology.AddQueue("payment.events");

        await topology.AddExchange("orders", ExchangeType.Fanout);
        await topology.AddQueue("orders.events");
        await topology.AddQueue("orders.analytics");
    }

    public async Task DisposeAsync()
    {
        await _pgContainer.StopAsync();
        await _rabbitContainer.StopAsync();
        await ServiceProvider.DisposeAsync();
    }
}