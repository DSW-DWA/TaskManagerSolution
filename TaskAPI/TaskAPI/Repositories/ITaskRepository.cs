using TaskAPI.Models;

namespace TaskAPI.Repositories
{
    public interface ITaskRepository
    {
        Task<UserTask> AddAsync(UserTask task);
        Task<UserTask?> GetByIdAsync(Guid id);
        Task<IEnumerable<UserTask>> GetAllAsync(Models.TaskStatus? status);
        Task<UserTask?> UpdateAsync(UserTask task);
        Task<bool> DeleteAsync(Guid id);
    }
}
