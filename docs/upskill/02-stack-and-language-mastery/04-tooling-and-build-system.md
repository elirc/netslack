# Tooling And Build System

## MSBuild And NuGet

`Directory.Build.props:1-10` centralizes target framework, nullable references, implicit usings, warnings-as-errors, analysis level, and language version. This means a warning introduced in any C# project should fail the build.

Project references:

- API references Application and Infrastructure in `src/NetSlack.Api/NetSlack.Api.csproj:2-5`.
- Infrastructure references Application in `src/NetSlack.Infrastructure/NetSlack.Infrastructure.csproj:2-4`.
- Application references Domain in `src/NetSlack.Application/NetSlack.Application.csproj:1-5`.
- Tests reference Application and Infrastructure in `tests/NetSlack.Application.Tests/NetSlack.Application.Tests.csproj:1-6`.

Package ownership:

- API owns Swagger/OpenTelemetry in `src/NetSlack.Api/NetSlack.Api.csproj:7-14`.
- Infrastructure owns EF/JWT in `src/NetSlack.Infrastructure/NetSlack.Infrastructure.csproj:6-14`.

## npm And Vite

Client scripts are in `client/package.json:6-10`. Vite uses port 5173 with `strictPort` in `client/vite.config.ts:4-9`. React, SignalR, and icons are runtime dependencies in `client/package.json:12-17`; TypeScript/Vite are dev dependencies in `client/package.json:18-23`.

Investigate: `client/package.json:10` defines `eslint`, but no ESLint dependency appears in `client/package.json:18-23`. The lint command may fail until lint packages/config are added.

## Docker

The API Dockerfile restores and publishes the solution in `deploy/dotnet-api.Dockerfile:1-11`. The client Dockerfile runs Vite dev server in `deploy/react-client.Dockerfile:1-7`. Compose wires API/client ports and data volume in `deploy/docker-compose.yml:1-29`.

## Command Status

See `../08-reference/command-cheatsheet.md` for verified vs inferred command status.

## Drill

Pick one package reference and explain why it belongs in that project instead of another. Example: why EF SQLite is in Infrastructure, not Domain.

