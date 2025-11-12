using LogService.Models;

namespace LogService.Endpoints;

/// <summary>
/// Статический класс для настройки эндпоинтов API логирования.
/// Определяет маршруты для приема и обработки логов событий.
/// </summary>
public static class LogEndpoints
{
    /// <summary>
    /// Настраивает эндпоинты для работы с логами.
    /// Регистрирует маршруты для приема логов событий через HTTP POST запросы.
    /// Эндпоинт POST /api/logs принимает события логирования и возвращает статус Accepted (202).
    /// </summary>
    /// <param name="app">Построитель маршрутов для настройки эндпоинтов.</param>
    public static void MapLogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/logs").WithTags("Logs");

        group.MapPost("/", (LogEvent logEvent) =>
        {
            return Results.Accepted();
        });
    }
}
