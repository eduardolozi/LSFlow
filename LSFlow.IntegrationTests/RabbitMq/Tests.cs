using LSFlow.IntegrationTests.Events;
using LSFlow.IntegrationTests.Events.Enum;
using LSFlow.IntegrationTests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace LSFlow.IntegrationTests.RabbitMq;

[Collection("BaseFixtureCollection")]
public class Tests(BaseFixture baseFixture)
{
    private readonly AppDbContext _dbContext = baseFixture.ServiceProvider.GetRequiredService<AppDbContext>();

    [Fact]
    public async Task Test1()
    {
        var paymentProcessing = new PaymentEvent
        {
            Id = Guid.NewGuid(),
            Price = 500.5m,
            PaymentAt = DateTime.UtcNow,
            UserId = Guid.NewGuid(),
            Status = PaymentStatus.Processing
        }.ToOutboxMessage("payments", PaymentStatus.Processing.GetDescription());

        await _dbContext.OutboxMessages.AddAsync(paymentProcessing);
        await _dbContext.SaveChangesAsync();

        var result = await Policy
            .HandleResult<bool>(r => r == false)
            .WaitAndRetryAsync(
                retryCount: 50,
                sleepDurationProvider: _ => TimeSpan.FromMilliseconds(200)
            )
            .ExecuteAsync(async () =>
            {
                var isProcessed = await _dbContext.OutboxMessages
                    .Where(x => x.Id == paymentProcessing.Id)
                    .Select(x => x.IsProcessed)
                    .FirstAsync();

                var processedMessage = await _dbContext.ProcessedMessages
                    .FirstOrDefaultAsync(x => x.Id == paymentProcessing.Id);

                return isProcessed && processedMessage != null;
            });


        Assert.True(result);
    }
}