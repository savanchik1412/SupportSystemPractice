namespace Domain.Enums
{
    public enum SupportEventType
    {
        TicketCreated,   // Когда наш пользователь создает обращение 
        CommentAdded,    // Когда собственно добавил комментарий к тикету
        StatusChanged    // Если статус вдруг был изменен
    }
}