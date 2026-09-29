using NetSlack.Domain.Common;

namespace NetSlack.Domain.Messages;

public sealed class Message : Entity
{
    private Message()
    {
        Body = string.Empty;
    }

    private Message(Guid id, Guid channelId, Guid authorId, string body, DateTimeOffset postedAt)
        : base(id)
    {
        ChannelId = channelId;
        AuthorId = authorId;
        Body = body;
        PostedAt = postedAt;
    }

    public Guid ChannelId { get; private set; }

    public Guid AuthorId { get; private set; }

    public string Body { get; private set; }

    public DateTimeOffset PostedAt { get; private set; }

    public DateTimeOffset? EditedAt { get; private set; }

    public static Message Post(Guid channelId, Guid authorId, string body, DateTimeOffset postedAt)
    {
        var message = new Message(
            Guid.NewGuid(),
            channelId,
            authorId,
            Guard.AgainstEmpty(body, nameof(body), 4_000),
            postedAt);

        message.AddDomainEvent(new MessagePosted(message.Id, channelId, authorId, postedAt));
        return message;
    }

    public void Edit(string body, DateTimeOffset editedAt)
    {
        Body = Guard.AgainstEmpty(body, nameof(body), 4_000);
        EditedAt = editedAt;
    }
}
