using System;
using Domain.Enums;

namespace Domain.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid TicketId { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public SupportEventType EventType { get; set; }
        public Priority Priority { get; set; }
    }
}