namespace LSFlow.Outbox;

public class ProcessedMessage
{
    public Guid Id { get; set; }
    public string EventType { get; set; }
    public DateTime ProcessedAt { get; set; }
}