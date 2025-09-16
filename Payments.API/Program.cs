using LSFlow.Messaging.Interfaces;
using LSFlow.Messaging.Rabbit;
using LSFlow.Messaging.Rabbit.Configurations;
using LSFlow.Messaging.Rabbit.Models;
using LSFlow.Outbox;
using LSFlow.Outbox.Interfaces;
using Payments.API;
using Payments.API.Events.Input;
using Payments.API.UseCases;

var builder = WebApplication.CreateBuilder(args);

Environment.SetEnvironmentVariable("RABBIT_USERNAME", "rabbitmq");
Environment.SetEnvironmentVariable("RABBIT_PASSWORD", "rabbitmq");
Environment.SetEnvironmentVariable("RABBIT_HOSTNAME", "localhost");
Environment.SetEnvironmentVariable("RABBIT_PORT", "5672");
Environment.SetEnvironmentVariable("RABBIT_VHOST", "/");

builder.Services.AddDbContext<IOutboxDbContext, AppDbContext>();

builder.Services.AddRabbitClient();
builder.Services.AddOutbox();

builder.Services.AddScoped<IEventDispatcher, EventDispatcher>();
builder.Services.AddScoped<IEventHandler<OrderCreated>, OrderCreatedHandler>();

var bindings = new List<QueueBinding>
{
    new() { ExchangeName = "orders", QueueName = "orders.events" }                          
};
builder.Services.AddRabbitConsumer(bindings);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var scope = app.Services.CreateScope();
var topology = scope.ServiceProvider.GetService<Topology>() ?? throw new NullReferenceException("Error occurred during topology creation");
await topology.AddExchange("payments", ExchangeType.Topic);
await topology.AddQueue("payments.events");
await topology.AddQueue("payments.notifications");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();