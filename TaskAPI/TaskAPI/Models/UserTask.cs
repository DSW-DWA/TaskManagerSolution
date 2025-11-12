namespace TaskAPI.Models
{
    public class UserTask
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; } = TaskStatus.New;
    }

    public enum TaskStatus
    {
        New,
        InProgress,
        Done
    }
}
