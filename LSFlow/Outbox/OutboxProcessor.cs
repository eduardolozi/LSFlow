using LSFlow.Messaging.Interfaces;
using LSFlow.Outbox.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LSFlow.Outbox;

public class OutboxProcessor(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<IOutboxDbContext>();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
            
            var pendingMessages = await dbContext
                .OutboxMessages
                .OrderBy(x => x.CreatedAt)
                .Where(x => x.IsProcessed == false)
                .Take(100)
                .ToListAsync(stoppingToken);

            if (pendingMessages.Count == 0)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }
            
            foreach (var message in pendingMessages) // study the options of parallelism
            {
                await publisher.Publish(message.Destination, message.Key, message);
                message.IsProcessed = true;
            }
            
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}