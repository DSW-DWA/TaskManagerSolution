using TaskAPI.DTOs;
using TaskAPI.Mappings;
using TaskAPI.Models;
using TaskAPI.Repositories;
using TaskAPI.Data;
using TaskStatus = TaskAPI.Models.TaskStatus;

namespace TaskAPI.Services;

/// <summary>
/// Сервис для управления задачами.
/// Предоставляет методы для создания, получения, обновления и удаления задач.
/// Автоматически отправляет события о действиях с задачами в LogService и RabbitMQ.
/// </summary>
public class TaskService : ITaskService
{
    /// <summary>
    /// Репозиторий для работы с данными задач.
    /// </summary>
    private readonly ITaskRepository _repository;
    
    /// <summary>
    /// Контекст базы данных для работы с данными.
    /// </summary>
    private readonly AppDbContext _db;
    
    /// <summary>
    /// Клиент для отправки событий в LogService через HTTP.
    /// </summary>
    private readonly LogClient _logClient;
    
    /// <summary>
    /// Продюсер для публикации событий в RabbitMQ.
    /// </summary>
    private readonly RabbitMqProducer _mqProducer;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TaskService"/>.
    /// </summary>
    /// <param name="repository">Репозиторий для работы с данными задач.</param>
    /// <param name="db">Контекст базы данных для работы с данными.</param>
    /// <param name="logClient">Клиент для отправки событий в LogService через HTTP.</param>
    /// <param name="mqProducer">Продюсер для публикации событий в RabbitMQ.</param>
    public TaskService(
        ITaskRepository repository,
        AppDbContext db,
        LogClient logClient,
        RabbitMqProducer mqProducer)
    {
        _repository = repository;
        _db = db;
        _logClient = logClient;
        _mqProducer = mqProducer;
    }

    /// <summary>
    /// Создает новую задачу.
    /// </summary>
    /// <param name="dto">DTO с данными для создания задачи.</param>
    /// <returns>DTO созданной задачи.</returns>
    public async Task<TaskDto> CreateAsync(CreateTaskDto dto)
    {
        var entity = new UserTask
        {
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status
        };

        await _repository.AddAsync(entity);
        var result = entity.ToDto(_db);

        await _logClient.SendEventAsync("TaskCreated", result);
        await _mqProducer.PublishAsync(result, "TaskCreated");

        return result;
    }

    /// <summary>
    /// Получает список всех задач, опционально отфильтрованных по статусу.
    /// </summary>
    /// <param name="status">Статус для фильтрации задач. Если null, возвращаются все задачи.</param>
    /// <returns>Список DTO задач.</returns>
    public async Task<IEnumerable<TaskDto>> GetAllAsync(TaskStatus? status)
    {
        var tasks = await _repository.GetAllAsync(status);
        return tasks.Select(t => t.ToDto(_db));
    }

    /// <summary>
    /// Получает задачу по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор задачи.</param>
    /// <returns>DTO задачи, если задача найдена; иначе null.</returns>
    public async Task<TaskDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity?.ToDto(_db);
    }

    /// <summary>
    /// Обновляет существующую задачу.
    /// </summary>
    /// <param name="id">Идентификатор задачи для обновления.</param>
    /// <param name="dto">DTO с новыми данными задачи.</param>
    /// <returns>DTO обновленной задачи, если задача найдена; иначе null.</returns>
    public async Task<TaskDto?> UpdateAsync(Guid id, UpdateTaskDto dto)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        existing.Title = dto.Title;
        existing.Description = dto.Description;
        existing.Status = dto.Status;

        await _repository.UpdateAsync(existing);
        var result = existing.ToDto(_db);

        await _logClient.SendEventAsync("TaskUpdated", result);
        await _mqProducer.PublishAsync(result, "TaskUpdated");

        return result;
    }

    /// <summary>
    /// Удаляет задачу по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор задачи для удаления.</param>
    /// <returns>true, если задача была удалена; иначе false.</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
        {
            await _logClient.SendEventAsync("TaskDeleted", new { TaskId = id });
            await _mqProducer.PublishAsync(new { TaskId = id }, "TaskDeleted");
        }

        return deleted;
    }
}
