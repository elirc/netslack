using NetSlack.Domain.Common;

namespace NetSlack.Domain.Channels;

public sealed class Channel : Entity
{
    private readonly List<ChannelMember> _members = [];

    private Channel()
    {
        Name = string.Empty;
        Topic = string.Empty;
    }

    private Channel(Guid id, Guid workspaceId, string name, string topic, ChannelType type, Guid createdById)
        : base(id)
    {
        WorkspaceId = workspaceId;
        Name = name;
        Topic = topic;
        Type = type;
        CreatedById = createdById;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid WorkspaceId { get; private set; }

    public string Name { get; private set; }

    public string Topic { get; private set; }

    public ChannelType Type { get; private set; }

    public Guid CreatedById { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<ChannelMember> Members => _members.AsReadOnly();

    public static Channel Create(Guid workspaceId, string name, string topic, ChannelType type, Guid createdById)
    {
        var channel = new Channel(
            Guid.NewGuid(),
            workspaceId,
            NormalizeName(name),
            topic.Trim(),
            type,
            createdById);

        channel.Join(createdById);
        return channel;
    }

    public void Join(Guid userId)
    {
        if (_members.Any(member => member.UserId == userId))
        {
            return;
        }

        _members.Add(new ChannelMember(Id, userId));
    }

    public void UpdateDetails(string name, string topic)
    {
        Name = NormalizeName(name);
        Topic = topic.Trim();
    }

    private static string NormalizeName(string name)
    {
        var normalized = Guard.AgainstEmpty(name, nameof(name), 80).ToLowerInvariant().Replace(' ', '-');
        if (normalized.Any(character => !char.IsLetterOrDigit(character) && character != '-' && character != '_'))
        {
            throw new ArgumentException("Channel names can only contain letters, numbers, dashes, and underscores.", nameof(name));
        }

        return normalized;
    }
}
