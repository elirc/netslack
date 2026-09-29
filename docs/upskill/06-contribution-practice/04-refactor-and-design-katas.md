# Refactor And Design Katas

1. **Boundary leak kata:** Fake PR moves channel creation into endpoint. Use `src/NetSlack.Api/Endpoints/ChannelEndpoints.cs:17-29` and `src/NetSlack.Application/Channels/ChannelService.cs:29-63` to review it.
2. **Outbox kata:** Design event persistence around `src/NetSlack.Domain/Common/Entity.cs:18-22` and message publish `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`.
3. **Split module kata:** Split `WorkspaceShell` around data loading (`client/src/features/workspace/WorkspaceShell.tsx:38-115`) and rendering (`client/src/features/workspace/WorkspaceShell.tsx:153-289`).
4. **Type safety kata:** Replace prompt-based creation in `client/src/features/workspace/WorkspaceShell.tsx:117-141` with typed form state and validation.
5. **Migration kata:** Create a migration plan for adding `ArchivedAt` to channels. Anchor to `src/NetSlack.Domain/Channels/Channel.cs:26-38` and `src/NetSlack.Infrastructure/Data/Configurations/ChannelConfiguration.cs:11-25`.
6. **N+1 kata:** Review message author projection in `src/NetSlack.Application/Messages/MessageService.cs:21-31`; write fake bad version and explain cost.
7. **RFC kata:** Write an RFC for OIDC migration using `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:15-40` as the thing being replaced.
8. **Flawed PR kata:** Review a PR that adds `channelId` group join without auth; use `src/NetSlack.Api/Hubs/MessageHub.cs:10-13`.

Self-grade: senior answers include migration, rollback, tests, security, performance, and owner communication.

