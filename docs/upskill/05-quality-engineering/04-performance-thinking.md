# Performance Thinking

Measure first. Then reduce the biggest bottleneck with the smallest change.

## Likely Hotspots

| Area | Evidence | What to measure |
| --- | --- | --- |
| Message history query | `src/NetSlack.Application/Messages/MessageService.cs:16-33` | SQL, rows scanned, latency, allocations. |
| Channel listing | `src/NetSlack.Application/Channels/ChannelService.cs:13-26` | Count subquery cost, private filter. |
| SignalR connection churn | `client/src/hooks/useSignalRChannel.ts:6-42` | Reconnect rate, duplicate connections. |
| React shell renders | `client/src/features/workspace/WorkspaceShell.tsx:14-36` | Render count when messages arrive. |
| Startup seeding | `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:12-39` | Dev startup delay. |
| Bundle size | `client/package.json:12-17` | Vite bundle output after install. |

## Senior Checks

- Is the query bounded? Message history clamps `take` in `src/NetSlack.Application/Messages/MessageService.cs:14`.
- Does an index match the filter/order? Message index is `ChannelId, PostedAt` in `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:17`.
- Are entities tracked unnecessarily? Message read uses `AsNoTracking` in `src/NetSlack.Application/Messages/MessageService.cs:17`.
- Are renders keyed correctly? Message list uses `message.id` keys in `client/src/features/workspace/WorkspaceShell.tsx:241-253`.
- Are network calls duplicated? Workspace/channel/message effects are separate in `client/src/features/workspace/WorkspaceShell.tsx:38-115`.

## Drill

Design a performance investigation for a channel with 1 million messages. Include query plan, index review, cursor pagination, API response size, UI virtualization, and retention.

