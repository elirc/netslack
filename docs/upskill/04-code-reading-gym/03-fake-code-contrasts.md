# Fake Code Contrasts

All snippets below are **Illustrative fake code: not from this repo**.

## 1. Entity Leak
Bad: return EF `Message` from endpoint. Better: return `MessageDto` like `src/NetSlack.Application/Messages/MessageDto.cs:5-11`. Failure: public API becomes coupled to persistence shape.

## 2. Missing Permission Filter
Bad:
```csharp
// Illustrative fake code: not from this repo
return db.Messages.Where(m => m.ChannelId == channelId).ToListAsync();
```
Better: call access guard first as in `src/NetSlack.Application/Messages/MessageService.cs:12` and `src/NetSlack.Application/Messages/MessageService.cs:53-76`.

## 3. N+1 Author Lookup
Bad: loop messages then query each author. Better: join/projection in `src/NetSlack.Application/Messages/MessageService.cs:21-31`.

## 4. Unbounded Query
Bad: return every message in a channel. Better: clamp and `Take` in `src/NetSlack.Application/Messages/MessageService.cs:14-20`.

## 5. Side Effect Before Commit
Bad: publish SignalR before saving. Better: save in `src/NetSlack.Application/Messages/MessageService.cs:41-42`, then publish in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34`. Senior caveat: outbox is stronger.

## 6. Swallowed Exception
Bad: catch and ignore SignalR connection errors. Better: current hook logs in `client/src/hooks/useSignalRChannel.ts:30-32`; production should show reconnect state.

## 7. Client-Trusted User Id
Bad: request body contains `authorId`. Better: server uses `currentUser.UserId` in `src/NetSlack.Application/Messages/MessageService.cs:39`.

## 8. Public Setters Everywhere
Bad: entity properties have public setters. Better: private setters and factories in `src/NetSlack.Domain/Messages/Message.cs:21-38`.

## 9. Manual String SQL
Bad: concatenate SQL with channel id. Better: LINQ with parameters in `src/NetSlack.Application/Messages/MessageService.cs:16-33`.

## 10. Casual Contract Change
Bad: rename `accessToken` client-side only. Better: coordinate API contract `src/NetSlack.Api/Contracts/AuthContracts.cs:7` and TypeScript type `client/src/api/types.ts:8-11`.

