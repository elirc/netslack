using NetSlack.Domain.Users;

namespace NetSlack.Domain.Workspaces;

public sealed class WorkspaceMember
{
    private WorkspaceMember()
    {
    }

    internal WorkspaceMember(Guid workspaceId, Guid userId, WorkspaceRole role)
    {
        WorkspaceId = workspaceId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTimeOffset.UtcNow;
    }

    public Guid WorkspaceId { get; private set; }

    public Guid UserId { get; private set; }

    public WorkspaceRole Role { get; private set; }

    public DateTimeOffset JoinedAt { get; private set; }

    public Workspace Workspace { get; private set; } = null!;

    public UserProfile User { get; private set; } = null!;

    public void ChangeRole(WorkspaceRole role) => Role = role;
}

