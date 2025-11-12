using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.Models;
using TaskStatus = TaskAPI.Models.TaskStatus;

namespace TaskAPI.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _db;

        public TaskRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<UserTask> AddAsync(UserTask task)
        {
            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();
            return task;
        }

        public async Task<UserTask?> GetByIdAsync(Guid id)
        {
            return await _db.Tasks.FindAsync(id);
        }

        public async Task<IEnumerable<UserTask>> GetAllAsync(TaskStatus? status)
        {
            var query = _db.Tasks.AsQueryable();
            if (status.HasValue)
                query = query.Where(t => t.Status == status);

            return await query
                .OrderByDescending(t => EF.Property<DateTime>(t, "CreatedAt"))
                .ToListAsync();
        }

        public async Task<UserTask?> UpdateAsync(UserTask task)
        {
            var existing = await _db.Tasks.FindAsync(task.Id);
            if (existing is null) return null;

            _db.Entry(existing).CurrentValues.SetValues(task);
            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _db.Tasks.FindAsync(id);
            if (entity is null) return false;

            _db.Tasks.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
