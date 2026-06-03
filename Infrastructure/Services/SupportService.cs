using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class SupportService : ISupportService
    {
        // Оставляем поля строго в одном экземпляре
        private readonly ISupportRequestRepository _repository;
        private readonly IMemoryCache _cache;

        // Один объединенный конструктор, принимающий и репозиторий, и кэш
        public SupportService(ISupportRequestRepository repository, IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
        }


        public async Task<Ticket> CreateEventAsync(Guid ticketId, string operatorName, string message, SupportEventType eventType, Priority priority)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Сообщение не может быть пустым.");
            }

            var newEvent = new Ticket
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                OperatorName = operatorName,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                EventType = eventType,
                Priority = priority
            };

            await _repository.AddAsync(newEvent);
            await _repository.SaveChangesAsync();

            return newEvent;
        }

        public async Task<IEnumerable<Ticket>> GetEventsAsync(Priority? priority)
        {
            return await _repository.GetAllAsync(priority);
        }

        public async Task<Ticket?> GetEventByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }


        public async Task<Dictionary<string, int>> GetCachedSummaryAsync()
        {
            const string cacheKey = "analytics_summary";

            if (!_cache.TryGetValue(cacheKey, out Dictionary<string, int>? summary))
            {
                summary = await _repository.GetAnalyticsSummaryAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, summary, cacheOptions);
            }

            return summary!;
        }
    }
}