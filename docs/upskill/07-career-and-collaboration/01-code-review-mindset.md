# Code Review Mindset

Review in this order:

1. Does it work?
2. Is it correct under edge cases and concurrency?
3. Will it stay correct?
4. Does it fit local patterns?
5. Is the feedback kind to maintainers?

## Repo Checklist

- Endpoint thinness: compare to `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35`.
- Service authorization: compare to `src/NetSlack.Application/Messages/MessageService.cs:53-114`.
- Domain invariants: compare to `src/NetSlack.Domain/Common/Guard.cs:5-18`.
- EF mapping/indexes: compare to `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:11-27`.
- Client API boundary: compare to `client/src/api/http.ts:15-87`.
- Test evidence: compare to `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63`.

## Good Comment Examples

> Blocking: this accepts a `userId` from the request, but current message posting uses `currentUser.UserId` at `MessageService.cs:39`. Please keep authorship server-derived or model impersonation as an explicit privileged workflow.

> Important: this query returns unbounded messages. Existing history clamps `take` at `MessageService.cs:14` and indexes channel/time at `MessageConfiguration.cs:17`; please add a bound or cursor.

> Optional: this duplicates error parsing already centralized in `client/src/api/http.ts:76-104`. Reusing that path will keep UI behavior consistent.

