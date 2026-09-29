# Framework Mental Models

## ASP.NET Core Pipeline

**Concept:** Middleware order changes behavior. Authentication must run before authorization, and exception handling should wrap later middleware.

**Real code:** Exception middleware is first in `src/NetSlack.Api/Program.cs:59`. HTTPS/CORS/auth/authorization are ordered in `src/NetSlack.Api/Program.cs:70-73`. Endpoints and SignalR are mapped afterward in `src/NetSlack.Api/Program.cs:75-80`.

**Failure modes:** CORS after endpoints may not apply; authorization before authentication cannot identify users; exception middleware too late misses failures.

**Drill:** Move `UseAuthorization` mentally before `UseAuthentication`. What breaks and why?

## Dependency Injection Lifetimes

**Concept:** DI controls object lifetime. Singleton lives for process lifetime, scoped usually lives for one request, transient is new per resolution.

**Real code:** `IClock` is singleton and application services are scoped in `src/NetSlack.Application/Common/ServiceCollectionExtensions.cs:10-16`. `AppDbContext` is scoped by `AddDbContext` in `src/NetSlack.Infrastructure/ServiceCollectionExtensions.cs:17-18`.

**Failure modes:** captive dependency bug: a singleton holding a scoped `DbContext` leaks request state and can fail under concurrency.

**Drill:** Which registered services are safe singletons? Why is `SystemClock` safe but `MessageService` is not?

## Model Binding And Contracts

**Concept:** Minimal APIs bind route parameters, query parameters, JSON bodies, and services from the delegate signature.

**Real code:** `MessageEndpoints` binds route `channelId`, query `take`, service `MessageService`, and `CancellationToken` in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:16-23`. Contract records are simple body shapes in `src/NetSlack.Api/Contracts/MessageContracts.cs:1-4`.

**Failure modes:** putting domain entities in request bodies couples external contract to internal model; missing validation moves errors deeper than necessary.

**Drill:** Add a hypothetical `EditMessageRequest`. Which project should own it?

## EF Core Query Mental Model

**Concept:** `IQueryable` builds SQL until materialized by `ToListAsync`, `SingleAsync`, etc.

**Real code:** `GetRecentAsync` chains filters/projection and materializes at `ToListAsync` in `src/NetSlack.Application/Messages/MessageService.cs:16-33`. `ChannelService` materializes channel DTOs in `src/NetSlack.Application/Channels/ChannelService.cs:13-26`.

**Failure modes:** calling `.ToList()` too early moves filtering into memory; navigation access without includes can cause N+1 in other EF setups.

**Drill:** Point to the exact line where the message query becomes a database call.

## React Render Model

**Concept:** React render calculates UI from state/props. Effects run after render and must clean up subscriptions.

**Real code:** `WorkspaceShell` state is declared in `client/src/features/workspace/WorkspaceShell.tsx:14-20`; memoized active objects are derived in `client/src/features/workspace/WorkspaceShell.tsx:22-30`; effects load data in `client/src/features/workspace/WorkspaceShell.tsx:38-115`.

**Failure modes:** stale closures, missing dependencies, setting state after unmount. This repo uses `disposed` flags in `client/src/features/workspace/WorkspaceShell.tsx:38-60`, `client/src/features/workspace/WorkspaceShell.tsx:68-89`, and `client/src/features/workspace/WorkspaceShell.tsx:98-114`.

**Drill:** Remove `api` from an effect dependency array mentally. What bug might appear after sign-in refresh?

