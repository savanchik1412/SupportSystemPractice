using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SupportRequestRepository : ISupportRequestRepository
    {
        private readonly AppDB _context;

        public SupportRequestRepository(AppDB context)
        {
            _context = context;
        }

        // Получение по ID
        public async Task<Ticket?> GetByIdAsync(Guid id)
        {
            return await _context.Ticket.FindAsync(id);
        }

        // GetAllAsync с фильтром Priority?
        public async Task<IEnumerable<Ticket>> GetAllAsync(Priority? priority)
        {
            if (priority.HasValue)
            {
                return await _context.Ticket
                    .Where(t => t.Priority == priority.Value)
                    .ToListAsync();
            }

            return await _context.Ticket.ToListAsync();
        }

        // Добавление новой записи
        public async Task AddAsync(Ticket ticketRequest)
        {
            await _context.Ticket.AddAsync(ticketRequest);
        }

        // Сохранение изменений в БД
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task<Dictionary<string, int>> GetAnalyticsSummaryAsync()
        {
            // Группируем по типу события (EventType) и считаем количество
            var summary = await _context.Ticket
                .GroupBy(t => t.EventType)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            // Также добавим общее количество записей в этот же словарь для удобства
            var total = await _context.Ticket.CountAsync();
            summary.Add("TotalCount", total);

            return summary;
        }
    }
}