using LSFlow.IntegrationTests.Models;
using LSFlow.Outbox;
using LSFlow.Outbox.Interfaces;
using LSFlow.Outbox.Models;
using Microsoft.EntityFrameworkCore;

namespace LSFlow.IntegrationTests;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IOutboxDbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddOutbox();
    }
}