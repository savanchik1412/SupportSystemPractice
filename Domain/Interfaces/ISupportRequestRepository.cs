using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces
{
    public interface ISupportRequestRepository
    {
        Task<Ticket?> GetByIdAsync(Guid id);
        Task<IEnumerable<Ticket>> GetAllAsync(Priority? priority);
        Task AddAsync(Ticket request);
        Task SaveChangesAsync();
        Task<Dictionary<string, int>> GetAnalyticsSummaryAsync();
    }
}