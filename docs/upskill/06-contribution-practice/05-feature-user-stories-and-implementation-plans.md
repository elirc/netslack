# Feature User Stories And Implementation Plans

This workbook contains 20 new feature stories. Each feature is intentionally scoped so a junior engineer can build it by following the repo's existing patterns: domain invariants in `src/NetSlack.Domain`, use cases in `src/NetSlack.Application`, persistence in `src/NetSlack.Infrastructure`, transport in `src/NetSlack.Api`, typed client calls in `client/src/api`, and UI behavior in `client/src/features/workspace`.

Before implementing any story:

1. Re-read the thin endpoint pattern in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35`.
2. Re-read service-level authorization in `src/NetSlack.Application/Messages/MessageService.cs:53-114`.
3. Re-read domain guard usage in `src/NetSlack.Domain/Common/Guard.cs:5-18`.
4. Re-read EF configuration style in `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:11-27`.
5. Re-read React API/client boundaries in `client/src/api/http.ts:15-87` and `client/src/features/workspace/WorkspaceShell.tsx:14-36`.

## Feature 1: Edit Message

**User story:** As a message author, I want to edit a message I posted so that I can correct mistakes without deleting conversation history.

**Why it matters:** Editing teaches ownership checks, domain mutation, optimistic UI choices, and audit-risk thinking.

**Read first:**

- `src/NetSlack.Domain/Messages/Message.cs:44-48` - existing edit behavior.
- `src/NetSlack.Application/Messages/MessageService.cs:36-50` - existing post flow.
- `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35` - endpoint style.
- `client/src/features/workspace/WorkspaceShell.tsx:230-255` - message rendering.

**Likely files touched:**

- `src/NetSlack.Application/Messages/MessageDto.cs` - add `EditMessageCommand`.
- `src/NetSlack.Application/Messages/MessageService.cs` - add edit use case.
- `src/NetSlack.Api/Contracts/MessageContracts.cs` - add `EditMessageRequest`.
- `src/NetSlack.Api/Endpoints/MessageEndpoints.cs` - add `PATCH /api/channels/{channelId}/messages/{messageId}`.
- `client/src/api/http.ts` - add `editMessage`.
- `client/src/features/workspace/WorkspaceShell.tsx` - add edit UI.
- `tests/NetSlack.Application.Tests/WorkflowTests.cs` - add author-only edit tests.

**Implementation plan:**

1. Add `public sealed record EditMessageCommand(string Body);` near `PostMessageCommand` in `MessageDto.cs`.
2. Add `public sealed record EditMessageRequest(string Body);` in API contracts.
3. In `MessageService`, add `EditAsync(Guid channelId, Guid messageId, EditMessageCommand command, CancellationToken cancellationToken)`.
4. Inside `EditAsync`, call the same channel access logic used by posting. Reuse `EnsureCanPostChannelAsync` or extract a shared helper so private-channel membership remains protected.
5. Load the message by `messageId` and `channelId`. Use both ids so a guessed message id from another channel cannot be edited.
6. If missing, throw `NotFoundException`, matching existing exception style in `src/NetSlack.Application/Messages/MessageService.cs:90-93`.
7. If `message.AuthorId != currentUser.UserId`, throw `ForbiddenException`.
8. Call `message.Edit(command.Body, clock.UtcNow)`. The domain method already enforces non-empty/max length through `Guard.AgainstEmpty` in `src/NetSlack.Domain/Messages/Message.cs:44-48`.
9. Save changes and project to `MessageDto`.
10. Add endpoint mapping that calls service and broadcasts the updated message. Decide whether to reuse `MessagePosted` SignalR method or add `MessageEdited`; prefer `MessageEdited` to avoid semantic confusion.
11. Add client API method. Keep fetch/auth/error handling inside `ApiClient`, following `client/src/api/http.ts:52-57`.
12. In React, add an "Edit" control only for current user's messages. Keep server authorization as the real protection.
13. Update local `messages` state by id after success.

**Tests:**

- Author can edit and `EditedAt` is set.
- Non-author cannot edit.
- Blank edit body fails.
- Message id from another channel cannot be edited through this channel route.

**Review questions:**

- Is authorship derived from server state, not request body?
- Does edit preserve original `PostedAt`?
- Does realtime update use an event name that clients can distinguish from create?

## Feature 2: Delete Message Softly

**User story:** As a message author, I want to delete a message while preserving an audit trail so that mistakes can be removed without corrupting compliance history.

**Read first:** `src/NetSlack.Domain/Messages/Message.cs:21-29`, `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:11-27`, `src/NetSlack.Application/Messages/MessageService.cs:53-114`.

**Likely files touched:** message entity, message mapping, message service, endpoint contracts/endpoints, client API, message UI, tests, migration if migrations are introduced.

**Implementation plan:**

1. Add `DeletedAt` and `DeletedById` nullable properties to `Message`.
2. Add `Delete(Guid deletedById, DateTimeOffset deletedAt)` domain method. It should be idempotent: if already deleted, return without changing `DeletedAt`.
3. Decide display behavior: keep row but return body as `[message deleted]`, or filter deleted rows. For Slack-like behavior, keep a tombstone.
4. Update `MessageDto` with `DeletedAt` and maybe `IsDeleted`.
5. Update EF mapping with nullable columns. If migrations exist by then, add migration; if not, document schema gap.
6. In `MessageService`, add `DeleteAsync(channelId, messageId, cancellationToken)`.
7. Reuse channel access checks, load message by channel/message id, check author or future admin role.
8. Save changes.
9. Endpoint: `DELETE /api/channels/{channelId}/messages/{messageId}` returns updated tombstone DTO or `204`.
10. Client: hide body text and disable edit/delete when `isDeleted`.
11. SignalR: add `MessageDeleted` event and update local state by id.

**Tests:** author can delete; non-author cannot; deleted message no longer exposes body; second delete is harmless; private channel outsider cannot delete.

**Pitfalls:** hard delete can break audit and thread/reaction relationships; client-side hiding is not authorization.

## Feature 3: Emoji Reactions

**User story:** As a workspace member, I want to react to a message with an emoji so that lightweight acknowledgement does not require extra messages.

**Read first:** `src/NetSlack.Domain/Messages/Message.cs:21-29`, `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:17-26`, `src/NetSlack.Application/Messages/MessageService.cs:53-114`, `client/src/features/workspace/WorkspaceShell.tsx:240-255`.

**Implementation plan:**

1. Create domain entity `MessageReaction` with `MessageId`, `UserId`, `Emoji`, `CreatedAt`.
2. Use composite key `{ MessageId, UserId, Emoji }` to make add reaction idempotent, mirroring membership composite keys in `src/NetSlack.Infrastructure/Data/Configurations/ChannelMemberConfiguration.cs:11-22`.
3. Add `DbSet<MessageReaction>` to `IAppDbContext` and `AppDbContext`.
4. Add EF configuration with FK to `Message` cascade delete and FK to `UserProfile` restrict/cascade based on product policy.
5. Add DTO shape: emoji, count, whether current user reacted.
6. Update message query projection. Start simple: query reactions separately for visible message ids after `GetRecentAsync`. Avoid per-message N+1.
7. Add service methods `AddReactionAsync` and `RemoveReactionAsync`; both must call message channel access checks first.
8. Add endpoints under `/api/channels/{channelId}/messages/{messageId}/reactions`.
9. Add client methods and UI buttons under each message.
10. Add SignalR events `ReactionAdded` and `ReactionRemoved`.

**Tests:** add reaction, duplicate reaction does not duplicate, remove reaction, outsider cannot react, reaction counts project correctly.

**Review questions:** Is emoji validated to a known set? Does reaction loading avoid N+1? Is the composite key documented?

## Feature 4: Message Threads

**User story:** As a user, I want to reply in a thread so that side conversations do not overwhelm the main channel.

**Read first:** `src/NetSlack.Domain/Messages/Message.cs:21-29`, `src/NetSlack.Application/Messages/MessageService.cs:10-50`, `client/src/features/workspace/WorkspaceShell.tsx:223-255`.

**Implementation plan:**

1. Add nullable `ParentMessageId` to `Message`.
2. Add domain factory overload or new `Reply` factory that requires parent id.
3. Add EF self-reference mapping. Use restrict delete to avoid cascade surprises.
4. Add index `(ParentMessageId, PostedAt)` for thread reads.
5. Add service method `ReplyAsync(channelId, parentMessageId, body, cancellationToken)`.
6. Verify parent exists in same channel before creating reply.
7. Add query method `GetThreadAsync(channelId, parentMessageId, take, cancellationToken)`.
8. Endpoints: `GET /api/channels/{channelId}/messages/{messageId}/thread`, `POST /api/channels/{channelId}/messages/{messageId}/thread`.
9. Client: add "Reply" button, side panel or inline thread panel, thread message list, composer.
10. SignalR: decide whether thread replies appear in main channel, thread panel, or both.

**Tests:** reply to existing parent; reject parent from another channel; outsider cannot read thread; thread results are bounded.

**Pitfalls:** recursive threads can create complex UX. Keep one-level threads for the first implementation.

## Feature 5: File Attachments Metadata

**User story:** As a user, I want to attach files to a message so that conversation can include supporting artifacts.

**Read first:** `src/NetSlack.Domain/Messages/Message.cs:31-41`, `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35`, `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:11-27`.

**Implementation plan:**

1. Start with metadata only: `MessageAttachment` entity with `Id`, `MessageId`, `FileName`, `ContentType`, `SizeBytes`, `StorageKey`, `CreatedAt`.
2. Do not store raw bytes in SQLite.
3. Add domain validation: file name required, size positive, content type allowlist.
4. Add EF mapping and index by `MessageId`.
5. Add `AttachmentDto` and include it in `MessageDto`.
6. Add an upload-init endpoint that returns a fake/local upload target for now, clearly documented.
7. Add message post request support for attachment ids or storage keys.
8. Add client file picker and show attachment chips under messages.
9. Add security checks: max size, content type, file extension mismatch risk.
10. Future: replace local/fake storage with S3/Azure Blob signed URLs.

**Tests:** metadata validation; posting message with attachment; outsider cannot access attachment metadata; large file rejected.

**Review questions:** Are upload bytes virus-scanned in production? Are signed URLs short-lived? Is SSRF avoided?

## Feature 6: Archive Channel

**User story:** As a workspace admin, I want to archive a channel so that stale channels stop receiving messages but history remains readable.

**Read first:** `src/NetSlack.Domain/Channels/Channel.cs:26-38`, `src/NetSlack.Application/Channels/ChannelService.cs:29-63`, `src/NetSlack.Application/Messages/MessageService.cs:79-114`.

**Implementation plan:**

1. Add `ArchivedAt` and `ArchivedById` to `Channel`.
2. Add domain method `Archive(Guid archivedById, DateTimeOffset archivedAt)` and `Unarchive(...)` if desired.
3. Add EF nullable columns and index if archived filtering becomes common.
4. Add application method `ArchiveAsync(workspaceId, channelId, cancellationToken)`.
5. Add role check. Current roles exist in `src/NetSlack.Domain/Workspaces/WorkspaceRole.cs:1-9`, but channel service currently only checks membership in `src/NetSlack.Application/Channels/ChannelService.cs:49-63`; add admin/owner policy carefully.
6. Add endpoint `POST /api/workspaces/{workspaceId}/channels/{channelId}/archive`.
7. Update `MessageService.EnsureCanPostChannelAsync` to reject posting to archived channels.
8. Client: mark archived channels, disable composer, hide create controls if role policy says so.
9. Tests: admin can archive; member cannot; archived channel history readable; archived channel rejects new posts.

**Pitfalls:** Archiving must not delete messages or memberships.

## Feature 7: Rename Channel And Edit Topic

**User story:** As a channel admin, I want to rename a channel and update its topic so that channel purpose stays accurate.

**Read first:** `src/NetSlack.Domain/Channels/Channel.cs:64-79`, `src/NetSlack.Application/Channels/ChannelService.cs:29-47`, `src/NetSlack.Infrastructure/Data/Configurations/ChannelConfiguration.cs:14-19`.

**Implementation plan:**

1. Add `UpdateChannelCommand(string Name, string Topic)`.
2. Add API request record.
3. In `ChannelService`, add `UpdateAsync(workspaceId, channelId, command, cancellationToken)`.
4. Verify current user belongs to workspace.
5. Add role check if this feature is admin-only.
6. Load channel by workspace/channel id.
7. Check duplicate normalized name in the same workspace, excluding this channel.
8. Call `channel.UpdateDetails(command.Name, command.Topic)`.
9. Save and return `ChannelDto`.
10. Endpoint: `PATCH /api/workspaces/{workspaceId}/channels/{channelId}`.
11. Client: add edit channel dialog with current name/topic.
12. Update local channel list by id after success.

**Tests:** rename success; duplicate name rejected; invalid name rejected; outsider cannot rename.

## Feature 8: Workspace Invites

**User story:** As a workspace admin, I want to invite a user by email so that teams can grow without database seeding.

**Read first:** `src/NetSlack.Domain/Workspaces/Workspace.cs:47-55`, `src/NetSlack.Domain/Users/UserProfile.cs:28-40`, `src/NetSlack.Application/Workspaces/WorkspaceService.cs:22-40`.

**Implementation plan:**

1. Create `WorkspaceInvite` entity: `Id`, `WorkspaceId`, `Email`, `Role`, `TokenHash`, `ExpiresAt`, `AcceptedAt`, `CreatedById`.
2. Add EF config with unique active invite per workspace/email if product wants that.
3. Add `InviteUserCommand`.
4. Add service method `CreateInviteAsync(workspaceId, command, cancellationToken)`.
5. Check inviter is owner/admin.
6. Normalize email like `UserProfile.Create` does in `src/NetSlack.Domain/Users/UserProfile.cs:28-35`.
7. Generate token through an infrastructure service, but store only hash.
8. For first version, return invite link in response instead of sending email.
9. Add accept endpoint. It should create or find user, call `workspace.AddMember`, mark invite accepted, save once.
10. Client: admin invite form.

**Tests:** admin creates invite; member cannot; expired invite rejected; accepting twice is idempotent.

**Pitfalls:** never store raw invite tokens; avoid account enumeration in public responses.

## Feature 9: Role-Based Permissions

**User story:** As a workspace owner, I want roles to control who can manage channels and invites so that workspace administration is safe.

**Read first:** `src/NetSlack.Domain/Workspaces/WorkspaceRole.cs:1-9`, `src/NetSlack.Application/Channels/ChannelService.cs:49-63`, `src/NetSlack.Domain/Workspaces/WorkspaceMember.cs:11-20`.

**Implementation plan:**

1. Add an application helper, for example `WorkspacePermissionService`, not a UI-only check.
2. Implement methods like `RequireWorkspaceRoleAsync(workspaceId, allowedRoles, cancellationToken)`.
3. Query `WorkspaceMembers` by `workspaceId` and `currentUser.UserId`.
4. Use it in channel creation, channel archive, channel rename, and invites.
5. Keep `Owner`, `Admin`, `Member`, `Guest` behavior documented in one table.
6. Add tests per role.
7. Client can hide buttons based on role only after API exposes current user's role in workspace/channel DTOs.
8. Do not accept role from client for authorization decisions.

**Tests:** owner/admin allowed; member/guest forbidden; non-member forbidden; missing user forbidden.

**Review questions:** Is role policy centralized? Are new endpoints using it?

## Feature 10: User Presence

**User story:** As a user, I want to see who is online so that I know whether teammates are available.

**Read first:** `src/NetSlack.Api/Hubs/MessageHub.cs:7-20`, `client/src/hooks/useSignalRChannel.ts:6-42`, `client/src/features/workspace/WorkspaceShell.tsx:182-188`.

**Implementation plan:**

1. Add `PresenceHub` or extend existing hub carefully. Prefer separate hub if presence grows.
2. Track connection id to user id in an in-memory store for first version.
3. On connect, mark user online; on disconnect, mark offline if no more active connections.
4. Broadcast `PresenceChanged` to workspace groups.
5. Add endpoint or hub method to join workspace presence group after validating workspace membership.
6. Add client hook `usePresence`.
7. Add presence indicator next to current user and message authors.
8. Document limitation: in-memory presence breaks across multiple API nodes.
9. Future: Redis/Azure SignalR backplane.

**Tests:** service-level membership check; hub integration if test infrastructure exists; client hook cleanup.

**Pitfalls:** presence is eventually consistent; disconnect events are not guaranteed immediately.

## Feature 11: Typing Indicators

**User story:** As a user, I want to see when someone is typing in a channel so that conversation feels responsive.

**Read first:** `src/NetSlack.Api/Hubs/MessageHub.cs:10-20`, `client/src/hooks/useSignalRChannel.ts:19-27`, `client/src/features/workspace/WorkspaceShell.tsx:258-289`.

**Implementation plan:**

1. Add hub method `Typing(Guid channelId)` that validates channel access before broadcasting.
2. Broadcast `UserTyping` to channel group except caller.
3. Client composer sends typing event with debounce, for example at most once every 2 seconds.
4. Client stores typing users per channel with expiry timestamp.
5. Clear typing indicator when message posts or timeout expires.
6. Do not persist typing events.
7. Add tests for authorization helper, not for timing-heavy UI unless using fake timers.

**Pitfalls:** without debounce, every keypress becomes network traffic; without auth, typing leaks private channel membership.

## Feature 12: Unread Counts

**User story:** As a user, I want unread counts per channel so that I can prioritize where to catch up.

**Read first:** `src/NetSlack.Application/Messages/MessageService.cs:10-33`, `src/NetSlack.Domain/Channels/ChannelMember.cs:11-18`, `client/src/features/workspace/WorkspaceShell.tsx:197-208`.

**Implementation plan:**

1. Add `LastReadAt` to `ChannelMember`.
2. Add domain method to mark read.
3. Add EF mapping for nullable/required timestamp.
4. Add service `MarkChannelReadAsync(channelId, cancellationToken)` with channel access check.
5. Add channel list projection field `UnreadCount`.
6. Query unread count as messages after `LastReadAt`. Watch performance; add index already exists on `(ChannelId, PostedAt)` in `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:17`.
7. Endpoint: `POST /api/channels/{channelId}/read`.
8. Client: call mark-read when active channel messages load or when channel opens.
9. Update sidebar count badges.

**Tests:** unread count increments on another user's message; mark read resets count; private access enforced.

## Feature 13: Mentions

**User story:** As a user, I want to mention teammates with `@name` so that they can find messages addressed to them.

**Read first:** `src/NetSlack.Application/Messages/MessageService.cs:36-50`, `src/NetSlack.Application/Messages/MessageDto.cs:3-13`, `client/src/features/workspace/WorkspaceShell.tsx:250`.

**Implementation plan:**

1. Decide mention syntax: start with `@email` or `@display-name`. Email is easier but less friendly.
2. Add `MessageMention` entity with `MessageId`, `MentionedUserId`.
3. On post, parse body in application layer after access check. Keep parser deterministic and unit-tested.
4. Resolve mentioned users who belong to the workspace/channel.
5. Persist mentions with message in same `SaveChangesAsync`.
6. Add mentions to `MessageDto`.
7. Client highlights mentions for current user.
8. Add notification event later, not in first version.

**Tests:** mention existing member; ignore non-member; current user mention highlights; private channel mention cannot leak outsider.

**Pitfalls:** display names are not unique; parsing can become surprisingly complex.

## Feature 14: Message Search

**User story:** As a user, I want to search channel messages so that I can find prior decisions.

**Read first:** `src/NetSlack.Application/Messages/MessageService.cs:16-33`, `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:17`, `client/src/api/http.ts:59-85`.

**Implementation plan:**

1. Add `SearchMessagesAsync(workspaceId, query, take, cancellationToken)`.
2. Validate query length and trim.
3. Check workspace membership before searching.
4. Start with SQLite `Contains` for lab, but document limitations.
5. Filter results to channels user can read. Reuse channel access logic or query public/member channels first.
6. Bound `take` to max 50.
7. Return `MessageSearchResultDto` with channel name and snippet.
8. Endpoint: `GET /api/workspaces/{workspaceId}/search/messages?q=...`.
9. Client: search input in sidebar/header, results panel, click result selects channel.
10. Future: full-text index and ranking.

**Tests:** empty query rejected; private channel outsider result excluded; max take enforced.

**Pitfalls:** naive `%LIKE%` search may be slow; never search across unauthorized channels.

## Feature 15: Pinned Messages

**User story:** As a channel member, I want to pin important messages so that decisions and references stay easy to find.

**Read first:** `src/NetSlack.Domain/Messages/Message.cs:21-29`, `src/NetSlack.Application/Messages/MessageService.cs:53-114`, `client/src/features/workspace/WorkspaceShell.tsx:211-224`.

**Implementation plan:**

1. Add `PinnedMessage` entity with `ChannelId`, `MessageId`, `PinnedById`, `PinnedAt`.
2. Use unique key on `MessageId` or `{ ChannelId, MessageId }`.
3. Add service methods `PinMessageAsync`, `UnpinMessageAsync`, `GetPinnedMessagesAsync`.
4. Validate channel access and message belongs to channel.
5. Decide role policy: any channel member or admin only.
6. Add endpoints under `/api/channels/{channelId}/pins`.
7. Client: pin button in message actions; pinned panel in channel header.
8. SignalR: optional `PinChanged` event.

**Tests:** pin existing message; duplicate pin idempotent; message from other channel rejected; non-member forbidden.

## Feature 16: Saved Messages

**User story:** As a user, I want to save messages privately so that I can return to them later.

**Read first:** `src/NetSlack.Application/Messages/MessageService.cs:53-76`, `src/NetSlack.Application/Common/ICurrentUser.cs:1-11`, `client/src/features/workspace/WorkspaceShell.tsx:240-255`.

**Implementation plan:**

1. Add `SavedMessage` entity with `UserId`, `MessageId`, `SavedAt`.
2. Composite key `{ UserId, MessageId }`.
3. Add service methods save, unsave, list saved.
4. Save/unsave must verify current user can read the message's channel.
5. Listing saved messages must re-check access or join through channels/workspace membership.
6. Add endpoints `/api/me/saved-messages` and `/api/channels/{channelId}/messages/{messageId}/save`.
7. Client: save icon per message and a saved-messages view.
8. Tests: save visible message, cannot save inaccessible private message, unsave idempotent.

**Pitfalls:** saved messages are private user state; do not expose to other workspace members.

## Feature 17: Notification Preferences

**User story:** As a user, I want per-channel notification preferences so that I can reduce noise.

**Read first:** `src/NetSlack.Domain/Channels/ChannelMember.cs:11-18`, `src/NetSlack.Infrastructure/Data/Configurations/ChannelMemberConfiguration.cs:11-22`, `client/src/features/workspace/WorkspaceShell.tsx:197-208`.

**Implementation plan:**

1. Add enum `NotificationLevel`: All, MentionsOnly, Muted.
2. Add property to `ChannelMember`.
3. Map enum as string in `ChannelMemberConfiguration`, similar to role mapping in `src/NetSlack.Infrastructure/Data/Configurations/WorkspaceMemberConfiguration.cs:14`.
4. Add service method update preference.
5. Validate current user is updating their own membership.
6. Add preference field to `ChannelDto` for current user.
7. Endpoint: `PATCH /api/channels/{channelId}/notification-preference`.
8. Client: menu on channel row.
9. Future: use preference when sending notifications.

**Tests:** user can update own preference; cannot update another user's preference; non-member forbidden.

## Feature 18: Audit Log

**User story:** As a workspace owner, I want an audit log for administrative actions so that security-sensitive changes are reviewable.

**Read first:** `src/NetSlack.Application/Workspaces/WorkspaceService.cs:22-40`, `src/NetSlack.Application/Channels/ChannelService.cs:29-47`, `src/NetSlack.Domain/Common/Entity.cs:18-22`.

**Implementation plan:**

1. Add `AuditLogEntry` entity: workspace id, actor id, action, target type/id, JSON metadata, created at.
2. Add EF mapping and index `(WorkspaceId, CreatedAt)`.
3. Add application helper `IAuditWriter` or direct `AuditLogService`.
4. Write audit entries for workspace invite, channel archive, role change, message delete.
5. Save audit entry in same transaction as the administrative change where possible.
6. Add read endpoint restricted to owner/admin.
7. Client: admin audit log table with filters.
8. Do not log secrets or raw invite tokens.

**Tests:** action writes audit row; member cannot read audit log; metadata does not include sensitive token.

**Pitfalls:** audit logs must be append-only in production; deletion policy requires compliance input.

## Feature 19: Message Retention Policy

**User story:** As a workspace owner, I want configurable message retention so that the workspace can meet compliance and cost requirements.

**Read first:** `src/NetSlack.Domain/Workspaces/Workspace.cs:23-29`, `src/NetSlack.Domain/Messages/Message.cs:21-29`, `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:17-26`.

**Implementation plan:**

1. Add retention setting to workspace, for example `RetentionDays`.
2. Add validation: null means keep forever; minimum maybe 1 or 30 days depending policy.
3. Add admin service to update retention.
4. Add query to identify expired messages by workspace/channel and `PostedAt`.
5. Because no `BackgroundService` exists, first implement a manual admin endpoint or command-like service. Later add scheduled worker.
6. Use soft delete/tombstone rather than hard delete until compliance policy is explicit.
7. Add audit log if Feature 18 exists.
8. Client: admin settings form.
9. Tests: update policy; expired messages selected; non-admin forbidden.

**Pitfalls:** retention is legal/compliance-sensitive; do not silently delete data in first version.

## Feature 20: Admin Workspace Settings Dashboard

**User story:** As a workspace owner, I want a settings dashboard so that I can manage workspace name, roles, invites, retention, and audit visibility from one place.

**Read first:** `client/src/features/workspace/WorkspaceShell.tsx:153-225`, `src/NetSlack.Application/Workspaces/WorkspaceService.cs:10-40`, `src/NetSlack.Domain/Workspaces/Workspace.cs:42-45`.

**Implementation plan:**

1. Start with read-only dashboard route/panel inside existing React app. Avoid adding a router until needed.
2. Add API endpoint `GET /api/workspaces/{workspaceId}/settings` returning workspace name, slug, current user role, counts.
3. Add application DTO and service method. Check current user is workspace member; expose admin-only fields only to admin/owner.
4. Add rename workspace endpoint using existing `Workspace.Rename` in `src/NetSlack.Domain/Workspaces/Workspace.cs:42-45`.
5. Add client API methods.
6. Add dashboard component with sections: General, Members, Invites, Retention, Audit.
7. Build one section fully first, likely General settings.
8. Add tests for settings visibility and rename authorization.
9. Wire future feature links to stories 8, 9, 18, and 19.

**Tests:** member can read basic settings; only owner/admin can rename; invalid name rejected; UI displays API errors.

**Review questions:** Is dashboard becoming a god component? Should settings become its own feature folder?

## Feature 21: Channel Member List

**User story:** As a channel member, I want to see who belongs to a channel so that I know who can read and participate in the conversation.

**Read first:** `src/NetSlack.Domain/Channels/ChannelMember.cs:11-18`, `src/NetSlack.Application/Channels/ChannelService.cs:9-26`, `src/NetSlack.Infrastructure/Data/Configurations/ChannelMemberConfiguration.cs:11-22`, `client/src/features/workspace/WorkspaceShell.tsx:211-224`.

**Likely files touched:** channel DTOs/service, channel endpoint, client API, conversation header/sidebar UI, tests.

**Implementation plan:**

1. Add `ChannelMemberDto(Guid UserId, string DisplayName, string Email, string AvatarUrl, DateTimeOffset JoinedAt)`.
2. Add `GetMembersAsync(Guid workspaceId, Guid channelId, CancellationToken cancellationToken)` to `ChannelService`.
3. First call or reuse workspace/channel access validation. A user should only see members for public channels in their workspace or private channels they belong to.
4. Query `ChannelMembers` filtered by `channelId`, join to `Users`, order by display name, and project to DTOs.
5. Add endpoint `GET /api/workspaces/{workspaceId}/channels/{channelId}/members`.
6. Add `getChannelMembers(workspaceId, channelId)` to `ApiClient`.
7. In the client, make the member count pill in `client/src/features/workspace/WorkspaceShell.tsx:217` open a member panel.
8. Display avatar initials using the existing `initials` helper in `client/src/features/workspace/WorkspaceShell.tsx:298-305`.
9. Add loading and error states inside the panel, not a global app failure.
10. Keep the first version read-only. Do not combine with invite/kick controls yet.

**Tests:** member can list public channel members; private channel outsider forbidden; unknown channel returns not found; result includes user profile fields.

**Pitfalls:** Do not expose private channel membership to workspace members who are not in that private channel.

## Feature 22: Add And Remove Channel Members

**User story:** As a channel admin, I want to add or remove members from a private channel so that access stays aligned with team needs.

**Read first:** `src/NetSlack.Domain/Channels/Channel.cs:54-62`, `src/NetSlack.Application/Channels/ChannelService.cs:49-63`, `src/NetSlack.Domain/Workspaces/WorkspaceMember.cs:11-20`, `src/NetSlack.Infrastructure/Data/Configurations/ChannelMemberConfiguration.cs:11-22`.

**Likely files touched:** channel domain entity, channel service, endpoint contracts/endpoints, EF config only if adding metadata, client UI, tests.

**Implementation plan:**

1. Add request records `AddChannelMemberRequest(Guid UserId)` and possibly `RemoveChannelMemberRequest(Guid UserId)` if using body; route parameter is cleaner for remove.
2. Add `AddMemberAsync(workspaceId, channelId, userId, cancellationToken)` to `ChannelService`.
3. Validate acting user is workspace admin/owner or channel creator. If Feature 9 exists, use centralized role checks.
4. Validate target user is a workspace member before adding them to the channel. Use `WorkspaceMembers` with `workspaceId` and `userId`.
5. Load channel with members by workspace/channel id.
6. Reject adding users to public channels if product decides public channel membership is implicit; otherwise allow idempotent `Join`.
7. Call `channel.Join(userId)`. It is idempotent in `src/NetSlack.Domain/Channels/Channel.cs:54-62`.
8. For remove, add a domain method `Leave(Guid userId)` that refuses to remove the last owner/admin if such policy exists.
9. Add endpoints `POST /api/workspaces/{workspaceId}/channels/{channelId}/members` and `DELETE /api/workspaces/{workspaceId}/channels/{channelId}/members/{userId}`.
10. Client: add member management panel from Feature 21.
11. SignalR: consider notifying removed users to leave channel group.

**Tests:** admin can add workspace member; non-admin forbidden; cannot add non-workspace user; remove member blocks future private-channel reads; operation is idempotent.

**Pitfalls:** Removing a user from a private channel should immediately affect REST and SignalR access.

## Feature 23: Direct Messages

**User story:** As a user, I want to start a direct message with another workspace member so that I can have private one-to-one conversations.

**Read first:** `src/NetSlack.Domain/Channels/ChannelType.cs:1-8`, `src/NetSlack.Domain/Channels/Channel.cs:40-51`, `src/NetSlack.Application/Channels/ChannelService.cs:13-26`, `src/NetSlack.Application/Messages/MessageService.cs:53-114`.

**Likely files touched:** channel domain/service, channel DTOs, endpoints, client channel list, tests.

**Implementation plan:**

1. Decide DM modeling: use `ChannelType.DirectMessage` from `src/NetSlack.Domain/Channels/ChannelType.cs:1-8`.
2. Add service method `CreateDirectMessageAsync(workspaceId, otherUserId, cancellationToken)`.
3. Validate current user and target user are workspace members.
4. Check for an existing direct-message channel with exactly those two members in the workspace. Return it if found to make creation idempotent.
5. Create a channel with type `DirectMessage`. Channel name can be deterministic, but UI should display participant names rather than raw channel name.
6. Ensure both current user and target user are channel members. Existing factory only joins creator in `src/NetSlack.Domain/Channels/Channel.cs:40-51`, so call `Join(otherUserId)` after create.
7. Update channel list query to include DMs only when current user is a member. Current private-channel filter in `src/NetSlack.Application/Channels/ChannelService.cs:16` is close; verify DirectMessage behavior.
8. Add endpoint `POST /api/workspaces/{workspaceId}/direct-messages`.
9. Client: add "New DM" action, user picker, and render DMs separately from channels.
10. Message posting can reuse existing message endpoints because DMs are channels.

**Tests:** create DM once; second create returns same channel; outsider cannot read DM; target must be workspace member; message post works in DM.

**Pitfalls:** Do not let users create DMs with arbitrary user ids outside the workspace.

## Feature 24: Group Direct Messages

**User story:** As a user, I want to start a small private group DM so that a few teammates can coordinate without creating a named channel.

**Read first:** `src/NetSlack.Domain/Channels/ChannelType.cs:1-8`, `src/NetSlack.Domain/Channels/Channel.cs:54-62`, `src/NetSlack.Infrastructure/Data/Configurations/ChannelConfiguration.cs:19-23`, `src/NetSlack.Application/Channels/ChannelService.cs:9-63`.

**Implementation plan:**

1. Decide whether group DMs reuse `DirectMessage` or add a new `ChannelType.GroupDirectMessage`. Adding enum values affects JSON contracts; string enum serialization is configured in `src/NetSlack.Api/Program.cs:13-17`.
2. Add request `CreateGroupDirectMessageRequest(Guid[] UserIds)`.
3. Validate 2-8 participants including current user. Keep first version bounded.
4. Validate all target users are workspace members.
5. Decide idempotency: exact same participant set should return existing group DM, or always create a new conversation. Document the product choice.
6. Create channel and join each participant.
7. Add a display-name projection for group DMs. Do not rely on raw channel name in UI.
8. Endpoint: `POST /api/workspaces/{workspaceId}/group-direct-messages`.
9. Client: multi-select member picker and separate "Direct messages" section.
10. Tests: participant set validation, outsider rejection, exact participant access, posting.

**Pitfalls:** Idempotency for group DMs is a product decision; make it explicit before coding.

## Feature 25: Channel Bookmarks

**User story:** As a channel member, I want to add important links to a channel bookmark bar so that recurring docs and dashboards are easy to find.

**Read first:** `src/NetSlack.Domain/Channels/Channel.cs:26-38`, `src/NetSlack.Application/Channels/ChannelService.cs:49-63`, `client/src/features/workspace/WorkspaceShell.tsx:211-218`.

**Likely files touched:** new domain entity, DbContext/interface, EF config, channel service, endpoints, client header UI, tests.

**Implementation plan:**

1. Add `ChannelBookmark` entity: `Id`, `ChannelId`, `Title`, `Url`, `CreatedById`, `CreatedAt`.
2. Validate title is required and max length, similar to `Guard.AgainstEmpty` in `src/NetSlack.Domain/Common/Guard.cs:5-18`.
3. Validate URL server-side. Start with absolute `https://` URLs only to reduce open redirect/unsafe scheme risk.
4. Add `DbSet<ChannelBookmark>` to `IAppDbContext` and `AppDbContext`.
5. Add EF configuration with index on `ChannelId`.
6. Add service methods `GetBookmarksAsync`, `CreateBookmarkAsync`, `DeleteBookmarkAsync`.
7. All methods must verify channel read access. Create/delete may require member or admin depending product policy.
8. Add endpoints under `/api/channels/{channelId}/bookmarks`.
9. Client: render bookmark chips under the channel topic in the conversation header.
10. Add create/delete controls in a small menu.

