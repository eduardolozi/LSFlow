using LSFlow.Outbox.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LSFlow.Outbox.Interfaces;

public interface IOutboxDbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}