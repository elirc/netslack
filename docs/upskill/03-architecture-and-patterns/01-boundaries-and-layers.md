# Boundaries And Layers

## Layer Rules

| Layer | Owns | Must not own | Evidence |
| --- | --- | --- | --- |
| Domain | Invariants and entity behavior | HTTP, EF, JWT, React | `src/NetSlack.Domain/Messages/Message.cs:31-48` |
| Application | Use cases, authorization, DTOs | HTTP details, concrete DbContext construction | `src/NetSlack.Application/Messages/MessageService.cs:8-50` |
| Infrastructure | EF, JWT, seed data | UI state or business policy | `src/NetSlack.Infrastructure/ServiceCollectionExtensions.cs:15-65` |
| API | Transport, middleware, hubs | Domain invariants | `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:26-35` |
| Client | UX state and HTTP calls through API client | Server authorization | `client/src/features/workspace/WorkspaceShell.tsx:14-36` |

## Boundary Leaks To Watch

- Endpoint business logic: current endpoints are thin, for example `src/NetSlack.Api/Endpoints/WorkspaceEndpoints.cs:17-24`.
- UI authorization: sidebar filters are convenience only; server checks membership in `src/NetSlack.Application/Channels/ChannelService.cs:49-63`.
- Domain persistence leakage: EF configs ignore domain events in `src/NetSlack.Infrastructure/Data/Configurations/MessageConfiguration.cs:14-15`, keeping persistence concerns out of entity public API.

## Drill

Design "archive channel." Put each responsibility in a layer:

- Request body contract.
- Authorization rule.
- Domain state transition.
- EF mapping/index.
- UI button state.
- Tests.

Self-grade: Strong answers keep authorization in Application and do not expose EF entities to React.

