namespace NetSlack.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}

