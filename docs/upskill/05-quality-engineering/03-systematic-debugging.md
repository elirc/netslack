# Systematic Debugging

Method: reproduce -> narrow layer -> hypothesize -> test cheaply -> fix root cause -> add regression coverage.

## Scenario: User Cannot See Channels

**Reproduction:** Sign in, workspace loads, channel list empty. **First question:** client fetch or server authz? **Narrowing path:** inspect network call from `client/src/features/workspace/WorkspaceShell.tsx:63-90`; check `ChannelService.GetForWorkspaceAsync` in `src/NetSlack.Application/Channels/ChannelService.cs:9-26`; inspect membership check in `src/NetSlack.Application/Channels/ChannelService.cs:49-63`. **Probe:** log current user id from `src/NetSlack.Api/Middleware/CurrentUser.cs:8-21`. **Regression test:** non-member gets forbidden; member sees public channels. **Senior lesson:** empty UI can hide auth failures.

## Scenario: Message Posts Twice

**Reproduction:** Send once, see duplicate. **First question:** REST append or SignalR append? **Narrowing path:** REST append in `client/src/features/workspace/WorkspaceShell.tsx:149-150`; SignalR append in `client/src/hooks/useSignalRChannel.ts:19-21`; duplicate guard in `client/src/features/workspace/WorkspaceShell.tsx:32-34`. **Probe:** log message ids. **Regression test:** append same id twice keeps one.

## Scenario: Private Channel Leaks

**Reproduction:** User guesses channel id and fetches messages. **First question:** endpoint authn or application authz? **Narrowing path:** endpoint requires auth in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:12-14`; service read guard in `src/NetSlack.Application/Messages/MessageService.cs:53-76`; test in `tests/NetSlack.Application.Tests/WorkflowTests.cs:43-63`. **Probe:** add test for guessed id. **Senior lesson:** every resource id is attacker-controlled.

## Scenario: SignalR Receives Unauthorized Messages

**Reproduction:** Authenticated user invokes `JoinChannel` with channel id they cannot read. **First question:** hub group join or message service? **Narrowing path:** hub adds group in `src/NetSlack.Api/Hubs/MessageHub.cs:10-13`; no membership check is visible there; message publish targets group in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`. **Probe:** SignalR integration test. **Likely root cause:** possible missing hub authorization. **Senior lesson:** realtime channels need the same resource checks as REST.

## Scenario: API Returns 500

**Reproduction:** Bad input causes server error. **First question:** expected domain validation or unexpected exception? **Narrowing path:** middleware mapping in `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:21-49`; guard throws `ArgumentException` in `src/NetSlack.Domain/Common/Guard.cs:7-15`. **Probe:** response ProblemDetails trace id. **Regression test:** bad input returns 400.

