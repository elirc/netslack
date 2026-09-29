# Risk Register

| Risk | Evidence | Impact | Likelihood | Suggested test | Suggested fix | Confidence |
| --- | --- | --- | --- | --- | --- | --- |
| SignalR group join may lack resource authorization | `src/NetSlack.Api/Hubs/MessageHub.cs:10-13` adds group by id; REST checks live in `src/NetSlack.Application/Messages/MessageService.cs:53-76` | Unauthorized realtime message receipt | Medium | Integration test unauthorized user invokes `JoinChannel` | Inject channel-access service into hub | High |
| Dev auth endpoint unsafe for production | `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:15-40` | Account spoofing if deployed | High if not gated | API test/env check | Replace with OIDC; disable in prod | High |
| JWT signing key in appsettings | `src/NetSlack.Api/appsettings.json:8-12` | Secret exposure | Medium | Secret scanning in CI | Secret manager/env var | High |
| No EF migrations found | Seeder uses `EnsureCreated` in `src/NetSlack.Infrastructure/Seed/DevelopmentSeeder.cs:14` | Schema drift, unsafe deploys | High for production | Migration smoke in CI | Add migrations/runbook | High |
| Message publish not durable | `src/NetSlack.Api/Endpoints/MessageEndpoints.cs:33-34` | Lost realtime notifications or inconsistent response | Medium | Force hub failure after DB save | Outbox dispatcher | High |
| Message post not idempotent | `src/NetSlack.Application/Messages/MessageService.cs:36-50` | Duplicate messages on retry | Medium | Retry same request after timeout | Idempotency key/client message id | Medium |
| No rate limiting found | `src/NetSlack.Api/Program.cs:70-80` has no limiter | Abuse/spam | Medium | Load auth/message endpoints | ASP.NET rate limiting | High |
| Client token in localStorage | `client/src/App.tsx:13-21` | XSS token theft | Medium | Security review | Consider BFF/cookie/short-lived tokens | Medium |
| Lint script may fail | `client/package.json:10`, no ESLint deps in `client/package.json:18-23` | Developer workflow break | High | Run `npm run lint` after install | Add ESLint packages/config | High |
| No CI workflows found | repo inventory | Regressions merge easily | Medium | N/A | Add build/test/client pipeline | High |
| No API integration tests | only service tests in `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63` | Middleware/auth routing regressions | Medium | WebApplicationFactory tests | Add API test project | High |
| No cursor pagination | message query `src/NetSlack.Application/Messages/MessageService.cs:16-33` | History latency for large channels | Medium later | Load test 1M messages | Cursor pagination/index review | Medium |
| Production client Dockerfile runs dev server | `deploy/react-client.Dockerfile:4-7` | Inefficient/insecure prod deployment | Medium | Build image review | Static build + CDN/server | High |

