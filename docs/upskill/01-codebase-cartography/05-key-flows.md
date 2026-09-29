# Key Flows

## Flow: Development Sign-In

**Why it matters:** Auth is the entry point to every authorization decision. Here it is intentionally a development shortcut.

**Open first:** `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:15-40` - endpoint creates or updates a user and returns a JWT.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | `client/src/App.tsx:35-75` | Form captures display name/email | strings | No client-side validation beyond email input type. |
| 2 | API client | `client/src/api/http.ts:18-23` | Posts anonymous dev-token request | `{ email, displayName }` | Dev endpoint must not survive production. |
| 3 | API | `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:21-33` | Normalizes email, upserts user | `UserProfile` | No password/OIDC; intentional dev shortcut. |
| 4 | Infrastructure | `src/NetSlack.Infrastructure/Auth/JwtTokenService.cs:17-45` | Emits JWT claims | token string | Claims must match `CurrentUser`. |
| 5 | UI | `client/src/App.tsx:13-21` | Stores token in localStorage | `TokenResponse` | XSS would expose token. |

**Validation & authorization:** Endpoint is `.AllowAnonymous()` in `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:39`; JWT key length is validated in `src/NetSlack.Infrastructure/Auth/JwtOptionsValidator.cs:7-15`.

**Persistence & side effects:** User creation/update calls `SaveChangesAsync` in `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:25-32`.

**Tests:** No direct auth endpoint test found. Existing tests use fake current user in `tests/NetSlack.Application.Tests/WorkflowTests.cs:65-74`.

**Juniors miss:** Authentication proves identity; authorization still needs resource checks.

**Seniors notice:** LocalStorage token plus dev-token endpoint is acceptable for a lab, not enterprise production.

**Drill:** Replace the endpoint mentally with OIDC. Which files stop changing if `ICurrentUser` remains stable?

**Self-grade:** Basic: name files. Solid: explain claims. Strong: list production migration risks.

## Flow: Workspace Creation

**Why it matters:** It shows cross-layer creation, default data, uniqueness, and transaction boundaries.

**Open first:** `src/NetSlack.Application/Workspaces/WorkspaceService.cs:22-40` - the use case creates both workspace and default `general` channel.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | `client/src/features/workspace/WorkspaceShell.tsx:117-126` | Prompts name, derives slug, posts | strings | Prompt UX bypasses rich validation. |
| 2 | API client | `client/src/api/http.ts:30-34` | Posts workspace request | `Workspace` response | Errors surfaced through `ApiError`. |
| 3 | Endpoint | `src/NetSlack.Api/Endpoints/WorkspaceEndpoints.cs:17-24` | Maps request to command | `CreateWorkspaceCommand` | Endpoint stays thin. |
| 4 | App | `src/NetSlack.Application/Workspaces/WorkspaceService.cs:24-38` | Auth check, slug conflict check, creates entities | `Workspace`, `Channel` | Race between uniqueness check and DB unique index. |
| 5 | Domain | `src/NetSlack.Domain/Workspaces/Workspace.cs:31-69` | Normalizes slug and owner membership | entity | Invalid slug throws `ArgumentException`. |
| 6 | EF | `src/NetSlack.Infrastructure/Data/Configurations/WorkspaceConfiguration.cs:11-19` | Maps table/key/unique slug | DB rows | No migration files found. |

**Validation & authorization:** `EnsureAuthenticated` in `src/NetSlack.Application/Workspaces/WorkspaceService.cs:43-49`.

**Persistence & side effects:** One `SaveChangesAsync` for workspace and channel in `src/NetSlack.Application/Workspaces/WorkspaceService.cs:36-38`.

**Tests:** Covered by `tests/NetSlack.Application.Tests/WorkflowTests.cs:28-40`.

**Juniors miss:** The slug check is not a substitute for the unique index.

**Seniors notice:** Schema migrations are missing; `EnsureCreated` is for simple local setup, not controlled schema evolution.

**Drill:** Add a trace row for what happens if two users create the same slug simultaneously.

**Self-grade:** Basic: follow request. Solid: explain transaction. Strong: propose migration-safe uniqueness handling.

## Flow: Channel List And Creation

**Why it matters:** It teaches resource scoping. A user can only see channels in a workspace they belong to, and private channels are filtered.

