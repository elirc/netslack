# Graduated practice

Work the exercises in order and write your prediction before reading the
**Check**. Line numbers refer to
[WorkspaceService.cs](../src/NetSlack.Application/Workspaces/WorkspaceService.cs).
Two runnable checks exist:

- the harness: `dotnet run --project tests/NetSlack.ScopeHarness --configuration Release -p:UseSharedCompilation=false`
  (.NET 10 SDK; prints `ok - <name>` per case);
- the xUnit suite: `dotnet test tests/NetSlack.Application.Tests` (targets
  `net8.0` via `Directory.Build.props`; with only .NET 10 installed set
  `DOTNET_ROLL_FORWARD=Major` first). It uses a real EF Core SQLite in-memory
  database, so it is the only place a real unique index exists.

## Prediction exercises

**P1. Existing slug.** `team-alpha` already exists; a request arrives with slug `Team Alpha`.
- Goal: say whether save runs, which exception is public, whether a channel is added.
- **Check:** harness case `preexisting slug is rejected before save` (asserts `Saves == 0`) and
  xUnit `Duplicate_workspace_slug_is_reported_as_conflict`. The normalization on line 26 maps
  `Team Alpha` to `team-alpha`; the throw is line 30.

**P2. Concurrent winner.** Requests A and B both see absence; B commits first; A's save throws `DbUpdateException`.
- Goal: predict the post-failure query result and A's public result.
- **Check:** harness case `concurrent slug is mapped to ConflictException` (lines 13-18 of
  `tests/NetSlack.ScopeHarness/Program.cs`). Then delete lines 47-53 of the service (the recovery
  query and its `if`, leaving only `throw;` in the catch), rerun, and confirm exactly this case
  reports `not ok`. (Deleting rather than commenting avoids an unused-variable diagnostic under
  `TreatWarningsAsErrors`.) Revert with
  `git checkout -- src`.

**P3. Unrelated unique violation.** The database rejects the channel row; no workspace with the slug exists.
- Goal: decide whether the service should translate it.
- **Check:** harness case `unrelated unique failure retains its original exception` asserts the
  *same exception object* is rethrown (`ReferenceEquals`), not merely the same type.

**P4. The naive fake.**
- Goal: explain why a fake that keeps pending Added entities visible to queries cannot prove the race behavior.
- **Check:** in `FakeDb.SaveChangesAsync` (harness `Program.cs:72`) delete `Workspaces.Clear();`
  from the generic-failure branch and rerun. Predict first: both `unrelated ...` cases now report
  `not ok` with the message `Workspace slug 'same' is already taken.`, because the recovery query
  finds the request's own pending workspace and converts the failure to a `ConflictException`,
  which `AssertThrows<DbUpdateException>` does not catch. Revert afterwards.

## Implementation exercises

**I1. Decision function.**
- Goal: sketch a pure function of `precheckExists`, `saveError`, `persistedSlugAfterError` that
  returns `Conflict`, `Rethrow` or `Success`, keeping authorization outside it.
- **Check:** your truth table has exactly the four rows of the 04 trace table, and the row
  `saveError = true, persistedSlugAfterError = false` yields `Rethrow`.

**I2. A real race against a real unique index.**
- Goal: add an xUnit test to `WorkflowTests.cs` that makes the race deterministic with real EF Core.
  Create a second `AppDbContext` on the fixture's open `SqliteConnection`, and register an
  `ISaveChangesInterceptor` on the first context whose `SavingChangesAsync` inserts a
  `same-slug` workspace through the second context the first time it runs.
- **Check:** the test asserts `ConflictException` and that exactly one `same-slug` row exists.
  Deleting service lines 47-53 must make it fail with a raw `DbUpdateException`. This is
  the first test in the repository that exercises the recovery path against a real provider;
  record your run's output, since none is recorded yet.

**I3. Success shape.**
- Goal: assert that success creates the workspace, the owner membership and the `general` channel.
- **Check:** harness case `successful create preserves workspace and general channel` is the
  reference (it also asserts the DTO slug is `same-slug` for input `Same Slug`).

## Review exercise

**R1. True or false**, each justified by a boundary (timing, constraint ownership, persistence,
test scope, authorization): "the precheck prevents races"; "all unique exceptions are slug
conflicts"; "a persisted re-query can identify a winner"; "the harness proves a real database
index"; "unauthenticated callers should learn slug existence"; "`AsNoTracking` is what makes the
recovery query see the database".
- **Check:** only the third is true. For the last one, read 06, finding B.

## Stretch

**S1. Recovery query failure.** Propose behavior if the post-failure query itself throws or is cancelled.
- Goal: a policy that preserves the original save error and records the query failure.
- **Check:** your design names the logging contract and a harness case in which `AnyAsync`
  throws inside the catch block; today that case would surface the query's exception, not the
  save error (stated as a limit in `astraupskill/evidence/results.json`).
