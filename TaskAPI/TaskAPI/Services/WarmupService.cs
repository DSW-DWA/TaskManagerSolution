using TaskAPI.Services;

namespace TaskAPI.Background;

public class WarmupService : IHostedService
{
    private readonly RabbitMqProducer _mqProducer;
    private readonly LogClient _logClient;
    private readonly ILogger<WarmupService> _logger;

    public WarmupService(RabbitMqProducer mqProducer, LogClient logClient, ILogger<WarmupService> logger)
    {
        _mqProducer = mqProducer;
        _logClient = logClient;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting warm-up for external services...");

        _ = Task.Run(async () =>
        {
            await WarmupRabbitMqAsync(cancellationToken);
            await WarmupLogServiceAsync(cancellationToken);
        }, cancellationToken);

        return Task.CompletedTask;
    }

    private async Task WarmupRabbitMqAsync(CancellationToken ct)
    {
        try
        {
            await _mqProducer.PublishAsync(new { message = "warmup" }, "Warmup");
            _logger.LogInformation("RabbitMQ warmed up successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RabbitMQ warm-up failed (will reconnect automatically later)");
        }
    }

    private async Task WarmupLogServiceAsync(CancellationToken ct)
    {
        try
        {
            await _logClient.SendEventAsync("Warmup", new { message = "warmup" });
            _logger.LogInformation("LogService warmed up successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠LogService warm-up failed (will retry on first request)");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WarmupService stopped");
        return Task.CompletedTask;
    }
}
