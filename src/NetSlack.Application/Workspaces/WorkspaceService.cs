using Microsoft.EntityFrameworkCore;
using NetSlack.Application.Common;
using NetSlack.Domain.Channels;
using NetSlack.Domain.Workspaces;

namespace NetSlack.Application.Workspaces;

public sealed class WorkspaceService(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<WorkspaceDto>> GetMineAsync(CancellationToken cancellationToken)
    {
        EnsureAuthenticated();

        return await db.Workspaces
            .AsNoTracking()
            .Where(workspace => workspace.Members.Any(member => member.UserId == currentUser.UserId))
            .OrderBy(workspace => workspace.Name)
            .Select(workspace => new WorkspaceDto(workspace.Id, workspace.Name, workspace.Slug, workspace.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkspaceDto> CreateAsync(CreateWorkspaceCommand command, CancellationToken cancellationToken)
    {
        EnsureAuthenticated();

        var slug = command.Slug.Trim().ToLowerInvariant().Replace(' ', '-');
        var slugExists = await db.Workspaces.AnyAsync(workspace => workspace.Slug == slug, cancellationToken);
        if (slugExists)
        {
            throw new ConflictException($"Workspace slug '{slug}' is already taken.");
        }

        var workspace = Workspace.Create(command.Name, command.Slug, currentUser.UserId);
        var general = Channel.Create(workspace.Id, "general", "Company-wide announcements and coordination", ChannelType.Public, currentUser.UserId);

        db.Workspaces.Add(workspace);
        db.Channels.Add(general);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // A concurrent writer can win after the pre-check. Ask the database for
            // the normalized slug; this avoids parsing provider-specific constraint
            // names and avoids turning unrelated unique violations into conflicts.
            var existsAfterSave = await db.Workspaces
                .AsNoTracking()
                .AnyAsync(existing => existing.Slug == slug, cancellationToken);
            if (existsAfterSave)
            {
                throw new ConflictException($"Workspace slug '{slug}' is already taken.");
            }

            throw;
        }

        return new WorkspaceDto(workspace.Id, workspace.Name, workspace.Slug, workspace.CreatedAt);
    }

    private void EnsureAuthenticated()
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new ForbiddenException("You must be authenticated.");
        }
    }
}
