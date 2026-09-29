# Good First Tickets

## Ticket 1: Empty Message Test
**Difficulty:** Easy **Time:** 30 min **Skills:** domain validation, xUnit. **Story:** As a maintainer, I want whitespace messages rejected by test so regressions are caught. **Read first:** `src/NetSlack.Domain/Messages/Message.cs:31-38`; `src/NetSlack.Domain/Common/Guard.cs:5-18`. **Files likely touched:** `tests/NetSlack.Application.Tests/WorkflowTests.cs`. **Acceptance criteria:** test asserts `ArgumentException`.

## Ticket 2: Workspace Duplicate Slug Test
**Difficulty:** Easy **Time:** 45 min. **Read first:** `src/NetSlack.Application/Workspaces/WorkspaceService.cs:26-31`; `src/NetSlack.Infrastructure/Data/Configurations/WorkspaceConfiguration.cs:17`. **Acceptance criteria:** duplicate create throws `ConflictException`.

## Ticket 3: Channel Duplicate Name Test
**Difficulty:** Easy. **Read first:** `src/NetSlack.Application/Channels/ChannelService.cs:33-40`. **Acceptance criteria:** duplicate name in same workspace fails.

## Ticket 4: Channel Name Normalization Test
**Difficulty:** Easy. **Read first:** `src/NetSlack.Domain/Channels/Channel.cs:70-79`. **Acceptance criteria:** spaces become dashes; invalid characters throw.

## Ticket 5: Workspace Slug Normalization Test
**Difficulty:** Easy. **Read first:** `src/NetSlack.Domain/Workspaces/Workspace.cs:57-69`. **Acceptance criteria:** uppercase and spaces normalize.

## Ticket 6: Message Take Clamp Test
**Difficulty:** Easy. **Read first:** `src/NetSlack.Application/Messages/MessageService.cs:14-20`. **Acceptance criteria:** requesting more than 100 returns max 100.

## Ticket 7: Add Missing Direct Auth Test For Channel Create
**Difficulty:** Medium. **Read first:** `src/NetSlack.Application/Channels/ChannelService.cs:49-63`. **Acceptance criteria:** non-member cannot create channel.

## Ticket 8: Improve README Command Status
**Difficulty:** Easy. **Read first:** `README.md:29-45`; `docs/upskill/08-reference/command-cheatsheet.md`. **Acceptance criteria:** mark commands requiring SDK/deps.

## Ticket 9: Add ESLint Dependency Or Remove Lint Script
**Difficulty:** Easy. **Read first:** `client/package.json:6-10`; `client/package.json:18-23`. **Acceptance criteria:** lint script is runnable or documented as future.

## Ticket 10: Add UI Error For Create Channel
**Difficulty:** Medium. **Read first:** `client/src/features/workspace/WorkspaceShell.tsx:129-141`; `client/src/api/http.ts:76-104`. **Acceptance criteria:** API errors render in banner.

## Ticket 11: Replace Prompt Workspace Form
**Difficulty:** Medium. **Read first:** `client/src/features/workspace/WorkspaceShell.tsx:117-126`. **Acceptance criteria:** accessible inline form, same API call.

## Ticket 12: Add AppException For Validation
**Difficulty:** Medium. **Read first:** `src/NetSlack.Application/Common/AppException.cs:1-9`; `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:21-30`. **Acceptance criteria:** validation maps consistently.

## Ticket 13: Add Health Check Documentation
**Difficulty:** Easy. **Read first:** `src/NetSlack.Api/Program.cs:45`; `src/NetSlack.Api/Program.cs:75`. **Acceptance criteria:** README mentions `/health`.

## Ticket 14: Add Seed Data Test
**Difficulty:** Medium. **Read first:** `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:12-39`. **Acceptance criteria:** seeder creates expected workspace/channels once.

## Ticket 15: Add UserProfile Rename Test
**Difficulty:** Easy. **Read first:** `src/NetSlack.Domain/Users/UserProfile.cs:28-40`. **Acceptance criteria:** blank name rejected, valid name updates.

## Ticket 16: Add Channel Member Idempotency Test
**Difficulty:** Easy. **Read first:** `src/NetSlack.Domain/Channels/Channel.cs:54-62`. **Acceptance criteria:** joining same user twice creates one member.

## Ticket 17: Add Workspace Member Idempotency Test
**Difficulty:** Easy. **Read first:** `src/NetSlack.Domain/Workspaces/Workspace.cs:47-55`. **Acceptance criteria:** adding same user twice creates one member.

## Ticket 18: Add ProblemDetails Client Display
**Difficulty:** Medium. **Read first:** `client/src/api/http.ts:95-104`; `client/src/features/workspace/WorkspaceShell.tsx:220-221`. **Acceptance criteria:** trace id appears in dev-only error details.

## Ticket 19: Add API Contract Docs
**Difficulty:** Easy. **Read first:** `src/NetSlack.Api/Contracts/*.cs`; endpoints in `src/NetSlack.Api/Endpoints/*.cs`. **Acceptance criteria:** docs table lists routes and payloads.

## Ticket 20: Add CancellationToken Test Design Doc
**Difficulty:** Easy. **Read first:** `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:16-31`; `src/NetSlack.Application/Messages/MessageService.cs:10-50`. **Acceptance criteria:** short docs note explains cancellation flow.

