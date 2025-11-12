using TaskAPI.DTOs;

namespace TaskAPI.Services
{
    public interface ITaskService
    {
        Task<TaskDto> CreateAsync(CreateTaskDto dto);
        Task<IEnumerable<TaskDto>> GetAllAsync(Models.TaskStatus? status);
        Task<TaskDto?> GetByIdAsync(Guid id);
        Task<TaskDto?> UpdateAsync(Guid id, UpdateTaskDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
