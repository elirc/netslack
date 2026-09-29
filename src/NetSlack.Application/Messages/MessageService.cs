using Microsoft.EntityFrameworkCore;
using NetSlack.Application.Common;
using NetSlack.Domain.Channels;
using NetSlack.Domain.Messages;

namespace NetSlack.Application.Messages;

public sealed class MessageService(IAppDbContext db, ICurrentUser currentUser, IClock clock)
{
    public async Task<IReadOnlyList<MessageDto>> GetRecentAsync(Guid channelId, int take, CancellationToken cancellationToken)
    {
        await EnsureCanReadChannelAsync(channelId, cancellationToken);

        var boundedTake = Math.Clamp(take, 1, 100);

        return await db.Messages
            .AsNoTracking()
            .Where(message => message.ChannelId == channelId)
            .OrderByDescending(message => message.PostedAt)
            .Take(boundedTake)
            .Join(
                db.Users.AsNoTracking(),
                message => message.AuthorId,
                user => user.Id,
                (message, user) => new MessageDto(
                    message.Id,
                    message.ChannelId,
                    message.Body,
                    message.PostedAt,
                    message.EditedAt,
                    new MessageAuthorDto(user.Id, user.DisplayName, user.Email, user.AvatarUrl)))
            .OrderBy(message => message.PostedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<MessageDto> PostAsync(Guid channelId, PostMessageCommand command, CancellationToken cancellationToken)
    {
        var channel = await EnsureCanPostChannelAsync(channelId, cancellationToken);
        var message = Message.Post(channelId, currentUser.UserId, command.Body, clock.UtcNow);

        db.Messages.Add(message);
        await db.SaveChangesAsync(cancellationToken);

        var author = await db.Users
            .AsNoTracking()
            .Where(user => user.Id == currentUser.UserId)
            .Select(user => new MessageAuthorDto(user.Id, user.DisplayName, user.Email, user.AvatarUrl))
            .SingleAsync(cancellationToken);

        return new MessageDto(message.Id, channel.Id, message.Body, message.PostedAt, message.EditedAt, author);
    }

    private async Task EnsureCanReadChannelAsync(Guid channelId, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new ForbiddenException("You must be authenticated.");
        }

        var channel = await db.Channels
            .AsNoTracking()
            .Include(item => item.Members)
            .SingleOrDefaultAsync(item => item.Id == channelId, cancellationToken);

        if (channel is null)
        {
            throw new NotFoundException("Channel was not found.");
        }

        var inWorkspace = await db.WorkspaceMembers
            .AnyAsync(member => member.WorkspaceId == channel.WorkspaceId && member.UserId == currentUser.UserId, cancellationToken);

        if (!inWorkspace || (channel.Type != ChannelType.Public && channel.Members.All(member => member.UserId != currentUser.UserId)))
        {
            throw new ForbiddenException("You do not have access to this channel.");
        }
    }

    private async Task<Channel> EnsureCanPostChannelAsync(Guid channelId, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new ForbiddenException("You must be authenticated.");
        }

        var channel = await db.Channels
            .Include(item => item.Members)
            .SingleOrDefaultAsync(item => item.Id == channelId, cancellationToken);

        if (channel is null)
        {
            throw new NotFoundException("Channel was not found.");
        }

        var inWorkspace = await db.WorkspaceMembers
            .AnyAsync(member => member.WorkspaceId == channel.WorkspaceId && member.UserId == currentUser.UserId, cancellationToken);

        if (!inWorkspace)
        {
            throw new ForbiddenException("You are not a member of this workspace.");
        }

        if (channel.Type == ChannelType.Public)
        {
            channel.Join(currentUser.UserId);
            return channel;
        }

        if (channel.Members.All(member => member.UserId != currentUser.UserId))
        {
            throw new ForbiddenException("You are not a member of this channel.");
        }

        return channel;
    }
}
