# Review Katas

## Kata 1: "Let Client Pick Author"
**Author intent:** support posting as another user. **Fake diff summary:** adds `AuthorId` to `PostMessageRequest`. **Resembles:** `src/NetSlack.Api/Contracts/MessageContracts.cs:1-4`. **Expected findings:** Blocking: trust boundary violation; server should use `ICurrentUser`. **Good comment:** > Blocking: this lets callers spoof authors. The current service uses `currentUser.UserId` at `MessageService.cs:39`; keep that boundary and model impersonation as an explicit admin feature.

## Kata 2: "Speed Up Message History"
**Fake diff:** removes `AsNoTracking`. **Resembles:** `src/NetSlack.Application/Messages/MessageService.cs:16-18`. **Finding:** Important: read-only query should avoid tracking. Ask for measurement if changing.

## Kata 3: "Add SignalR Join"
**Fake diff:** keeps existing hub join by id. **Resembles:** `src/NetSlack.Api/Hubs/MessageHub.cs:10-13`. **Finding:** Blocking/investigate: authenticated user may join unauthorized channel group.

## Kata 4: "Create Channel In Endpoint"
**Fake diff:** endpoint injects `AppDbContext` and constructs channel. **Resembles:** current endpoint `src/NetSlack.Api/Endpoints/ChannelEndpoints.cs:17-29`. **Finding:** Blocking: bypasses `ChannelService` membership check in `src/NetSlack.Application/Channels/ChannelService.cs:49-63`.

## Kata 5: "Return ProblemDetails Everywhere"
**Fake diff:** catches exceptions in each endpoint. **Resembles:** middleware `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:21-49`. **Finding:** Optional/Important: central middleware already owns this; avoid duplicated behavior.

## Kata 6: "Add Workspace Slug Check"
**Fake diff:** only checks slug in UI. **Resembles:** domain normalization `src/NetSlack.Domain/Workspaces/Workspace.cs:57-69` and DB unique index `src/NetSlack.Infrastructure/Data/Configurations/WorkspaceConfiguration.cs:17`. **Finding:** Blocking: server/database constraints still required.

## Kata 7: "Make DbContext Singleton"
**Fake diff:** registers `AppDbContext` singleton. **Resembles:** scoped registration `src/NetSlack.Infrastructure/ServiceCollectionExtensions.cs:17-18`. **Finding:** Blocking: DbContext is not thread-safe and holds tracking state.

## Kata 8: "Use EnsureCreated In Production"
**Fake diff:** runs seeder outside development. **Resembles:** dev gate `src/NetSlack.Api/Program.cs:61-68`. **Finding:** Blocking: use migrations for production schema.

## Kata 9: "Add Reactions Without Tests"
**Fake diff:** adds endpoint and UI only. **Resembles:** existing workflow tests `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63`. **Finding:** Important: cross-layer feature needs service tests for auth and persistence.

## Kata 10: "Store Token In State Only"
**Fake diff:** removes localStorage. **Resembles:** `client/src/App.tsx:13-21`. **Finding:** Optional: improves XSS exposure but changes refresh behavior; discuss product requirement.