**Open first:** `src/NetSlack.Application/Channels/ChannelService.cs:9-63`.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | `client/src/features/workspace/WorkspaceShell.tsx:63-90` | Loads channels when workspace changes | `Channel[]` | Non-null assertion hides type uncertainty. |
| 2 | API client | `client/src/api/http.ts:37-45` | GET/POST workspace channels | typed promises | Client trusts server for auth. |
| 3 | Endpoint | `src/NetSlack.Api/Endpoints/ChannelEndpoints.cs:10-29` | Requires authorization, calls service | route `workspaceId` | Endpoint does not itself check membership. |
| 4 | App | `src/NetSlack.Application/Channels/ChannelService.cs:49-63` | Verifies workspace membership | boolean | Critical IDOR boundary. |
| 5 | App query | `src/NetSlack.Application/Channels/ChannelService.cs:13-26` | Filters public or joined private channels | `ChannelDto` | Future direct-message rules need care. |
| 6 | Domain | `src/NetSlack.Domain/Channels/Channel.cs:40-79` | Creates normalized channel and joins creator | entity | No role check yet for who may create channels. |

**Validation & authorization:** `.RequireAuthorization()` in `src/NetSlack.Api/Endpoints/ChannelEndpoints.cs:10-12`; membership check in `src/NetSlack.Application/Channels/ChannelService.cs:49-63`.

**Persistence & side effects:** Unique workspace/name index in `src/NetSlack.Infrastructure/Data/Configurations/ChannelConfiguration.cs:19-23`.

**Tests:** No direct channel-list test found; creation is indirectly covered in `tests/NetSlack.Application.Tests/WorkflowTests.cs:29-32`.

**Juniors miss:** Filtering in the UI is not authorization.

**Seniors notice:** Channel creation lacks role policy; current rule is "any workspace member".

**Drill:** Write a test proving a non-member cannot create a channel in another workspace.

**Self-grade:** Basic: identify endpoint. Solid: identify membership check. Strong: design role policy without scattering checks.

## Flow: Message History

**Why it matters:** It exercises bounded queries, projections, authorization, and indexing.

**Open first:** `src/NetSlack.Application/Messages/MessageService.cs:10-33`.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | `client/src/features/workspace/WorkspaceShell.tsx:92-115` | Fetches messages on channel change | `Message[]` | State can briefly show old messages during fetch. |
| 2 | API client | `client/src/api/http.ts:48-50` | GET with `take` query | array | Query param not encoded, but value is numeric here. |
| 3 | Endpoint | `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:16-24` | Defaults `take` to 50 | int | Service clamps again. |
| 4 | App | `src/NetSlack.Application/Messages/MessageService.cs:12-14` | Checks access and clamps to 1..100 | int | Good blast-radius limit. |
| 5 | App query | `src/NetSlack.Application/Messages/MessageService.cs:16-33` | AsNoTracking, channel filter, order/take, join author | `MessageDto` | Offset pagination not present yet. |
| 6 | EF | `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:17` | Index supports channel/time read | DB index | Retention/delete strategy missing. |

**Validation & authorization:** Read access in `src/NetSlack.Application/Messages/MessageService.cs:53-76`.

**Persistence & side effects:** No writes in history flow.

**Tests:** No direct history pagination test; happy path reads messages in `tests/NetSlack.Application.Tests/WorkflowTests.cs:34-40`.

**Juniors miss:** `AsNoTracking` matters for read-only queries.

**Seniors notice:** A real chat app needs cursor pagination, retention policy, and search strategy.

**Drill:** Explain why the final ascending order in `src/NetSlack.Application/Messages/MessageService.cs:32` is needed after `OrderByDescending().Take()`.

**Self-grade:** Basic: describe query. Solid: explain projection. Strong: design cursor pagination.

## Flow: Post Message And Realtime Fanout

**Why it matters:** Durable write and realtime side effect must be ordered correctly.

