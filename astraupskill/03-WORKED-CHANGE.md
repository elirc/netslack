# Worked change: classify the save race by state

The baseline in [the original snapshot](snapshots/src/NetSlack.Application/Workspaces/WorkspaceService.cs) performed a slug precheck and then let every save exception escape. A tempting patch is `catch (DbUpdateException e) when (e.InnerException.Message.Contains("UNIQUE"))`, but that couples behavior to provider wording and mislabels a unique conflict from a different table. The staged implementation instead catches the save exception, queries the normalized slug with `AsNoTracking`, and converts the error only when that persisted row exists.

Before:

```csharp
await db.SaveChangesAsync(cancellationToken);
return new WorkspaceDto(workspace.Id, workspace.Name, workspace.Slug, workspace.CreatedAt);
```

After:

```csharp
try { await db.SaveChangesAsync(cancellationToken); }
catch (DbUpdateException)
{
    var exists = await db.Workspaces.AsNoTracking()
        .AnyAsync(x => x.Slug == slug, cancellationToken);
    if (exists) throw new ConflictException($"Workspace slug '{slug}' is already taken.");
    throw;
}
return new WorkspaceDto(workspace.Id, workspace.Name, workspace.Slug, workspace.CreatedAt);
```

Trace table: `command` → normalized `slug` → precheck → add workspace/channel → save → persisted re-query on failure → result. Existing row stops at precheck with zero saves. A race reaches save, then sees the winner and returns conflict. An outage reaches save, sees no row, and preserves the original `DbUpdateException`. Success returns the DTO and retains both additions.

The executable proof is [Program.cs](../tests/NetSlack.ScopeHarness/Program.cs), compiled against the actual staged service and domain files. It does not claim live SQL behavior. The exact baseline test file remains at [snapshots/tests/NetSlack.Application.Tests/WorkflowTests.cs](snapshots/tests/NetSlack.Application.Tests/WorkflowTests.cs); the staged integration test still covers the ordinary duplicate precheck.

When reviewing this code, ask what happens if the recovery query is canceled or itself unavailable. The current bounded behavior preserves the save exception only when the query finds no matching row; a query failure can replace the original exception through normal async propagation. That is a known operational edge, not evidence that the service has a complete retry policy. A future change could add structured logging or a carefully defined error wrapper, but it should bring its own runtime test and remain separate from slug classification. The current patch keeps the write contract legible and leaves that broader policy visible to the reviewer.