**Tests:** create valid bookmark; reject invalid URL; outsider cannot list private bookmark; delete authorization.

**Pitfalls:** `javascript:` and internal URLs are security risks; keep URL validation strict.

## Feature 26: Slash Commands

**User story:** As a power user, I want slash commands like `/shrug` and `/topic` so that common actions are fast from the composer.

**Read first:** `client/src/features/workspace/WorkspaceShell.tsx:258-289`, `src/NetSlack.Application/Messages/MessageService.cs:36-50`, `src/NetSlack.Domain/Channels/Channel.cs:64-68`.

**Implementation plan:**

1. Keep first version client-assisted but server-authoritative. The client can detect slash commands for UX, but server must enforce real actions.
2. Add `CommandService` in Application or methods on existing services for supported commands.
3. Start with `/shrug text`: transforms into message body `text ¯\_(ツ)_/¯`. This can remain client-side because it only changes the body the user sends.
4. Add `/topic new topic`: call channel update service, not message post.
5. Add parser with explicit command names and argument strings.
6. In the composer submit path, if input starts with `/`, parse and route to the correct API method.
7. Show command errors in the existing banner area in `client/src/features/workspace/WorkspaceShell.tsx:220-221`.
8. Add tests for parser if parser lives in TypeScript; add application tests for server-side topic authorization.
9. Document which commands are local text transforms versus server actions.

