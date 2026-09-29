using NetSlack.Domain.Users;

namespace NetSlack.Domain.Channels;

public sealed class ChannelMember
{
    private ChannelMember()
    {
    }

    internal ChannelMember(Guid channelId, Guid userId)
    {
        ChannelId = channelId;
        UserId = userId;
        JoinedAt = DateTimeOffset.UtcNow;
    }

    public Guid ChannelId { get; private set; }

    public Guid UserId { get; private set; }

    public DateTimeOffset JoinedAt { get; private set; }

    public Channel Channel { get; private set; } = null!;

    public UserProfile User { get; private set; } = null!;
}

