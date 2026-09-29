# Pattern Catalog

## Pattern: Thin Endpoint
**Problem:** Transport code grows business rules. **Real example:** `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35`. **Why it works:** endpoint maps request, service owns behavior, hub publishes. **Failure modes:** validation/auth duplicated in endpoints. **Use when:** adding REST operations. **Avoid when:** endpoint must orchestrate multiple bounded contexts without design. **Drill:** Refactor a fake endpoint that queries `DbContext` directly.

## Pattern: Application Service Use Case
**Problem:** Workflows need one place for authorization and persistence. **Real example:** `src/NetSlack.Application/Messages/MessageService.cs:36-50`. **Second:** `src/NetSlack.Application/Channels/ChannelService.cs:29-47`. **Failure modes:** service becomes a god object. **Drill:** Split query and command responsibilities for messages on paper.

## Pattern: Current User Port
**Problem:** Application needs identity without HTTP dependency. **Real example:** `src/NetSlack.Application/Common/ICurrentUser.cs:1-11`. **Second:** `src/NetSlack.Api/Middleware/CurrentUser.cs:6-21`. **Failure modes:** trusting request DTO user ids. **Drill:** Add `IsInRole` without coupling to ASP.NET claims.

## Pattern: Persistence Port
**Problem:** Application needs data access without constructing EF. **Real example:** `src/NetSlack.Application/Common/IAppDbContext.cs:9-24`. **Second:** `src/NetSlack.Infrastructure/Data/AppDbContext.cs:10-27`. **Failure modes:** exposing provider-specific APIs through the port. **Drill:** Decide whether transactions belong on this interface.

## Pattern: Domain Factory
**Problem:** Entity construction must enforce invariants. **Real example:** `src/NetSlack.Domain/Messages/Message.cs:31-41`. **Second:** `src/NetSlack.Domain/Workspaces/Workspace.cs:31-39`. **Failure modes:** public setters bypass rules. **Drill:** Create an `Archive` method for `Channel`.

## Pattern: Guard Clause
**Problem:** Invalid scalar input spreads. **Real example:** `src/NetSlack.Domain/Common/Guard.cs:5-18`. **Second:** `src/NetSlack.Domain/Channels/Channel.cs:70-79`. **Failure modes:** generic guards cannot express all business rules. **Drill:** Add a clear guard for max topic length.

## Pattern: DTO Projection
**Problem:** Returning entities leaks model and over-fetches. **Real example:** `src/NetSlack.Application/Messages/MessageService.cs:21-31`. **Second:** `src/NetSlack.Application/Channels/ChannelService.cs:19-25`. **Failure modes:** projections drift from client expectations. **Drill:** Add `isEdited` to message response.

## Pattern: Bounded Query
**Problem:** Unbounded reads become latency incidents. **Real example:** clamp in `src/NetSlack.Application/Messages/MessageService.cs:14`. **Second:** `Take` in `src/NetSlack.Application/Messages/MessageService.cs:19-20`. **Failure modes:** no cursor for deep history. **Drill:** Design cursor pagination.

## Pattern: EF Configuration Per Entity
**Problem:** Mapping rules clutter DbContext. **Real example:** `src/NetSlack.Infrastructure/Data/AppDbContext.cs:24-27`. **Second:** `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:7-27`. **Failure modes:** duplicate configs in multiple assemblies. **Drill:** Add a configuration file for reactions.

## Pattern: Composite Membership Key
**Problem:** Duplicate memberships corrupt authorization. **Real example:** `src/NetSlack.Infrastructure/Data/Configurations/WorkspaceMemberConfiguration.cs:11-24`. **Second:** `src/NetSlack.Infrastructure/Data/Configurations/ChannelMemberConfiguration.cs:11-22`. **Failure modes:** no audit trail for membership changes. **Drill:** Explain why composite keys fit join tables.

## Pattern: Central Error Mapping
**Problem:** APIs return inconsistent errors. **Real example:** `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:21-49`. **Second:** app exceptions in `src/NetSlack.Application/Common/AppException.cs:1-9`. **Failure modes:** leaking internal messages. **Drill:** Add a validation error shape.

## Pattern: Typed API Client
**Problem:** Components hand-roll fetch and error handling. **Real example:** `client/src/api/http.ts:15-87`. **Second:** client types in `client/src/api/types.ts:1-45`. **Failure modes:** stale types if server changes. **Drill:** Add an `editMessage` method.

## Pattern: React Effect Cleanup
**Problem:** Async effects set state after unmount. **Real example:** `client/src/features/workspace/WorkspaceShell.tsx:38-60`. **Second:** SignalR cleanup in `client/src/hooks/useSignalRChannel.ts:34-41`. **Failure modes:** stale closures and duplicate connections. **Drill:** Explain each dependency in `useSignalRChannel`.

## Pattern: Realtime Group
**Problem:** Broadcast only to channel listeners. **Real example:** group naming in `src/NetSlack.Api/Hubs/MessageHub.cs:20`. **Second:** publish in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`. **Failure modes:** group join lacks resource authorization; investigate. **Drill:** Add authorization to `JoinChannel`.

## Pattern: Dev Seeder
**Problem:** Local app needs useful starting state. **Real example:** `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:12-39`. **Second:** startup gate in `src/NetSlack.Api/Program.cs:61-68`. **Failure modes:** schema creation in app startup. **Drill:** Separate seed from migration.

## Pattern: Test Fixture With Real SQLite
**Problem:** EF behavior should be tested against a relational provider. **Real example:** `tests/NetSlack.Application.Tests/WorkflowTests.cs:81-112`. **Second:** behavior assertions in `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63`. **Failure modes:** fixtures becoming shared mutable state. **Drill:** Add a permission test with a second workspace.

