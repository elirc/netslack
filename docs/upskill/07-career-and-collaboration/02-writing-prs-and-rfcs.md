# Writing PRs And RFCs

## PR Template

```md
## What
[1-3 bullets]

## Why
[user/maintainer value]

## How Tested
- [ ] `dotnet test .\NetSlack.sln`
- [ ] `npm run build`
- [ ] Manual flow: [steps]

## Risks
[auth/data/perf/rollback]

## Follow-ups
[explicit non-goals]
```

Tie PRs to local patterns: service tests like `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63`, endpoint shape like `src/NetSlack.Api/Endpoints/WorkspaceEndpoints.cs:17-24`, and API client shape like `client/src/api/http.ts:18-57`.

## Commit Messages

Good: `Add private channel authorization test`

Weak: `fix stuff`

## When To RFC

Write an RFC when the change affects auth, persistence schema, public API, deploy/rollback, or cross-layer architecture. Examples: OIDC replacing `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:15-40`; migrations replacing `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:14`; outbox changing `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`.

## RFC Template

```md
# RFC: [Title]

## Problem
## Goals / Non-goals
## Current Code Anchors
## Proposed Design
## Alternatives
## Migration Plan
## Test Plan
## Security / Performance / Operations
## Rollback
## Open Questions
```

