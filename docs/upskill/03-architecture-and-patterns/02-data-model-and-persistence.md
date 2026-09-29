# Data Model And Persistence

## Model

| Entity | Fields/relationships | Evidence |
| --- | --- | --- |
| UserProfile | display name, normalized email, avatar | `src/NetSlack.Domain/Users/UserProfile.cs:22-35` |
| Workspace | name, slug, members | `src/NetSlack.Domain/Workspaces/Workspace.cs:23-39` |
| WorkspaceMember | composite workspace/user, role | `src/NetSlack.Domain/Workspaces/WorkspaceMember.cs:11-20` |
| Channel | workspace, name, topic, type, members | `src/NetSlack.Domain/Channels/Channel.cs:26-38` |
| ChannelMember | composite channel/user | `src/NetSlack.Domain/Channels/ChannelMember.cs:11-18` |
| Message | channel, author, body, timestamps | `src/NetSlack.Domain/Messages/Message.cs:21-29` |

## EF Core Mapping

- DbSets are exposed in `src/NetSlack.Infrastructure/Data/AppDbContext.cs:12-22`.
- Configurations are applied from assembly in `src/NetSlack.Infrastructure/Data/AppDbContext.cs:24-27`.
- Workspace slug is unique in `src/NetSlack.Infrastructure/Data/Configurations/WorkspaceConfiguration.cs:14-17`.
- Channel name is unique per workspace in `src/NetSlack.Infrastructure/Data/Configurations/ChannelConfiguration.cs:19`.
- Message read index is `(ChannelId, PostedAt)` in `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:17`.
- Message author uses restricted delete in `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:23-26`.

## Transactions

EF `SaveChangesAsync` is the transaction boundary for current use cases. Workspace creation adds both workspace and default channel then saves once in `src/NetSlack.Application/Workspaces/WorkspaceService.cs:33-38`. Message posting adds the message and saves before projecting author and publishing SignalR in `src/NetSlack.Application/Messages/MessageService.cs:39-50`.

## Safe Schema Change Guidance

No `Migrations/` folder was found. For real production:

1. Add migrations for schema changes rather than relying on `EnsureCreated` in `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:14`.
2. Backfill nullable columns before making them required.
3. Add indexes concurrently where the database supports it.
4. Write tests for query behavior before changing relationships.
5. Include rollback and data-retention plans.

## Drill

Add reactions to messages. Identify table, composite key, delete behavior, indexes, DTO changes, and tests. Anchor your plan to `MessageConfiguration.cs:11-27` and `MessageDto.cs:3-13`.

