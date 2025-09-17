using RabbitMQ.Client;

namespace LSFlow.Messaging.Rabbit.Configurations;

public class Client(string username, string password, string hostname, int port, string virtualHost)
{
    private readonly ConnectionFactory _connFactory = new()
    {
        UserName = username,
        Password = password,
        HostName = hostname,
        Port = port,
        VirtualHost = virtualHost
    };

    private IConnection? _connection;

    private async Task<IConnection?> GetConnection()
    {
        if (_connection is not { IsOpen: true })
        {
            _connection = await _connFactory.CreateConnectionAsync();
        }
        
        return _connection;
    }

    public async Task<IChannel> GetChannel()
    {
        var connection = await GetConnection();
        return await connection!.CreateChannelAsync();
    }
    
    public async Task<IChannel> GetChannelWithPublisherConfirmation()
    {
        var connection = await GetConnection();
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true
        );
        return await connection!.CreateChannelAsync();
    }
    
}