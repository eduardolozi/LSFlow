namespace LSFlow.Messaging.Rabbit.Models;

public static class ExchangeType
{
    public const string Direct = RabbitMQ.Client.ExchangeType.Direct;
    public const string Fanout = RabbitMQ.Client.ExchangeType.Fanout;
    public const string Headers = RabbitMQ.Client.ExchangeType.Headers;
    public const string Topic = RabbitMQ.Client.ExchangeType.Topic;

    private static readonly string[] ValidTypes = [Direct, Fanout, Headers, Topic];
    public static bool Exists(string exchangeType) => !string.IsNullOrWhiteSpace(exchangeType) && ValidTypes.Contains(exchangeType);
}