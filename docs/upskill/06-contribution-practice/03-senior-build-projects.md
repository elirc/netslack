# Senior Build Projects

## Project 1: Production Identity Migration
**Problem:** Dev-token auth is not enterprise auth. **Value:** real tenant identity and revocation. **Files:** `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:15-40`, `src/NetSlack.Infrastructure/ServiceCollectionExtensions.cs:29-65`, `src/NetSlack.Api/Middleware/CurrentUser.cs:8-21`. **Checklist:** OIDC provider, claims mapping, local dev fallback, audit, tests, rollback via feature flag. **Open questions:** token storage, refresh, SSO groups.

## Project 2: Realtime Authorization And Scale-Out
**Problem:** Hub group join risk and single-node SignalR. **Files:** `src/NetSlack.Api/Hubs/MessageHub.cs:10-20`, `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`, `client/src/hooks/useSignalRChannel.ts:6-42`. **Plan:** authorize join, add Azure SignalR/Redis backplane, integration tests, metrics. **Rollback:** disable realtime fallback to polling.

## Project 3: Message Delivery Outbox
**Problem:** DB commit and SignalR publish are not atomic. **Files:** `src/NetSlack.Application/Messages/MessageService.cs:36-50`, `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`, `src/NetSlack.Domain/Common/Entity.cs:18-22`. **Plan:** outbox table, dispatcher, retry/dead-letter, idempotent clients. **Tests:** commit succeeds/publish fails. **Rollout:** shadow log first.

## Project 4: Schema Migration Discipline
**Problem:** no migrations; seeder uses `EnsureCreated`. **Files:** `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:14`, `src/NetSlack.Infrastructure/Data/AppDbContext.cs:24-27`. **Plan:** initial migration, CI migration check, deploy runbook, rollback strategy. **Stretch:** zero-downtime migration template.

## Project 5: Enterprise Audit And Retention
**Problem:** chat apps need audit, retention, legal hold. **Files:** message model `src/NetSlack.Domain/Messages/Message.cs:21-29`; mappings `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:11-27`. **Plan:** audit events, retention jobs, admin policy, export. **Risks:** privacy, compliance, cost.

## Project 6: Test Pyramid Expansion
**Problem:** only service tests exist. **Files:** current tests `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63`, API composition `src/NetSlack.Api/Program.cs:84`, client shell `client/src/features/workspace/WorkspaceShell.tsx:14-36`. **Plan:** domain tests, API integration, SignalR integration, component tests, E2E smoke. **Rollout:** CI stages by cost.

## Project 7: Observability Productionization
**Problem:** console traces and basic health checks are not enough. **Files:** `src/NetSlack.Api/Program.cs:45-55`, `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:32-49`. **Plan:** structured logs, metrics, tracing backend, dashboards, alerts, SLOs. **Stretch:** per-workspace latency/error budgets.

