# Learning Rubrics

## Junior

| Behavior | Evidence of readiness |
| --- | --- |
| Can run or explain run commands | Knows command status in `command-cheatsheet.md`. |
| Can trace one flow | Explains message post from `client/src/features/workspace/WorkspaceShell.tsx:258-289` to `src/NetSlack.Application/Messages/MessageService.cs:36-50`. |
| Can make small safe changes | Adds test modeled after `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63`. |
| Respects boundaries | Does not put business rules in `src/NetSlack.Api/Endpoints/*.cs`. |
| Asks good questions | Uses code anchors and labels uncertainty. |

## Mid-Level

| Behavior | Evidence of readiness |
| --- | --- |
| Designs cross-layer changes | Plans contract, service, domain, EF, UI, tests. |
| Writes useful tests | Covers permission failure like `tests/NetSlack.Application.Tests/WorkflowTests.cs:43-63`. |
| Debugs systematically | Narrows UI/API/app/data using trace tables. |
| Reviews for correctness | Flags IDOR and unbounded query risks. |
| Manages blast radius | Avoids drive-by refactors. |

## Senior

| Behavior | Evidence of readiness |
| --- | --- |
| Critiques architecture | Prioritizes risks in `risk-register.md`. |
| Plans migrations | Handles OIDC, EF migrations, outbox with rollback. |
| Reasons about operations | Defines "how would we know this broke?" per flow. |
| Teaches transferable judgment | Explains invariant, boundary, ownership, consistency, latency, observability. |
| Leads collaboration | Writes RFCs, gives kind blocking reviews, responds with evidence. |

## Self-Assessment

Before claiming a level, write a one-page note:

1. Flow you traced with exact line anchors.
2. Bug or risk you identified.
3. Test you would add.
4. Tradeoff you considered.
5. What you still need to investigate.

