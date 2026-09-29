using Microsoft.EntityFrameworkCore;
using NetSlack.Domain.Channels;
using NetSlack.Domain.Messages;
using NetSlack.Domain.Users;
using NetSlack.Domain.Workspaces;

namespace NetSlack.Application.Common;

public interface IAppDbContext
{
    DbSet<UserProfile> Users { get; }

    DbSet<Workspace> Workspaces { get; }

    DbSet<WorkspaceMember> WorkspaceMembers { get; }

    DbSet<Channel> Channels { get; }

    DbSet<ChannelMember> ChannelMembers { get; }

    DbSet<Message> Messages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

