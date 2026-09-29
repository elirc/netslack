# Side Effects, Async, And Reliability

## Side Effects In This Repo

| Side effect | Evidence | Reliability note |
| --- | --- | --- |
| Database writes | `src/NetSlack.Application/Messages/MessageService.cs:41-42` | Durable source of truth. |
| SignalR publish | `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34` | Non-durable notification after commit. |
| Development seeding | `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:14-39` | Startup mutation, dev-only gate in `src/NetSlack.Api/Program.cs:61-68`. |
| localStorage auth | `client/src/App.tsx:13-21` | Browser-side persistence; XSS risk. |
| Console telemetry export | `src/NetSlack.Api/Program.cs:48-55` | Local visibility, not production monitoring. |

## Async Background Work

No `IHostedService`, `BackgroundService`, queue consumer, Polly retry, or `IHttpClientFactory` integration was found. The closest async/realtime flow is request-thread SignalR fanout after a database write in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`.

## Idempotency And Retries

Current message posting is not idempotent. If the client retries after a timeout, `src/NetSlack.Application/Messages/MessageService.cs:36-50` will create another message. A production chat app can use client-generated message ids or idempotency keys.

## Failure Table

| Failure | Current behavior | Senior improvement |
| --- | --- | --- |
| DB save fails | Exception middleware returns ProblemDetails via `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:21-49` | Add structured logs and retry only safe transient DB errors. |
| DB save succeeds, SignalR publish fails | Request likely fails after durable write because publish is awaited in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:34` | Outbox table plus background dispatcher. |
| Client SignalR connection fails | Error logged in `client/src/hooks/useSignalRChannel.ts:30-32` | UI reconnect state and fallback polling. |
| Seeder fails | Dev startup fails in `src/NetSlack.Api/Program.cs:66-67` | Separate migration/seed command. |

## Drill

Design an outbox for message fanout. Specify entity, EF mapping, dispatcher, retry policy, dead-letter visibility, tests, and rollout plan.

