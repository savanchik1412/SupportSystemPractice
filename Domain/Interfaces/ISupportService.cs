using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public interface ISupportService
    {
        Task<Ticket> CreateEventAsync(Guid ticketId, string operatorName, string message, SupportEventType eventType, Priority priority);
        Task<IEnumerable<Ticket>> GetEventsAsync(Priority? priority);
        Task<Ticket?> GetEventByIdAsync(Guid id);
        Task<Dictionary<string, int>> GetCachedSummaryAsync();
    }
}