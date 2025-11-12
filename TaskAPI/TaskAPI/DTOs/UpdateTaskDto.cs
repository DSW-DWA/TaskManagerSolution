using System.ComponentModel.DataAnnotations;
using TaskStatus = TaskAPI.Models.TaskStatus;

namespace TaskAPI.DTOs
{
    public class UpdateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; }
    }
}
