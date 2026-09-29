using Microsoft.EntityFrameworkCore;
using NetSlack.Application.Common;
using NetSlack.Domain.Channels;

namespace NetSlack.Application.Channels;

public sealed class ChannelService(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<ChannelDto>> GetForWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        await EnsureWorkspaceMemberAsync(workspaceId, cancellationToken);

        return await db.Channels
            .AsNoTracking()
            .Where(channel => channel.WorkspaceId == workspaceId)
            .Where(channel => channel.Type == ChannelType.Public || channel.Members.Any(member => member.UserId == currentUser.UserId))
            .OrderBy(channel => channel.Type)
            .ThenBy(channel => channel.Name)
            .Select(channel => new ChannelDto(
                channel.Id,
                channel.WorkspaceId,
                channel.Name,
                channel.Topic,
                channel.Type,
                channel.Members.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<ChannelDto> CreateAsync(Guid workspaceId, CreateChannelCommand command, CancellationToken cancellationToken)
    {
        await EnsureWorkspaceMemberAsync(workspaceId, cancellationToken);

        var normalizedName = command.Name.Trim().ToLowerInvariant().Replace(' ', '-');
        var nameExists = await db.Channels
            .AnyAsync(channel => channel.WorkspaceId == workspaceId && channel.Name == normalizedName, cancellationToken);

        if (nameExists)
        {
            throw new ConflictException($"Channel '{normalizedName}' already exists in this workspace.");
        }

        var channel = Channel.Create(workspaceId, command.Name, command.Topic, command.Type, currentUser.UserId);
        db.Channels.Add(channel);
        await db.SaveChangesAsync(cancellationToken);

        return new ChannelDto(channel.Id, channel.WorkspaceId, channel.Name, channel.Topic, channel.Type, channel.Members.Count);
    }

    private async Task EnsureWorkspaceMemberAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new ForbiddenException("You must be authenticated.");
        }

        var isMember = await db.WorkspaceMembers
            .AnyAsync(member => member.WorkspaceId == workspaceId && member.UserId == currentUser.UserId, cancellationToken);

        if (!isMember)
        {
            throw new ForbiddenException("You are not a member of this workspace.");
        }
    }
}

