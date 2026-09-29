# Testing Strategy

## Current Coverage

Two service-level tests exist:

- Happy path workspace/channel/message in `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-40`.
- Private channel outsider rejection in `tests/NetSlack.Application.Tests/WorkflowTests.cs:43-63`.

The fixture uses real SQLite in memory in `tests/NetSlack.Application.Tests/WorkflowTests.cs:93-103`, which is useful because EF relational behavior matters.

## What Belongs Where

| Test type | Use for | Repo anchor |
| --- | --- | --- |
| Domain unit | Guard/factory invariants | `src/NetSlack.Domain/Messages/Message.cs:31-48` |
| Application service | Authz, use-case rules, persistence interactions | `src/NetSlack.Application/Messages/MessageService.cs:53-114` |
| API integration | routing, auth middleware, ProblemDetails | `src/NetSlack.Api/Program.cs:70-80` |
| SignalR integration | group join and broadcast auth | `src/NetSlack.Api/Hubs/MessageHub.cs:10-20` |
| React component | loading/error/message composer state | `client/src/features/workspace/WorkspaceShell.tsx:220-289` |
| E2E | sign in, channel selection, post message | `client/src/App.tsx:35-75` and API endpoints |

## What Not To Test

- Do not snapshot entire UI trees unless layout stability matters.
- Do not test EF itself; test your mappings/queries.
- Do not mock `IAppDbContext` for query translation behavior; use SQLite like `WorkflowTests.cs:93-103`.

## Flake Prevention

- Control time through `IClock`, as tests do with `FixedClock` in `tests/NetSlack.Application.Tests/WorkflowTests.cs:76-79`.
- Avoid shared DB state; create fixture per test in `tests/NetSlack.Application.Tests/WorkflowTests.cs:18` and `tests/NetSlack.Application.Tests/WorkflowTests.cs:46`.
- Keep async awaited; no fire-and-forget assertions.

