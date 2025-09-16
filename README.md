### **LSFlow.Messaging**

**LSFlow.Messaging** is a robust and complete messaging library for .NET, focusing on simplicity and reliability. It's designed to handle asynchronous communication between services, ensuring message delivery through the **Outbox** pattern and simplifying the configuration of topologies and event consumption.

-----

### **Installation and Setup**

To use the library, add the appropriate NuGet packages to your project:

```bash
# Main package for RabbitMQ
dotnet add package LSFlow.Messaging.Rabbit

# Package for the Outbox pattern
dotnet add package LSFlow.Outbox

# Package for Entity Framework Core with Npgsql (or another provider)
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

-----

### **Client and Topology Configuration (Publisher)**

Configuring the RabbitMQ client and topology is done directly in the `Program.cs` file. You need to set environment variables and use the extension methods to configure the services.

```csharp
// 1. Set environment variables
// They must be accessible to the application (e.g., .env, Dockerfile)
Environment.SetEnvironmentVariable("RABBIT_USERNAME", "rabbitmq");
Environment.SetEnvironmentVariable("RABBIT_PASSWORD", "rabbitmq");
Environment.SetEnvironmentVariable("RABBIT_HOSTNAME", "localhost");
Environment.SetEnvironmentVariable("RABBIT_PORT", "5672");
Environment.SetEnvironmentVariable("RABBIT_VHOST", "/");

// 2. Add RabbitMQ services
builder.Services.AddRabbitClient();

// 3. Create and configure the topology (exchanges, queues)
var scope = app.Services.CreateScope();
var topology = scope.ServiceProvider.GetService<Topology>() ?? throw new NullReferenceException("Error occurred during topology creation.");
await topology.AddExchange("payments", ExchangeType.Topic);
await topology.AddQueue("payments.events");
await topology.AddQueue("payments.notifications");
```

-----

### **Outbox and DbContext Configuration**

The **Outbox** pattern ensures that messages are saved to the database and published to the queue atomically. For this, your application's `DbContext` must implement the `IOutboxDbContext` interface.

```csharp
// In your DbContext (e.g., AppDbContext.cs)
public class AppDbContext : DbContext, IOutboxDbContext
{
    // Add DbSets for the Outbox
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Use the extension method to configure the Outbox
        modelBuilder.AddOutbox();
    }

    // Configure your database (e.g., PostgreSQL)
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql($"Host=localhost;Port=5433;Username=postgres;Password=postgres;Database=postgres");
    }
}
```

To register the Outbox and `DbContext` in `Program.cs`, use the following extension methods:

```csharp
builder.Services.AddDbContext<IOutboxDbContext, AppDbContext>();
builder.Services.AddOutbox();
```

-----

### **RabbitConsumer Configuration (Consumer)**

To consume messages, you need to configure the `RabbitConsumer` and the **bindings** that define which queues your application will listen to.

```csharp
// 1. Define bindings for the queues the application will consume
var bindings = new List<QueueBinding>
{
    new() { ExchangeName = "orders", QueueName = "orders.events" }
};

// 2. Add the RabbitConsumer
builder.Services.AddRabbitConsumer(bindings);
```

-----

### **Creating and Configuring the EventDispatcher**

The `EventDispatcher` is responsible for publishing messages. It's a concrete class that implements the `IEventDispatcher` interface and must be injected in `Program.cs`.

```csharp
// In Program.cs, register the service
builder.Services.AddScoped<IEventDispatcher, EventDispatcher>();
```

-----

### **Creating and Configuring Event Handlers**

To process incoming messages, create concrete classes that implement `IEventHandler<T>`, where `T` is the message (event) type. Then, register them in `Program.cs`.

```csharp
// Example of an Event Handler
public class OrderCreatedHandler : IEventHandler<OrderCreated>
{
    public Task Handle(OrderCreated message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Received OrderCreated event: {message.OrderId}");
        // Your business logic goes here...
        return Task.CompletedTask;
    }
}

// In Program.cs, register the handler
builder.Services.AddScoped<IEventHandler<OrderCreated>, OrderCreatedHandler>();
```

-----

### **Upcoming Features**

  - **Kafka Integration:** We plan to add a client for Kafka, offering an alternative to RabbitMQ.
  - **Redis for Event Caching:** Functionality to use Redis as a cache for events will be implemented, optimizing the processing of duplicate messages.
