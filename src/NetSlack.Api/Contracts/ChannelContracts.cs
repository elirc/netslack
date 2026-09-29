using NetSlack.Domain.Channels;

namespace NetSlack.Api.Contracts;

public sealed record CreateChannelRequest(string Name, string Topic, ChannelType Type);