**Tests:** `/shrug` posts transformed message; `/topic` updates topic with permission; unknown command shows error and posts nothing.

**Pitfalls:** Do not build arbitrary command execution. Commands are product actions, not scripts.

## Feature 27: Scheduled Messages

**User story:** As a user, I want to schedule a message for later so that I can communicate at the right time without being online.

**Read first:** `src/NetSlack.Application/Messages/MessageService.cs:36-50`, `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35`, `src/NetSlack.Application/Common/IClock.cs:1-6`, `tests/NetSlack.Application.Tests/WorkflowTests.cs:76-79`.

**Implementation plan:**

1. Add `ScheduledMessage` entity: `Id`, `ChannelId`, `AuthorId`, `Body`, `SendAt`, `SentMessageId`, `CancelledAt`, `CreatedAt`.
2. Validate `SendAt` is in the future using `IClock`, not `DateTimeOffset.UtcNow` directly.
3. Add EF mapping and index on `SendAt`.
4. Add service method `ScheduleMessageAsync(channelId, body, sendAt, cancellationToken)` with same channel posting authorization as normal messages.
5. Add methods to list and cancel current user's scheduled messages.
6. Since no background worker exists, first add a service method `DispatchDueMessagesAsync(now, cancellationToken)` and test it directly.
7. Later add `BackgroundService` that calls dispatcher periodically. Document operational concerns.
8. Dispatch should create real messages through domain factory and mark scheduled row as sent in one save.
9. Endpoint: schedule, list mine, cancel.
10. Client: composer dropdown "Schedule send" with date/time input.

