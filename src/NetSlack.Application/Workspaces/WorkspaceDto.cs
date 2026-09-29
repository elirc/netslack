namespace NetSlack.Application.Workspaces;

public sealed record WorkspaceDto(Guid Id, string Name, string Slug, DateTimeOffset CreatedAt);

public sealed record CreateWorkspaceCommand(string Name, string Slug);

