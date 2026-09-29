using NetSlack.Domain.Channels;

namespace NetSlack.Application.Channels;

public sealed record ChannelDto(Guid Id, Guid WorkspaceId, string Name, string Topic, ChannelType Type, int MemberCount);

public sealed record CreateChannelCommand(string Name, string Topic, ChannelType Type);

