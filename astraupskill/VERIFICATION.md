# Verification record

The staged runtime check was executed with this reproducible command from the project root with:

```text
dotnet run --project tests/NetSlack.ScopeHarness --configuration Release -p:UseSharedCompilation=false
```

It compiled the actual staged `WorkspaceService`, `WorkspaceDto`, and `src/NetSlack.Domain/**/*.cs` files through the source links in [NetSlack.ScopeHarness.csproj](../tests/NetSlack.ScopeHarness/NetSlack.ScopeHarness.csproj). Everything else the service touches is replaced by test-defined stand-ins in [Contracts.cs](../tests/NetSlack.ScopeHarness/Contracts.cs): `ICurrentUser`, `IAppDbContext`, the `AppException` family, and in the `Microsoft.EntityFrameworkCore` namespace a hand-written `DbUpdateException`, `AnyAsync`, `ToListAsync` and a no-op `AsNoTracking`. No EF Core package is referenced, so the harness proves the service's control flow, not EF behavior. The final reviewed harness contains five cases: concurrent persisted slug maps to `ConflictException`; unrelated persistence failure is not swallowed; an already persisted slug is rejected before save; successful creation retains the normalized DTO, owner membership and general channel; and a different unique failure preserves the original exception object. The exact executed command and output are attached below after the final run. The captured run may use --no-restore because local harness assets already existed; omit that flag for a clean checkout.

The project’s native test target was not executed successfully because it targets .NET8 while this environment exposes only .NET10 SDK/runtime. Earlier native restore attempts stopped at dependency resolution; no claim is made about native integration tests, EF provider translation, database uniqueness, migrations, transaction isolation, or production logging. The fallback contracts intentionally model persisted state transitions and compile real production source; they do not copy the service algorithm.

Read the original source and test baselines at [snapshots/src/NetSlack.Application/Workspaces/WorkspaceService.cs](snapshots/src/NetSlack.Application/Workspaces/WorkspaceService.cs) and [snapshots/tests/NetSlack.Application.Tests/WorkflowTests.cs](snapshots/tests/NetSlack.Application.Tests/WorkflowTests.cs). The staged course links the changed source and executable check. These records describe isolated stage verification historically and do not assert that a canonical project remains unchanged after later delivery.

The check also validates the negative classification boundary: the fake save raises an unrelated database error while the post-failure slug query is empty, and the harness expects `DbUpdateException`. This guards against the common but unsafe shortcut of matching a provider error message containing `UNIQUE`. The concurrent case supplies a separately persisted workspace only after save begins, so the test exercises the service recovery decision rather than merely detecting its own pending object. That distinction is why the harness is useful evidence even though it cannot replace a native provider run.

Classification observes whether the requested slug is now occupied; it does not prove which database constraint caused the original error. If the recovery query itself fails, that query error propagates. No retry or live provider guarantee is claimed.

## Astra execution evidence

The captured focused production-source suite executed 5 tests: all passed, zero failed and zero skipped. Read the [exact command and limits](evidence/results.json) (run dated 2026-09-22). The JSON names a log file, `crud-b02-astra-harness.log`, that was not committed, and its `cwd` is the original staging machine's path; rerun the command above to see the five `ok -` lines yourself.

The existing xUnit project `tests/NetSlack.Application.Tests` has 3 `[Fact]` tests in `WorkflowTests.cs` (create workspace/channel/message, private-channel read denial, duplicate slug conflict) against EF Core SQLite in-memory. None of them exercises the post-save recovery path; 05 exercise I2 describes the test that would.
