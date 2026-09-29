# NetSlack Architecture

NetSlack is intentionally structured like a production product, not a tutorial toy.

## Backend

- `NetSlack.Domain` owns business concepts and invariants: workspaces, channels, users, memberships, and messages.
- `NetSlack.Application` owns use cases. Services accept commands, enforce authorization, and return DTOs.
- `NetSlack.Infrastructure` owns EF Core persistence, JWT creation, and development seed data.
- `NetSlack.Api` owns HTTP, SignalR, middleware, CORS, health checks, Swagger, and telemetry.

The boundary is deliberate: code that decides whether a user can post is not hidden in a controller, and persistence details do not leak into React.

## Frontend

- `src/api` is the typed API client boundary.
- `src/hooks` owns integration concerns such as SignalR.
- `src/features/workspace` owns Slack-like product workflows.
- `src/components` is reserved for cross-feature UI building blocks.

## Production Checklist

- Replace `/api/auth/dev-token` with OIDC Authorization Code + PKCE.
- Store `Jwt:SigningKey` in a secret manager and rotate it.
- Move from SQLite to managed PostgreSQL or SQL Server.
- Add EF migrations and apply them in CI/CD, not on app startup.
- Add Redis backplane or Azure SignalR Service before horizontal scaling.
- Add tenant-aware audit logs for membership, channel, and message changes.
- Add retention policies, legal hold, export, DLP, and eDiscovery requirements before enterprise rollout.
- Add rate limits for login, message posting, workspace creation, and channel creation.
- Add structured logs, distributed traces, metrics, alerts, and SLO dashboards.
- Add accessibility and browser automation coverage for the core messaging workflow.

