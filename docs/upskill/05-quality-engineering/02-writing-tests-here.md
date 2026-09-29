# Writing Tests Here

Commands are inferred because `dotnet` is not on PATH in this environment.

| Goal | Command |
| --- | --- |
| Run all tests | `dotnet test .\NetSlack.sln` |
| Run workflow tests | `dotnet test .\tests\NetSlack.Application.Tests\NetSlack.Application.Tests.csproj --filter WorkflowTests` |
| Run one test | `dotnet test .\tests\NetSlack.Application.Tests\NetSlack.Application.Tests.csproj --filter User_can_create_workspace_channel_and_message` |

## Recipes

1. **Happy path:** Follow `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-40`.
2. **Validation failure:** Call `MessageService.PostAsync` with whitespace and assert `ArgumentException`; domain guard is `src/NetSlack.Domain/Common/Guard.cs:5-18`.
3. **Permission failure:** Model after `tests/NetSlack.Application.Tests/WorkflowTests.cs:43-63`.
4. **Cross-workspace rejection:** Create two workspaces, user belongs to one, attempt channel read in the other; use checks in `src/NetSlack.Application/Channels/ChannelService.cs:49-63`.
5. **Async side effect:** API integration test should verify SignalR publish after `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`.
6. **Query bound:** Assert `GetRecentAsync(channelId, 500)` returns max 100; clamp is `src/NetSlack.Application/Messages/MessageService.cs:14`.
7. **Migration behavior:** Not currently possible because no migrations were found; add migrations first.
8. **UI state:** Component test message composer disabled behavior in `client/src/features/workspace/WorkspaceShell.tsx:278-289`.

## Test Review Checklist

- Does the test fail for the bug it claims to catch?
- Does it use real SQLite when EF translation matters?
- Does it avoid depending on test order?
- Does it assert behavior rather than implementation detail?

