# System Map

## Project Graph

| Project/module | Owns | Evidence |
| --- | --- | --- |
| `NetSlack.Api` | HTTP endpoints, middleware, SignalR, Swagger, CORS, telemetry | `NetSlack.sln:5-6`, `src/NetSlack.Api/Program.cs:13-80` |
| `NetSlack.Application` | Use cases, DTOs, app exceptions, current-user boundary | `NetSlack.sln:7-8`, `src/NetSlack.Application/Common/ServiceCollectionExtensions.cs:10-16` |
| `NetSlack.Domain` | Entities, invariants, domain events | `NetSlack.sln:9-10`, `src/NetSlack.Domain/Common/Entity.cs:3-22` |
| `NetSlack.Infrastructure` | EF Core, JWT, seed data | `NetSlack.sln:11-12`, `src/NetSlack.Infrastructure/ServiceCollectionExtensions.cs:15-65` |
| `NetSlack.Application.Tests` | Service-level behavior tests | `NetSlack.sln:13-14`, `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63` |
| `client` | React UI, typed API client, SignalR hook | `client/package.json:12-23`, `client/src/features/workspace/WorkspaceShell.tsx:14-36` |

## Ownership Boundaries

| Boundary | Public face | Private implementation | Watch for |
| --- | --- | --- | --- |
| UI -> API | `client/src/api/http.ts:18-57` | Fetch headers/error mapping in `client/src/api/http.ts:59-85` | Components bypassing `ApiClient`. |
| API -> Application | Endpoint maps in `src/NetSlack.Api/Endpoints/*.cs` | Services in `src/NetSlack.Application/*/*Service.cs` | Business rules in endpoints. |
| Application -> Data | `IAppDbContext` in `src/NetSlack.Application/Common/IAppDbContext.cs:9-24` | `AppDbContext` in `src/NetSlack.Infrastructure/Data/AppDbContext.cs:10-27` | EF-specific behavior leaking into Domain. |
| Auth claims -> current user | `ICurrentUser` contract in `src/NetSlack.Application/Common/ICurrentUser.cs:1-11` | Claims adapter in `src/NetSlack.Api/Middleware/CurrentUser.cs:6-21` | Trusting client-provided ids. |
| Realtime | Hub contract in `src/NetSlack.Api/Hubs/MessageHub.cs:23-26` | Client hook in `client/src/hooks/useSignalRChannel.ts:6-42` | Publishing before commit or joining unauthorized groups. |

## Public Interfaces

- REST auth: `/api/auth/dev-token` in `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:13-40`.
- REST workspaces: `/api/workspaces` in `src/NetSlack.Api/Endpoints/WorkspaceEndpoints.cs:10-24`.
- REST channels: `/api/workspaces/{workspaceId}/channels` in `src/NetSlack.Api/Endpoints/ChannelEndpoints.cs:10-29`.
- REST messages: `/api/channels/{channelId}/messages` in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:12-35`.
- SignalR hub: `/hubs/messages` mapped in `src/NetSlack.Api/Program.cs:80`, with group methods in `src/NetSlack.Api/Hubs/MessageHub.cs:10-20`.

## Verification Notes

- No controllers were found; this API uses minimal endpoints.
- No worker project or `BackgroundService` was found. Async/reliability examples use EF async calls and SignalR.
- No CI workflow files were found in the inspected repo inventory.

