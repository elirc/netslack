# NetSlack Upskill Lab

NetSlack is a compact Slack-style training app with an ASP.NET Core API, EF Core persistence, SignalR realtime messaging, and a React/TypeScript client. The solution lists five projects: API, Application, Domain, Infrastructure, and Application tests in `NetSlack.sln:5-14`. The backend composition happens in `src/NetSlack.Api/Program.cs:11-80`; the React product shell starts in `client/src/features/workspace/WorkspaceShell.tsx:14-36`. The core domain nouns are users, workspaces, channels, memberships, and messages, with invariants in `src/NetSlack.Domain/Workspaces/Workspace.cs:31-69`, `src/NetSlack.Domain/Channels/Channel.cs:40-79`, and `src/NetSlack.Domain/Messages/Message.cs:31-48`. Persistence is EF Core over SQLite via `src/NetSlack.Infrastructure/Data/AppDbContext.cs:10-27` and configuration classes such as `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:11-27`. Tests currently exercise application-service workflows in `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63`.

Senior mindset line: a junior asks "how do I make it work?", a mid-level engineer asks "is this the right pattern?", and a senior asks "what does this commit us to, who pays, and how do we reduce risk?"

## How To Use This Lab

| Track | Time | Path |
| --- | --- | --- |
| Weekend | 4-8 hours | Read `00-fast-track.md`, trace the message-post flow, run what your machine supports, make one docs-only annotation PR. |
| Two weeks | 30-60 min/day | Read cartography, stack mental models, then do two drills and one review kata per day. |
| Eight weeks | 3-5 hours/week | Work through architecture, quality, contribution tickets, and interview prep as a mock apprenticeship. |
| Ongoing | As needed | Use `08-reference/` before reviews, debugging, PRs, and design discussions. |

## Recommended Paths

- Brand-new junior: `00-fast-track.md` -> `01-codebase-cartography/02-file-reading-order.md` -> `04-code-reading-gym/01-annotation-drills.md`.
- Junior with C#/React familiarity: start at `01-codebase-cartography/05-key-flows.md`, then `05-quality-engineering/02-writing-tests-here.md`.
- Mid-level new to repo: `03-architecture-and-patterns/01-boundaries-and-layers.md`, `03-architecture-and-patterns/06-architecture-critique.md`, then mid-level tickets.
- Senior reviewer: `08-reference/risk-register.md`, `05-quality-engineering/05-security-checklist.md`, `07-career-and-collaboration/02-writing-prs-and-rfcs.md`.

## Conventions

- Codebase claims include exact path and line ranges, for example `src/NetSlack.Api/Program.cs:21-23`.
- Fake code is always labeled `Illustrative fake code: not from this repo`.
- "Investigate" means evidence suggests a risk, but the repo has not proven a defect.
- Each major doc includes drills or self-checks. Reading is not the goal; transferring judgment is.

## Verification Notes

- Inspected with `rg --files`, targeted `Get-Content` line-number passes, metadata reads, and scoped git status on June 9, 2026.
- `dotnet --version` failed because `dotnet` is not on PATH in this environment.
- React build was not run because `client/node_modules` is absent and the user requested not to install dependencies.

