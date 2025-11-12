using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace TaskAPI.Services;

public class RabbitMqProducer : IAsyncDisposable
{
    private readonly ILogger<RabbitMqProducer> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private const string ExchangeName = "log_exchange";
    private const string RoutingKey = "log.event";

    public RabbitMqProducer(ILogger<RabbitMqProducer> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
        };

        try
        {
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            _channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true
            ).GetAwaiter().GetResult();

            _logger.LogInformation("RabbitMQ Producer connected to {Host}:{Port}", factory.HostName, factory.Port);
        }
        catch (BrokerUnreachableException ex)
        {
            _logger.LogError(ex, "Unable to connect to RabbitMQ broker");
            throw;
        }
    }

    public async Task PublishAsync(object message, string eventType)
    {
        var json = JsonSerializer.Serialize(new
        {
            EventType = eventType,
            OccurredAt = DateTime.UtcNow,
            Source = "TaskAPI",
            Payload = message
        });

        var body = Encoding.UTF8.GetBytes(json);

        try
        {
            await _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                mandatory: false,
                body: body
            );

            _logger.LogInformation("Published message to RabbitMQ: {EventType}", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to RabbitMQ");
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_channel.IsOpen)
                await _channel.CloseAsync();

            if (_connection.IsOpen)
                await _connection.CloseAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing RabbitMQ connection/channel");
        }
    }
}
