using Microsoft.EntityFrameworkCore;
using NetSlack.Domain.Channels;
using NetSlack.Domain.Messages;
using NetSlack.Domain.Users;
using NetSlack.Domain.Workspaces;
using NetSlack.Infrastructure.Data;

namespace NetSlack.Infrastructure.Seed;

public sealed class DevelopmentSeeder(AppDbContext db)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (await db.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var ada = UserProfile.Create("Ada Lovelace", "ada@netslack.dev", "/avatars/ada.png");
        var grace = UserProfile.Create("Grace Hopper", "grace@netslack.dev", "/avatars/grace.png");

        var workspace = Workspace.Create("NetSlack", "netslack", ada.Id);
        workspace.AddMember(grace.Id, WorkspaceRole.Admin);

        var general = Channel.Create(workspace.Id, "general", "Announcements and durable coordination", ChannelType.Public, ada.Id);
        general.Join(grace.Id);

        var engineering = Channel.Create(workspace.Id, "engineering", "Build notes, incidents, and product delivery", ChannelType.Public, grace.Id);
        engineering.Join(ada.Id);

        db.Users.AddRange(ada, grace);
        db.Workspaces.Add(workspace);
        db.Channels.AddRange(general, engineering);
        db.Messages.Add(Message.Post(general.Id, ada.Id, "Welcome to NetSlack. Keep decisions visible and kind.", DateTimeOffset.UtcNow.AddMinutes(-12)));
        db.Messages.Add(Message.Post(general.Id, grace.Id, "I added #engineering for implementation chatter and release notes.", DateTimeOffset.UtcNow.AddMinutes(-8)));

        await db.SaveChangesAsync(cancellationToken);
    }
}

