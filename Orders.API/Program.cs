using LSFlow.Messaging.Rabbit;
using LSFlow.Messaging.Rabbit.Configurations;
using LSFlow.Messaging.Rabbit.Models;
using LSFlow.Outbox;
using LSFlow.Outbox.Interfaces;
using Orders.API;

var builder = WebApplication.CreateBuilder(args);

Environment.SetEnvironmentVariable("RABBIT_USERNAME", "rabbitmq");
Environment.SetEnvironmentVariable("RABBIT_PASSWORD", "rabbitmq");
Environment.SetEnvironmentVariable("RABBIT_HOSTNAME", "localhost");
Environment.SetEnvironmentVariable("RABBIT_PORT", "5672");
Environment.SetEnvironmentVariable("RABBIT_VHOST", "/");

builder.Services.AddDbContext<IOutboxDbContext, AppDbContext>();

builder.Services.AddRabbitClient();
builder.Services.AddOutbox();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var scope = app.Services.CreateScope();
var topology = scope.ServiceProvider.GetService<Topology>() ?? throw new NullReferenceException("Error occurred during topology creation");
await topology.AddExchange("orders", ExchangeType.Fanout);
await topology.AddQueue("orders.events");
await topology.AddQueue("orders.analytics");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();