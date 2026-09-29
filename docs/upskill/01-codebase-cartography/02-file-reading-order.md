# File Reading Order

## Junior Path

| Order | File | Why | What to look for |
| --- | --- | --- | --- |
| 1 | `README.md:1-63` | Product and commands | What is verified vs aspirational. |
| 2 | `NetSlack.sln:5-14` | Project map | Which project references which responsibility. |
| 3 | `src/NetSlack.Api/Program.cs:11-23` | DI composition | How services enter the app. |
| 4 | `src/NetSlack.Api/Program.cs:70-80` | Middleware and endpoints | Request pipeline order. |
| 5 | `client/src/App.tsx:9-31` | Auth shell | Local storage and conditional rendering. |
| 6 | `client/src/features/workspace/WorkspaceShell.tsx:14-36` | UI state | Selected workspace/channel/message state. |
| 7 | `client/src/api/http.ts:59-85` | API boundary | Headers, JWT, errors. |
| 8 | `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:12-35` | Endpoint pattern | Transport calls service, then publishes. |
| 9 | `src/NetSlack.Application/Messages/MessageService.cs:36-50` | Use case | Validation through domain, save, DTO return. |
| 10 | `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-40` | Test style | Arrange service graph, act, assert behavior. |

## Mid-Level Path

| File | Why | What to look for |
| --- | --- | --- |
| `src/NetSlack.Application/Common/IAppDbContext.cs:9-24` | Persistence port | How application depends on abstraction. |
| `src/NetSlack.Infrastructure/Data/AppDbContext.cs:10-27` | EF adapter | DbSet mapping and assembly configs. |
| `src/NetSlack.Infrastructure/Data/Configurations/ChannelConfiguration.cs:11-25` | Model shape | Uniqueness, FK, field-backed collection. |
| `src/NetSlack.Application/Channels/ChannelService.cs:9-63` | Authorization | Workspace membership before listing/creating channels. |
| `src/NetSlack.Application/Messages/MessageService.cs:53-114` | Resource access | Public vs private channel logic. |
| `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:21-49` | Error contract | Exception-to-ProblemDetails mapping. |
| `client/src/hooks/useSignalRChannel.ts:6-42` | Effect lifecycle | Connect, subscribe, cleanup. |
| `src/NetSlack.Infrastructure/Auth/JwtTokenService.cs:17-45` | Claims | Which claims current user later consumes. |
| `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:12-39` | Local state | What data exists after dev startup. |
| `deploy/docker-compose.yml:1-29` | Runtime topology | Ports, env, volumes. |

## Senior Path

| File | Senior question |
| --- | --- |
| `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:15-40` | What risk does a dev-token endpoint create if deployed? |
| `src/NetSlack.Api/Hubs/MessageHub.cs:10-20` | Can a user join a SignalR group for a channel they cannot read? Investigate. |
| `src/NetSlack.Application/Messages/MessageService.cs:16-33` | Is the query bounded, indexed, and projection-friendly? |
| `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:17-26` | Do indexes match read paths and retention needs? |
| `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:14-18` | What happens in production if `EnsureCreated` replaces migrations? |
| `client/src/features/workspace/WorkspaceShell.tsx:117-141` | Are `prompt` flows acceptable UX and validation surfaces? |
| `client/package.json:6-10` | Does lint script have the dependencies it needs? Investigate because no ESLint package is listed in `client/package.json:18-23`. |

