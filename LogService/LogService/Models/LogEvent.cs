namespace LogService.Models
{
    /// <summary>
    /// Представляет событие логирования, содержащее информацию о событии в системе.
    /// </summary>
    public class LogEvent
    {
        /// <summary>
        /// Уникальный идентификатор события.
        /// </summary>
        public Guid EventId { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// Тип события (например, "TaskCreated", "TaskUpdated").
        /// </summary>
        public string EventType { get; set; } = default!;
        
        /// <summary>
        /// Дата и время возникновения события в формате UTC.
        /// </summary>
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Источник события (например, "TaskAPI").
        /// </summary>
        public string Source { get; set; } = default!;
        
        /// <summary>
        /// Полезная нагрузка события, содержащая детальную информацию о событии.
        /// </summary>
        public object Payload { get; set; } = default!;
    }
}
