namespace TaskAPI.Services
{
    /// <summary>
    /// Клиент для отправки событий логирования в LogService через HTTP.
    /// Отправляет события о действиях в системе в сервис логирования.
    /// </summary>
    public class LogClient
    {
        /// <summary>
        /// HTTP клиент для отправки запросов в LogService.
        /// </summary>
        private readonly HttpClient _httpClient;
        
        /// <summary>
        /// Логгер для записи информации о работе клиента.
        /// </summary>
        private readonly ILogger<LogClient> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="LogClient"/>.
        /// </summary>
        /// <param name="httpClient">HTTP клиент для отправки запросов в LogService.</param>
        /// <param name="logger">Логгер для записи информации о работе клиента.</param>
        public LogClient(HttpClient httpClient, ILogger<LogClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Отправляет событие логирования в LogService.
        /// Создает объект события с указанным типом и полезной нагрузкой и отправляет его через HTTP POST.
        /// </summary>
        /// <param name="eventType">Тип события (например, "TaskCreated", "TaskUpdated").</param>
        /// <param name="payload">Полезная нагрузка события, содержащая детальную информацию о событии.</param>
        public async Task SendEventAsync(string eventType, object payload)
        {
            var logEvent = new
            {
                EventType = eventType,
                OccurredAt = DateTime.UtcNow,
                Source = "TaskAPI",
                Payload = payload
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/logs", logEvent);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to send log event: {Status}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending log event to LogService");
            }
        }
    }
}
