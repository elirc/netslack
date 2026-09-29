using NetSlack.Application.Common;
using NetSlack.Application.Workspaces;
using NetSlack.Domain.Channels;
using NetSlack.Domain.Users;
using NetSlack.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

var passed = 0;
var failed = 0;
async Task Run(string name, Func<Task> test) { try { await test(); passed++; Console.WriteLine($"ok - {name}"); } catch (Exception e) { failed++; Console.WriteLine($"not ok - {name}: {e.Message}"); } }
void Check(bool value, string message) { if (!value) throw new Exception(message); }

await Run("concurrent slug is mapped to ConflictException", async () => {
    var f = new Fixture("concurrent-slug");
    var service = new WorkspaceService(f.Db, f.User);
    await AssertThrows<ConflictException>(() => service.CreateAsync(new CreateWorkspaceCommand("First", "same"), CancellationToken.None));
    Check(f.Db.Saves == 1, "create did not attempt one save");
});
await Run("unrelated persistence error is not swallowed", async () => {
    var f = new Fixture("database unavailable");
    var service = new WorkspaceService(f.Db, f.User);
    await AssertThrows<DbUpdateException>(() => service.CreateAsync(new CreateWorkspaceCommand("First", "same"), CancellationToken.None));
    Check(f.Db.Saves == 1, "create did not attempt one save");
});
await Run("preexisting slug is rejected before save", async () => {
    var f = new Fixture(null);
    f.Db.Workspaces.Add(Workspace.Create("Existing", "same", f.User.UserId));
    var service = new WorkspaceService(f.Db, f.User);
    await AssertThrows<ConflictException>(() => service.CreateAsync(new CreateWorkspaceCommand("First", "same"), CancellationToken.None));
    Check(f.Db.Saves == 0, "precheck should avoid a write");
});
await Run("successful create preserves workspace and general channel", async () => {
    var f = new Fixture(null);
    var service = new WorkspaceService(f.Db, f.User);
    var result = await service.CreateAsync(new CreateWorkspaceCommand("First", "Same Slug"), CancellationToken.None);
    Check(result.Slug == "same-slug" && f.Db.Workspaces.Count == 1 && f.Db.Channels.Count == 1, "success payload or writes changed");
    Check(f.Db.Workspaces[0].Members.Single().UserId == f.User.UserId && f.Db.Workspaces[0].Members.Single().Role == WorkspaceRole.Owner, "owner membership changed");
    Check(f.Db.Channels[0].WorkspaceId == result.Id && f.Db.Channels[0].Name == "general", "initial channel relationship changed");
});
await Run("unrelated unique failure retains its original exception", async () => {
    var f = new Fixture("UNIQUE constraint failed: channels.Name");
    var service = new WorkspaceService(f.Db, f.User);
    var error = await AssertThrows<DbUpdateException>(() => service.CreateAsync(new CreateWorkspaceCommand("First", "same"), CancellationToken.None));
    Check(ReferenceEquals(error, f.Db.LastFailure), "original persistence error was replaced");
    Check(f.Db.Saves == 1, "unexpected retry");
});
Console.WriteLine($"# tests {passed + failed}\n# pass {passed}\n# fail {failed}\n# skipped 0");
return failed == 0 ? 0 : 1;

static async Task<T> AssertThrows<T>(Func<Task> action) where T : Exception { try { await action(); } catch (T error) { return error; } throw new Exception($"expected {typeof(T).Name}"); }

public sealed class Fixture
{
    public FakeDb Db { get; } = new();
    public FakeUser User { get; } = new();
    public Fixture(string? failure) { Db.Failure = failure; }
}

public sealed class FakeUser : ICurrentUser { public Guid UserId { get; } = Guid.NewGuid(); public string Email => "owner@test"; public string DisplayName => "Owner"; public bool IsAuthenticated => true; }
public sealed class FakeDb : IAppDbContext
{
    public FakeSet<Workspace> Workspaces { get; } = new(); public FakeSet<Channel> Channels { get; } = new(); public FakeSet<WorkspaceMember> WorkspaceMembers { get; } = new();
    public string? Failure { get; set; } public int Saves { get; private set; }
    public DbUpdateException? LastFailure { get; private set; }
    public Task<int> SaveChangesAsync(CancellationToken _) {
        Saves++;
        if (Failure == "concurrent-slug") {
            Workspaces.Clear();
            Workspaces.Add(Workspace.Create("Concurrent", "same", Guid.NewGuid()));
            throw new DbUpdateException("save failed", new Exception("provider constraint details"));
        }
        if (Failure is not null) { Workspaces.Clear(); Channels.Clear(); LastFailure = new DbUpdateException("save failed", new Exception(Failure)); throw LastFailure; }
        return Task.FromResult(1);
    }
}
