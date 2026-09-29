# Observability And Operations

## Current Observability

- Health checks registered and mapped at `src/NetSlack.Api/Program.cs:45` and `src/NetSlack.Api/Program.cs:75`.
- Swagger registered in `src/NetSlack.Api/Program.cs:46-47` and enabled in development in `src/NetSlack.Api/Program.cs:61-64`.
- OpenTelemetry tracing registered with console exporter in `src/NetSlack.Api/Program.cs:48-55`.
- Exception middleware logs unhandled 500s in `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:32-35` and returns trace id in `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:45`.

## How Would I Know This Broke?

| Flow | Signal | Missing today |
| --- | --- | --- |
| Sign-in | 4xx/5xx count on `/api/auth/dev-token` | Auth metrics and audit logs. |
| Message post | latency, error rate, DB write failures | Structured event with workspace/channel ids. |
| SignalR | connection count, reconnects, publish failures | Hub metrics and alerting. |
| Message history | slow query telemetry | DB query duration dashboards. |
| Seeder | startup failure logs | Separate migration/seeding command output. |

## Deploy And Rollback

Compose exposes API/client in `deploy/docker-compose.yml:1-29`. API image publishes release build in `deploy/dotnet-api.Dockerfile:4-11`. Client image currently runs Vite dev server in `deploy/react-client.Dockerfile:4-7`; production should build static assets and serve them via CDN/static host.

Rollback checklist:

1. Know which schema version is live.
2. Keep migrations backward compatible for one deploy.
3. Roll back API separately from client where possible.
4. Preserve message data; never rollback by deleting production DB rows without a recovery plan.

