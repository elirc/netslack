# Annotation Drills

For each drill, annotate inputs, outputs, dependencies, invariants, side effects, failure modes, and tests. Self-grade: Basic names files; Solid traces values; Strong identifies risk and a regression test.

1. **Message post use case** - `src/NetSlack.Application/Messages/MessageService.cs:36-50`. Focus: access check, domain factory, save, author projection.
2. **Private channel read guard** - `src/NetSlack.Application/Messages/MessageService.cs:53-76`. Focus: workspace membership plus private channel membership.
3. **Channel creation** - `src/NetSlack.Application/Channels/ChannelService.cs:29-47`. Focus: duplicate check, normalized name, domain factory.
4. **Workspace creation** - `src/NetSlack.Application/Workspaces/WorkspaceService.cs:22-40`. Focus: owner membership and default channel.
5. **Domain guard** - `src/NetSlack.Domain/Common/Guard.cs:5-18`. Focus: what it validates and what it does not.
6. **Message entity** - `src/NetSlack.Domain/Messages/Message.cs:31-48`. Focus: body invariant and edit timestamp.
7. **EF message mapping** - `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:11-27`. Focus: table, index, FK delete behavior.
8. **Exception middleware** - `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:21-49`. Focus: HTTP status mapping and trace id.
9. **Typed API client** - `client/src/api/http.ts:59-85`. Focus: auth header, JSON body, error handling.
10. **SignalR effect** - `client/src/hooks/useSignalRChannel.ts:6-42`. Focus: connection lifecycle and cleanup.
11. **React workspace loader** - `client/src/features/workspace/WorkspaceShell.tsx:38-115`. Focus: effect dependencies and disposed guards.
12. **Workflow test fixture** - `tests/NetSlack.Application.Tests/WorkflowTests.cs:81-112`. Focus: real SQLite and disposal.

## Annotation Template

| Category | Notes |
| --- | --- |
| Inputs | Route/body/state/constructor args. |
| Outputs | Return value, rendered UI, DB row, event. |
| Dependencies | Services, DbSets, hooks, config. |
| Invariants | Rules that must always hold. |
| Side effects | DB write, localStorage, SignalR, logs. |
| Failure modes | Exceptions, stale state, race, authorization gap. |
| Test to add | Smallest useful regression. |

