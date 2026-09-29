# Runtime And Tooling Map

## .NET

| Topic | Evidence | What it means |
| --- | --- | --- |
| Target framework | `Directory.Build.props:3` | All projects default to `net8.0`. |
| Nullable refs | `Directory.Build.props:4` | Nullability warnings are part of correctness. |
| Warnings as errors | `Directory.Build.props:6` | New warnings block builds. Treat warnings seriously. |
| Web SDK | `src/NetSlack.Api/NetSlack.Api.csproj:1` | API project gets ASP.NET Core hosting/build behavior. |
| EF packages | `src/NetSlack.Infrastructure/NetSlack.Infrastructure.csproj:8-12` | Infrastructure owns EF design-time and SQLite provider. |
| JWT package | `src/NetSlack.Infrastructure/NetSlack.Infrastructure.csproj:6-14` | Infrastructure owns token validation/creation dependencies. |

## ASP.NET Runtime

The server starts with `WebApplication.CreateBuilder` in `src/NetSlack.Api/Program.cs:11`, registers services in `src/NetSlack.Api/Program.cs:13-55`, builds the app in `src/NetSlack.Api/Program.cs:57`, then orders middleware and endpoints in `src/NetSlack.Api/Program.cs:59-80`.

Pipeline mental model:

1. Exception middleware wraps later work: `src/NetSlack.Api/Program.cs:59`.
2. Dev-only Swagger and seed run: `src/NetSlack.Api/Program.cs:61-68`.
3. HTTPS/CORS/auth run before endpoints: `src/NetSlack.Api/Program.cs:70-73`.
4. Endpoints and hub are mapped: `src/NetSlack.Api/Program.cs:75-80`.

## React Runtime

React starts at `client/src/main.tsx:1-9`, renders `App`, and `App` decides whether to show sign-in or workspace shell in `client/src/App.tsx:24-31`. The workspace shell uses `useEffect` to fetch workspaces, channels, and messages in `client/src/features/workspace/WorkspaceShell.tsx:38-115`. Effects must clean up async subscriptions; the SignalR hook does this in `client/src/hooks/useSignalRChannel.ts:34-41`.

## Environment And Secrets

- API reads SQLite connection, CORS origins, and JWT options from `src/NetSlack.Api/appsettings.json:1-20`.
- Compose overrides API values with environment variables in `deploy/docker-compose.yml:6-11`.
- Client API base URL comes from `VITE_API_BASE_URL` with fallback in `client/src/api/http.ts:3`.
- `Jwt:SigningKey` is in a checked-in development config at `src/NetSlack.Api/appsettings.json:8-12`; production must use secret management.

## Verification Notes

- No `global.json`, `Directory.Packages.props`, or `nuget.config` were found.
- No lockfile was found under `client`; dependency versions are semver ranges in `client/package.json:12-23`.

