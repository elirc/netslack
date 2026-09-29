# Type System And Contracts

## C# Nullable References

**Concept:** Nullable reference types make null a visible design choice.

**Real code:** Enabled globally in `Directory.Build.props:4`. EF materialization constructors initialize strings to empty in `src/NetSlack.Domain/Users/UserProfile.cs:7-12`, `src/NetSlack.Domain/Workspaces/Workspace.cs:9-13`, and `src/NetSlack.Domain/Channels/Channel.cs:9-13`.

**Failure modes:** silencing nullability with `!` without evidence; forgetting EF needs a constructor.

**Drill:** Explain why `private set` plus private constructors still lets EF materialize entities.

## Records And DTOs

**Concept:** Records are concise immutable-ish data carriers. They are good for contracts and command shapes.

**Real code:** Workspace DTO/command records in `src/NetSlack.Application/Workspaces/WorkspaceDto.cs:3-5`, channel DTO/command in `src/NetSlack.Application/Channels/ChannelDto.cs:5-7`, and message DTO/command in `src/NetSlack.Application/Messages/MessageDto.cs:3-13`.

**Failure modes:** using records for mutable domain entities; exposing persistence entities directly.

**Drill:** Compare `MessageDto` with `Message`. Which one should the client know?

## Generics And LINQ

**Concept:** Generic types give compile-time shape. LINQ over `IQueryable<T>` becomes SQL when EF provider translates it.

**Real code:** `DbSet<Message>` is exposed in `src/NetSlack.Application/Common/IAppDbContext.cs:21`; `MessageService` joins messages to users in `src/NetSlack.Application/Messages/MessageService.cs:21-31`.

**Failure modes:** mixing client-side methods into queries that EF cannot translate; building dynamic string queries.

**Drill:** Identify the generic type in `Task<IReadOnlyList<MessageDto>>` at `src/NetSlack.Application/Messages/MessageService.cs:10`.

## Pattern Matching And Exceptions

**Concept:** Pattern matching maps known exception types to HTTP status codes.

**Real code:** `ExceptionHandlingMiddleware` maps app exceptions in `src/NetSlack.Api/Middleware/ExceptionHandlingMiddleware.cs:21-49`; app exceptions are defined in `src/NetSlack.Application/Common/AppException.cs:1-9`.

**Failure modes:** leaking internal exception messages on 500; swallowing exceptions without logs.

**Drill:** Add a hypothetical `ValidationException`. Where would it be mapped?

## TypeScript Contracts

**Concept:** TypeScript mirrors server response shapes so UI code gets compile-time help.

**Real code:** Client types are in `client/src/api/types.ts:1-45`; API client methods return typed promises in `client/src/api/http.ts:18-57`.

**Failure modes:** server enum serialized as number while client expects string. This repo configures string enums in `src/NetSlack.Api/Program.cs:13-17` and `src/NetSlack.Api/Program.cs:36-43`.

**Drill:** Trace `ChannelType` from C# enum to TypeScript union.

