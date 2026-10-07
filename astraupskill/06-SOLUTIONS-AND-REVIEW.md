# Separate solutions and review rubric

Practice answers: P1 skips save and returns `ConflictException`; no new channel is committed. P2 reaches save, then the database query sees B’s committed row, so A receives `ConflictException`. P3 rethrows the channel failure because the workspace slug is absent; a message containing “UNIQUE” is not enough. P4 fails because the fake would confuse local pending state with another transaction’s durable row. In R1 the only true statement is that a persisted re-query can identify a winner; prechecks do not prevent races, not all unique errors are slug conflicts, the harness proves no live index, an unauthenticated caller should not learn existence, and `AsNoTracking` is not what reaches the database (finding B).

I2 is the most valuable exercise: it is the only way in this repository to see the recovery path against a real unique index (`WorkspaceConfiguration.cs:17`) without stubs. I1's table: precheck true → Conflict (no save); precheck false + no error → Success; precheck false + error + slug present → Conflict; precheck false + error + slug absent → Rethrow.

Review the change against five questions. Authorization: does authentication happen before the slug query? Data boundary: is the same normalized value used in precheck, create, and recovery? Failure boundary: does only observed persisted occupancy become conflict? Atomic result: does normal success retain workspace and general channel? Evidence boundary: do tests identify what is fake and what is actual source? A strong review asks for an actual native provider run before asserting SQL translation or isolation.

The before/after source is in [03-WORKED-CHANGE.md](03-WORKED-CHANGE.md). The exact baseline is [snapshots/src/NetSlack.Application/Workspaces/WorkspaceService.cs](snapshots/src/NetSlack.Application/Workspaces/WorkspaceService.cs). The executed source-linked cases are listed in [Program.cs](../tests/NetSlack.ScopeHarness/Program.cs). Reviewers should reject a patch that parses provider strings, maps every uniqueness error to slug conflict, or claims concurrency safety beyond this loser-classification behavior. They should also ask whether a failed recovery query needs a logging policy in the production environment.

The staged result preserves the original exception for unrelated failures and does not add retries. That tradeoff keeps the bounded CRUD contract small: callers get a stable duplicate response when evidence supports it, while operational errors remain diagnosable.

## Verified review findings (checked 2026-10-06)

Statically verified against the current source; none was executed.

**A. The slug is normalized twice, by two copies of the same rule.**
`WorkspaceService.cs:26` computes `Trim().ToLowerInvariant().Replace(' ', '-')`
for the precheck and the recovery query, while `Workspace.Create` stores the
result of its own `NormalizeSlug` (`Workspace.cs:57-68`, via
`Guard.AgainstEmpty`, which trims). The two agree today. If either copy changes
(say, collapsing repeated dashes), the precheck and recovery query look up a
different key from the one that is stored, and the race classification silently
breaks. Fix direction: expose one normalization function from the domain and
use it in all three places. A related consequence: a request body without
`slug` binds `null` into `CreateWorkspaceRequest` (`WorkspaceContracts.cs`), so
line 26 throws `NullReferenceException` before the domain's `ArgumentException`
can run; `ExceptionHandlingMiddleware.cs:23-29` maps that to 500, not 400.

**B. `AsNoTracking` is not what makes the recovery query "persisted".**
In EF Core, a LINQ query over a `DbSet` (such as `AnyAsync` on line 49) is
always translated to SQL and never returns Added-but-unsaved entities from the
change tracker; `AsNoTracking` only stops *materialized* results from being
tracked, and `AnyAsync` materializes nothing. The call is harmless, but a
learner should not conclude that removing it would make the query see pending
rows. In the harness, `AsNoTracking` is a no-op stub
(`tests/NetSlack.ScopeHarness/Contracts.cs:16`); the persisted/pending
distinction is modelled solely by `FakeDb` clearing its lists on a failed save.

**C. The `Location` header names a route that does not exist.**
`WorkspaceEndpoints.cs:23` returns `Results.Created($"/api/workspaces/{workspace.Id}", ...)`,
but the group maps only `GET /api/workspaces/` and `POST /api/workspaces/`
(lines 14 and 17). Following the header yields 404.
