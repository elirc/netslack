using Microsoft.EntityFrameworkCore;
using NetSlack.Application.Common;
using NetSlack.Domain.Channels;
using NetSlack.Domain.Messages;
using NetSlack.Domain.Users;
using NetSlack.Domain.Workspaces;

namespace NetSlack.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<UserProfile> Users => Set<UserProfile>();

    public DbSet<Workspace> Workspaces => Set<Workspace>();

    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();

    public DbSet<Channel> Channels => Set<Channel>();

    public DbSet<ChannelMember> ChannelMembers => Set<ChannelMember>();

    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

