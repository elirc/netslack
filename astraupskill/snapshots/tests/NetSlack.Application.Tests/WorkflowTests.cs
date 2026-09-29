using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetSlack.Application.Channels;
using NetSlack.Application.Common;
using NetSlack.Application.Messages;
using NetSlack.Application.Workspaces;
using NetSlack.Domain.Channels;
using NetSlack.Domain.Users;
using NetSlack.Infrastructure.Data;

namespace NetSlack.Application.Tests;

public sealed class WorkflowTests
{
    [Fact]
    public async Task User_can_create_workspace_channel_and_message()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var user = UserProfile.Create("Ada Lovelace", "ada@netslack.dev");
        fixture.Db.Users.Add(user);
        await fixture.Db.SaveChangesAsync();

        var currentUser = new TestCurrentUser(user.Id, user.Email, user.DisplayName);
        var workspaceService = new WorkspaceService(fixture.Db, currentUser);
        var channelService = new ChannelService(fixture.Db, currentUser);
        var messageService = new MessageService(fixture.Db, currentUser, new FixedClock(DateTimeOffset.Parse("2026-01-01T12:00:00Z")));

        var workspace = await workspaceService.CreateAsync(new CreateWorkspaceCommand("NetSlack", "netslack"), CancellationToken.None);
        var channel = await channelService.CreateAsync(
            workspace.Id,
            new CreateChannelCommand("product", "Product delivery", ChannelType.Public),
            CancellationToken.None);

        var posted = await messageService.PostAsync(channel.Id, new PostMessageCommand("Shipping useful software."), CancellationToken.None);
        var messages = await messageService.GetRecentAsync(channel.Id, 50, CancellationToken.None);

        Assert.Equal("netslack", workspace.Slug);
        Assert.Equal("product", channel.Name);
        Assert.Equal(posted.Id, Assert.Single(messages).Id);
        Assert.Equal("Ada Lovelace", posted.Author.DisplayName);
    }

    [Fact]
    public async Task Non_members_cannot_read_private_channel()
    {
        await using var fixture = await TestFixture.CreateAsync();
        var owner = UserProfile.Create("Owner", "owner@netslack.dev");
        var outsider = UserProfile.Create("Outsider", "outsider@netslack.dev");
        fixture.Db.Users.AddRange(owner, outsider);
        await fixture.Db.SaveChangesAsync();

        var ownerContext = new TestCurrentUser(owner.Id, owner.Email, owner.DisplayName);
        var workspace = await new WorkspaceService(fixture.Db, ownerContext)
            .CreateAsync(new CreateWorkspaceCommand("Secure Team", "secure-team"), CancellationToken.None);

        var privateChannel = await new ChannelService(fixture.Db, ownerContext)
            .CreateAsync(workspace.Id, new CreateChannelCommand("leadership", "Sensitive planning", ChannelType.Private), CancellationToken.None);

        var outsiderContext = new TestCurrentUser(outsider.Id, outsider.Email, outsider.DisplayName);
        var messages = new MessageService(fixture.Db, outsiderContext, new FixedClock(DateTimeOffset.UtcNow));

        await Assert.ThrowsAsync<ForbiddenException>(() => messages.GetRecentAsync(privateChannel.Id, 50, CancellationToken.None));
    }

    private sealed class TestCurrentUser(Guid userId, string email, string displayName) : ICurrentUser
    {
        public Guid UserId { get; } = userId;

        public string Email { get; } = email;

        public string DisplayName { get; } = displayName;

        public bool IsAuthenticated => true;
    }

    private sealed class FixedClock(DateTimeOffset utcNow) : IClock
    {
        public DateTimeOffset UtcNow { get; } = utcNow;
    }

    private sealed class TestFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestFixture(SqliteConnection connection, AppDbContext db)
        {
            _connection = connection;
            Db = db;
        }

        public AppDbContext Db { get; }

        public static async Task<TestFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var db = new AppDbContext(options);
            await db.Database.EnsureCreatedAsync();

            return new TestFixture(connection, db);
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
