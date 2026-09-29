# Mid-Level Feature Tickets

## Ticket 1: Cursor Pagination For Messages
**Difficulty:** Medium **Story:** As a user, I want older history loaded efficiently. **Read first:** `src/NetSlack.Application/Messages/MessageService.cs:10-33`; `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:17`. **Plan:** add query contract, cursor DTO, tests for ordering, client load-more. **Risk:** API contract change. **Rollback:** keep old endpoint param.

## Ticket 2: SignalR Join Authorization
**Difficulty:** Hard. **Read first:** `src/NetSlack.Api/Hubs/MessageHub.cs:10-13`; `src/NetSlack.Application/Messages/MessageService.cs:53-76`. **Plan:** add channel-access query service, inject into hub, test unauthorized join. **Risk:** duplicate auth logic.

## Ticket 3: Replace Dev Token With OIDC Adapter
**Difficulty:** Hard. **Read first:** `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:15-40`; `src/NetSlack.Application/Common/ICurrentUser.cs:1-11`. **Plan:** keep `ICurrentUser`, replace auth endpoint, document local dev auth. **Risk:** auth migration.

## Ticket 4: Add EF Migrations
**Difficulty:** Medium. **Read first:** `src/NetSlack.Infrastructure/Data/AppDbContext.cs:24-27`; `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:14`. **Plan:** create initial migration, stop using `EnsureCreated` for non-dev. **Risk:** schema drift.

## Ticket 5: Message Edit Endpoint
**Difficulty:** Medium. **Read first:** `src/NetSlack.Domain/Messages/Message.cs:44-48`; `src/NetSlack.Application/Messages/MessageService.cs:36-50`. **Plan:** command/service method, author-only check, endpoint, client UI, tests. **Risk:** edit permissions.

## Ticket 6: Role-Based Channel Creation
**Difficulty:** Medium. **Read first:** `src/NetSlack.Domain/Workspaces/WorkspaceRole.cs:1-9`; `src/NetSlack.Application/Channels/ChannelService.cs:29-63`. **Plan:** query member role, enforce admin/owner, test members forbidden. **Risk:** product policy.

## Ticket 7: Outbox For Message Events
**Difficulty:** Hard. **Read first:** `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`; `src/NetSlack.Domain/Common/Entity.cs:18-22`. **Plan:** persist event/outbox row in same transaction, dispatcher, retries. **Risk:** operational complexity.

## Ticket 8: Workspace Invite Flow
**Difficulty:** Medium. **Read first:** `src/NetSlack.Domain/Workspaces/Workspace.cs:47-55`; `src/NetSlack.Infrastructure/Data/Configurations/WorkspaceMemberConfiguration.cs:11-24`. **Plan:** invite command, email placeholder, role assignment, tests. **Risk:** abuse/rate limits.

## Ticket 9: UI Loading/Error State Refactor
**Difficulty:** Medium. **Read first:** `client/src/features/workspace/WorkspaceShell.tsx:38-115`; `client/src/features/workspace/WorkspaceShell.tsx:220-224`. **Plan:** split hooks/components, preserve behavior, add tests. **Risk:** stale state bugs.

## Ticket 10: API Integration Tests
**Difficulty:** Medium. **Read first:** `src/NetSlack.Api/Program.cs:84`; `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63`. **Plan:** add WebApplicationFactory project/tests for endpoints and ProblemDetails. **Risk:** test setup complexity.

## Ticket 11: Rate Limiting
**Difficulty:** Medium. **Read first:** `src/NetSlack.Api/Program.cs:70-80`; auth endpoint `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:15-40`. **Plan:** ASP.NET rate limiter policies for auth/message post. **Risk:** blocking legitimate traffic.

## Ticket 12: Search Messages
**Difficulty:** Hard. **Read first:** `src/NetSlack.Application/Messages/MessageService.cs:16-33`; `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:17`. **Plan:** DB-specific search strategy, authorization, pagination, UI. **Risk:** performance and data retention.

