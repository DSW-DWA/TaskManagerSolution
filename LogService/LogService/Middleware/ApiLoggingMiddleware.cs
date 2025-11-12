using System.Text.Json;
using LogService.Models;

namespace LogService.Middleware;

/// <summary>
/// Middleware для логирования входящих API запросов.
/// Перехватывает запросы к API эндпоинтам, десериализует тело запроса и логирует информацию о событиях.
/// </summary>
public class ApiLoggingMiddleware
{
    /// <summary>
    /// Следующий компонент в цепочке middleware.
    /// </summary>
    private readonly RequestDelegate _next;
    
    /// <summary>
    /// Логгер для записи информации о запросах.
    /// </summary>
    private readonly ILogger<ApiLoggingMiddleware> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ApiLoggingMiddleware"/>.
    /// </summary>
    /// <param name="next">Следующий компонент в цепочке middleware.</param>
    /// <param name="logger">Логгер для записи информации о запросах.</param>
    public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Обрабатывает HTTP запрос, извлекает и логирует информацию о событиях из тела запроса.
    /// </summary>
    /// <param name="context">Контекст HTTP запроса.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            context.Request.Body.Position = 0;

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    var logEvent = JsonSerializer.Deserialize<LogEvent>(body, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (logEvent?.Payload is not null)
                    {
                        var payloadJson = JsonSerializer.Serialize(
                            logEvent.Payload,
                            new JsonSerializerOptions { WriteIndented = true });

                        _logger.LogInformation(
                            "Received API event {EventType} from {Source}:\n{Payload}",
                            logEvent.EventType,
                            logEvent.Source,
                            payloadJson);
                    }
                    else
                    {
                        _logger.LogWarning("Received API event without payload. Body: {Body}", body);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize request payload: {Body}", body);
                }
            }
        }

        await _next(context);
    }
}
