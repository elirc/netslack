# Architecture Critique

## Strong Choices

| Choice | Evidence | Why it is strong |
| --- | --- | --- |
| Layered solution | `NetSlack.sln:5-14` | Gives learners clear ownership boundaries. |
| Thin endpoints | `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35` | Transport stays separate from use-case policy. |
| Service-level authz | `src/NetSlack.Application/Messages/MessageService.cs:53-114` | Protects direct API requests, not just UI paths. |
| Domain factories | `src/NetSlack.Domain/Workspaces/Workspace.cs:31-39` and `src/NetSlack.Domain/Messages/Message.cs:31-41` | Invalid state is rejected near creation. |
| Real relational test provider | `tests/NetSlack.Application.Tests/WorkflowTests.cs:93-103` | SQLite catches more EF behavior than pure mocks. |

## Risks And Tradeoffs

| Priority | Risk | Evidence | Recommendation |
| --- | --- | --- | --- |
| P0 | SignalR group join may not verify channel access | `src/NetSlack.Api/Hubs/MessageHub.cs:10-13` | Inject a query/service that validates current user before `AddToGroupAsync`; add integration test. |
| P0 | Development token endpoint is not production auth | `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:15-40` | Replace with OIDC Authorization Code + PKCE; keep `ICurrentUser`. |
| P1 | No migrations found | `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:14` uses `EnsureCreated` | Add EF migrations and deployment runbook. |
| P1 | Direct SignalR publish after DB save is not durable | `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34` | Add outbox and background dispatcher. |
| P1 | No CI workflows found | repo inventory | Add build/test/client build pipeline. |
| P2 | Client lint script may lack dependencies | `client/package.json:10` and `client/package.json:18-23` | Add ESLint packages/config or remove script. |
| P2 | Prompt-based create flows | `client/src/features/workspace/WorkspaceShell.tsx:117-141` | Replace with forms, validation, accessible errors. |

## If I Owned This For 3 Months

1. Secure resource boundaries: SignalR join authorization, endpoint tests, rate limits.
2. Productionize identity: OIDC, token storage strategy, auth integration tests.
3. Add migrations and CI: make schema and tests reproducible.
4. Improve reliability: outbox for message events, reconnect UX, observable failures.
5. Improve product UX: forms, loading states, optimistic message sending, pagination.

## Migration And Test Strategy

For each recommendation:

- Write characterization tests first: current permission tests are modeled in `tests/NetSlack.Application.Tests/WorkflowTests.cs:43-63`.
- Add the smallest boundary-preserving change.
- Add rollback notes: config flag for auth migration, additive migrations for schema, feature flag for outbox dispatcher.
- Measure before optimizing: message query is anchored at `src/NetSlack.Application/Messages/MessageService.cs:16-33`.

## Verification Notes

- Confirmed risks are based on inspected code lines.
- "May not verify" on SignalR is labeled risk because the hub requires authentication but no channel membership check appears in `src/NetSlack.Api/Hubs/MessageHub.cs:10-13`.