**Tests:** cannot schedule in past; due message dispatch creates message; cancel prevents dispatch; private channel access enforced.

**Pitfalls:** Time zones and worker reliability are hard. Store UTC and display local time.

## Feature 28: Draft Messages

**User story:** As a user, I want unsent message drafts to be preserved per channel so that I do not lose work when switching channels.

**Read first:** `client/src/features/workspace/WorkspaceShell.tsx:258-289`, `client/src/App.tsx:77-90`, `client/src/features/workspace/WorkspaceShell.tsx:92-115`.

**Implementation plan:**

1. Start client-only using `localStorage`, similar to auth persistence in `client/src/App.tsx:77-90`.
2. Key drafts by user id and channel id, for example `netslack.drafts.{userId}.{channelId}`.
3. Move composer into its own component file so draft behavior is isolated from `WorkspaceShell`.
4. When channel changes, load draft for that channel.
5. On composer body change, debounce-save draft.
6. On successful send, clear draft for that channel.
7. Add visual "draft saved" hint only if useful; avoid noisy UI.
8. Add unit tests if a React test setup exists; otherwise document manual test steps.
9. Future server-side drafts can reuse same concept with an API.

**Tests/manual checks:** type draft, switch channel, return and see draft; send clears draft; sign out does not show another user's draft.

