namespace LSFlow.Messaging.Rabbit.Models;

// Needs to study more about this
public class QueueArguments
{
    public int MessageExpirationMilliseconds { get; set; }
    
    public Dictionary<string, object?> ConvertToDictionary()
    {
        return new Dictionary<string, object?>
        {
            { "x-message-ttl", MessageExpirationMilliseconds }
        };
    }
}