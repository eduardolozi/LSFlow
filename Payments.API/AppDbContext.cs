using LSFlow.Outbox;
using LSFlow.Outbox.Interfaces;
using LSFlow.Outbox.Models;
using Microsoft.EntityFrameworkCore;
using Payments.API.Models;

namespace Payments.API;

public class AppDbContext : DbContext, IOutboxDbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddOutbox();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql($"Host=localhost;Port=5433;Username=postgres;Password=postgres;Database=postgres");
    }
}