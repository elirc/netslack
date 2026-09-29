# Fast Track: Weekend Win

Goal: run what you can, name the major parts, trace one feature end to end, and make one safe change using existing patterns.

## Commands

| Command | Status here | Evidence |
| --- | --- | --- |
| `dotnet restore .\NetSlack.sln` | Inferred, not run | README documents restore/test/run in `README.md:31-37`; local `dotnet --version` failed. |
| `dotnet test .\NetSlack.sln` | Inferred, not run | Test project exists in `NetSlack.sln:13-14`; tests are in `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63`. |
| `dotnet run --project .\src\NetSlack.Api\NetSlack.Api.csproj` | Inferred, not run | Launch URLs are `https://localhost:7208;http://localhost:5208` in `src/NetSlack.Api/Properties/launchSettings.json:4-12`. |
| `cd client; npm install; npm run dev` | Inferred, not run | Scripts are `dev`, `build`, `preview`, `lint` in `client/package.json:6-10`; dependencies were not installed. |
| `docker compose -f .\deploy\docker-compose.yml up --build` | Inferred, not run | Compose defines `api` and `client` services in `deploy/docker-compose.yml:1-26`. |

## First 10 Files

1. `README.md:1-63` - repo promise and run commands.
2. `NetSlack.sln:5-14` - project graph.
3. `Directory.Build.props:1-10` - global C# defaults.
4. `src/NetSlack.Api/Program.cs:11-80` - server composition.
5. `src/NetSlack.Application/Common/ServiceCollectionExtensions.cs:10-16` - application service registration.
6. `src/NetSlack.Infrastructure/ServiceCollectionExtensions.cs:15-65` - database and JWT wiring.
7. `src/NetSlack.Application/Messages/MessageService.cs:10-50` - read/post message use cases.
8. `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:12-35` - REST plus SignalR publish.
9. `client/src/features/workspace/WorkspaceShell.tsx:38-151` - client data flow.
10. `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63` - executable behavior examples.

## Trace One Flow

Trace "send a message":

1. Client composer submits body in `client/src/features/workspace/WorkspaceShell.tsx:258-289`.
2. Client posts to API in `client/src/features/workspace/WorkspaceShell.tsx:144-150` through `client/src/api/http.ts:52-57`.
3. API endpoint calls `MessageService.PostAsync` in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35`.
4. Application service checks access and saves in `src/NetSlack.Application/Messages/MessageService.cs:36-50`.
5. EF maps message table/index/FKs in `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:11-27`.
6. API publishes to SignalR group in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`.
7. Client receives `MessagePosted` in `client/src/hooks/useSignalRChannel.ts:19-21`.

## One Small Safe Change

Add a test for rejecting an empty message body. Read `src/NetSlack.Domain/Messages/Message.cs:31-38`, `src/NetSlack.Domain/Common/Guard.cs:5-18`, and `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-40`. The change should live in tests only unless the current behavior fails.

## Teach-Back

Explain in five minutes:

- What owns authorization: `MessageService` access checks in `src/NetSlack.Application/Messages/MessageService.cs:53-114`.
- What owns transport: minimal API endpoints in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:10-39`.
- What owns UI state: `WorkspaceShell` state in `client/src/features/workspace/WorkspaceShell.tsx:14-36`.

## What This Skips

It skips migrations, production auth, rate limiting, observability dashboards, CI, browser E2E tests, and SignalR scale-out. These are documented as risks in `08-reference/risk-register.md`.

