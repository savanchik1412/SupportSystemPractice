using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupportEventsController : ControllerBase
    {
        private readonly ISupportService _supportService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SupportEventsController> _logger;

        private const string EventsCacheKey = "SupportEvents_CacheKey";

        public SupportEventsController(ISupportService supportService, IMemoryCache cache, ILogger<SupportEventsController> logger)
        {
            _supportService = supportService;
            _cache = cache;
            _logger = logger;
        }

        // Получение списка с кэшированием и логированием
        [HttpGet]
        [Authorize(Roles = "Admin,Analyst,Operator")]
        public async Task<IActionResult> GetAll([FromQuery] Priority? priority)
        {
            _logger.LogInformation("Поступил запрос на получение списка событий. Фильтр приоритета: {Priority}", priority);

            if (priority.HasValue)
            {
                var filteredEvents = await _supportService.GetEventsAsync(priority);
                return Ok(filteredEvents);
            }

            if (!_cache.TryGetValue(EventsCacheKey, out IEnumerable<Ticket>? cachedEvents))
            {
                _logger.LogWarning("Кэш пуст. Запрашиваем данные из базы данных SQLite...");

                cachedEvents = await _supportService.GetEventsAsync(null);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(EventsCacheKey, cachedEvents, cacheEntryOptions);
            }
            else
            {
                _logger.LogInformation("Данные успешно извлечены из кэша без обращения к БД!");
            }

            return Ok(cachedEvents);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Analyst,Operator")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var supportEvent = await _supportService.GetEventByIdAsync(id);
            if (supportEvent == null)
            {
                return NotFound(new { message = "Событие не найдено" });
            }
            return Ok(supportEvent);
        }

        // Генерация аналитических CSV-отчетов для Аналитиков и Админов
        [HttpGet("export/csv")]
        [Authorize(Roles = "Admin,Analyst")]
        public async Task<IActionResult> ExportToCsv()
        {
            _logger.LogInformation("Аналитик запросил выгрузку всех событий в формате CSV.");

            var events = await _supportService.GetEventsAsync(null);
            var csvBuilder = new System.Text.StringBuilder();

            csvBuilder.AppendLine("Id;TicketId;OperatorName;Message;CreatedAt;EventType;Priority");

            foreach (var item in events)
            {
                csvBuilder.AppendLine($"{item.Id};{item.TicketId};{item.OperatorName};{item.Message};{item.CreatedAt:yyyy-MM-dd HH:mm:ss};{item.EventType};{item.Priority}");
            }

            var buffer = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();

            _logger.LogInformation("CSV-отчет успешно сформирован. Размер файла: {Size} байт.", buffer.Length);

            return File(buffer, "text/csv", "support_report.csv");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Operator")]
        public async Task<IActionResult> Create([FromBody] CreateSupportEventDto dto)
        {
            _logger.LogInformation("Оператор {Operator} создает новое событие", dto.OperatorName);

            try
            {
                var newEvent = await _supportService.CreateEventAsync(
                    dto.TicketId,
                    dto.OperatorName,
                    dto.Message,
                    dto.EventType,
                    dto.Priority
                );

                _cache.Remove(EventsCacheKey);
                _logger.LogInformation("Кэш очищен в связи с добавлением новой записи.");

                return CreatedAtAction(nameof(GetById), new { id = newEvent.Id }, newEvent);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Ошибка валидации: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
        }

        // Метод аналитики (теперь он находится строго внутри класса контроллера)
        [HttpGet("analytics/summary")]
        [Authorize(Roles = "Admin,Analyst")]
        public async Task<IActionResult> GetSummary()
        {
            var analytics = await _supportService.GetCachedSummaryAsync();
            return Ok(analytics);
        }
    }

    // Вспомогательный класс DTO для создания события
    public class CreateSupportEventDto
    {
        public Guid TicketId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public SupportEventType EventType { get; set; }
        public Priority Priority { get; set; }
    }
}