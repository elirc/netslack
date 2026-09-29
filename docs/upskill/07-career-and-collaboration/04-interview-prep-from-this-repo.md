# Interview Prep From This Repo

## C#/.NET Runtime

**Question:** Why is `DbContext` scoped? **Junior:** "Because the app registers it that way." **Mid-level:** "Each request gets a unit of work; EF tracks changes and is not thread-safe. Registration is in `src/NetSlack.Infrastructure/ServiceCollectionExtensions.cs:17-18`." **Senior:** "Scoped lifetime prevents captive dependency bugs and bounds tracking state; tests also dispose connections explicitly in `tests/NetSlack.Application.Tests/WorkflowTests.cs:108-112`."

**Question:** How does async help here? **Junior:** "It doesn't block." **Mid-level:** "EF async calls like `ToListAsync` in `src/NetSlack.Application/Messages/MessageService.cs:32-33` release the request thread during I/O." **Senior:** "Async improves throughput when I/O dominates, but cancellation tokens and bounded queries (`MessageService.cs:14`) matter for overload behavior."

## ASP.NET/EF/React

**Question:** Explain the ASP.NET pipeline. **Junior:** "Program registers services and maps endpoints." **Mid-level:** "Middleware order is exception, dev Swagger/seed, HTTPS/CORS/auth/authz, endpoints in `src/NetSlack.Api/Program.cs:59-80`." **Senior:** "Order is a risk surface; auth before endpoints, CORS before endpoint execution, and startup seeding must be environment-gated."

**Question:** Why project to DTOs? **Junior:** "The client needs JSON." **Mid-level:** "DTOs in `src/NetSlack.Application/Messages/MessageDto.cs:3-13` decouple API from entities." **Senior:** "Projection at query time (`MessageService.cs:21-31`) controls data shape, avoids over-fetching, and preserves domain encapsulation."

**Question:** Explain React effects in this app. **Junior:** "Effects fetch data." **Mid-level:** "Workspace/channel/message effects run when dependencies change in `client/src/features/workspace/WorkspaceShell.tsx:38-115`." **Senior:** "Cleanup avoids setting state after unmount; SignalR cleanup in `client/src/hooks/useSignalRChannel.ts:34-41` prevents connection leaks."

## Debugging

**Question:** A user sees messages from a private channel. What do you do? **Junior:** "Check the UI filter." **Mid-level:** "Reproduce direct API call; inspect `MessageService.cs:53-76` and `WorkflowTests.cs:43-63`." **Senior:** "Also test SignalR group join at `MessageHub.cs:10-13`, because realtime paths need resource authorization too."

## System Design

**Question:** How would you make message delivery reliable? **Junior:** "Retry SignalR." **Mid-level:** "Publish after saving; current code does save then publish in `MessageEndpoints.cs:33-34`." **Senior:** "Direct publish is not durable. Add outbox persisted with the message, background dispatcher, idempotent delivery, dead-letter metrics, and fallback polling."

**Question:** How would you add enterprise auth? **Junior:** "Use JWT." **Mid-level:** "Replace dev-token endpoint in `AuthEndpoints.cs:15-40` with OIDC and keep `ICurrentUser`." **Senior:** "Plan migration, claims mapping, tenant isolation, token storage, revocation, audit, local dev experience, and rollback."

## Code Review

**Question:** Review a PR that adds message search. **Junior:** "Looks okay if it works." **Mid-level:** "Ask for authorization, pagination, tests, and index review anchored to `MessageService.cs:16-33` and `MessageConfiguration.cs:17`." **Senior:** "Also ask about retention, database-specific search, rollout, backfill, SLOs, and query-plan evidence."

## Behavioral Prompts

- "Tell me about a time you reduced risk." Use SignalR join authorization risk from `src/NetSlack.Api/Hubs/MessageHub.cs:10-13`.
- "Tell me about debugging production latency." Use message query/index analysis from `src/NetSlack.Application/Messages/MessageService.cs:16-33`.
- "Tell me about improving test coverage." Use private-channel test pattern in `tests/NetSlack.Application.Tests/WorkflowTests.cs:43-63`.
- "Tell me about leading a migration." Use dev-token to OIDC or `EnsureCreated` to migrations.

