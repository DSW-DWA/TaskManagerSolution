using TaskAPI.DTOs;
using TaskAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace TaskAPI.Mappings
{
    public static class TaskMappings
    {
        public static TaskDto ToDto(this UserTask entity, DbContext db)
        {
            var createdAt = (DateTime)db.Entry(entity).Property("CreatedAt").CurrentValue!;
            var updatedAt = (DateTime)db.Entry(entity).Property("UpdatedAt").CurrentValue!;
            return new TaskDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                Status = entity.Status,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };
        }
    }
}
