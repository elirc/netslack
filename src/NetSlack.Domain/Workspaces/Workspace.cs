using NetSlack.Domain.Common;

namespace NetSlack.Domain.Workspaces;

public sealed class Workspace : Entity
{
    private readonly List<WorkspaceMember> _members = [];

    private Workspace()
    {
        Name = string.Empty;
        Slug = string.Empty;
    }

    private Workspace(Guid id, string name, string slug)
        : base(id)
    {
        Name = name;
        Slug = slug;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; }

    public string Slug { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<WorkspaceMember> Members => _members.AsReadOnly();

    public static Workspace Create(string name, string slug, Guid ownerId)
    {
        var workspace = new Workspace(
            Guid.NewGuid(),
            Guard.AgainstEmpty(name, nameof(name), 120),
            NormalizeSlug(slug));

        workspace.AddMember(ownerId, WorkspaceRole.Owner);
        return workspace;
    }

    public void Rename(string name)
    {
        Name = Guard.AgainstEmpty(name, nameof(name), 120);
    }

    public void AddMember(Guid userId, WorkspaceRole role)
    {
        if (_members.Any(member => member.UserId == userId))
        {
            return;
        }

        _members.Add(new WorkspaceMember(Id, userId, role));
    }

    private static string NormalizeSlug(string slug)
    {
        var normalized = Guard.AgainstEmpty(slug, nameof(slug), 64)
            .ToLowerInvariant()
            .Replace(' ', '-');

        if (normalized.Any(character => !char.IsLetterOrDigit(character) && character != '-'))
        {
            throw new ArgumentException("Workspace slug can only contain letters, numbers, and dashes.", nameof(slug));
        }

        return normalized;
    }
}
