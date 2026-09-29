# Security Checklist

## Pre-Merge Checklist

| Check | Code anchor | Question |
| --- | --- | --- |
| Authentication | `src/NetSlack.Infrastructure/ServiceCollectionExtensions.cs:29-65` | Is the endpoint protected unless intentionally anonymous? |
| Authorization | `src/NetSlack.Application/Messages/MessageService.cs:53-114` | Does the service check resource access, not just login? |
| IDOR | `src/NetSlack.Application/Channels/ChannelService.cs:49-63` | Can a caller use another workspace/channel id? |
| SignalR authz | `src/NetSlack.Api/Hubs/MessageHub.cs:10-13` | Does group join validate membership? Investigate. |
| Input validation | `src/NetSlack.Domain/Common/Guard.cs:5-18` | Are invariants enforced server-side? |
| SQL injection | `src/NetSlack.Application/Messages/MessageService.cs:16-33` | Are queries parameterized through LINQ rather than strings? |
| XSS | `client/src/features/workspace/WorkspaceShell.tsx:250` | React escapes text by default; avoid `dangerouslySetInnerHTML`. |
| CSRF | `client/src/api/http.ts:65-68` | Bearer token header reduces classic cookie CSRF; token storage has XSS tradeoff. |
| Secrets | `src/NetSlack.Api/appsettings.json:8-12` | Development signing key must move to secret manager in production. |
| CORS | `src/NetSlack.Api/Program.cs:25-34` | Origins must be explicit in production. |
| Rate limits | No rate limiting found | Add limits for auth and message posting. |
| Dependency risk | `client/package.json:12-23`, `*.csproj` package refs | Audit packages in CI. |
| File uploads/SSRF | No upload/external HTTP found | Reassess when adding integrations. |

## Drill

Write a threat model for `/api/channels/{channelId}/messages`. Include spoofed author, guessed channel id, XSS in message body, replayed JWT, and SignalR group abuse.

