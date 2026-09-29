# Validation, Auth, And Permissions

## Validation Layers

| Layer | Example | Purpose |
| --- | --- | --- |
| Client | Composer trims and rejects empty input in `client/src/features/workspace/WorkspaceShell.tsx:262-267` | UX feedback only. |
| API contract | Request records in `src/NetSlack.Api/Contracts/*.cs` | Shape of external input. |
| Application | Conflict checks in `src/NetSlack.Application/Channels/ChannelService.cs:33-40` | Use-case validation with persistence context. |
| Domain | `Guard.AgainstEmpty` in `src/NetSlack.Domain/Common/Guard.cs:5-18` | Invariants that must always hold. |
| Database | Unique indexes in `src/NetSlack.Infrastructure/Data/Configurations/WorkspaceConfiguration.cs:17` and `src/NetSlack.Infrastructure/Data/Configurations/ChannelConfiguration.cs:19` | Race-safe constraints. |

## Authentication

JWT authentication is wired in `src/NetSlack.Infrastructure/ServiceCollectionExtensions.cs:29-65`. Token creation adds subject/email/name claims in `src/NetSlack.Infrastructure/Auth/JwtTokenService.cs:21-32`. `CurrentUser` reads claim values in `src/NetSlack.Api/Middleware/CurrentUser.cs:8-21`.

## Authorization

- Endpoints require authenticated users for workspaces/channels/messages in `src/NetSlack.Api/Endpoints/WorkspaceEndpoints.cs:10-12`, `src/NetSlack.Api/Endpoints/ChannelEndpoints.cs:10-12`, and `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:12-14`.
- Workspace membership gates channel listing/creation in `src/NetSlack.Application/Channels/ChannelService.cs:49-63`.
- Message read/write checks are in `src/NetSlack.Application/Messages/MessageService.cs:53-114`.

## Junior Misses Vs Senior Checks

| Junior miss | Senior check |
| --- | --- |
| "The button is hidden, so it is safe." | Can a direct API request pass? Check `MessageService.cs:70-76`. |
| "We already check slug exists." | Is there a unique index for races? Check `WorkspaceConfiguration.cs:17`. |
| "JWT means authorized." | Is resource ownership checked? Check `ChannelService.cs:56-62`. |
| "SignalR hub is authorized." | Does joining a specific group check resource access? Investigate `MessageHub.cs:10-13`. |

## Drill

Threat model private messages. List every place a user-controlled `channelId` is accepted and what check follows.