**Pitfalls:** Do not store drafts under channel id only; that leaks between users on shared browser profiles.

## Feature 29: Channel Sections And Favorites

**User story:** As a user, I want to favorite channels and organize them into sections so that my sidebar matches my workflow.

**Read first:** `client/src/features/workspace/WorkspaceShell.tsx:197-208`, `src/NetSlack.Application/Channels/ChannelService.cs:13-26`, `src/NetSlack.Domain/Channels/ChannelMember.cs:11-18`.

**Implementation plan:**

1. Add user-channel preference entity: `UserChannelPreference` with `UserId`, `ChannelId`, `IsFavorite`, `SectionName`, `SortOrder`.
2. Composite key `{ UserId, ChannelId }`.
3. Add EF mapping and DbSets.
4. Add service methods update preference and list channels with current user's preference fields.
5. Update `ChannelDto` with `IsFavorite`, `SectionName`, `SortOrder`.
6. Ensure preferences only apply if user can access channel.
7. Endpoint: `PATCH /api/channels/{channelId}/preference`.
8. Client: star icon on channel rows, render Favorites section above Channels.
9. For custom sections, start with a simple text field; drag/drop can come later.
10. Tests: favorite channel; cannot favorite inaccessible private channel; channel list orders favorites first.

**Pitfalls:** Preferences are per user, not workspace-global. Avoid changing channel entity for personal UI state.

