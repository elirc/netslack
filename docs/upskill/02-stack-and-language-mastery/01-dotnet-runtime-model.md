# .NET Runtime Model

## CLR, JIT, And Process Startup

**Concept:** ASP.NET Core runs inside a .NET process. The CLR loads assemblies, the JIT compiles methods as they execute, and `Program.cs` builds the host.

**Why it matters:** Startup code commits the process to service lifetimes, middleware order, telemetry, and endpoint surfaces.

**Real code:** `WebApplication.CreateBuilder` starts host construction in `src/NetSlack.Api/Program.cs:11`; services are registered in `src/NetSlack.Api/Program.cs:13-55`; `RunAsync` starts the server in `src/NetSlack.Api/Program.cs:82`.

**Failure modes:** heavy startup side effects make deploys slow; missing config fails late; environment-specific behavior hides production gaps. Development seeding is gated by environment in `src/NetSlack.Api/Program.cs:61-68`.

**Drill:** Draw the startup sequence from `builder` to `app.RunAsync`. Mark every line that can fail before the first request.

## GC, Allocations, And DTO Projection

**Concept:** Managed objects are allocated on the heap and collected by the GC. High-throughput APIs reduce unnecessary tracking and object churn.

**Real code:** Message history uses `AsNoTracking` and projects directly to `MessageDto` in `src/NetSlack.Application/Messages/MessageService.cs:16-33`. The DTO shape is defined as records in `src/NetSlack.Application/Messages/MessageDto.cs:3-13`.

**Failure modes:** loading full EF entities and mapping later can allocate more and track more than needed. Returning entities can also leak internal shape.

**Drill:** Explain why `AsNoTracking` is appropriate for message reads but not for updating a channel membership.

## async/await, Task, CancellationToken

**Concept:** `Task` represents async work. `await` frees the request thread while I/O runs. `CancellationToken` lets the request abort database work.

**Real code:** Endpoints accept `CancellationToken` in `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:16-31`; services pass it into EF calls in `src/NetSlack.Application/Messages/MessageService.cs:16-33` and `src/NetSlack.Application/Messages/MessageService.cs:44-48`.

**Failure modes:** forgetting cancellation creates wasted work after clients disconnect; sync-over-async can starve ASP.NET thread pool; fire-and-forget hides failures.

**Drill:** Identify every `await` in `MessageService`. Which ones are DB I/O? Which one is pure in-memory domain code?

## IDisposable And Resource Lifetime

**Concept:** Disposable resources like DB connections must be scoped and disposed.

**Real code:** Test fixture opens an in-memory SQLite connection and disposes it in `tests/NetSlack.Application.Tests/WorkflowTests.cs:93-112`. ASP.NET creates scoped services per request; `AppDbContext` is registered scoped through `AddDbContext` in `src/NetSlack.Infrastructure/ServiceCollectionExtensions.cs:17-18`.

**Failure modes:** long-lived DbContext instances retain tracking state; undisposed test connections leak handles.

**Drill:** Explain why `DbContext` is scoped, not singleton.

## ConfigureAwait, ValueTask, Spans

No repo use of `ConfigureAwait`, `ValueTask`, or `Span<T>` was found. That is fine. In ASP.NET Core application code, plain `await` is usually correct. Use `ValueTask` only when profiling proves the allocation matters and the API is designed for it.

**Investigate drill:** Search for a hot path before suggesting `ValueTask`. In this repo, likely hot paths would be message history (`src/NetSlack.Application/Messages/MessageService.cs:10-33`) and SignalR receive handling (`client/src/hooks/useSignalRChannel.ts:19-21`).

