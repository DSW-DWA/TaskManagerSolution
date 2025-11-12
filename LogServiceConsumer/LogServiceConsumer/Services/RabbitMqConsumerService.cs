using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace LogServiceConsumer.Services;

/// <summary>
/// Фоновый сервис для потребления сообщений из RabbitMQ.
/// Получает логи событий из очереди и обрабатывает их с использованием OpenTelemetry трейсинга.
/// </summary>
public class RabbitMqConsumerService : BackgroundService
{
    /// <summary>
    /// Логгер для записи информации о работе сервиса.
    /// </summary>
    private readonly ILogger<RabbitMqConsumerService> _logger;
    
    /// <summary>
    /// Источник активности для создания OpenTelemetry трейсов.
    /// </summary>
    private readonly ActivitySource _activitySource;
    
    /// <summary>
    /// Подключение к RabbitMQ.
    /// </summary>
    private IConnection? _connection;
    
    /// <summary>
    /// Канал для работы с RabbitMQ.
    /// </summary>
    private IChannel? _channel;

    /// <summary>
    /// Имя обмена (exchange) для маршрутизации сообщений.
    /// </summary>
    private const string ExchangeName = "log_exchange";
    
    /// <summary>
    /// Имя очереди для получения сообщений.
    /// </summary>
    private const string QueueName = "log_consumer_queue";
    
    /// <summary>
    /// Ключ маршрутизации для привязки очереди к обмену.
    /// </summary>
    private const string RoutingKey = "log.event";

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="RabbitMqConsumerService"/>.
    /// </summary>
    /// <param name="logger">Логгер для записи информации о работе сервиса.</param>
    /// <param name="activitySource">Источник активности для создания OpenTelemetry трейсов.</param>
    public RabbitMqConsumerService(ILogger<RabbitMqConsumerService> logger, ActivitySource activitySource)
    {
        _logger = logger;
        _activitySource = activitySource;
    }

    /// <summary>
    /// Выполняет основную логику фонового сервиса.
    /// Устанавливает подключение к RabbitMQ, создает обмен и очередь, подписывается на получение сообщений.
    /// </summary>
    /// <param name="stoppingToken">Токен отмены для остановки сервиса.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        await _channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey);

        _logger.LogInformation("RabbitMQ Consumer connected. Waiting for messages...");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (ch, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var doc = JsonDocument.Parse(json);

                using var activity = _activitySource.StartActivity("ProcessRabbitMessage", ActivityKind.Consumer);

                if (doc.RootElement.TryGetProperty("Payload", out var payload) && doc.RootElement.TryGetProperty("EventType", out var EventType))
                {
                    var formattedPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                    _logger.LogInformation("Received async {EventType} event:\n{Payload}",EventType, formattedPayload);

                    activity?.SetTag("messaging.system", "rabbitmq");
                    activity?.SetTag("messaging.destination", ea.RoutingKey);
                    activity?.SetTag("messaging.message_id", ea.DeliveryTag);
                }
                else
                {
                    _logger.LogWarning("Message without Payload: {Body}", json);
                    activity?.SetStatus(ActivityStatusCode.Error, "Message without Payload");
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RabbitMQ message");
            }
        };

        await _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    /// <summary>
    /// Останавливает сервис и закрывает подключения к RabbitMQ.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены для остановки сервиса.</param>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();

        await base.StopAsync(cancellationToken);
    }
}
