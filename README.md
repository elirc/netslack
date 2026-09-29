# NetSlack

NetSlack is a Slack-style collaboration app built with ASP.NET Core, EF Core, SignalR, and React. The codebase is shaped to teach junior engineers how a senior engineer keeps a product understandable as it grows: clear boundaries, typed contracts, centralized errors, focused tests, and production notes where the demo intentionally stops.

## What Works

- Development sign-in with JWTs.
- Workspace and channel creation.
- Public and private channel authorization rules.
- Message history over REST.
- Realtime message fanout over SignalR.
- SQLite-backed persistence for local development.
- Health checks, Swagger, CORS, structured problem responses, and OpenTelemetry tracing.

## Project Layout

```text
src/
  NetSlack.Domain/          Business entities and invariants
  NetSlack.Application/     Use cases, DTOs, authorization checks
  NetSlack.Infrastructure/  EF Core, JWTs, seed data
  NetSlack.Api/             HTTP endpoints, middleware, SignalR
client/                     React + TypeScript workspace UI
tests/                      Service-level tests
docs/                       Architecture and production notes
deploy/                     Dockerfiles and compose
```

## Run Locally

The backend expects .NET 8 SDK.

```powershell
dotnet restore .\NetSlack.sln
dotnet test .\NetSlack.sln
dotnet run --project .\src\NetSlack.Api\NetSlack.Api.csproj
```

The client expects Node 22 or compatible.

```powershell
cd client
npm install
npm run dev
```

Open `http://localhost:5173`. The default development sign-in is:

- Email: `ada@netslack.dev`
- Display name: `Ada Lovelace`

## Senior Engineering Notes

- The API does not contain business logic beyond transport concerns. Endpoints call application services.
- Application services enforce workspace/channel access. This matters more than UI hiding.
- Domain objects normalize names and reject invalid state close to where state is created.
- Realtime delivery is additive. Message creation still commits through the application service first, then publishes to SignalR.
- Errors are mapped centrally to Problem Details so clients receive a consistent shape.
- The React app has a single typed API boundary. Components do not hand-roll fetch calls.

## Production Next Steps

Read [docs/architecture.md](docs/architecture.md) for the full checklist. The biggest real-world upgrades are OIDC auth, database migrations, managed database infrastructure, SignalR scale-out, audit logging, rate limits, retention policies, and deeper automated tests.

