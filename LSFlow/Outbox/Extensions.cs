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

    public static void AddOutbox(this IServiceCollection services)
    {
        services.AddHostedService<OutboxProcessor>();
    }
}