## Feature 30: Workspace-Wide Announcement Channel

**User story:** As a workspace owner, I want an announcement-only channel so that important updates are visible but replies are controlled.

**Read first:** `src/NetSlack.Domain/Channels/ChannelType.cs:1-8`, `src/NetSlack.Application/Messages/MessageService.cs:79-114`, `src/NetSlack.Application/Channels/ChannelService.cs:29-63`, `src/NetSlack.Domain/Workspaces/WorkspaceRole.cs:1-9`.

**Implementation plan:**

1. Decide whether announcements are a new `ChannelType` or a boolean `IsAnnouncementOnly` on `Channel`. Prefer boolean if a channel can be public/private and announcement-only.
2. Add domain property and method `SetAnnouncementOnly(bool enabled)`.
3. Add EF mapping.
4. Add service method to toggle announcement-only, restricted to owner/admin.
5. Update `MessageService.EnsureCanPostChannelAsync` so only owner/admin can post when announcement-only is enabled.
6. Allow all channel readers to read message history.
7. Add endpoint `PATCH /api/workspaces/{workspaceId}/channels/{channelId}/announcement-settings`.
8. Client: show "Announcement-only" pill and disable composer for users who cannot post.
9. Show helpful disabled composer text: "Only admins can post in this channel."
10. Tests: admin can toggle; member cannot toggle; member cannot post after enabled; admin can post; history remains readable.

**Pitfalls:** Do not confuse read access with post permission. Announcement-only changes write authorization, not channel visibility.
