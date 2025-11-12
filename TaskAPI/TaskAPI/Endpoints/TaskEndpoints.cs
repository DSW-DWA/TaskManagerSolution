using FluentValidation;
using System;
using TaskAPI.DTOs;
using TaskAPI.Services;
using TaskStatus = TaskAPI.Models.TaskStatus;

namespace TaskAPI.Endpoints;

/// <summary>
/// Статический класс для настройки эндпоинтов API задач.
/// Определяет маршруты для создания, получения, обновления и удаления задач.
/// </summary>
public static class TaskEndpoints
{
    /// <summary>
    /// Настраивает эндпоинты для работы с задачами.
    /// Регистрирует следующие маршруты:
    /// - POST /api/tasks - создает новую задачу (валидирует входные данные)
    /// - GET /api/tasks - получает список всех задач (с опциональной фильтрацией по статусу)
    /// - GET /api/tasks/{id} - получает задачу по идентификатору
    /// - PUT /api/tasks/{id} - обновляет существующую задачу (валидирует входные данные)
    /// - DELETE /api/tasks/{id} - удаляет задачу по идентификатору
    /// </summary>
    /// <param name="app">Построитель маршрутов для настройки эндпоинтов.</param>
    public static void MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks");

        group.MapPost("/", async (
            ITaskService service,
            IValidator<CreateTaskDto> validator,
            CreateTaskDto dto) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            var created = await service.CreateAsync(dto);
            return Results.Created($"/api/tasks/{created.Id}", created);
        });

        group.MapGet("/", async (ITaskService service, TaskStatus? status) =>
        {
            if (status != null && Enum.IsDefined(typeof(TaskStatus), status) == false)
            {
                return Results.BadRequest();
            }
            var tasks = await service.GetAllAsync(status);
            return Results.Ok(tasks);
        });

        group.MapGet("/{id:guid}", async (ITaskService service, Guid id) =>
        {
            var task = await service.GetByIdAsync(id);
            return task is null ? Results.NotFound(new { message = "Task not found" }) : Results.Ok(task);
        });

        group.MapPut("/{id:guid}", async (
            ITaskService service,
            IValidator<UpdateTaskDto> validator,
            Guid id,
            UpdateTaskDto dto) =>
        {
            var validationResult = await validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return Results.ValidationProblem(validationResult.ToDictionary());

            var updated = await service.UpdateAsync(id, dto);
            return updated is null
                ? Results.NotFound(new { message = "Task not found" })
                : Results.Ok(updated);
        });

        group.MapDelete("/{id:guid}", async (ITaskService service, Guid id) =>
        {
            var deleted = await service.DeleteAsync(id);
            return deleted
                ? Results.NoContent()
                : Results.NotFound(new { message = "Task not found" });
        });
    }
}
