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
            
            try
            {
                await publisher.Publish(pendingMessages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error at publishing the message batch: {ex.Message}");
            }
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}