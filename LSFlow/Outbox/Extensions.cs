using LSFlow.Messaging.Interfaces;
using LSFlow.Outbox.Interfaces;
using LSFlow.Outbox.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LSFlow.Outbox;

public static class Extensions
{
    public static void AddOutbox(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>();
        modelBuilder.Entity<ProcessedMessage>();
    }

    public static void AddOutbox<TDbContext, TPublisher, TEventDispatcher>(this IServiceCollection services)
        where TDbContext : DbContext, IOutboxDbContext 
        where TPublisher : class, IPublisher
        where TEventDispatcher : class, IEventDispatcher
    {
        services.AddScoped<IOutboxDbContext, TDbContext>();
        services.AddScoped<IPublisher, TPublisher>();
        services.AddScoped<IEventDispatcher, TEventDispatcher>();
        services.AddHostedService<OutboxProcessor>();
    }
}