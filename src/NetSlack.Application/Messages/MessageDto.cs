namespace NetSlack.Application.Messages;

public sealed record MessageAuthorDto(Guid Id, string DisplayName, string Email, string AvatarUrl);

public sealed record MessageDto(
    Guid Id,
    Guid ChannelId,
    string Body,
    DateTimeOffset PostedAt,
    DateTimeOffset? EditedAt,
    MessageAuthorDto Author);

public sealed record PostMessageCommand(string Body);

