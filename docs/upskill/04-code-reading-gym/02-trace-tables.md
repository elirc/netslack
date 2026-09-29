# Trace Tables

## Trace: UI To API Message Post

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `client/src/features/workspace/WorkspaceShell.tsx:258-276` | composer string | UI | trim, send | Client validation only. |
| 2 | `client/src/features/workspace/WorkspaceShell.tsx:144-150` | `body` | UI | calls API client | Duplicate append managed by id. |
| 3 | `client/src/api/http.ts:52-57` | `{ body }` | API client | POST JSON | Auth header added elsewhere. |
| 4 | `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35` | `PostMessageRequest` | API | command + hub publish | Publish after save is non-durable. |
| 5 | `src/NetSlack.Application/Messages/MessageService.cs:36-50` | `MessageDto` | Application | save and project | Needs authz. |

## Trace: Persistence Query

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `src/NetSlack.Application/Messages/MessageService.cs:16-20` | `IQueryable<Message>` | Application | channel/time/take | Cursor absent. |
| 2 | `src/NetSlack.Application/Messages/MessageService.cs:21-31` | joined rows | EF | author projection | Query translation must be verified by tests/profiling. |
| 3 | `src/NetSlack.Application/Messages/MessageService.cs:32-33` | `List<MessageDto>` | EF | materialize | DB call happens here. |
| 4 | `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:17` | DB index | Infrastructure | supports channel/time | Index may need direction/cursor strategy later. |

## Trace: Auth Boundary

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `src/NetSlack.Infrastructure/Auth/JwtTokenService.cs:21-32` | claims | Infrastructure | signs token | Dev auth only. |
| 2 | `src/NetSlack.Infrastructure/ServiceCollectionExtensions.cs:29-65` | bearer config | Infrastructure | validates token | Key from config. |
| 3 | `src/NetSlack.Api/Middleware/CurrentUser.cs:8-21` | `ICurrentUser` | API adapter | claims to app identity | Missing claim -> unauthenticated. |
| 4 | `src/NetSlack.Application/Channels/ChannelService.cs:49-63` | boolean | Application | resource authorization | Core IDOR check. |

## Trace: Error Contract

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `src/NetSlack.Application/Common/AppException.cs:3-9` | typed exception | Application | semantic failure | App exception messages become client detail. |
| 2 | `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:21-30` | HTTP status | API middleware | map type | New exceptions need mapping. |
| 3 | `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:37-49` | ProblemDetails | API middleware | JSON response | 500 detail intentionally generic. |
| 4 | `client/src/api/http.ts:76-104` | `ApiError` | Client | parse problem | Trace id exposed to UI code. |

## Trace: SignalR Connect

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `client/src/features/workspace/WorkspaceShell.tsx:32-36` | callback + channel | UI | subscribe hook | Dependency correctness matters. |
| 2 | `client/src/hooks/useSignalRChannel.ts:13-17` | connection | Client hook | build with access token | Token in query for hub. |
| 3 | `client/src/hooks/useSignalRChannel.ts:23-27` | channel id | Client hook | invoke `JoinChannel` | Server group join check may be missing. |
| 4 | `src/NetSlack.Api/Hubs/MessageHub.cs:10-20` | SignalR group | API hub | add/remove connection | Investigate resource auth. |

## Trace: Test Workflow

| Step | File | Value shape | Owner | Transformation | Risk |
| --- | --- | --- | --- | --- | --- |
| 1 | `tests/NetSlack.Application.Tests/WorkflowTests.cs:93-103` | SQLite DbContext | Test | real relational fixture | No migrations exercised. |
| 2 | `tests/NetSlack.Application.Tests/WorkflowTests.cs:19-26` | user/services | Test | arrange | Fake current user bypasses JWT. |
| 3 | `tests/NetSlack.Application.Tests/WorkflowTests.cs:28-40` | workspace/channel/message | Test | act/assert | Good happy path. |
| 4 | `tests/NetSlack.Application.Tests/WorkflowTests.cs:43-63` | forbidden exception | Test | permission regression | Covers private read only. |