**Open first:** `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35`.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | UI | `client/src/features/workspace/WorkspaceShell.tsx:258-289` | Composer validates non-empty trimmed body | string | Server still must validate. |
| 2 | UI | `client/src/features/workspace/WorkspaceShell.tsx:144-150` | Posts message and appends result | `Message` | Duplicate possible from REST + SignalR, handled by id. |
| 3 | API client | `client/src/api/http.ts:52-57` | POST body | `{ body }` | Token header injected in `client/src/api/http.ts:65-68`. |
| 4 | Endpoint | `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35` | Saves through service, then publishes | `MessageDto` | Publish is not transactional with DB. |
| 5 | App | `src/NetSlack.Application/Messages/MessageService.cs:36-50` | Access check, domain create, save, author projection | `MessageDto` | Author query after save is another DB round trip. |
| 6 | Domain | `src/NetSlack.Domain/Messages/Message.cs:31-41` | Enforces body and records domain event | entity | Domain event not dispatched. |
| 7 | Realtime | `client/src/hooks/useSignalRChannel.ts:19-21` | Receives event and appends | `Message` | Connection failure is logged only. |

**Validation & authorization:** Post access in `src/NetSlack.Application/Messages/MessageService.cs:79-114`.

**Persistence & side effects:** Save before SignalR publish in `src/NetSlack.Application/Messages/MessageService.cs:41-42` and `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`.

**Tests:** Happy path post in `tests/NetSlack.Application.Tests/WorkflowTests.cs:34-40`; no SignalR integration test found.

**Juniors miss:** Realtime is a notification, not the source of truth.

**Seniors notice:** For guaranteed delivery, use outbox/event processing rather than publishing directly from the request.

**Drill:** Design a failure table for "DB commit succeeds, SignalR publish fails".

**Self-grade:** Basic: trace files. Solid: explain duplicate protection. Strong: propose an outbox migration.

## Flow: Private Channel Authorization

**Why it matters:** This is the IDOR training flow. Resource ids from the client are not enough.

**Open first:** `src/NetSlack.Application/Messages/MessageService.cs:53-76`.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | Endpoint | `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:12-14` | Requires authenticated user | route channel id | Authn only, not authz. |
| 2 | App | `src/NetSlack.Application/Messages/MessageService.cs:60-68` | Loads channel and members | channel aggregate | Include needed for private membership. |
| 3 | App | `src/NetSlack.Application/Messages/MessageService.cs:70-76` | Checks workspace and channel membership | booleans | This prevents cross-workspace reads. |
| 4 | Test | `tests/NetSlack.Application.Tests/WorkflowTests.cs:43-63` | Proves outsider cannot read private channel | exception | Good regression seed. |

**Validation & authorization:** Service-level checks, not UI-level checks.

**Persistence & side effects:** `ChannelMember` composite key in `src/NetSlack.Infrastructure/Data/Configurations/ChannelMemberConfiguration.cs:11-22`.

**Tests:** Direct permission test in `tests/NetSlack.Application.Tests/WorkflowTests.cs:43-63`.

**Juniors miss:** A hidden sidebar entry does not stop a direct API request.

**Seniors notice:** SignalR `JoinChannel` currently joins by channel id without checking membership in `src/NetSlack.Api/Hubs/MessageHub.cs:10-13`. Investigate as a possible risk.

**Drill:** Write the missing SignalR group authorization test design.

**Self-grade:** Basic: define IDOR. Solid: point to checks. Strong: fix group-join risk with service reuse.

## Flow: Development Seeding

**Why it matters:** Seed data shapes local learning and can accidentally become production behavior.

**Open first:** `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:12-39`.

| Step | Owner | File | What happens | Data shape | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | API startup | `src/NetSlack.Api/Program.cs:61-68` | Runs only in development | service scope | Startup side effects can slow boot. |
| 2 | Seeder | `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:14-18` | `EnsureCreated`, skip if users exist | database | Not migration-based. |
| 3 | Seeder | `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:21-31` | Creates users/workspace/channels | entities | Seed data depends on domain invariants. |
| 4 | Seeder | `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:33-39` | Adds rows and saves | database | No rollback beyond DB transaction. |

**Validation & authorization:** Not a user request; no auth required.

**Persistence & side effects:** Direct DB creation in startup path.

**Tests:** No seeder test found.

**Juniors miss:** `EnsureCreated` is not the same as migrations.

**Seniors notice:** Production boot should not mutate schema.

**Drill:** Explain when seed data belongs in app startup and when it belongs in migrations/scripts.

**Self-grade:** Basic: trace seed. Solid: explain `EnsureCreated`. Strong: propose migration + seed strategy.

