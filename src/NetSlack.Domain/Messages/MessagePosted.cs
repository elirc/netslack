using NetSlack.Domain.Common;

namespace NetSlack.Domain.Messages;

public sealed record MessagePosted(Guid MessageId, Guid ChannelId, Guid AuthorId, DateTimeOffset OccurredAt) : IDomainEvent;

